using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain.TwoFactor.Entities;

/// <summary>
/// Entity containing the status of logging in with a two-factor
/// recovery code.
/// </summary>
public class RecoveryLoginStatus : IHasValidations
{
    private readonly bool _succeeded = true;
    
    /// <summary>
    /// Associated validations.
    /// </summary>
    public DomainValidations Validations { get; } = new();

    /// <summary>
    /// Indicates that the login was successful.
    /// </summary>
    public bool Valid => _succeeded && !Validations.Items.Any();
    
    /// <summary>
    /// Indicates that the login was not successful.
    /// </summary>
    public bool NotValid => !Valid;
    
    /// <summary>
    /// Indicates the user has the minimal number of remaining recovery codes
    /// and should be notified to create a new list. 
    /// </summary>
    public bool LowRecoveryCodeCount { get; }

    public RecoveryLoginStatus()
    {
        
    }
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="succeeded">Indicates if the recovery code login was successful.</param>
    /// <param name="lowRecoveryCodeCount">Indicates the minimum number of recovery codes remain.</param>
    public RecoveryLoginStatus(bool succeeded, bool lowRecoveryCodeCount)
    {
        _succeeded = succeeded;
        LowRecoveryCodeCount = lowRecoveryCodeCount;
    }
}