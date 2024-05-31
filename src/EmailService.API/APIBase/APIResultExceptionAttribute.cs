using EmailService.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmailService.API;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class APIResultExceptionAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        base.OnException(context);
        var logger = context.HttpContext.RequestServices.GetService<ILogger<APIResultExceptionAttribute>>();
        var correlatedId = Guid.NewGuid();
        logger?.LogError("CorrelatedId:{correlatedId}.{e}", correlatedId, context.Exception);

        if (!context.ExceptionHandled)
        {
            if (context.Exception is BaseException)
            {
                var ex = context.Exception as BaseException;
                var response = ex!.GenerateResponse();
                response.CorrelatedId = correlatedId;
                context.Result = new OkObjectResult(response);
                context.ExceptionHandled = true;
            }
            else
            {
                context.Result = new OkObjectResult(new BaseResponse<object>());
                context.ExceptionHandled = true;
            }
        }
    }
}
