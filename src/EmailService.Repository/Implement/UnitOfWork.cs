using EmailService.Domain;
using System.Data;

namespace EmailService.Repository;

public class UnitOfWork : IUnitOfWork
{
    private IDbConnectionProvider dbConnectionProvider;
    private bool disposed;
    private IEmailRepository? emaiRepository;
    private IsolationLevel isolationLevel;

    public UnitOfWork(
        IDbConnectionProvider dbConnectionProvider,
        IsolationLevel isolationLevel)
    {
        this.dbConnectionProvider = dbConnectionProvider;
        this.Transaction = this.dbConnectionProvider.DbConnection.BeginTransaction(isolationLevel);
        this.isolationLevel = isolationLevel;
    }

    public IDbTransaction Transaction { get; private set; }

    public IEmailRepository EmailRepository
    {
        get 
        {
            if (emaiRepository == null)
            {
                if (string.Equals(dbConnectionProvider.DatabaseType, Constants.Configuration.PostgreSql))
                    emaiRepository = new EmailPgRepository(dbConnectionProvider, Transaction);
                else
                    emaiRepository = new EmailRepository(dbConnectionProvider, Transaction);
            }
            return emaiRepository;
        }
    }

    public void Commit()
    {
        try
        {
            Transaction.Commit();
        }
        catch
        {
            Transaction.Rollback();
            throw;
        }
        finally
        {
            var isolationLevel = this.isolationLevel;
            Transaction.Dispose();
            Transaction = dbConnectionProvider.DbConnection.BeginTransaction(isolationLevel);
            if (string.Equals(dbConnectionProvider.DatabaseType, Constants.Configuration.PostgreSql))
                    emaiRepository = new EmailPgRepository(dbConnectionProvider, Transaction);
                else
                    emaiRepository = new EmailRepository(dbConnectionProvider, Transaction);
        }
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
                Transaction?.Dispose();
                dbConnectionProvider?.Dispose();
            }
            disposed = true;
        }
    }
}
