using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain.Registration.Entities;

/// <summary>
/// Entity created to confirm a created account.
/// </summary>
public record AccountConfirmation
{
    /// <summary>
    /// The email for the account being confirmed.
    /// </summary>
    public string Email { get; }
    
    /// <summary>
    /// The token sent to the account's associated email address used to authenticate the action. 
    /// </summary>
    public string Token { get; }

    private AccountConfirmation(string email, string token)
    {
        Email = email;
        Token = token;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <param name="token"></param>
    /// <returns>List of domain validations and the created entity.  If the provided values fail validation,
    /// the null will be returned for the entity.</returns>
    public static (DomainValidations, AccountConfirmation?) Create(string email, string token)
    {
        var validations = new DomainValidations();
        validations.ValidateEmail(email);
        validations.ValidateConfirmationToken(token);

        if (!validations.Valid)
        {
            return (validations, null);
        }

        var confirmation = new AccountConfirmation(email, token);
        return (DomainValidations.Empty, confirmation);
    }
}