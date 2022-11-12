using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain.Authentication.Entities;

/// <summary>
/// Entity used to authenticate a user.
/// </summary>
public record UserLogin
{
    /// <summary>
    /// The user's email address to login with.
    /// </summary>
    public string Email { get; }
    
    /// <summary>
    /// The user's password to login with.
    /// </summary>
    public string Password { get; }
    
    /// <summary>
    /// Indicates that when the user closes their browser they will remain logged the
    /// next time the open the browser.
    /// </summary>
    public bool RememberClient { get; }

    private UserLogin(string email, string password, bool rememberClient)
    {
        Email = email;
        Password = password;
        RememberClient = rememberClient;
    }
    
    /// <summary>
    /// Used to create a request to authenticate a user.
    /// </summary>
    /// <param name="email">The user's email address to login with.</param>
    /// <param name="password">The user's password to login with.</param>
    /// <param name="rememberClient">Indicates that when the user closes their browser they
    /// will remain logged the next time the open the browser.</param>
    /// <returns></returns>
    public static (DomainValidations, UserLogin?) Create(string email, string password, bool rememberClient)
    {   
        var validations = new DomainValidations();
        validations.ValidateEmail(email);
        validations.ValidatePassword(password);

        if (!validations.Valid)
        {
            return (validations, null);
        }

        var registration = new UserLogin(email, password, rememberClient);
        return (DomainValidations.Empty, registration);
    }
}