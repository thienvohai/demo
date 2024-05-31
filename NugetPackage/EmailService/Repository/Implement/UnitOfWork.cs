using System.Data;

namespace EmailService;

public class UnitOfWork : IUnitOfWork
{
    private IDbConnectionProvider dbConnectionProvider;
    private bool disposed;
    private IEmailRepository? emailRepository;
    private IPolicyRepository? policyRepository;
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
            if (emailRepository == null)
            {
                if (string.Equals(dbConnectionProvider.DatabaseType, "PostgreSql"))
                    emailRepository = new EmailPgRepository(dbConnectionProvider, Transaction);
                else
                    emailRepository = new EmailRepository(dbConnectionProvider, Transaction);
            }
            return emailRepository;
        }
    }

    public IPolicyRepository PolicyRepository
    {
        get
        {
            if (policyRepository == null)
            {
                if (string.Equals(dbConnectionProvider.DatabaseType, "PostgreSql"))
                    policyRepository = new PolicyPgRepository(dbConnectionProvider, Transaction);
                else
                    policyRepository = new PolicyRepository(dbConnectionProvider, Transaction);
            }
            return policyRepository;
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
            if (string.Equals(dbConnectionProvider.DatabaseType, "PostgreSql"))
            {
                emailRepository = new EmailPgRepository(dbConnectionProvider, Transaction);
                policyRepository = new PolicyPgRepository(dbConnectionProvider, Transaction);
            }
            else
            {
                emailRepository = new EmailRepository(dbConnectionProvider, Transaction);
                policyRepository = new PolicyRepository(dbConnectionProvider, Transaction);
            }
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
