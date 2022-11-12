using NetFusion.Identity.Domain.Authentication.Entities;

namespace NetFusion.Identity.Domain.Authentication.Services;

/// <summary>
/// Responsible for authentication related services.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Logs user into application using a set of specified credentials.
    /// </summary>
    /// <param name="login">The credentials to use for login.</param>
    /// <returns>The result status of the attempted login.</returns>
    Task<LoginStatus> LoginAsync(UserLogin login);

    /// <summary>
    /// Implements the changing of a logged-in authenticated user's password.
    /// </summary>
    /// <param name="changedPassword">The information used to change the account's password.</param>
    /// <returns>The result status of the attempted password change.</returns>
    Task<ResultStatus> ChangePassword(UserChangePassword changedPassword);

    /// <summary>
    /// Sends an email allowing the user to recover their account by changing their email.
    /// </summary>
    /// <param name="emailAddress">The email address of the account.</param>
    /// <returns>The result status of the attempted password recovery.</returns>
    Task<ResultStatus> SendPasswordRecovery(string emailAddress);

    /// <summary>
    /// Invoked by the user after receiving password recovery confirmation. 
    /// </summary>
    /// <param name="recovery">Contains the recovery token, their email address, and new password.</param>
    /// <returns></returns>
    Task<ResultStatus> ResetPasswordAsync(PasswordRecovery recovery);

    /// <summary>
    /// Logs the user out of the application. 
    /// </summary>
    /// <param name="forgetTwoFactorClient">Determines if the client should forget the
    /// user's last two-factor authentication.</param>
    /// <returns>Future Result.</returns>
    Task LogOutAsync(bool forgetTwoFactorClient = false);
}