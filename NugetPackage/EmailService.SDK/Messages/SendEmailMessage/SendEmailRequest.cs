using EmailService.Domain;

namespace EmailService.SDK;

internal class SendEmailRequest : EmailServiceRequest
{
    public SendEmailRequest(EmailMessage emailMessage)
    {
        Method = HttpMethod.Post;
        RequestPath = "api/v1/emails";
        InputParameters = emailMessage;
    }
}
