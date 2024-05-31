namespace EmailService.Domain;

public class EmailClientResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}