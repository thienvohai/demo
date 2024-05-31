using Asp.Versioning;
using EmailService.Domain;
using EmailService.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API;

[ApiVersion("1")]
public class EmailController : APIBaseController
{
    private readonly IEmailHandlerService emailHandler;

    public EmailController(
        ILogger<EmailController> logger,
        IEmailHandlerService emailHandler) : base(logger)
    {
        this.emailHandler = emailHandler;
    }

    [HttpPost("emails")]
    [Authorize(Policy = Constants.Authentication.DefaultPolicyName)]
    public async Task<BaseResponse<SaveEmailResponse>> SendEmailAsync([FromBody] EmailMessage message)
    {
        return await RunAsync(() =>
        {
            return emailHandler.CreateEmailRecordAsync(message);
        });
    }

    [HttpGet("emaildetail/{emailId}")]
    [Authorize(Policy = Constants.Authentication.DefaultPolicyName)]
    public async Task<BaseResponse<EmailDetailResponse>> GetEmailDetailAsync([FromRoute] Guid emailId)
    {
        return await RunAsync(() =>
        {
            return emailHandler.GetEmailDetailAsync(emailId);
        });
    }
}
