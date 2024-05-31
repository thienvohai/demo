using EmailService.Domain;

namespace EmailService.Service
{
    public interface IFilterRecipientService
    {
        Task<bool> FilterRecipientAsync(EmailMessage emailMessage);
    }
}
