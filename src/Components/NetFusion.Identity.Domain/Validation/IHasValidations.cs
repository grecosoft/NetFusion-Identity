namespace NetFusion.Identity.Domain.Validation;

/// <summary>
/// Represents a set of validates associated with an entity.
/// </summary>
public interface IHasValidations
{
    /// <summary>
    /// Contains a set of validations.
    /// </summary>
    DomainValidations Validations { get; }
}