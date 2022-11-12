namespace NetFusion.Identity.Domain.Registration.Services;

/// <summary>
/// Service responsible for creating tokens used to authenticate requests
/// completed when user is not logged in and authenticated.  These tokens
/// are provided to the user and used to complete the action.
/// </summary>
public interface IConfirmationService
{
    /// <summary>
    /// Provides a token to the user to confirm they are the owner
    /// of the email address.
    /// </summary>
    /// <param name="userIdentity">The identity to be provided the token.</param>
    /// <returns>Future result</returns>
    Task SendAccountConfirmationAsync(IUserIdentity userIdentity);
    
    /// <summary>
    /// Provides a token to the user allowing them to reset the password of
    /// their account without having to be logged in.
    /// </summary>
    /// <param name="userIdentity">The identity to provide the token.</param>
    /// <returns>Future result</returns>
    Task SendPasswordRecoveryAsync(IUserIdentity userIdentity);
}