using Microsoft.Extensions.Logging;

namespace EmailService;

/*
This class represents a sender, to create an instance we need provide setting and logger (use to write log for tracking)
User can create an instance, use Initialize if they need, then use TrySend method to send email with the validation
*/
public class EmailSender : IEmailSender
{
    private bool disposed;
    //Logger use to write log for tracking
    private readonly ILogger logger;
    //Service use to migrate database schema
    private readonly IAutoMigration autoMigration;
    //Service to write or read the policy to database for our validation logic
    private readonly IEmailPolicyService policyService;
    //Service to write or read email record to database
    private readonly IEmailRecordService emailRecordService;
    //Service to write or read email counter record to database which will be use for EmailCounterValidation
    private readonly IEmailCounterService emailCounterService;

    //Contructor of EmailSender with setting and logger instance
    public EmailSender(EmailSenderSetting settings, ILogger logger)
    {
        this.logger = logger;
        this.autoMigration = new AutoMigration(settings.DbType.ToString(), settings.ConnectionString, settings.TableSchema);
        this.policyService = new EmailPolicyService(settings.DbType.ToString(), settings.ConnectionString, settings.TableSchema);
        this.emailRecordService = new EmailRecordService(logger, settings.DbType.ToString(), settings.ConnectionString, settings.TableSchema);
        this.emailCounterService = new EmailCounterService(settings.DbType.ToString(), settings.ConnectionString, settings.TableSchema);
    }

    /*
    This function is used to initialize the database table structure
    */
    public void Initialize()
    {
        try
        {
            this.autoMigration.EnsureDBCreatedAndMigrated();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Email sender error: An error occurred while initializing. Message: {ex.Message}");
            throw;
        }
    }

    /*
    This function is used to save the default settings for the email sender's policy. 
    (The default settings will be used when no tenant is specified or when the tenant is not found.)
    Input: a model representing the policy
    */
    public async Task SetDefaultPolicyAsync(EmailSenderPolicy defaultPolicy)
    {
        try
        {
            await policyService.AddDefaultPolicyAsync(defaultPolicy);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, $"Email sender error: An error occurred while setting policy. Message: {ex.Message}");
            throw;
        }
        
    }

    /*
    This function is used to save the default settings for the email sender's policy.
    Input: a model representing the policy
    */
    public async Task SetTenantPolicyAsync(string tenantId, EmailSenderPolicy policy)
    {
        try
        {
            await policyService.AddTenantPolicyAsync(tenantId, policy);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, $"Email sender error: An error occurred while setting policy. Message: {ex.Message}");
            throw;
        }
    }

    /*
     Method to try send email
     Input:
        - tenantId: Tenant of current context
        - message: the model of email message
        - sendEmailDelegate: Func use to send email, need to defind the message and service to send email in client code then push to our method
     Output:
        - SendEmailResponse: model of the response
     */
    public async Task<SendEmailResponse> TrySendAsync(string tenantId, EmailMessage message, Func<Task> sendEmailDelegate)
    {
        try
        {
            var emailRecipientValidator = await CreateEmailRecipientValidatorAsync(tenantId);
            if (!emailRecipientValidator.Validate(tenantId, message, out string recipientReason))
            {
                await emailRecordService.TrackingMessageRecordAsync(message, EmailTrackingStatus.Failed, comment: recipientReason, tenantId: tenantId);
                return new SendEmailResponse { Result = SendEmailResult.RecipientNotAllowed, Message = recipientReason };
            }

            var emailCounterValidator = await CreateEmailCounterValidatorAsync(tenantId);
            if (!await emailCounterValidator.ValidateAsync(message, out string counterReason))
            {
                await emailRecordService.TrackingMessageRecordAsync(message, EmailTrackingStatus.Failed, comment: recipientReason, tenantId: tenantId);
                return new SendEmailResponse { Result = SendEmailResult.ReachQuotaLimit, Message = recipientReason };
            }

            await sendEmailDelegate();

            await emailRecordService.TrackingMessageRecordAsync(message, EmailTrackingStatus.Sent, string.Empty, tenantId);
            await emailCounterService.UpdateEmailCounterRecord(message, tenantId);
            return new SendEmailResponse { Result = SendEmailResult.Success, Message = string.Empty };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Email sender error: An error occurred while trying to send the email message. Message: {ex.Message}");
            await emailRecordService.TrackingMessageRecordAsync(message, EmailTrackingStatus.Failed, comment: ex.Message, tenantId: tenantId);
            return new SendEmailResponse { Result = SendEmailResult.InternalError, Message = ex.Message };
        }
    }

    /*
     Method to try send email
     Input:
        - message: the model of email message
        - sendEmailDelegate: Func use to send email, need to defind the message and service to send email in client code then push to our method
     Output:
        - SendEmailResponse: model of the response
     */
    public async Task<SendEmailResponse> TrySendAsync(EmailMessage message, Func<Task> sendEmailDelegate)
    {
        try
        {
            var emailRecipientValidator = await CreateEmailRecipientValidatorAsync();
            if (!emailRecipientValidator.Validate(message, out string recipientReason))
            {
                await emailRecordService.TrackingMessageRecordAsync(message, EmailTrackingStatus.Failed, comment: recipientReason);
                return new SendEmailResponse { Result = SendEmailResult.RecipientNotAllowed, Message = recipientReason };
            }

            await sendEmailDelegate();

            await emailRecordService.TrackingMessageRecordAsync(message, EmailTrackingStatus.Sent);
            await emailCounterService.UpdateEmailCounterRecord(message);
            return new SendEmailResponse { Result = SendEmailResult.Success, Message = string.Empty };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Email sender error: An error occurred while trying to send the email message. Message: {ex.Message}");
            await emailRecordService.TrackingMessageRecordAsync(message, EmailTrackingStatus.Failed, comment: ex.Message);
            return new SendEmailResponse { Result = SendEmailResult.InternalError, Message = ex.Message };
        }
    }

    private async Task<IEmailRecipientValidator> CreateEmailRecipientValidatorAsync(string tenantId = "")
    {
        var validator = new EmailRecipientValidator();
        var defaultPolicy = await policyService.GetDefaultPolicyAsync();
        if (defaultPolicy != null)
        {
            validator.SetupDefaultBlockList(defaultPolicy.BlockList);
            validator.SetupDefaultAllowList(defaultPolicy.AllowList);
        }
        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenantPolicy = await policyService.GetTenantPolicyAsync(tenantId);
            if (tenantPolicy != null)
            {
                validator.SetupBlockList(tenantId, tenantPolicy.BlockList);
                validator.SetupAllowList(tenantId, tenantPolicy.AllowList);
            }
        }

        return validator;
    }

    private async Task<IEmailCounterValidator> CreateEmailCounterValidatorAsync(string tenantId = "")
    {
        var validator = new EmailCounterValidator();
        var defaultPolicy = await policyService.GetDefaultPolicyAsync();
        if (defaultPolicy != null)
        {
            validator.SetupDefaultLimit(defaultPolicy.UserReceiveLimit, defaultPolicy.TenantSendLimit);
        }
        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenantPolicy = await policyService.GetTenantPolicyAsync(tenantId);
            if (tenantPolicy != null)
            {
                validator.SetupTenantLimit(tenantId, defaultPolicy.UserReceiveLimit, defaultPolicy.TenantSendLimit);
            }
        }

        return validator;
    }

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                policyService?.Dispose();
                GC.SuppressFinalize(this);
            }
            disposed = true;
        }
    }
}
