namespace EmailService;

public interface IEmailCounterValidator
{
    Task<bool> ValidateAsync(EmailMessage emailMessage, out string reason);
    Task<bool> ValidateAsync(string tenantId, EmailMessage emailMessage, out string reason);

    void SetupDefaultLimit(int userReceiveLimit, int tenantSendLimit);
    void SetupTenantLimit(string tenantId, int userReceiveLimit, int tenantSendLimit);
}
