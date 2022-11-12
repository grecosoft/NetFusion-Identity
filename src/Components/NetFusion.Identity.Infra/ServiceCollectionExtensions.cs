using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Identity.App;
using NetFusion.Identity.App.Implementations;
using NetFusion.Identity.App.Repositories;
using NetFusion.Identity.App.Services;
using NetFusion.Identity.App.Settings;
using NetFusion.Identity.Domain;
using NetFusion.Identity.Domain.Authentication.Services;
using NetFusion.Identity.Domain.Registration.Services;
using NetFusion.Identity.Domain.TwoFactor.Services;
using NetFusion.Identity.Infra.Repositories;
using NetFusion.Identity.Infra.Services;

namespace NetFusion.Identity.Infra;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterIdentityServices<TIdentity>(this IServiceCollection services)
        where TIdentity : class, IUserIdentity, new()
    {
        services.AddSingleton<UrlEncoderService>();
        services.AddScoped<IAuthenticationContext<TIdentity>, AuthenticationContext<TIdentity>>();
        
        // Registration:
        services.AddScoped<IRegistrationService, RegistrationService<TIdentity>>();
        services.AddScoped<IConfirmationService, ConfirmationService<TIdentity>>();

        // Authentication:
        services.AddScoped<IAuthenticationService, AuthenticationService<TIdentity>>();
        services.AddScoped<IAuthenticatorService, AuthenticatorService<TIdentity>>();
        services.AddScoped<ITwoFactorService, TwoFactorService<TIdentity>>();
        services.AddScoped<ITokenService, TokenService<TIdentity>>();

        // Claims:
        services.AddScoped<IUserClaimsPrincipalFactory<TIdentity>, ClaimsPrincipleFactory<TIdentity>>();

        // Integration:
        services.AddScoped<IConfirmationSender, ConsoleConfirmationEmailSender>();
        services.AddSingleton<IServiceContext, NullServiceContext>();
        services.AddScoped<IAccountRepository, AccountRepository<TIdentity>>();

        return services;
    }

    public static void RegisterIdentityConfigurations(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IdentitySettings>(configuration.GetSection(IdentitySettings.OptionsName));
        services.Configure<DashboardSettings>(configuration.GetSection(DashboardSettings.OptionsName));

        var identitySettings = GetIdentitySettings(configuration);
        
        services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(identitySettings.ExpireMinutes);
            options.SlidingExpiration = identitySettings.SlidingExpiration;
            options.ClaimsIssuer = identitySettings.ClaimsIssuer;
        
            options.LoginPath = "/Authentication/Login";
            options.LogoutPath = "/Authentication/Logout";
            options.AccessDeniedPath = "";
        });
    }
    
    private static IdentitySettings GetIdentitySettings(IConfiguration configuration)
    {
        var settings = new IdentitySettings();
        
        var section = configuration.GetSection(IdentitySettings.OptionsName);
        if (section.Exists())
        {
            section.Bind(settings);
        }

        return settings;
    }
}