using System.Data;

namespace EmailService;

public class EmailPgRepository : BasePgRepository, IEmailRepository
{
    public EmailPgRepository(IDbConnectionProvider dbConnectionProvider, IDbTransaction transaction)
        : base(dbConnectionProvider, transaction)
    {

    }
}
