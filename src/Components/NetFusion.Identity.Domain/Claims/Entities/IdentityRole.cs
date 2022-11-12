namespace NetFusion.Identity.Domain.Claims.Entities;

/// <summary>
/// Entity defining a specific type of claim used to determine a user's
/// assigned roles.
/// </summary>
public class IdentityRole
{
    /// <summary>
    /// Identity value of the role.
    /// </summary>
    public int RoleTypeId { get; init; }
    
    /// <summary>
    /// The value of the role assigned to a role claim for a given user and scope.
    /// </summary>
    public string Value { get; init; } = null!;
    
    /// <summary>
    /// Description of the role indicting the user's privileges.
    /// </summary>
    public string? Description { get; init; }
}