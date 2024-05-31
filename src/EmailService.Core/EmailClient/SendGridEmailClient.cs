using EmailService.Domain;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmailService.Core;

public class SendGridEmailClient : IEmailClient
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly SendGridOptions sendGridOptions;

    public SendGridEmailClient(
        IHttpClientFactory httpClientFactory,
        IOptions<EmailClientOptions> emailClientGridOptions)
    {
        this.httpClientFactory = httpClientFactory;
        this.sendGridOptions = emailClientGridOptions.Value.SendGrid;
    }

    public async Task<EmailClientResponse> SendEmailAsync(EmailMessage emailMessage)
    {
        try
        {
            using (var httpClient = this.httpClientFactory.CreateClient(Constants.HttpClientDefault.Default))
            {
                var sendGridClient = GenerateSendGridClient(httpClient);
                var msg = GenerateMessage(emailMessage);
                var response = await sendGridClient.SendEmailAsync(msg);
                return new EmailClientResponse()
                {
                    StatusCode = (int)response.StatusCode,
                    Message = response.IsSuccessStatusCode ? "" : await response.Body.ReadAsStringAsync(),
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

    private SendGridClient GenerateSendGridClient(HttpClient httpClient)
    {
        var options = new SendGridClientOptions()
        {
            ApiKey = sendGridOptions.ApiKey,
        };
        return new SendGridClient(httpClient, options);
    }

    private SendGridMessage GenerateMessage(EmailMessage emailMessage)
    {
        EmailMessageHelper.DeDuplicateRecipient(emailMessage);
        var msg = new SendGridMessage();

        msg.SetFrom(emailMessage.From, emailMessage.SenderName);

        msg.SetSubject(emailMessage.Subject);

        if (emailMessage.IsBodyHtml)
        {
            msg.AddContent("text/html", emailMessage.Body);
        }
        else
        {
            msg.AddContent("text/plain", emailMessage.Body);
        }

        msg.AddTos(emailMessage.To.Select(e => new EmailAddress(e)).ToList());

        if (emailMessage.Cc != null && emailMessage.Cc.Count != 0)
        {
            msg.AddCcs(emailMessage.Cc.Select(e => new EmailAddress(e)).ToList());
        }

        if (emailMessage.Bcc != null && emailMessage.Bcc.Count != 0)
        {
            msg.AddBccs(emailMessage.Bcc.Select(e => new EmailAddress(e)).ToList());
        }

        if (emailMessage.Attachments != null && emailMessage.Attachments.Count != 0)
        {
            msg.AddAttachments(emailMessage.Attachments.Select(a => new Attachment()
            {
                Content = a.Base64Content,
                Type = a.ContentType,
                Filename = a.FileName,
                Disposition = a.IsInlineDisposition ? "inline" : "attachment",
                ContentId = a.ContentId,
            }));
        }

        return msg;
    }
}
