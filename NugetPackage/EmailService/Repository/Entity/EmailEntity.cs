namespace EmailService;

[Table(TableNames.EmailTable)]
public class EmailEntity : BaseEntity
{
    [Column(EmailColumns.TenantId)]
    public string? TenantId { get; set; }
    [Column(EmailColumns.Status)]
    public int Status { get; set; }

    [Column(EmailColumns.Sender)]
    public string Sender { get; set; } = string.Empty;
    [Column(EmailColumns.To)]
    public string To { get; set; }
    [Column(EmailColumns.CC)]
    public string? CC { get; set; }
    [Column(EmailColumns.Bcc)]
    public string? Bcc { get; set; }
    [Column(EmailColumns.Comment)]
    public string? Comment { get; set; }
}