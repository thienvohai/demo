using System.Data;

namespace EmailService;

public interface IDbConnectionProvider : IDisposable
{
    IDbConnection DbConnection { get; }
    string Schema { get; }
    string DatabaseType { get; }
}
