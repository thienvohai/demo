namespace EmailService.Domain;

[Serializable]
public class BaseException : Exception
{
    public int ErrorCode { get; set; }

    public BaseException(string message) : base(message)
    {
        ErrorCode = (int)Domain.ErrorCode.Unknow;
    }

    public BaseException(string message, int errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public BaseException(string message, Exception innerException) : base(message, innerException)
    {

    }

    public virtual BaseResponse<object> GenerateResponse()
    {
        var response = new BaseResponse<object>()
        {
            ErrorCode = (ErrorCode)this.ErrorCode,
            Result = null,
            Message = this.Message,
        };
        return response;
    }
}