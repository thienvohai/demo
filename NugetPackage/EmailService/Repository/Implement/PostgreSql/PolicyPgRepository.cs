using Dapper;
using System.Data;
using System.Reflection;

namespace EmailService;

public class PolicyPgRepository : BasePgRepository, IPolicyRepository
{
    public PolicyPgRepository(IDbConnectionProvider dbConnectionProvider, IDbTransaction transaction)
        : base(dbConnectionProvider, transaction)
    {

    }

    public async Task<EmailPolicyEntity> GetByTenantIdAsync(string tenantId)
    {
        var parameter = new DynamicParameters();
        parameter.Add("@tenantId", tenantId);
        var query = $@"Select * From ""{Schema}"".""{TableNames.EmailPolicyTable}""
                       WHERE ""{EmailPolicyColumns.TenantId}"" = @tenantId";

        var result = await DbConnection.QueryAsync<EmailPolicyEntity>(query, parameter, DbTransaction, CommandTimeout);
        if (result != null && result.Any())
            return result.First();

        return null;
    }

    public async Task<int> UpdatePolicyDetail(EmailPolicyEntity entity)
    {
        var parameter = new DynamicParameters();
        parameter.Add("@tenantId", entity.TenantId);
        parameter.Add("@now", DateTime.UtcNow, DbType.DateTime);
        parameter.Add("@detail", entity.Content);
        var query = $@"UPDATE ""{Schema}"".""{TableNames.EmailPolicyTable}""
                       SET
                        ""{BaseColumns.Modified}"" = @now,
                        ""{EmailPolicyColumns.Content}"" = @detail
                       WHERE ""{EmailPolicyColumns.TenantId}"" = @tenantId";

        return await DbConnection.ExecuteAsync(query, parameter, DbTransaction, CommandTimeout);
    }
}
