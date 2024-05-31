using EmailService.Core;
using EmailService.Domain;
using EmailService.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace EmailService.Service
{
    public class EmailRetryTimerService : BackgroundService
    {
        private int numParallelTask = 0;
        private readonly ILogger<EmailMessageHandlerHostedService> logger;
        private readonly IUnitOfWorkFactory uowFactory;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IFilterRecipientService filterRecipientService;
        private readonly BackgroundWorkerOptions backgroundWorkerOptions;

        public EmailRetryTimerService(
            ILogger<EmailMessageHandlerHostedService> logger,
            IUnitOfWorkFactory uowFactory,
            IServiceScopeFactory scopeFactory,
            IOptions<BackgroundWorkerOptions> backgroundWorkerOptions,
            IFilterRecipientService filterRecipientService)
        {
            this.logger = logger;
            this.uowFactory = uowFactory;
            this.scopeFactory = scopeFactory;
            this.backgroundWorkerOptions = backgroundWorkerOptions.Value;
            this.filterRecipientService = filterRecipientService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"Email Retry Timer Hosted Service is running");
            while (!stoppingToken.IsCancellationRequested)
            {
                if (numParallelTask >= backgroundWorkerOptions.MaxConcurrentTask)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                await Task.Delay(1000, stoppingToken);
                await RetryEmails();
            }
        }

        private async Task RetryEmails()
        {
            var logIds = "";
            try
            {
                using var uow = uowFactory.CreateUnitOfWork();
                var emailDetails = await uow.EmailRepository.GetFailedEmailsAsync(pageSize: 10, pageIndex: 1, limitRetry: 30);
                uow.Commit();
                if (!emailDetails.Any())
                    return;

                var emailMessages = ConvertToEmailMessageHelper.ConvertEmailDetailsToEmailMessages(emailDetails);
                logIds = string.Join(";", emailMessages.Select(e => e.EmailId));
                foreach (var emailMessage in emailMessages)
                {
                    var updateRetrying = await uow.EmailRepository.UpdateRetryingAsync(new EmailEntity()
                    {
                        Id = emailMessage.EmailId,
                        Modified = emailMessage.Modified,
                    });
                    uow.Commit();
                    if (updateRetrying > 0)
                    {
                        _ = RetryEmail(scopeFactory, emailMessage);
                        Interlocked.Increment(ref numParallelTask);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred retry email item: {logIds}");
            }
        }

        private async Task RetryEmail(IServiceScopeFactory scopeFactory, EmailMessageConvertedModel emailMessage)
        {
            try
            {
                using var uow = uowFactory.CreateUnitOfWork();
                var filterStatus = await filterRecipientService.FilterRecipientAsync(emailMessage.EmailMessage);
                if (!filterStatus)
                {
                    await uow.EmailRepository.UpdateFilteredEmail(new EmailEntity
                    {
                        Id = emailMessage.EmailId
                    });
                    uow.Commit();
                    logger.LogInformation($"Email item {emailMessage.EmailId} handled with status: {EmailStatus.Filtered}");
                    return;
                }
                using var scope = scopeFactory.CreateScope();
                var emailClient = scope.ServiceProvider.GetService<IEmailClient>();
                var response = await emailClient!.SendEmailAsync(emailMessage.EmailMessage);
                var logStatus = "";
                
                if ((response.StatusCode >= 200) && (response.StatusCode <= 299))
                {
                    await uow.EmailRepository.UpdateSentEmail(new EmailEntity()
                    {
                        Id = emailMessage.EmailId,
                        RetryCount = emailMessage.RetryCount + 1
                    });
                    logStatus = EmailStatus.Sent.ToString();
                }
                else
                {
                    await uow.EmailRepository.UpdateFailedEmail(new EmailEntity
                    {
                        Id = emailMessage.EmailId,
                        Status = (int)EmailStatus.Failed,
                        RetryCount = emailMessage.RetryCount + 1,
                        NextRetryTime = ConvertToEntityHelper.ConvertNextRetryTimeFromRetryCount(emailMessage.RetryCount),
                    });
                    logStatus = EmailStatus.Failed.ToString();
                }
                uow.Commit();
                logger.LogInformation($"Retry email {emailMessage.EmailId} with Status: {logStatus}, Retry Count: {emailMessage.RetryCount + 1}, Finish at {DateTime.UtcNow}");
            }
            catch(Exception ex)
            {
                logger.LogInformation($"Retry email {emailMessage.EmailId} failed, Error: {ex}, Finish at {DateTime.UtcNow}");
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
}
