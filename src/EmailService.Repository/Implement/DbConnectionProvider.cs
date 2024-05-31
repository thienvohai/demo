using EmailService.Domain;
using Npgsql;
using System.Data;
using System.Data.SqlClient;

namespace EmailService.Repository;

public class DbConnectionProvider : IDbConnectionProvider
{
    private IDbConnection? dbConnection;
    private bool disposed;
    private readonly string connectionError = "A connection was successfully established with the server, but then an error occurred during the login process";
    private string connectionString;

    public DbConnectionProvider(string connectionString, string databaseType, string schema = "")
    {
        this.DatabaseType = databaseType;
        this.connectionString = connectionString;
        this.Schema = schema;
    }

    public string Schema { get; private set; } = string.Empty;
    public string DatabaseType { get; private set; } = string.Empty;

    public IDbConnection DbConnection
    {
        get
        {
            if (dbConnection == null)
            {
                dbConnection = CreateDbConnection();
                var retryCount = 0;
                while (retryCount < 3)
                {
                    try
                    {
                        dbConnection.Open();
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (!string.IsNullOrEmpty(ex?.Message) && ex.Message.Contains(connectionError))
                        {
                            retryCount++;
                            Thread.Sleep(TimeSpan.FromSeconds(retryCount + 5));
                        }
                        else
                        {
                            throw;
                        }
                        
                    }
                }
            }
            return dbConnection;
        }
        protected set { dbConnection = value; }
    }

    private IDbConnection CreateDbConnection()
    {
        if (DatabaseType.Equals(Constants.Configuration.PostgreSql))
            return new NpgsqlConnection(connectionString);

        return new SqlConnection(connectionString);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                dbConnection?.Dispose();
            }
            disposed = true;
        }
    }
}
