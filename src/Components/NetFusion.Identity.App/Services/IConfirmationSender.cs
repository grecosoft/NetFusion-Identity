using NetFusion.Identity.Domain;

namespace NetFusion.Identity.App.Services;

/// <summary>
/// Service responsible for sending tokens to users required confirming specific actions.
/// </summary>
public interface IConfirmationSender
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userIdentity">The user for which the token is to be sent.</param>
    /// <param name="confirmationToken">Token used to confirm the user's account.</param>
    /// <returns>Future Result.</returns>
    Task SendAccountConfirmationAsync(IUserIdentity userIdentity, string confirmationToken);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userIdentity">The user for which the token is to be sent.</param>
    /// <param name="recoveryToken">Token used to recover the user's account password.</param>
    /// <returns>Future Result.</returns>
    Task SendPasswordRecoveryAsync(IUserIdentity userIdentity, string recoveryToken);
}