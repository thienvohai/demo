using EmailService.Core;
using EmailService.Domain;
using EmailService.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace EmailService.Service;

public class EmailMessageHandlerHostedService : BackgroundService
{
    private int numParallelTask = 0;
    private readonly ILogger<EmailMessageHandlerHostedService> logger;
    private readonly IUnitOfWorkFactory uowFactory;
    private readonly IEmailMessageTaskQueue taskQueue;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IFilterRecipientService filterRecipientService;
    private readonly BackgroundWorkerOptions backgroundWorkerOptions;

    public EmailMessageHandlerHostedService(
        IEmailMessageTaskQueue taskQueue,
        ILogger<EmailMessageHandlerHostedService> logger,
        IUnitOfWorkFactory uowFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<BackgroundWorkerOptions> backgroundWorkerOptions,
        IFilterRecipientService filterRecipientService)
    {
        this.taskQueue = taskQueue;
        this.logger = logger;
        this.uowFactory = uowFactory;
        this.scopeFactory = scopeFactory;
        this.backgroundWorkerOptions = backgroundWorkerOptions.Value;
        this.filterRecipientService = filterRecipientService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Email Message Handler Hosted Service is running");

        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (numParallelTask >= backgroundWorkerOptions.MaxConcurrentTask)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            try
            {
                var workItem = await taskQueue.DequeueAsync<SendEmailEventMessage>(stoppingToken);
                if (workItem != null)
                {
                    _ = HandleEmail(workItem);
                    Interlocked.Increment(ref numParallelTask);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Email Message Handler Hosted Service Error");
            }

        }
    }

    private async Task HandleEmail(SendEmailEventMessage eventMessage)
    {
        try
        {
            using var uow = uowFactory.CreateUnitOfWork();
            var filterStatus = await filterRecipientService.FilterRecipientAsync(eventMessage.EmailDetails);
            if (!filterStatus)
            {
                await uow.EmailRepository.UpdateFilteredEmail(new EmailEntity
                {
                    Id = eventMessage.EmailId,
                });
                uow.Commit();
                logger.LogInformation($"Email item {eventMessage.EmailId} handled with status: {EmailStatus.Filtered}");
                return;
            }
            using var scope = scopeFactory.CreateScope();
            var emailClient = scope.ServiceProvider.GetService<IEmailClient>();
            var policyWithRetry = Policy.HandleResult<EmailClientResponse>(r => !((r.StatusCode >= 200) && (r.StatusCode <= 299)))
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(10 * (retryAttempt + 1)));
            var response = await policyWithRetry.ExecuteAsync(() => emailClient.SendEmailAsync(eventMessage.EmailDetails));
            if ((response.StatusCode >= 200) && (response.StatusCode <= 299))
            {
                await uow.EmailRepository.UpdateSentEmail(new EmailEntity
                {
                    Id = eventMessage.EmailId,
                });
                uow.Commit();
                logger.LogInformation($"Email item {eventMessage.EmailId} handled with status: {EmailStatus.Sent}, Message: {response.Message}");
            }
            else
            {
                await uow.EmailRepository.UpdateFailedEmail(new EmailEntity
                {
                    Id = eventMessage.EmailId,
                    Status = (int)EmailStatus.Failed,
                    RetryCount = 2,
                    NextRetryTime = ConvertToEntityHelper.ConvertNextRetryTimeFromRetryCount(2)
                });
                uow.Commit();
                logger.LogInformation($"Email item {eventMessage.EmailId} handled with status: {EmailStatus.Failed}, Message: {response.Message}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error occurred executing email item: {eventMessage.EmailId}");
        }
        finally
        {
            Interlocked.Decrement(ref numParallelTask);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email Message Handler Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}
