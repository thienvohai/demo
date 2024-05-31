namespace EmailService.Core;

public class BaseEventMessage
{
    public BaseEventMessage()
    {
        Created = DateTime.UtcNow;
    }

    public DateTime Created {  get; set; }
}
