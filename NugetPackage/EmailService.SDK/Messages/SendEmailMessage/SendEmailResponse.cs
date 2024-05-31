using EmailService.Domain;
using System.Text.Json;

namespace EmailService.SDK;

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
