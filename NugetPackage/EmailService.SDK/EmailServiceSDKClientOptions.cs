namespace EmailService.SDK;

public class EmailServiceSDKClientOptions
{
    public string EmailServiceEndpoint { get; set; }
    public string From { get; set; }
    public string SenderName { get; set; } = "";
    public string AuthEndpoint { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}
