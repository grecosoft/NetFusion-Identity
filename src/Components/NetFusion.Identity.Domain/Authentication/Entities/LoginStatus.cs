using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain.Authentication.Entities;

/// <summary>
/// Entity containing the details of a user's login attempt.
/// </summary>
public class LoginStatus : IHasValidations
{
    private readonly bool _succeeded = true;
    
    public DomainValidations Validations { get; } = new();
    public bool Valid => _succeeded && Validations.Valid;
    public bool NotValid => !Valid;
    
    /// <summary>
    /// Indicates that the current account is locked.
    /// </summary>
    public bool LockedOut { get; }
    
    /// <summary>
    /// Indicates that two-factor authentication is enabled and the
    /// user must provide a second form of authentication to their
    /// email and password.
    /// </summary>
    public bool RequiredTwoFactor { get; }
    
    /// <summary>
    /// Indicates the user was not allowed to login the application.
    /// This is normally due to invalid credentials. 
    /// </summary>
    public bool NotAllowed { get; }
    
    /// <summary>
    /// Indicates that they can't login since the email address associated
    /// with their account has not been confirmed.
    /// </summary>
    public bool EmailNotConfirmed { get; }

    public LoginStatus()
    {
        
    }
    
    public LoginStatus(
        bool succeeded, 
        bool lockedOut, 
        bool requiredTwoFactor,
        bool notAllowed, 
        bool emailNotConfirmed)
    {
        _succeeded = succeeded;
        LockedOut = lockedOut;
        RequiredTwoFactor = requiredTwoFactor;
        NotAllowed = notAllowed;
        EmailNotConfirmed = emailNotConfirmed;
    }
    
    /// <summary>
    /// Indicates that the user was denied access due to invalid credentials. 
    /// </summary>
    public bool InvalidCredentials =>
        !_succeeded && !LockedOut && !RequiredTwoFactor && !NotAllowed;
}
