# email-service
## I. How to use EmailValidator
### 1. Create EmailValidator Service:
- The Constructor and builder method

    ```C#
    var emailValidator = new EmailValidator();
    emailValidator
        .SetupDefaultAllowList(defaultAllowList)
        .SetupDefaultBlockList(defaultBlockList)
        .SetupAllowList(profileId, allowList)
        .SetupBlockList(profileId, blockList);
    ```
    Our email validator will have Allow/Block List for each profileId (And Default Allow/Block List in the case our validator can't find Allow/Block List for profileId. If can find Allow/Block List for current profileId, the Allow List for current profileId will override default list while the Block List will extend default block list). Allow List is used to setup which email address we allow to add in message. Block List is used to set up which address we will block in message. We can setup for default allow/block list and can set up the allow/block list for profileId (maybe tenant id or module id, ...) follow the example.
- Block List have higher priority than Allow List
- We support the asterisk pattern like "*@gmail.com" or "\*" (any email address)
- The default value of DefaultAllowList (if we don't setup default allow list) is {"*"}. And for DefaultBlockList, it is empty list of string
    ```C#
    var emailValidator = new EmailValidator();
    emailValidator
        .SetupAllowList(profileId, allowList)
        .SetupBlockList(profileId, blockList);
    ```
- The Allow list of profile id will override the DefaultAllowList, while the Block List of profile id will extend the DefaultBlockList, for example:
    
    We have this validator:
    ```C#
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
    var emailValidator = new EmailValidator()
    emailValidator
        .SetupDefaultBlockList(defaultBlockList)
        .SetupAllowList(profileId, allowList)
        .SetupBlockList(profileId, blockList)
    ```
    If we validate Email Message by the profileId, the blocklist will be {"*"}. If we validate without the profileId, the blocklist will be DefaultBlockList {"Young.Nguyen@gmail.com"}
    
    We have other validator:
    ```C#
    var DefaultAllowList = new List<string>()
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
    var emailValidator = new EmailValidator()
    emailValidator
        .SetupDefaultAllowList(defaultBlockList)
        .SetupAllowList(profileId, allowList)
        .SetupBlockList(profileId, blockList)
    ```
    If we validate Email Message by profile id, the Allow List will be {"Young.Nguyen@gmail.com"}. If we validate without existed profile id, the blocklist will be DefaultBlockList {"Young.Nguyen@gmail.com"}
- Create service by DI

    ```C#
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IEmailValidator, EmailValidator>(ctx =>
    {
        var emailValidator = new EmailValidator();
        emailValidator
            .SetupDefaultAllowList(defaultAllowList)
            .SetupDefaultBlockList(defaultBlockList)
            .SetupAllowList(profileId, allowList)
            .SetupBlockList(profileId, blockList);
        return emailValidator;
    });
    var serviceProvider = serviceCollection.BuildServiceProvider();
    var emailValidator = serviceProvider.GetRequiredService<IEmailValidator>();
    return emailValidator;
    ```
### 2. Validate Email Method
- We provide 4 method with different input:

    ```C#
    public interface IEmailValidator
    {
        bool Validate(MailMessage emailMessage, out string reason);
        bool Validate(string profileId, MailMessage emailMessage, out string reason);
        bool Validate(EmailMessage emailMessage, out string reason);
        bool Validate(string profileId, EmailMessage emailMessage, out string reason);
    }
    ```

### 3. How to use the method:
- Prepare message (we have Email Message class or MailMessage of System.Net.Mail)
- Use the Validate method (Use profile Id if we want to use allow/block list of profile Id)
- The reason output will be the list of email was blocked or not allowed
- Example:

    We have this validator:
    ```C#
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
    var emailValidator = new EmailValidator();
        emailValidator
            .SetupDefaultAllowList(defaultAllowList)
            .SetupDefaultBlockList(defaultBlockList)
            .SetupAllowList(profileId, allowList)
            .SetupBlockList(profileId, blockList);
    ```
    Our validator will have default allow list {"*@gmail.com"}, default block list {"Victor.Ly@gmail.com"}, and Allow List for profileId "newid" {"\*"}, Block List for profileId "newid" {"Victor.Ly@gmail.com", "*@gmail.com"}

    We will validate our email message:
    ```C#
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
    var result = emailValidator.Validate(profileId, emailMessage, out string reason);
    ```
    we will got the failed result and reason "Young.Nguyen@gmail.com;Victor.Ly@gmail.com" because of block list

    We will validate our email message:
    ```C#
    var result = emailValidator.Validate(emailMessage, out string reason);
    //Or var result = emailValidator.Validate("other", emailMessage, out string reason);
    ```
    We will got the failed result and reason "Young.Nguyen@gmail.com;Victor.Ly@gmail.com" because of default block list and default allow list

## II. How to use Email Service SDK
We can create EmailServiceSDKClient then send message to our Email Service, follow this step:
1. In Client Service, we need to have the configuration value to create Options (we have Options class in SDK package) for our sdk client, can follow these properties:

    ```C#
    public class EmailServiceSDKClientOptions
    {
        public string EmailServiceEndpoint { get; set; }
        public string From { get; set; }
        public string SenderName { get; set; } = "";
        public string AuthEndpoint { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
    }
    ```

- EmailServiceEndpoint: endpoint of our email service to send request
- From: email address of sender
- SenderName: name of Sender
- AuthEndpoint: endpoint of our IDS to get token which use to authenticate for the request (will update this feature later)
- ClientId, Client Secret: Credential to get token (will update this feature later)

2. Create Email Client:
We have 2 ways:
- We can Add EmailServiceSDKClient to service collection using AddEmailClient method (Included in SDK package) then use DI to create client:

    ```C#
    public static IServiceCollection AddEmailClient(this IServiceCollection services, Action<EmailServiceSDKClientOptions> configureOptions, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.Configure(configureOptions);
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.TryAddSingleton<IEmailServiceSDKClient, EmailServiceSDKClient>();
                break;
            case ServiceLifetime.Scoped:
                services.TryAddScoped<IEmailServiceSDKClient, EmailServiceSDKClient>();
                break;
            case ServiceLifetime.Transient:
                services.TryAddTransient<IEmailServiceSDKClient, EmailServiceSDKClient>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
        return services;
    }
    ```
    Using:
    ```C#
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddEmailClient(option =>
    {
        option.EmailServiceEndpoint = "https://localhost:44305/";
        option.From = "Leo.Vo@gmail.com";
        option.SenderName = "Leo.Vo";
        option.AuthEndpoint = "";
        option.ClientId = "";
        option.Secret = "";
    });
    var serviceProvider = serviceCollection.BuildServiceProvider();
    using var emailclient = serviceProvider.GetService<IEmailServiceSDKClient>();
    ```

- You can create new EmailServiceSDKClient by constructor method (include in SDK package):

    ```C#
    public EmailServiceSDKClient(HttpClient httpClient, EmailServiceSDKClientOptions options)
    {
        this.options = options;
        this.Client = httpClient;
    }
    ```

3. Send request to Email Service using Message Model (included in SDK package)
- Message Model:

    ```C#
    public class EmailMessage
    {
        public string From { get; set; } = string.Empty;
        public string? SenderName { get; set; }
        public List<string> To { get; set; } = [];
        public List<string>? Cc { get; set; }
        public List<string>? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public bool IsBodyHtml { get; set; }
        public string Body { get; set; } = string.Empty;
        public List<AttachmentItem>? Attachments { get; set; }
    }

    public class AttachmentItem
    {
        public string FileName { get; set; } = string.Empty;
        public string Base64Content { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public bool IsInlineDisposition { get; set; }
        public string? ContentId { get; set; }
    }
    ```

- Send request to send email:

    ```C#
    using var emailclient = serviceProvider.GetService<IEmailServiceSDKClient>();
    var healhzResponse = await emailclient!.HealthzAsync();

    var emailMessage = new EmailMessage()
    {
        To = new List<string> { "Leo.Vo@gmail.com" },
        Cc = new List<string> { "young.Nguyen@gmail.com" },
        Bcc = new List<string> { "Clara.Nguyen@gmail.com" },
        Body = "<h1>Hi Clara aka Voi Ban Don</h1><br><img src=\"cid:inlineidabc123\"></img>",
        Subject = $"Test Email Service At {DateTime.UtcNow}",
        IsBodyHtml = true,
        Attachments = new List<AttachmentItem>{
            new AttachmentItem
            {
                FileName = "test.png",
                ContentType = "image/png ",
                IsInlineDisposition = true,
                Base64Content = "...",
                ContentId = "inlineidabc123"
            }
        }
    };

    var sendEmailResponse = await emailclient.SendEmailAsync(emailMessage);
    ```

4. Read the response, you can check the Response Model (Include in SDK package)
- Example of Response Model:
  * IsSucess: Status code success or not
  * StatusCode: http status code of the response
  * Error Message: message of the error if we can get the message from http response content
  * Response: the response model of request, in the Send email request we will return object with Result have Id of email (Included in SDK package), sometime if there is error in Email service, the respone will have status failed and with error message and error code

    ```C#
    public class SendEmailResponse : EmailServiceResponse
    {
        public BaseResponse<SaveEmailResponse>? Response { get; set; }

        public override async Task ConvertResponseAsync(HttpContent content)
        {
            var data = await content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Response = JsonSerializer.Deserialize<BaseResponse<SaveEmailResponse>>(data, options);
        }
    }
    public abstract class EmailServiceResponse
    {
        public bool IsSuccess { get; set; } = true;
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public virtual Task ConvertResponseAsync(HttpContent content)
        {
            return Task.CompletedTask;
        }
    }
    [DataContract]
    public class BaseResponse<T>
    {
        [DataMember(Name = "isError")]
        public bool IsError { get; set; }
        [DataMember(Name = "errorCode")]
        public ErrorCode? ErrorCode { get; set; }
        [DataMember(Name = "message")]
        public string? Message { get; set; }
        [DataMember(Name = "result")]
        public T? Result { get; set; }
        [DataMember(Name = "correlatedId")]
        public Guid? CorrelatedId { get; set; } = null;
    }
    ```

5. Exception:
- We will throw exception if any exception occured when send request, for example: Target Machine refused connection (The Email service endpoint is not valid or not open) with this exception message format:
    ```C#
    throw new Exception($"Request Failed. URL: {url}", ex);
    ```
