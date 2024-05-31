using System.Data;

namespace EmailService;

public class EmailRepository : BaseRepository, IEmailRepository
{
    public EmailRepository(IDbConnectionProvider dbConnectionProvider, IDbTransaction transaction)
        : base(dbConnectionProvider, transaction)
    {

    }
}
