namespace EmailService;

[Table(TableNames.EmailPolicyTable)]
public class EmailPolicyEntity : BaseEntity
{
    [Column(EmailPolicyColumns.TenantId)]
    public string TenantId { get; set; }
    [Column(EmailPolicyColumns.Content)]
    public string Content { get; set; }
}