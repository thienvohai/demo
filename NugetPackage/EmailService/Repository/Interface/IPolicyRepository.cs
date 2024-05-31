namespace EmailService;

public interface IPolicyRepository
{
    Task AddAsync<T>(T entity) where T : BaseEntity;
    Task<EmailPolicyEntity?> GetByTenantIdAsync(string tenantId);
    Task<int> UpdatePolicyDetail(EmailPolicyEntity entity);
}
