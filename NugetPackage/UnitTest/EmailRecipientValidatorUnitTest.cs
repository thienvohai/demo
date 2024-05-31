using EmailService;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTest
{
    public class EmailRecipientValidatorUnitTest
    {
        private EmailRecipientValidator CreateEmailValidator(string profileId, List<string> allowList, List<string> blockList, List<string> defaultAllowList = null, List<string> defaultBlockList = null)
        {
            var emailValidator = new EmailRecipientValidator();
            emailValidator.SetupDefaultAllowList(defaultAllowList);
            emailValidator.SetupDefaultBlockList(defaultBlockList);
            emailValidator.SetupAllowList(profileId, allowList);
            emailValidator.SetupBlockList(profileId, blockList);
            return emailValidator;
        }

        private IEmailRecipientValidator CreateEmailValidatorByDI(string profileId, List<string> allowList, List<string> blockList, List<string> defaultAllowList = null, List<string> defaultBlockList = null)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IEmailRecipientValidator, EmailRecipientValidator>(ctx =>
            {
                var emailValidator = new EmailRecipientValidator();
                emailValidator.SetupDefaultAllowList(defaultAllowList);
                emailValidator.SetupDefaultBlockList(defaultBlockList);
                emailValidator.SetupAllowList(profileId, allowList);
                emailValidator.SetupBlockList(profileId, blockList);
                return emailValidator;
            });
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var emailValidator = serviceProvider.GetRequiredService<IEmailRecipientValidator>();
            return emailValidator;
        }

        [Fact]
        public void TestSetupBoth()
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
            var emailValidator = new EmailRecipientValidator();
            emailValidator.SetupDefaultAllowList(defaultAllowList);
            emailValidator.SetupDefaultBlockList(defaultBlockList);
            emailValidator.SetupAllowList(profileId, allowList);
            emailValidator.SetupBlockList(profileId, blockList);
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
            var result = emailValidator.Validate(profileId, emailMessage, out string reason);
            Assert.False(result);
            Assert.True(string.Equals(reason, "Young.Nguyen@gmail.com;Victor.Ly@gmail.com", StringComparison.OrdinalIgnoreCase));

            var result1 = emailValidator.Validate(emailMessage, out string reason1);
            Assert.False(result1);
            Assert.True(string.Equals(reason1, "Young.Nguyen@gmail.com;Victor.Ly@gmail.com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestOnlySetupBlockList()
        {
            var profileId = Guid.NewGuid().ToString();
            var blockList = new List<string>()
            {
                "*@gmail.com",
            };
            var emailValidator = new EmailRecipientValidator();
            emailValidator.SetupBlockList(profileId, blockList);
            var emailMessage1 = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {
                    "Young.Nguyen@gmail.com",
                    "Young.Nguyen@gmail.com",
                },
            };
            var result1 = emailValidator.Validate(emailMessage1, out string reason1);
            Assert.True(result1);

            var result3 = emailValidator.Validate(profileId, emailMessage1, out string reason3);
            Assert.False(result3);
            Assert.True(string.Equals(reason3, "Young.Nguyen@gmail.com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestOnlySetupAllowList()
        {
            var profileId = Guid.NewGuid().ToString();
            var allowList = new List<string>()
            {
                "*@gmail.com",
            };
            var emailValidator = new EmailRecipientValidator();
            emailValidator.SetupAllowList(profileId, allowList);
            var emailMessage1 = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {
                    "Young.Nguyen@gmail.com",
                    "Young.Nguyen@gmail.com",
                },
            };
            var result1 = emailValidator.Validate(emailMessage1, out string reason1);
            Assert.True(result1);

            var result3 = emailValidator.Validate(profileId, emailMessage1, out string reason3);
            Assert.False(result3);
            Assert.True(string.Equals(reason3, "Young.Nguyen@gmail.com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestOnlySetupAllowAndBlockList()
        {
            var profileId = Guid.NewGuid().ToString();
            var allowList = new List<string>()
            {
                "*",
            };
            var blockList = new List<string>()
            {
                "*@gmail.com",
            };
            var emailValidator = new EmailRecipientValidator();
            emailValidator.SetupAllowList(profileId, allowList);
            emailValidator.SetupBlockList(profileId, blockList);
            var emailMessage1 = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {
                    "Young.Nguyen@gmail.com",
                    "Young.Nguyen@gmail.com",
                },
            };
            var result1 = emailValidator.Validate(emailMessage1, out string reason1);
            Assert.True(result1);

            var result3 = emailValidator.Validate(profileId, emailMessage1, out string reason3);
            Assert.False(result3);
            Assert.True(string.Equals(reason3, "Young.Nguyen@gmail.com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestChangeDefaultBlockListAndExtend()
        {
            var profileId = Guid.NewGuid().ToString();
            var DefaultBlockList = new List<string>()
            {
                "Young.Nguyen@gmail.com",
            };
            var allowList = new List<string>()
            {
                "*",
            };
            var blockList = new List<string>()
            {
                "*@gmail.com",
            };
            var emailValidator = CreateEmailValidator(profileId, allowList, blockList, defaultBlockList: DefaultBlockList);
            var emailMessage1 = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {
                    "Young.Nguyen@gmail.com",
                    "Young.Nguyen@gmail.com",
                },
            };
            var result1 = emailValidator.Validate(emailMessage1, out string reason1);
            Assert.False(result1);
            Assert.True(string.Equals(reason1, "Young.Nguyen@gmail.Com", StringComparison.OrdinalIgnoreCase));

            var result2 = emailValidator.Validate(Guid.NewGuid().ToString(), emailMessage1, out string reason2);
            Assert.False(result2);
            Assert.True(string.Equals(reason2, "Young.Nguyen@gmail.Com", StringComparison.OrdinalIgnoreCase));

            var result3 = emailValidator.Validate(profileId, emailMessage1, out string reason3);
            Assert.False(result3);
            Assert.True(string.Equals(reason3, "Young.Nguyen@gmail.com;Young.Nguyen@gmail.com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestChangeDefaultAllowListAndOverride()
        {
            var profileId = Guid.NewGuid().ToString();
            var DefaultAllowList = new List<string>()
            {
                "Young.Nguyen@gmail.com",
            };
            var allowList = new List<string>()
            {
                "Scott.Nguyen@gmail.com",
            };
            var blockList = new List<string>()
            {
            };
            var emailValidator = CreateEmailValidator(profileId, allowList, blockList, DefaultAllowList);
            var emailMessage1 = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {
                    "Young.Nguyen@gmail.com",
                    "Scott.nguyen@gmail.com"
                },
            };
            var result1 = emailValidator.Validate(emailMessage1, out string reason1);
            Assert.False(result1);
            Assert.True(string.Equals(reason1, "Scott.Nguyen@gmail.com", StringComparison.OrdinalIgnoreCase));

            var result2 = emailValidator.Validate(Guid.NewGuid().ToString(), emailMessage1, out string reason2);
            Assert.False(result2);
            Assert.True(string.Equals(reason2, "Scott.Nguyen@gmail.com", StringComparison.OrdinalIgnoreCase));

            var result3 = emailValidator.Validate(profileId, emailMessage1, out string reason3);
            Assert.False(result3);
            Assert.True(string.Equals(reason3, "Young.Nguyen@gmail.Com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestBlockByAsterisk()
        {
            var profileId = Guid.NewGuid().ToString();
            var allowList = new List<string>()
            {
                "*",
            };
            var blockList = new List<string>()
            {
                "*@gmail.com",
            };
            var emailValidator = CreateEmailValidator(profileId, allowList, blockList);
            var emailMessage1 = new EmailMessage()
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
            var result1 = emailValidator.Validate(profileId, emailMessage1, out string reason1);
            Assert.False(result1);
            Assert.True(string.Equals(reason1, "Young.Nguyen@gmail.Com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestBlockByToAddress()
        {
            var profileId = Guid.NewGuid().ToString();
            var allowList = new List<string>()
            {
                "*@gmail.com",
            };
            var blockList = new List<string>()
            {
                "Clara.nguyen@gmail.com",
                "young.Nguyen@gmail.com"
            };
            var emailValidator = CreateEmailValidator(profileId, allowList, blockList);
            var emailMessage1 = new EmailMessage()
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
            var result1 = emailValidator.Validate(profileId, emailMessage1, out string reason1);
            Assert.False(result1);
            Assert.True(string.Equals(reason1, "Young.Nguyen@gmail.Com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestBlockByBCCAddress()
        {
            var profileId = Guid.NewGuid().ToString();
            var allowList = new List<string>()
            {
                "*@gmail.com",
            };
            var blockList = new List<string>()
            {
                "Clara.nguyen@gmail.com",
                "young.Nguyen@gmail.com"
            };
            var emailValidator = CreateEmailValidator(profileId, allowList, blockList);
            var emailMessage3 = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {

                    "Victor.Ly@gmail.com"
                },
                Cc = new List<string>()
                {
                    "Aaron.Vo@gmail.com"
                },
                Bcc = new List<string>()
                {
                    "Young.Nguyen@gmail.com",
                    "Ezreal.Luu@gmail.com"
                }
            };
            var result3 = emailValidator.Validate(profileId, emailMessage3, out string reason3);
            Assert.False(result3);
            Assert.True(string.Equals(reason3, "Young.Nguyen@gmail.Com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestBlockByCCAddress()
        {
            var profileId = Guid.NewGuid().ToString();
            var allowList = new List<string>()
            {
                "*@gmail.com",
            };
            var blockList = new List<string>()
            {
                "Clara.nguyen@gmail.com",
                "young.Nguyen@gmail.com"
            };
            var emailValidator = CreateEmailValidator(profileId, allowList, blockList);
            var emailMessage3 = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {

                    "Victor.Ly@gmail.com"
                },
                Cc = new List<string>()
                {
                    "Young.Nguyen@gmail.com",
                    "Aaron.Vo@gmail.com"
                },
                Bcc = new List<string>()
                {
                    "Ezreal.Luu@gmail.com"
                }
            };
            var result3 = emailValidator.Validate(profileId, emailMessage3, out string reason3);
            Assert.False(result3);
            Assert.True(string.Equals(reason3, "Young.Nguyen@gmail.Com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestBlockByFromAddress() 
        {
            var profileId = Guid.NewGuid().ToString();
            var allowList = new List<string>()
            {
                "*@gmail.com",
            };
            var blockList = new List<string>()
            {
                "Clara.nguyen@gmail.com",
                "young.Nguyen@gmail.com"
            };
            var emailValidator = CreateEmailValidator(profileId, allowList, blockList);
            var emailMessage4 = new EmailMessage()
            {
                From = "Young.Nguyen@gmail.com",
                To = new List<string>()
                {
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
            var result4 = emailValidator.Validate(profileId, emailMessage4, out string reason4);
            Assert.True(result4);
        }

        [Fact]
        public void TestAllowEmailMessageByAsterisk()
        {
            var profileId = Guid.NewGuid().ToString();
            var allowList = new List<string>()
            {
                "*@gmail.com",
            };
            var blockList = new List<string>()
            {
                "Clara.nguyen@gmail.com",
                "young.Nguyen@gmail.com"
            };
            var emailValidator = CreateEmailValidator(profileId, allowList, blockList);

            var emailMessage = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {
                    "Scott.Nguyen@gmail.com",
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
            var result = emailValidator.Validate(profileId, emailMessage, out string reason);
            Assert.True(result);
        }

        [Fact]
        public void TestCreateEmailValidatorByDI()
        {
            var profileId = Guid.NewGuid().ToString();
            var allowList = new List<string>()
            {
                "*@gmail.com",
            };
            var blockList = new List<string>()
            {
                "Clara.nguyen@gmail.com",
                "young.Nguyen@gmail.com"
            };
            var emailValidator = CreateEmailValidatorByDI(profileId, allowList, blockList);

            var emailMessage = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                To = new List<string>()
                {
                    "Scott.Nguyen@gmail.com",
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
            var result = emailValidator.Validate(profileId, emailMessage, out string reason);
            Assert.True(result);
        }
    }
}