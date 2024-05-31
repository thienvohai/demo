namespace EmailService;

/*
This class represents the policy 
 */
public class EmailSenderPolicy
{
    //This list includes email addresses permitted for use as a recipient, support asterisk: "*", "*@gmail.com"
    public List<string> AllowList { get; set; }
    //This list includes email addresses not permitted for use as a recipient, support asterisk: "*", "*@gmail.com"
    public List<string> BlockList { get; set; }
    //This configuration is to limit the number of emails a recipient can receive within a certain time frame
    public int UserReceiveLimit { get; set; }
    //This configuration limits the number of emails a tenant can send within a specified time frame
    public int TenantSendLimit { get; set; }
}
