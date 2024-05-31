using System.Data;

namespace EmailService;

public interface IUnitOfWorkFactory
{
    IUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
}

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly string databaseType;
    private readonly string connectionString;
    private readonly string tableSchema;

    public UnitOfWorkFactory(string databaseType, string connectionString, string tableSchema)
    {
        this.databaseType = databaseType;
        this.connectionString = connectionString;
        this.tableSchema = tableSchema;
    }

    public IUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        var uow = new UnitOfWork(new DbConnectionProvider(databaseType, connectionString, tableSchema), isolationLevel);
        return uow;
    }
}
