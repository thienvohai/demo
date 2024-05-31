namespace EmailService;

public class SendEmailResponse
{
    public SendEmailResult Result { get; set; }
    public string Message { get; set; }
}

public enum SendEmailResult
{
    Success,
    InternalError,
    RecipientNotAllowed,
    ReachQuotaLimit
}
