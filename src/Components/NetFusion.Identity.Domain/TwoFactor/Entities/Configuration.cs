namespace NetFusion.Identity.Domain.TwoFactor.Entities;

/// <summary>
/// Entity containing the current Two-Factor authentication for an account.
/// </summary>
public class Configuration
{
    /// <summary>
    /// Indicates an the account has a key that can be used to configure an authenticator.
    /// </summary>
    public bool HasAuthenticator { get; init; }
    
    /// <summary>
    /// Indicates if two-factor authentication is configured.
    /// </summary>
    public bool IsEnabled { get; init; }
    
    /// <summary>
    /// Indicates that a successful two-factor login should be remembered
    /// on the client and not needed to subsequent logins.
    /// </summary>
    public bool IsMachineRemembered { get; init; }
    
    /// <summary>
    /// The number of remaining recovery codes that can be used when the
    /// primary method of two-factor authentication (such as an authenticator)
    /// is not available. 
    /// </summary>
    public int RemainingRecoveryCodes { get; init; }
    
    /// <summary>
    /// The list of remaining recover codes.
    /// </summary>
    public string[] RecoveryCodes { get; set; } = Array.Empty<string>();
}
