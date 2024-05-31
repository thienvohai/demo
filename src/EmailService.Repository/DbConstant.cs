namespace EmailService.Repository;

// FIXME: We should use Dapper to manipulate sql operations, so don't need to use constants
public static class TableNames
{
    public const string EmailTable = "Email";
    public const string RecipientTable = "Recipient";
}

public static class EmailColumns
{
    public const string Sender = "Sender";
    public const string SenderName = "SenderName";
    public const string Subject = "Subject";
    public const string Body = "Body";
    public const string IsBodyHtml = "IsBodyHtml";
    public const string Attachment = "Attachment";
    public const string Status = "Status";
    public const string RetryCount = "RetryCount";
    public const string IsRetrying = "IsRetrying";
    public const string NextRetryTime = "NextRetryTime";
    public const string To = "To";
    public const string CC = "CC";
    public const string Bcc = "Bcc";
}

public static class BaseColumns
{
    public const string Id = "Id";
    public const string Created = "Created";
    public const string Modified = "Modified";
    public const string IsDeleted = "IsDeleted";
}
