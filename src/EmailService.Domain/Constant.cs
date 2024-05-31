namespace EmailService.Domain;

public static class Constants
{
    public static class Authentication
    {
        public const string BearerScheme = "Bearer";
        public const string DefaultPolicyName = "ApiScope";
        public const string ScopeClaim = "scope";
        public const string ValidScope = "emailservice.all";

    }
    public static class LogProviderName
    {
        public const string Common = "Common";
    }

    public static class Configuration
    {
        public const string SendGridType = "SendGrid";
        public const string StmpType = "Stmp";
        public const string SqlServerType = "SqlServer";
        public const string PostgreSql = "PostgreSql";
        public const string MysqlType = "Mysql";
    }

    public static class HttpClientDefault
    {
        public const string Default = "Default";
    }        
}
