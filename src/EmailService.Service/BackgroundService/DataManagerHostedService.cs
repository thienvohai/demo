using EmailService.Repository;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmailService.Service;

public class DataManagerHostedService : BackgroundService
{
    private readonly ILogger<DataManagerHostedService> logger;
    private readonly IUnitOfWorkFactory uowFactory;

    public DataManagerHostedService(
        ILogger<DataManagerHostedService> logger,
        IUnitOfWorkFactory uowFactory)
    {
        this.logger = logger;
        this.uowFactory = uowFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Data Manager Hosted Service is running");

        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RemoveOldEmail();
            await Task.Delay(1000 * 60 * 60 * 8, stoppingToken);
        }
    }

    private async Task RemoveOldEmail()
    {
        try
        {
            using var uow = uowFactory.CreateUnitOfWork();
            var before = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7));
            var result = await uow.EmailRepository.RemoveOldEmail(before);
            uow.Commit();
            logger.LogInformation($"Data Manager Hosted Service removed {result} email record modifed before {before}");
            return;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error occurred executing remove old email record: {ex.Message}");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email Message Handler Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}
