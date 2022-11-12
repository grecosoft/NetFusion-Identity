namespace NetFusion.Identity.App.Settings;

/// <summary>
/// Settings specific to the identity dashboard.
/// </summary>
public class DashboardSettings
{
    /// <summary>
    /// The name used when registering the option within the configuration.
    /// </summary>
    public const string OptionsName = "Dashboard";
    
    /// <summary>
    /// The optional title to display on the header of the client.
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// The label specified when configuring an authenticator used to identity the site
    /// to which it is associated. 
    /// </summary>
    public string? AuthenticatorLabel { get; set; }
    
    /// <summary>
    /// List of applications to be listed on the dashboard.
    /// </summary>
    public ApplicationConfig[] ApplicationConfigs { get; set; } = Array.Empty<ApplicationConfig>();
}

public class ApplicationConfig
{
    /// <summary>
    /// The name of the application shown on the applications tile.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The URL to be navigated when the application is selected.
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// The color of the application's name header.
    /// </summary>
    public string Color { get; set; } = "#5f9ea0";
    
    /// <summary>
    /// If specified, the application will only be displayed if the user is a member
    /// of the specified role.
    /// </summary>
    public string RequiredRoleName { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description associated with the application.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// The optional icon to identify the application.
    /// </summary>
    public string? IconName { get; set; }
}