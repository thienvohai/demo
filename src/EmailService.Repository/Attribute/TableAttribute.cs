namespace EmailService.Repository;

public class TableAttribute : Attribute
{
    public TableAttribute(string tableName)
    {
        Name = tableName;
    }

    public string Name { get; set; }
}
