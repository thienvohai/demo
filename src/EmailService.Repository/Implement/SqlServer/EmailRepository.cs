using EmailService.Domain;
using Dapper;
using System.Data;

namespace EmailService.Repository;

public class EmailRepository : BaseRepository, IEmailRepository
{
    public EmailRepository(IDbConnectionProvider dbConnectionProvider, IDbTransaction transaction)
        : base(dbConnectionProvider, transaction)
    {

    }

    public async Task<int> UpdateSentEmail(EmailEntity emailEntity)
    {
        var parameter = new DynamicParameters();
        parameter.Add("@status", (int)EmailStatus.Sent);
        parameter.Add("@id", emailEntity.Id.ToString());
        parameter.Add("@modified", DateTime.UtcNow, DbType.DateTime2);
        parameter.Add("@retryCount", emailEntity.RetryCount);
        parameter.Add("@false", 0);
        parameter.Add("@null", null);
        var query = $@"UPDATE [{Schema}].[{TableNames.EmailTable}]
                       SET
                        [{EmailColumns.Status}] = @status,
                        [{EmailColumns.IsRetrying}] = @false,
                        [{BaseColumns.Modified}] = @modified,
                        [{EmailColumns.NextRetryTime}] = @null,
                        [{EmailColumns.RetryCount}] = @retryCount
                       WHERE [{BaseColumns.Id}] = @id";

        return await DbConnection.ExecuteAsync(query, parameter, DbTransaction, CommandTimeout);
    }

    public async Task<int> UpdateFilteredEmail(EmailEntity emailEntity)
    {
        var parameter = new DynamicParameters();
        parameter.Add("@status", (int)EmailStatus.Filtered);
        parameter.Add("@id", emailEntity.Id.ToString());
        parameter.Add("@modified", DateTime.UtcNow, DbType.DateTime2);
        parameter.Add("@null", null);
        parameter.Add("@false", 0);
        var query = $@"UPDATE [{Schema}].[{TableNames.EmailTable}]
                       SET
                        [{EmailColumns.Status}] = @status,
                        [{BaseColumns.Modified}] = @modified,
                        [{EmailColumns.NextRetryTime}] = @null,
                        [{EmailColumns.IsRetrying}] = @false
                       WHERE [{BaseColumns.Id}] = @id";

        return await DbConnection.ExecuteAsync(query, parameter, DbTransaction, CommandTimeout);
    }

    public async Task<int> UpdateFailedEmail(EmailEntity emailEntity)
    {
        var parameter = new DynamicParameters();
        parameter.Add("@status", emailEntity.Status);
        parameter.Add("@retry", emailEntity.RetryCount);
        parameter.Add("@nextRetry", emailEntity.NextRetryTime, DbType.DateTime2);
        parameter.Add("@id", emailEntity.Id.ToString());
        parameter.Add("@modified", DateTime.UtcNow, DbType.DateTime2);
        parameter.Add("@false", 0);
        var query = $@"UPDATE [{Schema}].[{TableNames.EmailTable}]
                       SET
                        [{EmailColumns.Status}] = @status,
                        [{EmailColumns.IsRetrying}] = @false,
                        [{EmailColumns.RetryCount}] = @retry,
                        [{EmailColumns.NextRetryTime}] = @nextRetry,
                        [{BaseColumns.Modified}] = @modified
                       WHERE [{BaseColumns.Id}] = @id";

        return await DbConnection.ExecuteAsync(query, parameter, DbTransaction, CommandTimeout);
    }

    public async Task<int> UpdateRetryingAsync(EmailEntity emailEntity)
    {
        var parameter = new DynamicParameters();
        parameter.Add("@true", 1);
        parameter.Add($"@emailId", emailEntity.Id);
        parameter.Add($"@modified", emailEntity.Modified, DbType.DateTime2);
        parameter.Add($"@now", DateTime.UtcNow, DbType.DateTime2);

        var query = $@"UPDATE [{Schema}].[{TableNames.EmailTable}]
                       SET 
                        [{EmailColumns.IsRetrying}] = @true
                       WHERE [{BaseColumns.Id}] = @emailId
                       AND [{BaseColumns.Modified}] = @modified
                       AND [{EmailColumns.NextRetryTime}] < @now";
        return await DbConnection.ExecuteAsync(query, parameter, DbTransaction, CommandTimeout);
    }

    public async Task<int> UpdateRetryingRangeAsync(List<Guid> emailIds)
    {
        if (emailIds.Count == 0)
            return 0;

        var count = 0;
        var idsString = new List<string>();
        var parameter = new DynamicParameters();
        parameter.Add("@true", 1);
        foreach (var emailId in emailIds)
        {
            parameter.Add($"@emailid{count}", emailId);
            idsString.Add($"@emailid{count}");
        }

        var whereClause = string.Join(",", idsString);
        var query = $@"UPDATE [{Schema}].[{TableNames.EmailTable}]
                       SET 
                        [{EmailColumns.IsRetrying}] = @true
                       WHERE [{BaseColumns.Id}] IN ({whereClause})";
        return await DbConnection.ExecuteAsync(query, parameter, DbTransaction, CommandTimeout);
    }

    public async Task<IEnumerable<EmailEntity>> GetFailedEmailsAsync(int pageSize, int pageIndex, int limitRetry = 25)
    {
        var parameter = new DynamicParameters();
        parameter.Add("@pageSize", pageSize);
        parameter.Add("@pageIndex", (pageIndex - 1) * pageSize);
        parameter.Add("@emailStatus", (int)EmailStatus.Failed);
        parameter.Add("@retryCount", limitRetry);
        parameter.Add("@now", DateTime.UtcNow, DbType.DateTime2);
        parameter.Add("@false", 0);
        var query = $@"SELECT *
                    FROM
                        [{Schema}].[{TableNames.EmailTable}] AS E
                    WHERE
                        E.[{EmailColumns.Status}] = @emailStatus
                        AND E.[{EmailColumns.IsRetrying}] = @false
                        AND E.[{EmailColumns.RetryCount}] < @retryCount
                        AND E.[{EmailColumns.NextRetryTime}] < @now
                        AND E.[{BaseColumns.IsDeleted}] = @false
                    ORDER BY
                        E.[{EmailColumns.NextRetryTime}] DESC
                    OFFSET @pageIndex ROWS FETCH NEXT @pageSize ROWS ONLY;";
        return await DbConnection.QueryAsync<EmailEntity>(query, parameter, DbTransaction, CommandTimeout);
    }

    public async Task<int> UpdateStatusAndRetryCountRangeAsync(List<EmailEntity> emailEntities)
    {
        if (emailEntities.Count == 0)
            return 0;

        var parameter = new DynamicParameters();
        var count = 0;
        var statusClause = "";
        var retryClause = "";
        var idsString = new List<string>();
        foreach (var emailEntity in emailEntities)
        {
            statusClause += "\n" + $@"WHEN [{BaseColumns.Id}] = @emailid{count} THEN @status{count}";
            retryClause += "\n" + $@"WHEN [{BaseColumns.Id}] = @emailid{count} THEN @retry{count}";
            parameter.Add($"@emailid{count}", emailEntity.Id);
            parameter.Add($"@status{count}", emailEntity.Status);
            parameter.Add($"@retry{count}", emailEntity.RetryCount);
            idsString.Add($"@emailid{count}");
            count++;
        }
        parameter.Add("@now", DateTime.UtcNow);

        var whereClause = string.Join(",", idsString);
        var query = $@"UPDATE [{Schema}].[{TableNames.EmailTable}]
                       SET 
                        [{EmailColumns.Status}] = 
                            CASE {statusClause}
                            ELSE [{EmailColumns.Subject}]  
                            END,
                        [{EmailColumns.RetryCount}] = 
                            CASE {retryClause}
                            ELSE [{EmailColumns.RetryCount}]  
                            END
                       WHERE [{BaseColumns.Id}] IN ({whereClause})
                       AND [{EmailColumns.NextRetryTime}] < @now";
        return await DbConnection.ExecuteAsync(query, parameter, DbTransaction, CommandTimeout);
    }

    public async Task<int> RemoveOldEmail(DateTime before)
    {
        var parameter = new DynamicParameters();
        parameter.Add("@before", before, DbType.DateTime2);

        var query = $@"DELETE FROM [{Schema}].[{TableNames.EmailTable}]
                       WHERE [{BaseColumns.Modified}] < @before";
        return await DbConnection.ExecuteAsync(query, parameter, DbTransaction, CommandTimeout);
    }
}
