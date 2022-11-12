using System.Text.RegularExpressions;
using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain.TwoFactor.Entities;

/// <summary>
/// Entity containing information when a user is challenged and must
/// provided a token generated from an authenticator.
/// </summary>
public record AuthenticatorLogin
{
    /// <summary>
    /// The token generated from an authenticator associated with the account.
    /// </summary>
    public string Token { get; private init; } = string.Empty;
    
    /// <summary>
    /// Determines if the successful authenticator login should be remembered. 
    /// </summary>
    public bool RememberClient { get; private init; }

    /// <summary>
    /// Creates an entity used to login with an authenticator. 
    /// </summary>
    /// <param name="token">The token generated from an authenticator associated with the account.</param>
    /// <param name="rememberClient">Determines if the successful authenticator login should be remembered. </param>
    /// <returns>Validations or the created entity.</returns>
    public static (DomainValidations, AuthenticatorLogin?) Create(string token, bool rememberClient)
    {
        var validation = new DomainValidations();

        validation.ValidateFalse(string.IsNullOrWhiteSpace(token), ValidationLevel.Error,
            "Token not specified.");

        if (!validation.Valid)
        {
            return (validation, null);
        }

        var login = new AuthenticatorLogin
        {
            Token = Regex.Replace(token, @"\s", ""),
            RememberClient = rememberClient
        };

        return (validation, login);
    }
}

