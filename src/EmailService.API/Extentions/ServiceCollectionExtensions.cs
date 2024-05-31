using EmailService.Core;
using EmailService.Domain;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Net.Security;
using static EmailService.Domain.Constants;

namespace EmailService.API;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailClientOptions>(configuration.GetSection(EmailClientOptions.EmailClient));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.Database));
        services.Configure<BackgroundWorkerOptions>(configuration.GetSection(BackgroundWorkerOptions.BackgroundWorker));
        services.Configure<FilterRecipientOptions>(configuration.GetSection(FilterRecipientOptions.FilterRecipient));
        return services;
    }

    public static IServiceCollection AddEmailClient(this IServiceCollection services, IConfiguration configuration)
    {
        var emailClientType = configuration.GetSection($"{EmailClientOptions.EmailClient}")["EmailClientType"]?.ToString();
        if (string.Equals(emailClientType, Configuration.SendGridType))
        {
            services.AddScoped<IEmailClient, SendGridEmailClient>();
        }
        else if (string.Equals(emailClientType, Configuration.StmpType))
        {
            services.AddScoped<IEmailClient, SmtpEmailClient>();
        }
        return services;
    }

    public static IServiceCollection AddCusHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient(HttpClientDefault.Default, (client) =>
        {
            client.Timeout = TimeSpan.FromSeconds(100);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new SocketsHttpHandler()
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true,
                ConnectTimeout = TimeSpan.FromSeconds(100)
            };
            return handler;
        });

        return services;
    }

    public static IServiceCollection AddOTel(this IServiceCollection services, IConfiguration configuration)
    {
        var tracingOtlpEndpoint = configuration["OTLP_ENDPOINT_URL"];
        var otel = services.AddOpenTelemetry();
        
        otel.ConfigureResource(resource => resource
            .AddService(serviceName: "Email Service API"));

        otel.WithMetrics(metrics => 
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddHttpClientInstrumentation();
            if (!string.IsNullOrEmpty(tracingOtlpEndpoint))
            {
                metrics.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                });
            }
            else
            {
                metrics.AddConsoleExporter();
            }
        });

        
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            if (!string.IsNullOrEmpty(tracingOtlpEndpoint))
            {
                tracing.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                });
            }
            else
            {
                tracing.AddConsoleExporter();
            }
        });

        return services;
    }
}
