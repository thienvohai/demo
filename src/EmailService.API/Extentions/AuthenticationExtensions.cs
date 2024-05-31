using EmailService.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EmailService.API;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationHandlerService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Constants.Authentication.BearerScheme;
        });

        var authProvider = configuration.GetSection(AuthProviderOptions.AuthProvider).Get<AuthProviderOptions>()!;
        services.AddAuthentication(Constants.Authentication.BearerScheme)
           .AddJwtBearer(Constants.Authentication.BearerScheme, options =>
           {
               options.Authority = authProvider.Authority;
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateIssuerSigningKey = true,
                   ValidAudience = authProvider.Audience
               };
               if (authProvider.ValidIssuers != null && authProvider.ValidIssuers.Count > 0)
                   options.TokenValidationParameters.ValidIssuers = authProvider.ValidIssuers;

               options.Events = new JwtBearerEvents
               {
                   OnAuthenticationFailed = context =>
                   {
                       context.HttpContext.Items["ErrorMessage"] = context.Exception.Message;
                       return Task.CompletedTask;
                   },
               };
           });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Constants.Authentication.DefaultPolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(Constants.Authentication.ScopeClaim, Constants.Authentication.ValidScope);
            });
        });

        return services;
    }
}
