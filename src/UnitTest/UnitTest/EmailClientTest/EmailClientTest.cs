using EmailService.API;
using EmailService.Core;
using EmailService.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace UnitTest;

public class EmailClientTest
{   
    [Theory]
    [MemberData(nameof(EmailClientTestData.GetEmailMessageData), MemberType = typeof(EmailClientTestData))]
    public async Task TestSendGridClient(EmailMessageTestModel testModel)
    {
        var serviceCollection = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder().AddUserSecrets(Assembly.GetExecutingAssembly());
        serviceCollection.AddCusHttpClient();
        serviceCollection.AddConfigurationOptions(configBuilder.Build());

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var sendGridClient = new SendGridEmailClient(
            serviceProvider.GetService<IHttpClientFactory>()!,
            serviceProvider.GetService<IOptions<EmailClientOptions>>()!);

        var result = await sendGridClient.SendEmailAsync(testModel.Message);
        
        if (testModel.CaseType == MessageCaseType.Success)
        {
            Assert.Equal(202, result.StatusCode);
        }
        else if (testModel.CaseType == MessageCaseType.WrongFrom)
        {
            Assert.Equal(202, result.StatusCode);
        }
        else if(testModel.CaseType == MessageCaseType.WrongTo)
        {
            Assert.Equal(202, result.StatusCode);
        }
        else if(testModel.CaseType == MessageCaseType.WrongPartOfTo)
        {
            Assert.Equal(202, result.StatusCode);
        }
        else if(testModel.CaseType == MessageCaseType.WrongCC)
        {
            Assert.Equal(202, result.StatusCode);
        }
        else if (testModel.CaseType == MessageCaseType.WrongBcc)
        {
            Assert.Equal(202, result.StatusCode);
        }
        else if (testModel.CaseType == MessageCaseType.WithAttachment)
        {
            Assert.Equal(202, result.StatusCode);
        }
        else if (testModel.CaseType == MessageCaseType.WithInlineAttachment)
        {
            Assert.Equal(202, result.StatusCode);
        }
        else if (testModel.CaseType == MessageCaseType.DuplicateRecipient)
        {
            Assert.Equal(202, result.StatusCode);
        }
    }
}