using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain.Registration.Entities;

/// <summary>
/// Entity containing information used to create a new account.
/// </summary>
public record UserRegistration 
{
    /// <summary>
    /// The email address to be associated with the account.
    /// </summary>
    public string Email { get; }
    
    /// <summary>
    /// The confirmed password to be used when creating the account.
    /// </summary>
    public ConfirmedPassword ConformedPassword { get; } 
    
    private UserRegistration (string email, ConfirmedPassword confirmedPassword)
    {
        Email = email;
        ConformedPassword = confirmedPassword;
    }

    /// <summary>
    /// Creates a validated registration entity used to create new account.
    /// </summary>
    /// <param name="email">The email address to be associated with the account.</param>
    /// <param name="password">The confirmed password to be used when creating the account.</param>
    /// <returns>List of validations or the created entity.</returns>
    public static (DomainValidations, UserRegistration?) Create(string email, ConfirmedPassword password)
    {   
        var validations = new DomainValidations();
        validations.ValidateEmail(email);
        validations.ValidatePassword(password);

        if (!validations.Valid)
        {
            return (validations, null);
        }

        var registration = new UserRegistration(email, password);
        return (DomainValidations.Empty, registration);
    }
}

public record ConfirmedPassword(string Chosen, string Verified);

