namespace EmailService.Domain;

public class EmailMessage
{
    public string From { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public List<string> To { get; set; } = [];
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public bool IsBodyHtml { get; set; }
    public string Body { get; set; } = string.Empty;
    public List<AttachmentItem>? Attachments { get; set; }
}

public class AttachmentItem
{
    public string FileName { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public bool IsInlineDisposition { get; set; }
    public string? ContentId { get; set; }
}
