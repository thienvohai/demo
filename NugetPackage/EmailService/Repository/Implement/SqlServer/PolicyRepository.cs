using Dapper;
using System.Data;

namespace EmailService;

public class PolicyRepository : BaseRepository, IPolicyRepository
{
    public PolicyRepository(IDbConnectionProvider dbConnectionProvider, IDbTransaction transaction)
        : base(dbConnectionProvider, transaction)
    {

    }

    public async Task<EmailPolicyEntity?> GetByTenantIdAsync(string tenantId)
    {
        var parameter = new DynamicParameters();
        parameter.Add("@tenantId", tenantId);
        var query = $@"Select * From [{Schema}].[{TableNames.EmailPolicyTable}]
                       WHERE [{EmailPolicyColumns.TenantId}] = @tenantId";

        var result = await DbConnection.QueryAsync<EmailPolicyEntity>(query, parameter, DbTransaction, CommandTimeout);
        if (result != null && result.Any())
            return result.First();

        return null;
    }

    public async Task<int> UpdatePolicyDetail(EmailPolicyEntity entity)
    {
        var parameter = new DynamicParameters();
        parameter.Add("@now", DateTime.UtcNow, DbType.DateTime2);
        parameter.Add("@detail", entity.Content);
        parameter.Add("@tenantId", entity.TenantId);

        var query = $@"UPDATE [{Schema}].[{TableNames.EmailPolicyTable}]
                       SET 
                        [{BaseColumns.Modified}] = @now,
                        [{EmailPolicyColumns.Content}] = @detail
                       WHERE [{EmailPolicyColumns.TenantId}] = @tenantId";
        return await DbConnection.ExecuteAsync(query, parameter, DbTransaction, CommandTimeout);
    }
}
