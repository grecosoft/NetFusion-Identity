namespace NetFusion.Identity.Domain.TwoFactor.Entities;

/// <summary>
/// Entity containing the information used to configure an authenticator. 
/// </summary>
public class AuthenticatorSetup 
{
    /// <summary>
    /// The email address of the account for which the authenticator is being configured.
    /// </summary>
    public string Email { get; }
    
    /// <summary>
    /// The key to be entered when setting up an authenticator.
    /// </summary>
    public string AuthenticatorKey { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="email">The email address of the account for which the authenticator is being configured.</param>
    /// <param name="authenticatorKey">The key to be entered when setting up an authenticator.</param>
    public AuthenticatorSetup(string email, string authenticatorKey)
    {
        Email = email;
        AuthenticatorKey = authenticatorKey;
    }
}
   


