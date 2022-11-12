using NetFusion.Identity.Domain.Registration.Entities;

namespace NetFusion.Identity.Domain.Registration.Services;

/// <summary>
/// Responsible for creating and confirming accounts.
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Creates a new account.
    /// </summary>
    /// <param name="registration">Entity containing the details of the account to be created.</param>
    /// <returns>The status result of the registration.</returns>
    Task<RegistrationStatus> RegisterAsync(UserRegistration registration);

    /// <summary>
    /// Confirms an existing created account.
    /// </summary>
    /// <param name="confirmation">Entity containing details used to confirm the account.</param>
    /// <returns>The status result of the confirmation.</returns>
    Task<ConfirmEmailStatus> ConfirmEmailAsync(AccountConfirmation confirmation);

    /// <summary>
    /// Resends an email confirmation for an account.
    /// </summary>
    /// <param name="email">The email address associated with the account.</param>
    /// <returns>The status result of resending the confirmation.</returns>
    Task<ConfirmEmailStatus> ResendEmailConfirmationAsync(string email);
}