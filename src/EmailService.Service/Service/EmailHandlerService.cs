using EmailService.Core;
using EmailService.Domain;
using EmailService.Repository;
using Microsoft.Extensions.Logging;

namespace EmailService.Service;

public class EmailHandlerService : IEmailHandlerService
{
    private readonly IFilterRecipientService filterRecipientService;
    private readonly IUnitOfWorkFactory uowFactory;
    private readonly IEventBus eventBus;
    private readonly ILogger<EmailHandlerService> logger;

    public EmailHandlerService(
       IFilterRecipientService filterRecipientService,
       IUnitOfWorkFactory uowFactory,
       IEventBus eventBus,
       ILogger<EmailHandlerService> logger)
    {
        this.filterRecipientService = filterRecipientService;
        this.uowFactory = uowFactory;
        this.eventBus = eventBus;
        this.logger = logger;
    }

    public async Task<SaveEmailResponse> CreateEmailRecordAsync(EmailMessage emailMessage)
    {
        using (var uow = uowFactory.CreateUnitOfWork())
        {
            var emailEntity = ConvertToEntityHelper.ConvertMessageToEmailEntity(emailMessage);
            await uow.EmailRepository.AddAsync(emailEntity);
            uow.Commit();
            try
            {
                var eventMessage = new SendEmailEventMessage()
                {
                    EmailDetails = emailMessage,
                    EmailId = emailEntity.Id,
                };
                await eventBus.PublishAsync(eventMessage);
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occured when published event message for email: {emailEntity.Id}, Error: {ex}");
            }
            var filterStatus = await filterRecipientService.FilterRecipientAsync(emailMessage);
            return new SaveEmailResponse { Id = emailEntity.Id , IsFiltered = !filterStatus };
        }
    }

    public async Task<EmailDetailResponse> GetEmailDetailAsync(Guid emailId)
    {
        using var uow = uowFactory.CreateUnitOfWork();
        var emailEntity = await uow.EmailRepository.GetByIdAsync<EmailEntity>(emailId);
        uow.Commit();
        if (emailEntity == null)
            return new EmailDetailResponse();

        return new EmailDetailResponse()
        {
            Id = emailEntity.Id,
            Attachment = emailEntity.Attachment,
            Bcc = emailEntity.Bcc,
            Body = emailEntity.Body,
            CC = emailEntity.CC,
            IsBodyHtml = emailEntity.IsBodyHtml,
            IsRetrying = emailEntity.IsRetrying,
            RetryCount = emailEntity.RetryCount,
            Sender = emailEntity.Sender,
            SenderName = emailEntity.SenderName,
            Status = emailEntity.Status,
            Subject = emailEntity.Subject,
            To = emailEntity.To
        };
    }

    public async Task<EmailsResponse> GetEmailsDetailAsync(EmailPageQuery query)
    {
        using var uow = uowFactory.CreateUnitOfWork();
        var emailEntity = await uow.EmailRepository.GetRangeAsync<EmailEntity>(query);
        var total = await uow.EmailRepository.CountAsync<EmailEntity>();
        uow.Commit();
        if (!emailEntity.Any())
            return new EmailsResponse() { Total = total, Emails = [] };

        return new EmailsResponse()
        {
            Total = total,
            Emails = emailEntity.Select(x => new EmailDetailResponse()
            {
                Id = x.Id,
                Attachment = x.Attachment,
                Bcc = x.Bcc,
                Body = x.Body,
                CC = x.CC,
                IsBodyHtml = x.IsBodyHtml,
                IsRetrying = x.IsRetrying,
                RetryCount = x.RetryCount,
                Sender = x.Sender,
                SenderName = x.SenderName,
                Status = x.Status,
                Subject = x.Subject,
                To = x.To
            }).ToList()
        };
    }
}
