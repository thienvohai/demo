using Asp.Versioning;
using EmailService.Domain;
using EmailService.Service;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API;

[ApiVersion("1")]
public class AdminController : APIBaseController
{
    private readonly IEmailHandlerService emailHandler;

    public AdminController(
        ILogger<AdminController> logger,
        IEmailHandlerService emailHandler) : base(logger)
    {
        this.emailHandler = emailHandler;
    }


    [HttpPost("admin/emails")]
    public async Task<BaseResponse<EmailsResponse>> GetEmailDetailsAsync([FromBody] EmailPageQuery query)
    {
        return await RunAsync(() =>
        {
            return emailHandler.GetEmailsDetailAsync(query);
        });
    }
}
