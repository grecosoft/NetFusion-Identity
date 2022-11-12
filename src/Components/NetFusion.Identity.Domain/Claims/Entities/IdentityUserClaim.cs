using System.Security.Claims;

namespace NetFusion.Identity.Domain.Claims.Entities;

/// <summary>
/// Entity used to specify a user's claim values for a given scope.
/// </summary>
public class IdentityUserClaim
{
    /// <summary>
    /// Value used to identify the value for a given claim.
    /// </summary>
    public int ClaimUserValueId { get; init; }
    
    /// <summary>
    /// Indicates the claim for which the value is defined.
    /// </summary>
    public int ClaimTypeId { get; init; }
    
    /// <summary>
    /// Indicates the scope in which the value of the claims applies.
    /// </summary>
    public string Scope { get; init; } = null!;
    
    /// <summary>
    /// The short name of the claim.
    /// </summary>
    public string Name { get; init; } = null!;
    
    /// <summary>
    /// The full name space of the claim.
    /// </summary>
    public string Namespace { get; init; } = null!;
    
    /// <summary>
    /// The value of the claim for the user.
    /// </summary>
    public string Value { get; init; } = null!;
    
    /// <summary>
    /// Reference to a built claim instance using the values defined by
    /// IdentityUserClaim.
    /// </summary>
    public Claim Claim { get; init; } =  null!;
}