using EmailService.Domain;

namespace EmailService.Core;

public class EmailMessageHelper
{
    public static void DeDuplicateRecipient(EmailMessage message)
    {
        var duplicateRecipient = new HashSet<string>(message.To, StringComparer.OrdinalIgnoreCase);
        if (message.Cc != null && message.Cc.Count != 0)
        {
            message.Cc.RemoveAll(a => duplicateRecipient.Contains(a));
            foreach (var item in message.Cc)
            {
                duplicateRecipient.Add(item);
            };
        }

        if (message.Bcc != null && message.Bcc.Count != 0)
        {
            message.Bcc.RemoveAll(a => duplicateRecipient.Contains(a));
        }
    }
}
