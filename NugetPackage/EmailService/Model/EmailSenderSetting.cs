namespace EmailService;

/*
 This class represents the setting we used for Email Sender:
    - Database server setting
 */
public class EmailSenderSetting
{
    //Type of database server: SqlServer, PostgreSql
    public DatabaseType DbType { get; set; }
    public string ConnectionString { get; set; }
    //Table schema, Example: SqlServer use "dbo", PostgreSql use "public"
    public string TableSchema { get; set; }
    public int RetentionDays { get; set; }
}

public enum DatabaseType
{
    SqlServer,
    PostgreSql
}