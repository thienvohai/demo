using EmailService.Domain;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EmailService.Core;

public class SmtpEmailClient : IEmailClient
{
    private readonly SmtpOptions smtpOptions;

    public SmtpEmailClient(
        IOptions<EmailClientOptions> emailClientGridOptions)
    {
        this.smtpOptions = emailClientGridOptions.Value.Smtp;
    }

    public async Task<EmailClientResponse> SendEmailAsync(EmailMessage emailMessage)
    {
        try
        {
            using (var client = GenerateSmtpClient())
            {
                var mailMessage = GenerateMessage(emailMessage);
                await client.SendMailAsync(mailMessage);
                return new EmailClientResponse()
                {
                    StatusCode = 200,
                };
            }
        }
        catch (Exception ex)
        {
            return new EmailClientResponse()
            {
                StatusCode = 500,
                Message = ex.Message
            };
        }
    }

    private SmtpClient GenerateSmtpClient()
    {
        var smtpClient = new SmtpClient
        {
            EnableSsl = smtpOptions.EnableSsl,
            Host = smtpOptions.Host,
            DeliveryMethod = (SmtpDeliveryMethod)smtpOptions.DeliveryMethod,
            Timeout = 12000,
            Port = Convert.ToInt32(smtpOptions.Port)
        };
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential(smtpOptions.UserName, smtpOptions.Password);
        return smtpClient;
    }

    private MailMessage GenerateMessage(EmailMessage emailMessage)
    {
        EmailMessageHelper.DeDuplicateRecipient(emailMessage);
        var msg = new MailMessage();
        msg.From = new MailAddress(emailMessage.From);
        msg.Subject = emailMessage.Subject ?? "(No Subject)";
        msg.Body = emailMessage.Body ?? "";
        msg.To.Add(string.Join(",", emailMessage.To));
        //TODO: Handle extention properties
        return msg;
    }
}
