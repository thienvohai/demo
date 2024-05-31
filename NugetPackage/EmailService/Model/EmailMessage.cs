namespace EmailService;

public class EmailMessage
{
    public string From { get; set; } = string.Empty;
    public List<string> To { get; set; } = [];
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
}

