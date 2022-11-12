namespace NetFusion.Identity.Infra.Repositories.Entities;

public class ClaimUserValue
{
    public int ClaimUserValueId { get; init; }
    public Guid ClaimScopeId { get; init; } 
    public string UserId { get; init; } = null!;
    public int ClaimTypeId { get; init; }
    public string Value { get; set; } = null!;

    public ClaimScope ClaimScope { get; init; } = null!;
    public UserIdentity User { get; init; } = null!;
    public ClaimType ClaimType { get; init; } = null!;
}