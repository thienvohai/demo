namespace EmailService;

public interface IEmailRecordService
{
    Task TrackingMessageRecordAsync(EmailMessage message, EmailTrackingStatus status, string comment = "", string tenantId = "");
}
