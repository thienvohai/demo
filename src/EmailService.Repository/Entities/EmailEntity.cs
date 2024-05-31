namespace EmailService.Repository;

[Table(TableNames.EmailTable)]
public class EmailEntity : BaseEntity
{
    [Column(EmailColumns.Status)]
    public int Status { get; set; }
    [Column(EmailColumns.RetryCount)]
    public int RetryCount { get; set; }
    [Column(EmailColumns.IsRetrying)]
    public bool IsRetrying { get; set; }
    [Column(EmailColumns.NextRetryTime)]
    public DateTime? NextRetryTime { get; set; }
    [Column(EmailColumns.Body)]
    public string? Body { get; set; }
    [Column(EmailColumns.IsBodyHtml)]
    public bool IsBodyHtml { get; set; }
    [Column(EmailColumns.Sender)]
    public string Sender { get; set; } = string.Empty;
    [Column(EmailColumns.SenderName)]
    public string? SenderName { get; set; }
    [Column(EmailColumns.Subject)]
    public string? Subject { get; set; }
    [Column(EmailColumns.Attachment)]
    public string? Attachment { get; set; }
    [Column(EmailColumns.To)]
    public string To { get; set; }
    [Column(EmailColumns.CC)]
    public string? CC { get; set; }
    [Column(EmailColumns.Bcc)]
    public string? Bcc { get; set; }
}