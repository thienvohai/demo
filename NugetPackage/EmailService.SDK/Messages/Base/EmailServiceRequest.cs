namespace EmailService.SDK;

internal class EmailServiceRequest
{
    public string RequestPath { get; set; } = string.Empty;
    public HttpMethod Method { get; set; } = new("GET");
    public object InputParameters { get; set; } = new();
}
