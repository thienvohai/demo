using System.Runtime.Serialization;

namespace EmailService.Domain;

[DataContract]
public class SaveEmailResponse
{
    [DataMember(Name = "id")]
    public Guid Id { get; set; }
    [DataMember(Name = "isFiltered")]
    public bool IsFiltered { get; set; }
}
