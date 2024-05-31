using Microsoft.Extensions.Logging;

namespace EmailService;

//This class represents service use to read/write Email message record
public class EmailRecordService : IEmailRecordService
{
    private readonly ILogger logger;
    private readonly IUnitOfWorkFactory uowFactory;
    public EmailRecordService(ILogger logger, string databaseType, string connectionString, string tableSchema)
    {
        this.logger = logger;
        this.uowFactory = new UnitOfWorkFactory(databaseType, connectionString, tableSchema);
    }

    public async Task TrackingMessageRecordAsync(EmailMessage message, EmailTrackingStatus status, string comment = "", string tenantId = "")
    {
        var emailEntity = ModelConverter.ConvertEmailMessageToNewEntityWithStatus(message, (int)status, comment, tenantId);
        await SaveEmailEntityAsync(emailEntity);
    }

    private async Task SaveEmailEntityAsync(EmailEntity emailEntity)
    {
        try
        {
            using var uow = uowFactory.CreateUnitOfWork();
            await uow.EmailRepository.AddAsync(emailEntity);
            uow.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Email sender error: An error occurred while saving the email message entity. Message: {ex.Message}");
        }
    }
}
