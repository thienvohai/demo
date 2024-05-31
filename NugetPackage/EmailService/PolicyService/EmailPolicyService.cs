using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace EmailService;

//This class represents service use to read/write Policy record which used for our validators
public class EmailPolicyService : IEmailPolicyService
{
    private bool disposed;
    private readonly IUnitOfWorkFactory uowFactory;
    private readonly MemoryCache memoryCache;
    private const string defaultPolicyKey = "_emailsenderdefaultpolicy";
    public EmailPolicyService(string databaseType, string connectionString, string tableSchema)
    {
        this.uowFactory = new UnitOfWorkFactory(databaseType, connectionString, tableSchema);
        memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    public Task AddDefaultPolicyAsync(EmailSenderPolicy policy)
    {
        return AddTenantPolicyAsync(defaultPolicyKey, policy);
    }

    public async Task AddTenantPolicyAsync(string tenantId, EmailSenderPolicy policy)
    {
        var policyEntity = ModelConverter.ConvertPolicyToNewEntityWithTenantId(policy, tenantId);
        using var uow = uowFactory.CreateUnitOfWork();
        var oldEntity = await uow.PolicyRepository.GetByTenantIdAsync(tenantId);
        if (oldEntity == null)
        {
            await uow.PolicyRepository.AddAsync(policyEntity);
        }
        else
        {
            await uow.PolicyRepository.UpdatePolicyDetail(policyEntity);
        }
        uow.Commit();
        memoryCache.Set(tenantId, policy, DateTimeOffset.Now.AddMinutes(5));
    }

    public async Task<EmailSenderPolicy?> GetTenantPolicyAsync(string tenantId)
    {
        if (!memoryCache.TryGetValue(tenantId, out EmailSenderPolicy policy))
        {
            using var uow = uowFactory.CreateUnitOfWork();
            var policyEntity = await uow.PolicyRepository.GetByTenantIdAsync(tenantId);
            uow.Commit();
            if (policyEntity != null)
            {
                policy = JsonSerializer.Deserialize<EmailSenderPolicy>(policyEntity.Content);
                memoryCache.Set(tenantId, policy, DateTimeOffset.Now.AddMinutes(5));
            }
        }
        return policy;

    }

    public Task<EmailSenderPolicy?> GetDefaultPolicyAsync()
    {
        return GetTenantPolicyAsync(defaultPolicyKey);
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
                memoryCache?.Dispose();
                GC.SuppressFinalize(this);
            }
            disposed = true;
        }
    }
}
