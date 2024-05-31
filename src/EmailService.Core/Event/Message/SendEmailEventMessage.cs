using EmailService.Domain;

namespace EmailService.Core;

public class SendEmailEventMessage : BaseEventMessage
{
    public Guid EmailId { get; set; }
    public EmailMessage EmailDetails { get; set; } = new();
}
