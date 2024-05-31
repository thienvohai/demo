namespace EmailService;

//This class represents service use to read/write Email counter record
public class EmailCounterService : IEmailCounterService
{
    private readonly IUnitOfWorkFactory uowFactory;

    public EmailCounterService(string databaseType, string connectionString, string tableSchema)
    {
        this.uowFactory = new UnitOfWorkFactory(databaseType, connectionString, tableSchema);
    }

    public Task UpdateEmailCounterRecord(EmailMessage emailMessage, string tenantId = "")
    {
        /*
         TODO: Need logic to Add or Update Email counter record
         */
        return Task.CompletedTask;
    }
}
