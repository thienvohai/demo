using EmailService.Domain;

namespace EmailService.SDK;

internal class GetEmailDetailRequest : EmailServiceRequest
{
    public GetEmailDetailRequest(Guid emailId)
    {
        Method = HttpMethod.Get;
        RequestPath = $"api/v1/emaildetail/{emailId}";
    }
}
