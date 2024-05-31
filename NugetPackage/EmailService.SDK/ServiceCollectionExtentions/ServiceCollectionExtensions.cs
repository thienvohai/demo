using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EmailService.SDK;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailClient(this IServiceCollection services, Action<EmailServiceSDKClientOptions> configureOptions, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.Configure(configureOptions);
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.TryAddSingleton<IEmailServiceSDKClient, EmailServiceSDKClient>();
                break;
            case ServiceLifetime.Scoped:
                services.TryAddScoped<IEmailServiceSDKClient, EmailServiceSDKClient>();
                break;
            case ServiceLifetime.Transient:
                services.TryAddTransient<IEmailServiceSDKClient, EmailServiceSDKClient>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
        return services;
    }
}
