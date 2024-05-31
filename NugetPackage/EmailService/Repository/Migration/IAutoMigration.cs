namespace EmailService
{
    public interface IAutoMigration
    {
        void EnsureDBCreatedAndMigrated();
    }
}
