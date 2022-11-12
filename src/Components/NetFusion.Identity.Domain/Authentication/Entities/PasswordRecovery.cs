using NetFusion.Identity.Domain.Registration.Entities;
using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain.Authentication.Entities;

/// <summary>
/// Entity for a request used to recover an account's password.
/// </summary>
public record PasswordRecovery
{
    /// <summary>
    /// The email associated with the account.
    /// </summary>
    public string Email { get; }
    
    /// <summary>
    /// The user's selected updated password.
    /// </summary>
    public ConfirmedPassword Password { get; }
    
    /// <summary>
    /// The token sent to the user used to authenticate the request.
    /// </summary>
    public string Token { get; }

    private PasswordRecovery(string email, ConfirmedPassword password, string token)
    {
        Email = email;
        Password = password;
        Token = token;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email">The email associated with the account.</param>
    /// <param name="password">The user's selected updated password.</param>
    /// <param name="token">The token sent to the user used to authenticate the request.</param>
    /// <returns>List of domain validations the create entity if all inputs are valid.  Otherwise null is
    /// returned.</returns>
    public static (DomainValidations, PasswordRecovery?) Create(string email, ConfirmedPassword password, string token)
    {
        var validation = new DomainValidations();
        validation.ValidateEmail(email);
        validation.ValidatePassword(password);
        validation.ValidateConfirmationToken(token);

        if (!validation.Valid)
        {
            return (DomainValidations.Empty, null);
        }

        var recovery = new PasswordRecovery(email, password, token);
        return (DomainValidations.Empty, recovery);
    }
  
}