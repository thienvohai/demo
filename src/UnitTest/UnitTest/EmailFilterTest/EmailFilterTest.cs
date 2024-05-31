using EmailService.API;
using EmailService.Domain;
using EmailService.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UnitTest;

public class EmailFilterTest
{
    private async Task<bool> RunTestCase(List<KeyValuePair<string, string?>> config, EmailMessage testModel)
    {
        var serviceCollection = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(config);
        var configuration = configBuilder.Build();
        serviceCollection.AddConfigurationOptions(configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var filterService = new FilterRecipientByConfigurationService(
            serviceProvider.GetService<IOptions<FilterRecipientOptions>>());
        var result = await filterService.FilterRecipientAsync(testModel);
        return result;
    }

    [Fact]
    public async Task TestFilterEmailEmptyCCBcc()
    {
        var testModel = new EmailMessage()
        {
            From = "Leo.Vo@gmail.com",
            SenderName = "Leo.Vo",
            To = new List<string> { "Scott.Nguyen@gmail.com" },
            Bcc = new List<string> { "Young.Nguyen@gmail.com" },
            Cc = new List<string> { },
        };
        var config = new List<KeyValuePair<string, string?>>()
            {
                new KeyValuePair<string, string?>("FilterRecipient:BlockList:0","Stellantt.Nguyen@gmail.Com"),
                new KeyValuePair<string, string?>("FilterRecipient:BlockList:1","Young.Nguyen@gmail.Com"),
                new KeyValuePair<string, string?>("FilterRecipient:AllowList:0","*")
            };
        var result = await RunTestCase(config, testModel);
        Assert.True(result);
        Assert.Single(testModel.To);
        Assert.Empty(testModel.Bcc);
        Assert.Empty(testModel.Cc);

        var testModel1 = new EmailMessage()
        {
            From = "Leo.Vo@gmail.com",
            SenderName = "Leo.Vo",
            To = new List<string> { "Scott.Nguyen@gmail.com" },
            Bcc = new List<string> { },
            Cc = new List<string> { "Young.Nguyen@gmail.com" },
        };
        var result1 = await RunTestCase(config, testModel1);
        Assert.True(result1);
        Assert.Single(testModel.To);
        Assert.Empty(testModel.Cc);
        Assert.Empty(testModel.Bcc);
    }

    [Fact]
    public async Task TestFilterEmailEmptyTo()
    {
        var testModel = new EmailMessage()
        {
            From = "Leo.Vo@gmail.com",
            SenderName = "Leo.Vo",
            To = new List<string> { "Stellantt.Nguyen@gmail.com" },
            Bcc = new List<string> { "Young.Nguyen@gmail.com" },
            Cc = new List<string> { "clara.Nguyen@gmail.com" },
        };
        var config = new List<KeyValuePair<string, string?>>()
            {
                new KeyValuePair<string, string?>("FilterRecipient:BlockList:0","Stellantt.Nguyen@gmail.Com"),
                new KeyValuePair<string, string?>("FilterRecipient:BlockList:1","Young.Nguyen@gmail.Com"),
                new KeyValuePair<string, string?>("FilterRecipient:AllowList:0","clara.nguyen@gmail.Com")
            };
        var result = await RunTestCase(config, testModel);
        Assert.False(result);
    }

    [Fact]
    public async Task TestFilterEmailBlockCCAndBcc()
    {
        var testModel = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                SenderName = "Leo.Vo",
                To = new List<string> { "clara.Nguyen@gmail.com" },
                Bcc = new List<string> { "Young.Nguyen@gmail.com" },
                Cc = new List<string> { "Stellantt.Nguyen@gmail.com" },
            };
        var config = new List<KeyValuePair<string, string?>>()
            {
                new KeyValuePair<string, string?>("FilterRecipient:BlockList:0","Stellantt.Nguyen@gmail.Com"),
                new KeyValuePair<string, string?>("FilterRecipient:BlockList:1","Young.Nguyen@gmail.Com"),
                new KeyValuePair<string, string?>("FilterRecipient:AllowList:0","clara.nguyen@gmail.Com")
            };
        var result = await RunTestCase(config, testModel);
        Assert.True(result);
        Assert.Empty(testModel.Bcc);
        Assert.Empty(testModel.Cc);
    }

    [Fact]
    public async Task TestFilterEmailAllowByAsterisk()
    {
        var testModel = new EmailMessage()
        {
            From = "Leo.Vo@gmail.com",
            SenderName = "Leo.Vo",
            To = new List<string> { "clara.Nguyen@gmail.com" },
            Bcc = new List<string> { "Young.Nguyen@gmail.com" },
            Cc = new List<string> { "Stellantt.Nguyen@gmail.com" },
        };
        var config = new List<KeyValuePair<string, string?>>()
            {
                new KeyValuePair<string, string?>("FilterRecipient:BlockList:0","Stellantt.Nguyen@gmail.Com"),
                new KeyValuePair<string, string?>("FilterRecipient:BlockList:1","Young.Nguyen@gmail.Com"),
                new KeyValuePair<string, string?>("FilterRecipient:AllowList:0","*@gmail.Com")
            };
        var result = await RunTestCase(config, testModel);
        Assert.True(result);
        Assert.Empty(testModel.Bcc);
        Assert.Empty(testModel.Cc);
    }

    [Fact]
    public async Task TestFilterEmailBlockTo()
    {
        var testModel = new EmailMessage()
        {
            From = "Leo.Vo@gmail.com",
            SenderName = "Leo.Vo",
            To = new List<string> { "clara.Nguyen@gmail.com" },
            Bcc = new List<string> { "Young.Nguyen@gmail.com" },
            Cc = new List<string> { "Stellantt.Nguyen@gmail.com" },
        };
        var config = new List<KeyValuePair<string, string?>>()
            {
                new KeyValuePair<string, string?>("FilterRecipient:BlockList:0","Stellantt.Nguyen@gmail.Com"),
                new KeyValuePair<string, string?>("FilterRecipient:BlockList:1","clara.Nguyen@gmail.Com"),
                new KeyValuePair<string, string?>("FilterRecipient:AllowList:0","*")
            };
        var result = await RunTestCase(config, testModel);
        Assert.False(result);
    }

}