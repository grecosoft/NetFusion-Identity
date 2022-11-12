using NetFusion.Identity.Domain.TwoFactor.Entities;

namespace NetFusion.Identity.Domain.TwoFactor.Services;

/// <summary>
/// Service used to manage two-factor configuration.
/// </summary>
public interface ITwoFactorService
{
    /// <summary>
    /// Returns the current account's two-factor configuration.
    /// </summary>
    /// <param name="includeRecoveryCodes">Indicates that the current list
    /// of recovery codes should be included.</param>
    /// <returns>Two-Factor configuration details.</returns>
    Task<Configuration> GetConfiguration(bool includeRecoveryCodes = false);
    
    /// <summary>
    /// Disables two-factor authentication.
    /// </summary>
    /// <returns>The result status of disabling.</returns>
    Task<ResultStatus> DisableAsync();
    
    /// <summary>
    /// Completes a two-factor login using a recovery code.
    /// </summary>
    /// <param name="recoveryCode">Recovery code associated with account.</param>
    /// <returns>The result of the recovery code login.</returns>
    Task<RecoveryLoginStatus> ConfirmLoginRecoveryTokenAsync(string recoveryCode);
    
    /// <summary>
    /// Regenerates the current list of recovery codes associated with the account.
    /// </summary>
    /// <returns>Future result.</returns>
    Task RegenerateRecoveryCodes();
}


