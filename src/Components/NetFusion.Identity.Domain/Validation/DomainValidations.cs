namespace NetFusion.Identity.Domain.Validation;

/// <summary>
/// Class used to record domain validations. 
/// </summary>
public class DomainValidations 
{
    private readonly List<ValidationResult> _validations = new();
    
    /// <summary>
    /// List of associated validations.
    /// </summary>
    public IReadOnlyCollection<ValidationResult> Items { get; }
    
    /// <summary>
    /// Indicates there are no recorded validations.
    /// </summary>
    public bool Valid => !_validations.Any();
    
    /// <summary>
    /// Indicates validations have been recorded.
    /// </summary>
    public bool NotValid => _validations.Any();
    
    /// <summary>
    /// Creates an empty validation list.
    /// </summary>
    public static DomainValidations Empty => new ();

    public DomainValidations()
    {
        Items = _validations;
    }

    /// <summary>
    /// Records a new validation.
    /// </summary>
    /// <param name="level">The validation level.</param>
    /// <param name="message">The message for the validation.</param>
    public void Add(ValidationLevel level, string message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        
        _validations.Add(new ValidationResult(level, message));
    }

    /// <summary>
    /// Validates that the value is true.  If not true, the validation is recorded.
    /// </summary>
    /// <param name="value">The value to check if true.</param>
    /// <param name="level">The associated validation level.</param>
    /// <param name="message">The validation message.</param>
    /// <returns>The result of the validation.</returns>
    public bool ValidateTrue(bool value, ValidationLevel level, string message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        
        if (! value)
        {
            _validations.Add(new ValidationResult(level, message));
        }

        return value;
    }
    
    /// <summary>
    /// Validates that the value is false.  If true, the validation is recorded.
    /// </summary>
    /// <param name="value">The value to check if false.</param>
    /// <param name="level">The associated validation level.</param>
    /// <param name="message">The validation message.</param>
    /// <returns>The result of the validation.</returns>
    public bool ValidateFalse(bool value, ValidationLevel level, string message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        
        if (value)
        {
            _validations.Add(new ValidationResult(level, message));
        }

        return !value;
    }

    /// <summary>
    /// Validates that the passed value is not null.  If null, the validation is recorded.
    /// </summary>
    /// <param name="value">The value to check for null.</param>
    /// <param name="level">The level of the validation.</param>
    /// <param name="message">The validation message.</param>
    /// <returns>The result of the validation.</returns>
    public bool ValidateNotNull(object? value, ValidationLevel level, string message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        if (value == null)
        {
            _validations.Add(new ValidationResult(level, message));
        }

        return value != null;
    }  
    
    /// <summary>
    /// Appends a list of validations.
    /// </summary>
    /// <param name="validations">The list of validation to be added.</param>
    public void Append(IEnumerable<ValidationResult> validations)
    {
        if (validations == null) throw new ArgumentNullException(nameof(validations));
        _validations.AddRange(validations);
    }
}