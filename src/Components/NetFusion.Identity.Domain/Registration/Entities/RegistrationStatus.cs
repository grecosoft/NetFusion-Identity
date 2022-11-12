using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain.Registration.Entities;

/// <summary>
/// Entity containing the status of registering a new user.
/// </summary>
public class RegistrationStatus : IHasValidations
{
    public DomainValidations Validations { get; } = new();

    /// <summary>
    /// Indicates that the account can't be created since it is already associated
    /// with an existing email.  This must be false for the registration to succeed.
    /// </summary>
    public bool ExistingUser { get; }
    
    /// <summary>
    /// If there is an existing account with the specified email address, this
    /// indicates if the account's email has been confirmed.
    /// </summary>
    public bool PendingConfirmation { get; }
    
    /// <summary>
    /// The identity value for the created account if successful.
    /// </summary>
    public string? Id { get; private set; }
    
    /// <summary>
    /// The email address of the created account if successful.
    /// </summary>
    public string? Email { get; private set; }
    
    /// <summary>
    /// Creates a new registration entity.
    /// </summary>
    /// <param name="existingUser">Indicates account already associated with email.</param>
    /// <param name="pendingConfirmation">Indicates that the existing account has a pending confirmation.</param>
    public RegistrationStatus(bool existingUser, bool pendingConfirmation)
    {
        ExistingUser = existingUser;
        PendingConfirmation = pendingConfirmation;
    }

    public void SetUserIdentity(string id, string email)
    {
        Id = id;
        Email = email;
    }

    public bool Valid => !ExistingUser && !PendingConfirmation  && !Validations.Items.Any();
}
 