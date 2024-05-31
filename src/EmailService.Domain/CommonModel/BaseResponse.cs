using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EmailService.Domain;

[DataContract]
public class BaseResponse<T>
{
    [DataMember(Name = "isError")]
    public bool IsError { get; set; }
    [DataMember(Name = "errorCode")]
    public ErrorCode? ErrorCode { get; set; }
    [DataMember(Name = "message")]
    public string? Message { get; set; }
    [DataMember(Name = "result")]
    public T? Result { get; set; }
    [DataMember(Name = "correlatedId")]
    public Guid? CorrelatedId { get; set; } = null;
}
