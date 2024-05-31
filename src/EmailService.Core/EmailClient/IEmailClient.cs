using EmailService.Domain;

namespace EmailService.Core;

public interface IEmailClient
{
    Task<EmailClientResponse> SendEmailAsync(EmailMessage emailMessage);
}
