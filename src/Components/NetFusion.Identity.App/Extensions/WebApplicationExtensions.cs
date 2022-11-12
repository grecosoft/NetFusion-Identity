using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using NetFusion.Identity.App.Settings;

namespace NetFusion.Identity.App.Extensions;

/// <summary>
/// Web application extensions.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures Cors for all client applications so they can initiate
    /// a call, passing the authentication cookie, to obtain a corresponding
    /// JWT token used to authenticate with their corresponding API.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>Web application being configured.</returns>
    public static WebApplication ConfigureDashboardCors(this WebApplication app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));
        
        var settings = GetDashboardSettings(app);
        foreach (var appConfig in settings.ApplicationConfigs)
        {
            app.UseCors(cors => cors.WithOrigins(new Uri(appConfig.Url).Authority)
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("WWW-Authenticate", "resource-404")
                .AllowAnyHeader());
        }
        
        return app;
    }

    private static DashboardSettings GetDashboardSettings(WebApplication app)
    {
        var settings = new DashboardSettings();
        
        var section = app.Configuration.GetSection(DashboardSettings.OptionsName);
        if (section.Exists())
        {
            section.Bind(settings);
        }

        return settings;
    }
}