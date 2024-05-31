using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace EmailService.Domain
{
    [DataContract]
    public class EmailDetailResponse
    {
        [DataMember(Name = "id")]
        public Guid Id { get; set; }
        [DataMember(Name = "status")]
        public int Status { get; set; }
        [DataMember(Name = "retryCount")]
        public int RetryCount { get; set; }
        [DataMember(Name = "isRetrying")]
        public bool IsRetrying { get; set; }
        [DataMember(Name = "body")]
        public string? Body { get; set; }
        [DataMember(Name = "isHtmlBody")]
        public bool IsBodyHtml { get; set; }
        [DataMember(Name = "sender")]
        public string Sender { get; set; }
        [DataMember(Name = "senderName")]
        public string? SenderName { get; set; }
        [DataMember(Name = "subject")]
        public string? Subject { get; set; }
        [DataMember(Name = "attachment")]
        public string? Attachment { get; set; }
        [DataMember(Name = "to")]
        public string To { get; set; }
        [DataMember(Name = "cc")]
        public string? CC { get; set; }
        [DataMember(Name = "bcc")]
        public string? Bcc { get; set; }
    }

    [DataContract]
    public class EmailsResponse
    {
        [DataMember(Name = "total")]
        public long Total { get; set; }
        [DataMember(Name = "emails")]
        public List<EmailDetailResponse> Emails { get; set; }
    }
}
