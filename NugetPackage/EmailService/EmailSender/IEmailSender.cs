namespace EmailService;

public interface IEmailSender : IDisposable
{
    void Initialize();
    Task SetDefaultPolicyAsync(EmailSenderPolicy policy);
    Task SetTenantPolicyAsync(string tenantId, EmailSenderPolicy policy);
    Task<SendEmailResponse> TrySendAsync(string tenantId, EmailMessage message, Func<Task> sendEmailDelegate);
    Task<SendEmailResponse> TrySendAsync(EmailMessage message, Func<Task> sendEmailDelegate);
}
