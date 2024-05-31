namespace EmailService;

public interface IEmailPolicyService : IDisposable
{
    Task<EmailSenderPolicy?> GetTenantPolicyAsync(string tenantId);
    Task<EmailSenderPolicy?> GetDefaultPolicyAsync();
    Task AddTenantPolicyAsync(string tenantId, EmailSenderPolicy policy);
    Task AddDefaultPolicyAsync(EmailSenderPolicy policy);
}
