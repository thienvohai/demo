using EmailService.Domain;
using Microsoft.Extensions.Options;
using System.Data;

namespace EmailService.Repository;

public interface IUnitOfWorkFactory
{
    IUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
}

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly DatabaseOptions databaseOptions;
    public UnitOfWorkFactory(
        IOptions<DatabaseOptions> databaseOptions)
    {
        this.databaseOptions = databaseOptions.Value;
    }

    public IUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        var uow = new UnitOfWork(new DbConnectionProvider(databaseOptions.ConnectionString, databaseOptions.DatabaseType, databaseOptions.TableSchema), isolationLevel);
        return uow;
    }
}
