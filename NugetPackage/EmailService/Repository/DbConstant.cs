namespace EmailService;

// FIXME: We should use Dapper to manipulate sql operations, so don't need to use constants
public static class TableNames
{
    public const string EmailTable = "Email";
    public const string EmailPolicyTable = "EmailPolicy";
    public const string EmailCounterTable = "EmailCounter";
}

public static class EmailColumns
{
    public const string TenantId = "TenantId";
    public const string Sender = "Sender";
    public const string Status = "Status";
    public const string To = "To";
    public const string CC = "CC";
    public const string Bcc = "Bcc";
    public const string Comment = "Comment";
}

public static class EmailPolicyColumns
{
    public const string TenantId = "TenantId";
    public const string Content = "Content";
}

public static class EmailCounterColumns
{
    public const string Type = "Type";
    public const string Target = "Target";
    public const string Count = "Count";
    public const string Minutes = "Minutes";
}

public static class BaseColumns
{
    public const string Id = "Id";
    public const string Created = "Created";
    public const string Modified = "Modified";
    public const string IsDeleted = "IsDeleted";
}
