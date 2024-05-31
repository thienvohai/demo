namespace EmailService;

public interface IEmailCounterService
{
    Task UpdateEmailCounterRecord(EmailMessage emailMessage, string tenantId = "");
}
