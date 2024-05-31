namespace EmailService.SDK;

internal class EmailServiceHealthzRequest : EmailServiceRequest
{
    public EmailServiceHealthzRequest()
    {
        Method = HttpMethod.Get;
        RequestPath = "healthz";
    }
}
