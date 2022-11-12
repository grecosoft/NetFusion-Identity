using NetFusion.Identity.Domain.TwoFactor.Entities;

namespace NetFusion.Identity.Domain.TwoFactor.Services;

/// <summary>
/// Service responsible for managing an authenticator used for two-factor authentication.
/// </summary>
public interface IAuthenticatorService
{
    /// <summary>
    /// Returns an entity containing information to configure an authenticator.
    /// </summary>
    /// <returns>Configuration</returns>
    Task<AuthenticatorSetup> GetSetupInformationAsync();
    
    /// <summary>
    /// Confirms an authenticator after configured using a token it generates.
    /// </summary>
    /// <param name="setupToken">Token used to validate and complete setup.</param>
    /// <returns>The result of the setup.</returns>
    Task<ResultStatus> ConfirmSetupTokenAsync(string setupToken);
    
    /// <summary>
    /// Completes a user's login using a token generated from an authenticator.
    /// </summary>
    /// <param name="login">Authenticator login information.</param>
    /// <returns>The result of the login.</returns>
    Task<ResultStatus> ConfirmLoginTokenAsync(AuthenticatorLogin login);
    
    /// <summary>
    /// Resets the account's current authenticator key and disables two-factor authentication.
    /// </summary>
    /// <returns>Future result</returns>
    Task Reset();
}