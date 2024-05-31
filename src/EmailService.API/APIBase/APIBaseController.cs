using EmailService.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API;

[Route("api/v{version:apiVersion}")]
[ApiController]
[APIResultException]
public class APIBaseController : ControllerBase
{
    protected readonly ILogger logger;

    public APIBaseController(ILogger logger)
    {
        this.logger = logger;
    }

    protected async Task<BaseResponse<T>> RunAsync<T>(Func<Task<T>> method)
    {
        var methodName = method.Method.Name;
        logger.LogDebug($"Entered method Run<T>, method name: {methodName}");
        try
        {
            var data = await method();
            var result = AssembleResponse(data);
            logger.LogDebug($"Leaving method Run<T>, method name: {methodName}");
            return result;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception e)
        {
            return new BaseResponse<T>()
            {
#if DEBUG
                Message = e.Message,
#endif
                ErrorCode = GetErrorCode(e),
                IsError = true,
                CorrelatedId = GenerateCorrelatedId(e, logger, methodName),
            };
        }
    }

    private static Guid GenerateCorrelatedId(Exception e, ILogger logger, string methodName)
    {
        var correlatedId = Guid.NewGuid();
        logger.LogError("An error occured while executing {0}, CorrelatedId:{1}, Exception: {2}", new object[] { methodName, correlatedId, e });
        return correlatedId;
    }

    private static ErrorCode GetErrorCode(Exception e)
    {
        if (e is BaseException ex)
        {
            return (ErrorCode)ex.ErrorCode;
        }
        return ErrorCode.Unknow;
    }

    private static BaseResponse<T> AssembleResponse<T>(T data)
    {
        var result = new BaseResponse<T>();
        result.Result = data;
        return result;
    }
}
