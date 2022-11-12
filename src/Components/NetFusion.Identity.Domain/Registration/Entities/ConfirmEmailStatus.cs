using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain.Registration.Entities;

/// <summary>
/// Entity used to provide the status result of confirming an email.
/// </summary>
public class ConfirmEmailStatus : IHasValidations
{
    public DomainValidations Validations { get; } = new();

    /// <summary>
    /// Indicates that the account is know for an existing user.  For the confirmation to succeed,
    /// this value must be true.
    /// </summary>
    public bool ExistingUser { get; }
    
    /// <summary>
    /// Indicates if there is a pending confirmation for the account.  For the confirmation to
    /// succeed, this value must be false.
    /// </summary>
    public bool PendingConfirmation { get; }

    public ConfirmEmailStatus()
    {
        ExistingUser = true;
        PendingConfirmation = true;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="existingUser">Indicates that the account is know for an existing user.</param>
    /// <param name="pendingConfirmation">Indicates if there is a pending confirmation for the account.</param>
    public ConfirmEmailStatus(bool existingUser, bool pendingConfirmation)
    {
        ExistingUser = existingUser;
        PendingConfirmation = pendingConfirmation;
    }
    
    /// <summary>
    /// Indicates if the result is valid based on the result's properties and validations.
    /// </summary>
    public bool Valid => ExistingUser && PendingConfirmation  && !Validations.Items.Any();
    
    /// <summary>
    /// Indicates if the result is not valid based on the result's properties and validations.
    /// </summary>
    public bool NotValid => !Valid;
}
 