using NetFusion.Identity.Domain.Registration.Entities;
using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain.Authentication.Entities;

/// <summary>
/// Entity used to change password for a logged in and authenticated user.
/// </summary>
public record UserChangePassword
{
    /// <summary>
    /// The user's current password.
    /// </summary>
    public string CurrentPassword { get; }
    
    /// <summary>
    /// The user's selected updated password.
    /// </summary>
    public ConfirmedPassword ConfirmedPassword { get; }

    private UserChangePassword(string currentPassword, ConfirmedPassword confirmedPassword)
    {
        CurrentPassword = currentPassword;
        ConfirmedPassword = confirmedPassword;
    }

    /// <summary>
    /// Entity used to change password for a logged in and authenticated user.
    /// </summary>
    /// <param name="currentPassword">The user's current password.</param>
    /// <param name="confirmedPassword">The user's selected updated password.</param>
    /// <returns>List of validation issues or the created entity.</returns>
    public static (DomainValidations, UserChangePassword?) Create(string currentPassword, ConfirmedPassword confirmedPassword)
    {
        var validation = new DomainValidations();

        validation.ValidateEmail(currentPassword);
        validation.ValidatePassword(confirmedPassword);

        if (validation.Valid)
        {
            var passwordChange = new UserChangePassword(currentPassword, confirmedPassword);
            return (validation, passwordChange);
        }

        return (DomainValidations.Empty, null);
    }
}