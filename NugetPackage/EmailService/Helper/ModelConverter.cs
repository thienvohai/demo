using System.Text.Json;

namespace EmailService;

public class ModelConverter
{
    public static EmailEntity ConvertEmailMessageToNewEntityWithStatus(EmailMessage message, int status, string comment = "", string tenantId = "")
    {
        var newEmailEntity = new EmailEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Status = status,
            Sender = message.From,
            To = string.Join(";", message.To),
            CC = string.Join(";", message.Cc),
            Bcc = string.Join(";", message.Bcc),
            Comment = comment
        };
        return newEmailEntity;
    }

    public static EmailPolicyEntity ConvertPolicyToNewEntityWithTenantId(EmailSenderPolicy policy, string tenantId)
    {
        var newPolicyEntity = new EmailPolicyEntity
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            TenantId = tenantId,
            Content = JsonSerializer.Serialize(policy)
        };
        return newPolicyEntity;
    }
}
