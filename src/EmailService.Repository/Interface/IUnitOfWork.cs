using System.Data;

namespace EmailService.Repository;

public interface IUnitOfWork : IDisposable
{
    IEmailRepository EmailRepository { get; }
    IDbTransaction Transaction { get; }

    void Commit();
}
