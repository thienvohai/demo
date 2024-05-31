using EmailService.Domain;

namespace EmailService.Service;

public interface IEmailHandlerService
{
    Task<SaveEmailResponse> CreateEmailRecordAsync(EmailMessage emailMessage);
    Task<EmailDetailResponse> GetEmailDetailAsync(Guid emailId);
    Task<EmailsResponse> GetEmailsDetailAsync(EmailPageQuery query);
}
