using EmailService.Domain;

namespace EmailService.Repository;

public interface IEmailRepository
{
    Task<T?> GetByIdAsync<T>(Guid id) where T : BaseEntity;
    Task<IEnumerable<T>> GetRangeAsync<T>(EmailPageQuery pageQuery) where T : BaseEntity;
    Task AddAsync<T>(T entity) where T : BaseEntity;
    Task AddRangeAsync<T>(IEnumerable<T> entities) where T : BaseEntity;
    Task<long> CountAsync<T>() where T : BaseEntity;
    Task<int> UpdateFailedEmail(EmailEntity emailEntity);
    Task<int> UpdateSentEmail(EmailEntity emailEntity);
    Task<int> UpdateFilteredEmail(EmailEntity emailEntity);
    Task<int> UpdateRetryingRangeAsync(List<Guid> emailIds);
    Task<int> UpdateRetryingAsync(EmailEntity emailEntity);
    Task<IEnumerable<EmailEntity>> GetFailedEmailsAsync(int pageSize, int pageIndex, int limitRetry = 25);
    Task<int> UpdateStatusAndRetryCountRangeAsync(List<EmailEntity> emailEntities);
    Task<int> RemoveOldEmail(DateTime before);
}
