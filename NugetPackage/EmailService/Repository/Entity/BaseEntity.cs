namespace EmailService;

public class BaseEntity
{
    [Column(BaseColumns.Id)]
    public Guid Id { get; set; }
    [Column(BaseColumns.IsDeleted)]
    public bool IsDeleted { get; set; }
    [Column(BaseColumns.Created)]
    public DateTime Created { get; set; }
    [Column(BaseColumns.Modified)]
    public DateTime Modified { get; set; }
}
