using NetFusion.Identity.Domain.Registration.Entities;

namespace NetFusion.Identity.Domain.Validation;

public static class ValidationExtensions
{
    public static bool ValidateEmail(this DomainValidations validation, string email)
    {
        if (validation == null) throw new ArgumentNullException(nameof(validation));
        
        return validation.ValidateFalse(string.IsNullOrWhiteSpace(email),
            ValidationLevel.Error,
            "Email address required");
    }
    
    public static bool ValidatePassword(this DomainValidations validation, string password)
    {
        if (validation == null) throw new ArgumentNullException(nameof(validation));
        
        return validation.ValidateFalse(string.IsNullOrWhiteSpace(password),
            ValidationLevel.Error,
            "Password required");
    }

    public static bool ValidatePassword(this DomainValidations validation, ConfirmedPassword password)
    {
        if (validation == null) throw new ArgumentNullException(nameof(validation));
        if (password == null) throw new ArgumentNullException(nameof(password));

        bool isSpecified = validation.ValidateTrue(
            !string.IsNullOrWhiteSpace(password.Chosen) && 
            !string.IsNullOrWhiteSpace(password.Verified), 
            ValidationLevel.Error,
            "Passwords are required");

        if (isSpecified)
        {
            return validation.ValidateTrue(password.Verified.Equals(password.Chosen), 
                ValidationLevel.Error, 
                "Passwords must match");
        }
        
        return false;
    }

    public static bool ValidateConfirmationToken(this DomainValidations validation, string token)
    {
        if (validation == null) throw new ArgumentNullException(nameof(validation));

        if (!string.IsNullOrWhiteSpace(token)) return true;
        validation.Add(ValidationLevel.Error, "Confirmation token not provided.");
        return false;
    }

}