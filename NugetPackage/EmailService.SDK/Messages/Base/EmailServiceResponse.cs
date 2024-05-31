using System.Net;

namespace EmailService.SDK;

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
