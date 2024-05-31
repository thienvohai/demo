namespace EmailService.Domain;

public class DatabaseOptions
{
    public const string Database = "Database";

    public string DatabaseType { get; set; } = Constants.Configuration.SqlServerType;
    public string ConnectionString { get; set; } = string.Empty;
    public string TableSchema { get; set; } = "dbo";
}

public class EmailClientOptions
{
    public const string EmailClient = "EmailClient";

    public string EmailClientType { get; set; } = Constants.Configuration.SendGridType;
    public SendGridOptions SendGrid { get; set; } = new();
    public SmtpOptions Smtp { get; set; } = new();
}

public class SendGridOptions
{
    public string ApiKey { get; set; } = string.Empty;
}

public class SmtpOptions
{
    public bool EnableSsl { get; set; }
    public string Host { get; set; } = string.Empty;
    public int DeliveryMethod { get; set; }
    public string Port { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}


public class AuthProviderOptions
{
    public const string AuthProvider = "AuthProvider";
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public List<string> ValidIssuers { get; set; }
}

public class BackgroundWorkerOptions
{
    public const string BackgroundWorker = "BackgroundWorker";
    public int MaxConcurrentTask { get; set; } = 100;
}

public class FilterRecipientOptions
{
    public const string FilterRecipient = "FilterRecipient";

    public List<string> BlockList { get; set; }
    public List<string> AllowList { get; set; }
}
