namespace NetFusion.Identity.App.Settings;

/// <summary>
/// Settings specific to the configuration of ASP.NET Core Identity.
/// </summary>
public class IdentitySettings
{
    /// <summary>
    /// The name used when registering the option within the configuration.
    /// </summary>
    public const string OptionsName = "Identity";
    
    /// <summary>
    /// The number of minutes the authentication should be valid.
    /// </summary>
    public int JwtExpireMinutes { get; set; } = 10080;
    
    /// <summary>
    /// The key used when signing generated JTW tokens.
    /// </summary>
    public string? JwtSecurityKey { get; set; }
    
    /// <summary>
    /// The number of recovery codes that should be generated.
    /// </summary>
    public int NumberRecoveryCodes { get; set; } = 12;
    
    /// <summary>
    /// The minimum number of remaining recovery codes for when the user should be notified.
    /// </summary>
    public int MinNumberRecoveryCodesWarning { get; set; } = 3;

    /// <summary>
    /// Controls how much time the authentication ticket stored in the cookie will remain valid from the point it is created
    /// The expiration information is stored in the protected cookie ticket.
    /// </summary>
    public int ExpireMinutes { get; set; } = 10080;

    /// <summary>
    /// The SlidingExpiration is set to true to instruct the handler to re-issue a new cookie with a new
    /// expiration time any time it processes a request which is more than halfway through the expiration window.
    /// </summary>
    public bool SlidingExpiration { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the issuer that should be used for any claims that are created
    /// </summary>
    public string? ClaimsIssuer { get; set; }
}