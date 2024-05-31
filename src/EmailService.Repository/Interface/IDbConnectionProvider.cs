using System.Data;

namespace EmailService.Repository;

public interface IDbConnectionProvider : IDisposable
{
    IDbConnection DbConnection { get; }
    string Schema { get; }
    string DatabaseType { get; }
}
