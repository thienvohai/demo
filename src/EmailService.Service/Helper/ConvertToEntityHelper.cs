using EmailService.Domain;
using EmailService.Repository;
using Newtonsoft.Json;

namespace EmailService.Service;

public class ConvertToEntityHelper
{
    public static EmailEntity ConvertMessageToEmailEntity(EmailMessage messageDTO)
    {
        var emailEntity = new EmailEntity()
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Status = (int)EmailStatus.New,
            RetryCount = 0,
            Sender = messageDTO.From,
            SenderName = messageDTO.SenderName,
            Attachment = JsonConvert.SerializeObject(messageDTO.Attachments),
            IsBodyHtml = messageDTO.IsBodyHtml,
            Body = messageDTO.Body,
            Subject = messageDTO.Subject,
            To = string.Join(";", messageDTO.To),
            CC = messageDTO.Cc != null && messageDTO.Cc.Count > 0 ? string.Join(";", messageDTO.Cc) : null,
            Bcc = messageDTO.Bcc != null && messageDTO.Bcc.Count > 0 ? string.Join(";", messageDTO.Bcc) : null
        };

        return emailEntity;
    }

    public static DateTime ConvertNextRetryTimeFromRetryCount(int retryCount)
    {
        switch (retryCount)
        {
            case 0:
                return DateTime.UtcNow.AddSeconds(5);
            case 1:
                return DateTime.UtcNow.AddSeconds(30);
            case 2:
                return DateTime.UtcNow.AddMinutes(1);
            case 3:
                return DateTime.UtcNow.AddMinutes(5);
            case 4:
                return DateTime.UtcNow.AddMinutes(30);
            case 5:
                return DateTime.UtcNow.AddHours(1);
            default: 
                return DateTime.UtcNow.AddHours(1);
        }
    }
}
