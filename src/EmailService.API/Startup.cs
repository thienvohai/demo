using Asp.Versioning;
using EmailService.Core;
using EmailService.Repository;
using EmailService.Service;

namespace EmailService.API;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddConfigurationOptions(Configuration);
        services.AddEmailClient(Configuration);
        services.AddCusHttpClient();
        services.AddScoped<AutoMigration>();
        services.AddScoped<IEmailHandlerService, EmailHandlerService>();
        services.AddScoped<IEventBus, BackgroundServiceEventBus>();
        services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddSingleton<IFilterRecipientService, FilterRecipientByConfigurationService>();
        services.AddHostedService<EmailMessageHandlerHostedService>();
        services.AddHostedService<DataManagerHostedService>();
        services.AddHostedService<EmailRetryTimerService>();
        services.AddSingleton<IEmailMessageTaskQueue>(ctx =>
        {
            if (!int.TryParse(Configuration["QueueCapacity"], out var queueCapacity))
                queueCapacity = 1000;
            return new EmailMesssageTaskQueue(queueCapacity);
        });
        services.AddAuthenticationHandlerService(Configuration);
        services.AddSwaggerGen();
        services.AddApiVersioning(opts =>
        {
            opts.ReportApiVersions = true;
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.DefaultApiVersion = ApiVersion.Default;
        });
        services.AddOTel(Configuration);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.Map("/healthz", delegate (IApplicationBuilder appConfig)
        {
            appConfig.Run(async delegate (HttpContext context)
            {
                await context.Response.WriteAsync("ok");
            });
        });
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapFallbackToFile("/admin", "index.html");
        });
    }
}
