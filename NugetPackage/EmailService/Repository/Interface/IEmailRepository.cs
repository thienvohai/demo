namespace EmailService;

public interface IEmailRepository
{
    Task AddAsync<T>(T entity) where T : BaseEntity;
}
