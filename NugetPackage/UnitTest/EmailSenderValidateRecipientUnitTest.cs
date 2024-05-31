using EmailService;
using Microsoft.Extensions.Logging;

namespace UnitTest
{
    public class EmailSenderValidateRecipientUnitTest
    {
        [Fact]
        public async Task TestSenderSetupDefaultAndTenant()
        {
            var profileId = "newid";
            var defaultAllowList = new List<string>()
            {
                "*@gmail.com",
            };
                    var defaultBlockList = new List<string>()
            {
                "Victor.Ly@gmail.com",
            };
            var allowList = new List<string>()
            {
                "*",
            };
            var blockList = new List<string>()
            {
                "*@gmail.com",
            };
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
            }, logger);
            await emailSender.SetDefaultPolicyAsync(new EmailSenderPolicy()
            {
                AllowList = defaultAllowList,
                BlockList = defaultBlockList
            });
            await emailSender.SetTenantPolicyAsync(profileId, new EmailSenderPolicy()
            {
                AllowList = allowList,
                BlockList = blockList
            });
            var emailMessage = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {
                    "Young.Nguyen@gmail.com",
                    "Victor.Ly@gmail.com"
                },
                Cc = new List<string>()
                {
                    "Aaron.Vo@gmail.com"
                },
                Bcc = new List<string>()
                {
                    "Ezreal.Luu@gmail.com"
                }
            };
            Func<Task> mockDelegateSend = async () =>
            {
                await Task.Delay(1);
            };
            var result = await emailSender.TrySendAsync(profileId, emailMessage, mockDelegateSend);
            Assert.True(result.Result == SendEmailResult.RecipientNotAllowed);
            Assert.True(string.Equals(result.Message, "Young.Nguyen@gmail.com;Victor.Ly@gmail.com", StringComparison.OrdinalIgnoreCase));

            var result1 = await emailSender.TrySendAsync(emailMessage, mockDelegateSend);
            Assert.True(result1.Result == SendEmailResult.RecipientNotAllowed);
            Assert.True(string.Equals(result.Message, "Young.Nguyen@gmail.com;Victor.Ly@gmail.com", StringComparison.OrdinalIgnoreCase));

            var rightEmailMessage = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {
                    "Young.Nguyen@gmail.com",
                },
                Cc = new List<string>()
                {
                    "Aaron.Vo@gmail.com"
                },
                Bcc = new List<string>()
                {
                    "Ezreal.Luu@gmail.com"
                }
            };
            var result2 = await emailSender.TrySendAsync(rightEmailMessage, mockDelegateSend);
            Assert.True(result2.Result == SendEmailResult.Success);
        }

        [Fact]
        public async Task TestSenderForSuccessfulTenantId()
        {
            var profileId = "newid";
            var defaultAllowList = new List<string>()
            {
                "*@gmail.com",
            };
            var allowList = new List<string>()
            {
                "*",
            };
            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole());
            ILogger<EmailSenderInitilizeTest> logger = loggerFactory.CreateLogger<EmailSenderInitilizeTest>();
            var emailSender = new EmailSender(new EmailSenderSetting
            {
                ConnectionString = "Server=edu-pgflex-dev-sg.postgres.database.azure.com;Port=6432;Database=nus-email-service;User Id=postgres;Password=Ex4FzFZdew;Timeout=300;CommandTimeout=300;",
                DbType = DatabaseType.PostgreSql,
                RetentionDays = 7,
                TableSchema = "public"
            }, logger);
            await emailSender.SetDefaultPolicyAsync(new EmailSenderPolicy()
            {
                AllowList = defaultAllowList,
            });
            await emailSender.SetTenantPolicyAsync(profileId, new EmailSenderPolicy()
            {
                AllowList = allowList,
            });
            var emailMessage = new EmailMessage()
            {
                From = "walt.ji@gmail.com",
                To = new List<string>()
                {
                    "walt.ji@gmail.com"
                },
                Cc = new List<string>()
                {
                    "walt.ji@gmail.com"
                },
                Bcc = new List<string>()
                {
                    "walt.ji@gmail.com"
                }
            };
            Func<Task> mockDelegateSend = async () =>
            {
                await Task.Delay(1);
            };
            var result = await emailSender.TrySendAsync(profileId, emailMessage, mockDelegateSend);
            Assert.True(result.Result == SendEmailResult.Success);

            var result1 = await emailSender.TrySendAsync(emailMessage, mockDelegateSend);
            Assert.True(result1.Result == SendEmailResult.Success);          
        }

        [Fact]
        public async Task TestSenderCache()
        {
            var profileId = "cache";
            var defaultAllowList = new List<string>()
            {
                "*.@gmail.com",
            };
            var allowList = new List<string>()
            {
                "123.@qq.com",
            };
            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole());
            ILogger<EmailSenderInitilizeTest> logger = loggerFactory.CreateLogger<EmailSenderInitilizeTest>();
            var emailSender = new EmailSender(new EmailSenderSetting
            {
                ConnectionString = "Server=edu-pgflex-dev-sg.postgres.database.azure.com;Port=6432;Database=nus-email-service;User Id=postgres;Password=Ex4FzFZdew;Timeout=300;CommandTimeout=300;",
                DbType = DatabaseType.PostgreSql,
                RetentionDays = 7,
                TableSchema = "public"
            }, logger);
            await emailSender.SetDefaultPolicyAsync(new EmailSenderPolicy()
            {
                AllowList = defaultAllowList,
            });
            await emailSender.SetTenantPolicyAsync(profileId, new EmailSenderPolicy()
            {
                AllowList = allowList,
            });
            var allowList2 = new List<string>()
            {
                "456.@qq.com",
            };
            await emailSender.SetTenantPolicyAsync(profileId, new EmailSenderPolicy()
            {
                AllowList = allowList2,
            });

            var emailMessage = new EmailMessage()
            {
                From = "walt.ji@gmail.com",
                To = new List<string>()
                {
                    "123.@qq.com"
                },
                Cc = new List<string>()
                {
                    "123.@qq.com"
                },
                Bcc = new List<string>()
                {
                    "123.@qq.com"
                }
            };
            Func<Task> mockDelegateSend = async () =>
            {
                await Task.Delay(1);
            };
            var result = await emailSender.TrySendAsync(profileId, emailMessage, mockDelegateSend);
            Assert.True(result.Result == SendEmailResult.RecipientNotAllowed);
        }
    }
}