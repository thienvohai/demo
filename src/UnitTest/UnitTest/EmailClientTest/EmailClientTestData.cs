using EmailService.Domain;
using System.Text;

namespace UnitTest;

public enum MessageCaseType
{
    Success,
    WrongFrom,
    WrongTo,
    WrongPartOfTo,
    WrongCC,
    WrongBcc,
    WithAttachment,
    WithInlineAttachment,
    DuplicateRecipient
}

public class EmailClientTestData
{
    public static IEnumerable<object[]> GetEmailMessageData()
    {
        yield return new object[] { new EmailMessageTestModel()
        {
            Message = new EmailMessage()
            {
                From = "Leo.Vo@gmail.com",
                SenderName = "Leo.Vo",
                To = new List<string>{ "clara.Nguyen@gmail.com" },
                Cc = new List<string>{ "Clara.Nguyen@gmail.com", "Young.Nguyen@gmail.com" },
                Bcc = new List<string>{ "Clara.nguyen@gmail.com", "Scott.Nguyen@gmail.com" },
                Body = "Hai Nguyen Den Roi",
                Subject = "Test Email Service",
                IsBodyHtml = false
            },
            CaseType = MessageCaseType.DuplicateRecipient
        } };

        //yield return new object[] { new EmailMessageTestModel()
        //{
        //    Message = new EmailMessage()
        //    {
        //        From = "Leo.Vo@gmail.com",
        //        SenderName = "Leo.Vo",
        //        To = new List<string>{ "Clara.Nguyen@gmail.com" },
        //        Cc = new List<string>{ "Young.Nguyen@gmail.com" },
        //        Bcc = new List<string>{ "Scott.Nguyen@gmail.com" },
        //        Body = "Hai Nguyen Den Roi",
        //        Subject = "Test Email Service",
        //        IsBodyHtml = false
        //    },
        //    CaseType = MessageCaseType.Success
        //} };

        //yield return new object[] { new EmailMessageTestModel()
        //{
        //    Message = new EmailMessage()
        //    {
        //        From = "213123sadasd@gmail.com",
        //        SenderName = "Leo.Vo",
        //        To = new List<string>{ "Clara.Nguyen@gmail.com" },
        //        Cc = new List<string>{ "Young.Nguyen@gmail.com" },
        //        Bcc = new List<string>{ "Scott.Nguyen@gmail.com" },
        //        Body = "Hai Nguyen Den Roi",
        //        Subject = "Test Email Service",
        //        IsBodyHtml = false
        //    },
        //    CaseType = MessageCaseType.WrongFrom
        //} };

        //yield return new object[] { new EmailMessageTestModel()
        //{
        //    Message = new EmailMessage()
        //    {
        //        From = "Leo.Vo@gmail.com",
        //        SenderName = "Leo.Vo",
        //        To = new List<string>{ "213123sadasd@gmail.com" },
        //        Cc = new List<string>{ "Young.Nguyen@gmail.com" },
        //        Bcc = new List<string>{ "Scott.Nguyen@gmail.com" },
        //        Body = "Hai Nguyen Den Roi",
        //        Subject = "Test Email Service",
        //        IsBodyHtml = false
        //    },
        //    CaseType = MessageCaseType.WrongTo
        //} };

        //yield return new object[] { new EmailMessageTestModel()
        //{
        //    Message = new EmailMessage()
        //    {
        //        From = "Leo.Vo@gmail.com",
        //        SenderName = "Leo.Vo",
        //        To = new List<string>{ "213123sadasd@gmail.com", "Clara.Nguyen@gmail.com" },
        //        Cc = new List<string>{ "Young.Nguyen@gmail.com" },
        //        Bcc = new List<string>{ "Scott.Nguyen@gmail.com" },
        //        Body = "Hai Nguyen Den Roi",
        //        Subject = "Test Email Service",
        //        IsBodyHtml = false
        //    },
        //    CaseType = MessageCaseType.WrongPartOfTo
        //} };

        //yield return new object[] { new EmailMessageTestModel()
        //{
        //    Message = new EmailMessage()
        //    {
        //        From = "Leo.Vo@gmail.com",
        //        SenderName = "Leo.Vo",
        //        To = new List<string>{ "Clara.Nguyen@gmail.com" },
        //        Cc = new List<string>{ "Young.Nguyen@gmail.com" },
        //        Bcc = new List<string>{ "Scott.Nguyen@gmail.com" },
        //        Body = "Harvey Den Roi",
        //        Subject = "Test Email Service",
        //        IsBodyHtml = false,
        //        Attachments = new List<AttachmentItem>{ new AttachmentItem
        //        {
        //            FileName = "test.txt",
        //            ContentType = "text/plain",
        //            IsInlineDisposition = false,
        //            Base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes("Day la test xem harvey den chua"))
        //        } }
        //    },
        //    CaseType = MessageCaseType.WithAttachment
        //} };

        //yield return new object[] { new EmailMessageTestModel()
        //{
        //    Message = new EmailMessage()
        //    {
        //        From = "Leo.Vo@gmail.com",
        //        SenderName = "Leo.Vo",
        //        To = new List<string>{ "Clara.Nguyen@gmail.com" },
        //        Cc = new List<string>{ "Young.Nguyen@gmail.com" },
        //        Bcc = new List<string>{ "Scott.Nguyen@gmail.com" },
        //        Body = "<h1>Hi there</h1><br><img src=\"cid:inlineidabc123\"></img>",
        //        Subject = "Test Email Service",
        //        IsBodyHtml = true,
        //        Attachments = new List<AttachmentItem>{ new AttachmentItem
        //        {
        //            FileName = "test.txt",
        //            ContentType = "image/png ",
        //            IsInlineDisposition = true,
        //            Base64Content = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAoCAIAAAADnC86AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAACAJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDcuMi1jMDAwIDc5LjFiNjVhNzliNCwgMjAyMi8wNi8xMy0yMjowMTowMSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdEV2dD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlRXZlbnQjIiB4bWxuczpzdFJlZj0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlUmVmIyIgeG1sbnM6cGhvdG9zaG9wPSJodHRwOi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCAyMy41IChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwMjMtMDUtMjRUMTE6NTQ6MDcrMDg6MDAiIHhtcDpNZXRhZGF0YURhdGU9IjIwMjMtMDUtMjRUMTU6MDY6MjgrMDg6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDIzLTA1LTI0VDE1OjA2OjI4KzA4OjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo4MTJGRDVCNkZBMDExMUVEQjQzMDhDQjU4Qzc3OTI1NiIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDo4MTJGRDVCN0ZBMDExMUVEQjQzMDhDQjU4Qzc3OTI1NiIgeG1wTU06T3JpZ2luYWxEb2N1bWVudElEPSJ4bXAuZGlkOmZkMDgzN2EyLTRiZWMtN2U0OC05ZmVhLWMxYTAyNmVhMzU5NSI+IDx4bXBNTTpIaXN0b3J5PiA8cmRmOlNlcT4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImNyZWF0ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6ZmQwODM3YTItNGJlYy03ZTQ4LTlmZWEtYzFhMDI2ZWEzNTk1IiBzdEV2dDp3aGVuPSIyMDIzLTA1LTI0VDExOjU0OjA3KzA4OjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgMjMuNSAoV2luZG93cykiLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249InNhdmVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmNhZDE4OTNlLWNhMWItZGY0NS04NzhlLTUzMzk4NDE0NmJmMiIgc3RFdnQ6d2hlbj0iMjAyMy0wNS0yNFQxMTo1NTo1MyswODowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIDIzLjUgKFdpbmRvd3MpIiBzdEV2dDpjaGFuZ2VkPSIvIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDpmZDQ4NzFjNC01MGU1LTA4NGMtYWY2Mi0xYzY5OTYzNzVlYjciIHN0RXZ0OndoZW49IjIwMjMtMDUtMjRUMTU6MDU6MTUrMDg6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCAyMy41IChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8L3JkZjpTZXE+IDwveG1wTU06SGlzdG9yeT4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6ZmQ0ODcxYzQtNTBlNS0wODRjLWFmNjItMWM2OTk2Mzc1ZWI3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",
        //            ContentId = "inlineidabc123"
        //        } }
        //    },
        //    CaseType = MessageCaseType.WithInlineAttachment
        //} };
    }
}

public class EmailMessageTestModel
{
    public EmailMessage Message { get; set; }
    public MessageCaseType CaseType { get; set; }
}
