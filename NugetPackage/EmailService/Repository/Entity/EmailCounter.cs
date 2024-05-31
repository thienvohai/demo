namespace EmailService;

[Table(TableNames.EmailCounterTable)]
public class EmailCounterEntity : BaseEntity
{
    [Column(EmailCounterColumns.Type)]
    public int Type { get; set; }
    [Column(EmailCounterColumns.Target)]
    public string Target { get; set; }
    [Column(EmailCounterColumns.Count)]
    public int Count {  get; set; }
    [Column(EmailCounterColumns.Minutes)]
    public int Minutes { get; set; }
}