namespace NetFusion.Identity.Domain.Validation;

/// <summary>
/// Validation levels.
/// </summary>
public enum ValidationLevel
{
    Warning = 1,
    Error = 2
}

/// <summary>
/// used to record a validation associated with an entity.
/// </summary>
/// <param name="Level">The importance of the validation.</param>
/// <param name="Message">Message associated with validation.</param>
public record ValidationResult(ValidationLevel Level, string Message);



