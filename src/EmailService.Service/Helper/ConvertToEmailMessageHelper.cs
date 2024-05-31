using EmailService.Domain;
using EmailService.Repository;
using Newtonsoft.Json;

namespace EmailService.Service;

public class ConvertToEmailMessageHelper
{
    public static List<EmailMessageConvertedModel> ConvertEmailDetailsToEmailMessages(IEnumerable<EmailEntity> emailDetails)
    {
        var result = new List<EmailMessageConvertedModel>();
        var groupedEmails = emailDetails.GroupBy(e => e.Id);
        foreach (var group in groupedEmails)
        {
            var emailMessage = new EmailMessage
            {
                From = group.First().Sender,
                SenderName = group.First().SenderName,
                To = [.. group.First().To.Split(';')],
                Cc = group.First().CC?.Split(';').ToList(),
                Bcc = group.First().Bcc?.Split(';').ToList(),
                Subject = group.First().Subject,
                IsBodyHtml = group.First().IsBodyHtml,
                Body = group.First().Body,
                Attachments = string.IsNullOrEmpty(group.First().Attachment) 
                    ? null : JsonConvert.DeserializeObject<List<AttachmentItem>>(group.First().Attachment)
            };

            result.Add(new EmailMessageConvertedModel()
            {
                EmailId = group.Key,
                EmailMessage = emailMessage,
                RetryCount = group.First().RetryCount,
                Modified = group.First().Modified,
            });
        }

        return result;
    }
}

public class EmailMessageConvertedModel
{
    public Guid EmailId { get; set; }
    public EmailMessage EmailMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime Modified { get; set; }
}
