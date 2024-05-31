namespace EmailService;

/*
This class represents a validator for the number of emails allowed within a specified time frame
We can create an instance then use setup method then use the validate method to validate
*/
public class EmailCounterValidator : IEmailCounterValidator
{
    private int DefaultUserReceiveLimit;
    private int DefaultTenantSendLimit;
    private Dictionary<string, int> TenantUserReceiveLimit = new Dictionary<string, int>();
    private Dictionary<string, int> TenantTenantSendLimit = new Dictionary<string, int>();

    //Method to setup default policy for tenant in current validator
    public void SetupDefaultLimit(int userReceiveLimit, int tenantSendLimit)
    {
        DefaultTenantSendLimit = tenantSendLimit;
        DefaultUserReceiveLimit = userReceiveLimit;
    }

    //Method to setup policy for tenant in current validator
    public void SetupTenantLimit(string tenantId, int userReceiveLimit, int tenantSendLimit)
    {
        if (TenantUserReceiveLimit.ContainsKey(tenantId))
            TenantUserReceiveLimit[tenantId] = userReceiveLimit;
        else
            TenantUserReceiveLimit.Add(tenantId, userReceiveLimit);

        if (TenantTenantSendLimit.ContainsKey(tenantId))
            TenantTenantSendLimit[tenantId] = tenantSendLimit;
        else
            TenantTenantSendLimit.Add(tenantId, tenantSendLimit);
    }

    //Method to validate number of received emails allowed for recipients within a specified time frame
    public Task<bool> ValidateAsync(EmailMessage emailMessage, out string reason)
    {
        /*
         TODO: 
            - we need to get EmailCounter for recipient 
            - Then we should validate about the rate limit by the Default or Tenant setting
         */
        reason = "";
        return Task.FromResult(true);
    }

    //Method to validate number of received emails allowed for recipients, and number of sent emails allow for tenant, within a specified time frame
    public Task<bool> ValidateAsync(string tenantId, EmailMessage emailMessage, out string reason)
    {
        /*
         TODO: 
            - we need to get EmailCounter for recipient
            - Then validate about the rate limit by the Default or Tenant setting
         */
        reason = "";
        return Task.FromResult(true);
    }
}
