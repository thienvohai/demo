using System.Data;

namespace EmailService;

public interface IUnitOfWork : IDisposable
{
    IEmailRepository EmailRepository { get; }
    IPolicyRepository PolicyRepository { get; }
    IDbTransaction Transaction { get; }

    void Commit();
}
