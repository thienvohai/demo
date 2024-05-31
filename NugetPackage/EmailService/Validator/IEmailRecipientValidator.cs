namespace EmailService;

public interface IEmailRecipientValidator
{
    bool Validate(EmailMessage emailMessage, out string reason);
    bool Validate(string tenantId, EmailMessage emailMessage, out string reason);

    void SetupAllowList(string tenantId, List<string> allowList);
    void SetupBlockList(string tenantId, List<string> blockList);
    void SetupDefaultAllowList(List<string> defaultAllowList);
    void SetupDefaultBlockList(List<string> defaultBlockList);
}
