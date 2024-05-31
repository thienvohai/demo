namespace EmailService.SDK;

public class EmailServiceHealthzResponse : EmailServiceResponse
{
    public string? Response { get; set; }
    public override async Task ConvertResponseAsync(HttpContent content)
    {
        var data = await content.ReadAsStringAsync();
        Response = data;
    }
}
