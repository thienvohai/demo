using EmailService;
using Microsoft.Extensions.Logging;

namespace UnitTest
{
    public class EmailSenderInitilizeTest
    {
        [Fact]
        public void TestMigrateDataToSqlServer()
        {
            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole());
            ILogger<EmailSenderInitilizeTest> logger = loggerFactory.CreateLogger<EmailSenderInitilizeTest>();

            var emailSender = new EmailSender(new EmailSenderSetting
            {
                ConnectionString = "Data Source=localhost;Initial Catalog=ES-CoreDB-UnitTest-2;User ID=sa;Password=1qaz2wsxE;TrustServerCertificate=True;",
                DbType = DatabaseType.SqlServer,
                RetentionDays = 7,
                TableSchema = "dbo"
            },
            logger);
            
            emailSender.Initialize();
        }

        [Fact]
        public void TestMigrateDataToPostgreSql()
        {
            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole());
            ILogger<EmailSenderInitilizeTest> logger = loggerFactory.CreateLogger<EmailSenderInitilizeTest>();

            var emailSender = new EmailSender(new EmailSenderSetting
            {
                ConnectionString = "Server=localhost;Database=EmailService-UnitTest-2;Port=5432;Maximum Pool Size=9999;NoResetOnClose=true;ReadBufferSize=204800;WriteBufferSize=204800;UserId=postgres;Password=1qaz2wsxE;Timeout=300;CommandTimeout=300;",
                DbType = DatabaseType.PostgreSql,
                RetentionDays = 7,
                TableSchema = "public"
            },
            logger);

            emailSender.Initialize();
        }
    }
}