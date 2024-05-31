using EmailService.Domain;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace EmailService.Repository;

public class AutoMigration
{
    private readonly ILogger<AutoMigration> logger;
    private readonly DatabaseOptions databaseOptions;

    public AutoMigration(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<AutoMigration> logger)
    {
        this.logger = logger;
        this.databaseOptions = databaseOptions.Value;
    }

    public void EnsureDBCreatedAndMigrated()
    {
        try
        {
            // FIXME: there may be several instances running, so there is potential concurrent issues
            EnsureDatabaseExists();
            RunUpdateScript();
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occured while trying to ensure DB exist and migrated. Error: {ex}");
            throw;
        }
    }

    private void RunUpdateScript()
    {
        var subFolder = string.Empty;
        if (databaseOptions.DatabaseType.Equals(Constants.Configuration.PostgreSql))
            subFolder = "EmailScriptPg";
        else
            subFolder = "EmailScript";

        string path = Path.Combine(AppContext.BaseDirectory, "SqlScript", subFolder);
        if (!Directory.Exists(path))
        {
            logger.LogInformation($"Skipping create or migrate db, since no script folder was found.");
            return;
        }
        List<KeyValuePair<int, string>> scripts = new List<KeyValuePair<int, string>>();
        foreach (var filePath in Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var index = fileName.LastIndexOf('_');
            if (index > 0 && int.TryParse(fileName.Substring(index + 1), out int version))
            {
                scripts.Add(new KeyValuePair<int, string>(version, filePath));
            }
        }
        if (scripts.Count > 0)
        {
            var scriptFiles = scripts.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList();
            RunUpdateScriptInternal(scriptFiles);
        }
        else
        {
            logger.LogInformation($"Skipping create or migrate db, since no script was found.");
        }
    }

    private void RunUpdateScriptInternal(List<string> scriptFiles)
    {
        foreach (var file in scriptFiles)
        {
            var script = File.ReadAllText(file, Encoding.UTF8);
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
        if (databaseOptions.DatabaseType.Equals(Constants.Configuration.PostgreSql))
        {
            checkExistQuery = "SELECT CASE WHEN EXISTS((SELECT FROM pg_database WHERE datname = @Name)) THEN 1 ELSE 0 END";
            var dbNameBuilder = new NpgsqlConnectionStringBuilder(databaseOptions.ConnectionString);
            dbName = $"\"{dbNameBuilder.Database}\"";
            dbNameBuilder.Database = "postgres";
            connectionString = dbNameBuilder.ConnectionString;
        }
        else
        {
            checkExistQuery = "SELECT CASE WHEN EXISTS((SELECT * FROM sys.databases WHERE name = @Name)) THEN 1 ELSE 0 END";
            var dbNameBuilder = new SqlConnectionStringBuilder(databaseOptions.ConnectionString);
            dbName = $"[{dbNameBuilder.InitialCatalog}]";
            dbNameBuilder.InitialCatalog = "master";
            connectionString = dbNameBuilder.ConnectionString;
        }
        
        using (var sqlProvider = new DbConnectionProvider(connectionString, databaseOptions.DatabaseType))
        {
            var connection = sqlProvider.DbConnection;
            bool databaseExists = connection.ExecuteScalar<bool>(checkExistQuery, new { Name = dbName.Replace("[", "").Replace("]", "").Replace("\"","") });
            if (!databaseExists)
            {
                connection.Execute($"CREATE DATABASE {dbName}", commandTimeout: 300);
            }
        }
    }

    private void RunScript(string script)
    {
        using (var sqlProvider = new DbConnectionProvider(databaseOptions.ConnectionString, databaseOptions.DatabaseType))
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
                    command.CommandText = script.Replace("@_tableSchema", databaseOptions.TableSchema);
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
