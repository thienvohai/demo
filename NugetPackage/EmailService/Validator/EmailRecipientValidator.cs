using System.Text.RegularExpressions;

namespace EmailService;

/*
This class represents a validator for email's recipients
We can create an instance then use setup method then use the validate method to validate email's recipients
*/
public class EmailRecipientValidator : IEmailRecipientValidator
{
    //Key is tenant id, The value list includes email addresses permitted for use as a recipient, support asterisk: "*", "*@gmail.com"
    //For each tenant, This list will override the default list
    private Dictionary<string, HashSet<string>> AllowList = [];
    //Key is tenant id, The value list includes email addresses not permitted for use as a recipient, support asterisk: "*", "*@gmail.com"
    //For each tenant, This list will extend the default list
    private Dictionary<string, HashSet<string>> BlockList = [];

    //This default list includes email addresses permitted for use as a recipient, support asterisk: "*", "*@gmail.com"
    //Default value is "*"
    private List<string> DefaultAllowList = new List<string>() { "*" };
    //This default list includes email addresses not permitted for use as a recipient, support asterisk: "*", "*@gmail.com"
    //Default value is Empty list
    private List<string> DefaultBlockList = [];

    public EmailRecipientValidator() { }

    /*
    Method to validate Recipient in Email Message without tenant information
    Input:
        - emailMessage: model of email message
    Return: bool result of the validation logic, string reason includes email addresses which are not valid
    */
    public bool Validate(EmailMessage emailMessage, out string reason)
    {
        return Validate(tenantId: "", emailMessage, out reason);
    }

    /*
    Method to validate Recipient in Email Message
    Input:
        - tenantId: tenant of current context
        - emailMessage: model of email message
    Return: bool result of the validation logic, string reason includes email addresses which are not valid
    */
    public bool Validate(string tenantId, EmailMessage emailMessage, out string reason)
    {
        var result = true;
        var filteredAddress = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        //Tenant allow list will override default allow list
        if (string.IsNullOrEmpty(tenantId) || !AllowList.TryGetValue(tenantId, out var allowHashSet) || allowHashSet.Count == 0)
        {
            allowHashSet = new HashSet<string>(DefaultAllowList, StringComparer.OrdinalIgnoreCase);
        }

        //Tenant block list will extend default block list
        if (string.IsNullOrEmpty(tenantId) || !BlockList.TryGetValue(tenantId, out var blockHashSet))
        {
            blockHashSet = new HashSet<string>(DefaultBlockList, StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            foreach (var block in DefaultBlockList)
                blockHashSet.Add(block);
        }
        
        if (emailMessage.To != null && emailMessage.To.Count(t => FilterEmail(blockHashSet, allowHashSet, t, filteredAddress)) > 0)
            result = false;

        if (emailMessage.Cc != null && emailMessage.Cc.Count(t => FilterEmail(blockHashSet, allowHashSet, t, filteredAddress)) > 0)
            result = false;

        if (emailMessage.Bcc != null && emailMessage.Bcc.Count(t => FilterEmail(blockHashSet, allowHashSet, t, filteredAddress)) > 0)
            result = false;

        reason = $"{string.Join(";", filteredAddress)}";
        return result;
    }

    private bool FilterEmail(HashSet<string> blockHashSet, HashSet<string> allowHashSet, string email, HashSet<string> filteredAddress)
    {
        if (IsMatched(blockHashSet, email) || !IsMatched(allowHashSet, email))
        {
            filteredAddress.Add(email);
            return true;
        }

        return false;
    }

    private bool IsMatched(HashSet<string> patterns, string email)
    {
        if (patterns.Contains("*"))
            return true;

        foreach (var pattern in patterns)
        {
            string regexPattern = Regex.Escape(pattern).Replace("*", ".*");
            if (Regex.IsMatch(email, regexPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)))
                return true;
        }

        return false;
    }

    //Method to setup allow list for tenant in current validator
    public void SetupAllowList(string tenantId, List<string> allowList)
    {
        SetupEmailList(AllowList, tenantId, allowList);
    }

    //Method to setup block list for tenant in current validator
    public void SetupBlockList(string tenantId, List<string> blockList)
    {
        SetupEmailList(BlockList, tenantId, blockList);
    }

    //Method to setup default allow list in current validator
    public void SetupDefaultAllowList(List<string> defaultAllowList)
    {
        if (defaultAllowList != null && defaultAllowList.Count > 0)
        {
            DefaultAllowList.Clear();
            DefaultAllowList.AddRange(defaultAllowList);
        }
    }

    //Method to setup default block list in current validator
    public void SetupDefaultBlockList(List<string> defaultBlockList)
    {
        if (defaultBlockList != null && defaultBlockList.Count > 0)
        {
            DefaultBlockList.AddRange(defaultBlockList);
        }
    }

    private void SetupEmailList(Dictionary<string, HashSet<string>> emailHashset, string tenantId, List<string> emailList)
    {
        if (emailList == null)
            return;

        if (emailHashset.TryGetValue(tenantId, out var profileAllowList))
        {
            foreach (var email in emailList)
            {
                profileAllowList.Add(email);
            }
        }
        else
        {
            var newProfileAllowList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var email in emailList)
            {
                newProfileAllowList.Add(email);
            }
            emailHashset.Add(tenantId, newProfileAllowList);
        }
    }
}
