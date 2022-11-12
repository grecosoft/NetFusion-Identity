using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Domain;

/// <summary>
/// Basic result status containing only validations and a boolean
/// result indicating if successful.
/// </summary>
public class ResultStatus : IHasValidations
{
    private readonly bool _isSuccess = true;
    
    /// <summary>
    /// Status is successful and contains no validations.
    /// </summary>
    public bool Valid => _isSuccess && !Validations.Items.Any();
    
    /// <summary>
    /// Status is not successful or contains validations.
    /// </summary>
    public bool NotValid => !Valid;

    /// <summary>
    /// List of validations.
    /// </summary>
    public DomainValidations Validations { get; } = new();

    public ResultStatus()
    {

    }
    
    /// <summary>
    /// Creates a result status.
    /// </summary>
    /// <param name="isSuccess">Indicates if the status is successful.</param>
    public ResultStatus(bool isSuccess)
    {
        _isSuccess = isSuccess;
    }
}