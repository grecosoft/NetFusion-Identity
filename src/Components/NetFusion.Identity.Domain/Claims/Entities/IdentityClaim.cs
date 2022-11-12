namespace NetFusion.Identity.Domain.Claims.Entities;

/// <summary>
/// Entity defining a claim for which a value can be associated with
/// a user for a given scope.
/// </summary>
public class IdentityClaim
{
    /// <summary>
    /// The identity value of the claim type.
    /// </summary>
    public int ClaimTypeId { get; init; }
    
    /// <summary>
    /// The short name of the claim for presentation.
    /// </summary>
    public string Name { get; init; } = null!;
    
    /// <summary>
    /// The publicly defined name used to identify the claim.
    /// </summary>
    public string Namespace { get; init; } = null!;
    
    /// <summary>
    /// Description of the namespace.
    /// </summary>
    public string? Description { get; init; }
}