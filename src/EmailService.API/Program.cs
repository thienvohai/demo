using EmailService.Repository;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NLog;
using NLog.Web;
using System.Security.Cryptography.X509Certificates;

namespace EmailService.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        await BuildAndRun<Startup>(args, RunProcessBuildinDataService);
    }

    public static IHostBuilder CreateHostBuilder<T>(string[] args) where T : class
    {
        return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(delegate (IWebHostBuilder webBuilder)
        {
            webBuilder.ConfigureAppConfiguration(delegate (WebHostBuilderContext env, IConfigurationBuilder config)
            {
                config.AddEnvironmentVariables();
            });
            webBuilder.ConfigureLogging(delegate (WebHostBuilderContext context, ILoggingBuilder logging)
            {
                logging.ClearProviders();
                if (string.Equals(context.HostingEnvironment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
                {
                    logging.AddConsole();
                }
            }).UseNLog();

            webBuilder.ConfigureKestrel(delegate (WebHostBuilderContext env, KestrelServerOptions options)
            {
                if (!env.HostingEnvironment.IsDevelopment())
                {
                    options.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 1024, gracePeriod: TimeSpan.FromSeconds(30));
                    options.ConfigureHttpsDefaults(listenOptions =>
                    {
                        listenOptions.ServerCertificate = new X509Certificate2(X509Certificate2.CreateFromPemFile("tls.crt", "tls.key").Export(X509ContentType.Pfx));
                    });
                }
            });
            webBuilder.UseStartup<T>();
        });
    }

    public static async Task BuildAndRun<T>(string[] args, Func<IHost, Task>? afterBuildAction = null) where T : class
    {
        Microsoft.Extensions.Logging.ILogger<Program>? logger = null;
        try
        {
            IHostBuilder hostBuilder = CreateHostBuilder<T>(args);
            IHost host = hostBuilder.Build();
            logger = host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
            if (afterBuildAction != null)
            {
                await afterBuildAction(host);
            }
            logger.LogInformation("Email Service API Starting.");
            host.Run();
            logger.LogInformation("Email Service API Stoping.");
        }
        catch (Exception e)
        {
            logger?.LogError($"{e}");
        }
        finally
        {
            LogManager.Shutdown();
        }
    }
    private static Task RunProcessBuildinDataService(IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var autoMigration = scope.ServiceProvider.GetRequiredService<AutoMigration>();
            autoMigration.EnsureDBCreatedAndMigrated();
        }

        return Task.CompletedTask;
    }
}
