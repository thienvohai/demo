using Dapper;
using Npgsql;
using System.Data;
using System.Data.SqlClient;

namespace EmailService;

public class AutoMigration : IAutoMigration
{
    private readonly string connectionString;
    private readonly string databaseType;
    private readonly string tableSchema;

    public AutoMigration(string databaseType, string connectionString, string tableSchema)
    {
        this.databaseType = databaseType;
        this.connectionString = connectionString;
        this.tableSchema = tableSchema;
    }

    public void EnsureDBCreatedAndMigrated()
    {
        // FIXME: there may be several instances running, so there is potential concurrent issues
        EnsureDatabaseExists();
        RunUpdateScript();
    }

    private void RunUpdateScript()
    {
        if (databaseType.Equals("PostgreSql"))
        {
            var scriptFiles = MigrationScript.PostgreSqlScripts.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList();
            RunUpdateScriptInternal(scriptFiles);
        }
        else
        {
            var scriptFiles = MigrationScript.SqlServerScripts.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList();
            RunUpdateScriptInternal(scriptFiles);
        }
    }

    private void RunUpdateScriptInternal(List<string> scripts)
    {
        foreach (var script in scripts)
        {
            if (!string.IsNullOrWhiteSpace(script))
            {
                RunScript(script);
            }
        }
    }

    private void EnsureDatabaseExists()
    {
        var checkExistQuery = string.Empty;
        var dbName = string.Empty;
        var connectionString = string.Empty;
        if (this.databaseType.Equals("PostgreSql"))
        {
            checkExistQuery = "SELECT CASE WHEN EXISTS((SELECT FROM pg_database WHERE datname = @Name)) THEN 1 ELSE 0 END";
            var dbNameBuilder = new NpgsqlConnectionStringBuilder(this.connectionString);
            dbName = $"\"{dbNameBuilder.Database}\"";
            dbNameBuilder.Database = "postgres";
            connectionString = dbNameBuilder.ConnectionString;
        }
        else
        {
            checkExistQuery = "SELECT CASE WHEN EXISTS((SELECT * FROM sys.databases WHERE name = @Name)) THEN 1 ELSE 0 END";
            var dbNameBuilder = new SqlConnectionStringBuilder(this.connectionString);
            dbName = $"[{dbNameBuilder.InitialCatalog}]";
            dbNameBuilder.InitialCatalog = "master";
            connectionString = dbNameBuilder.ConnectionString;
        }

        using (var sqlProvider = new DbConnectionProvider(this.databaseType, connectionString))
        {
            var connection = sqlProvider.DbConnection;
            bool databaseExists = connection.ExecuteScalar<bool>(checkExistQuery, new { Name = dbName.Replace("[", "").Replace("]", "").Replace("\"", "") });
            if (!databaseExists)
            {
                connection.Execute($"CREATE DATABASE {dbName}", commandTimeout: 300);
            }
        }
    }

    private void RunScript(string script)
    {
        using (var sqlProvider = new DbConnectionProvider(this.databaseType, this.connectionString))
        {
            var connection = sqlProvider.DbConnection;
            using var transaction = connection.BeginTransaction();
            try
            {
                using (var command = connection.CreateCommand())
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    command.Transaction = transaction;
                    command.CommandText = script.Replace("@_tableSchema", this.tableSchema);
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}

