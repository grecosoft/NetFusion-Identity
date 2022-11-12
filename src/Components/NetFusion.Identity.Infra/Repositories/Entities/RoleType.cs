namespace NetFusion.Identity.Infra.Repositories.Entities;

public class RoleType
{
    public int RoleTypeId { get; init; }
    public Guid ClaimScopeId { get; init; }
    public string Value { get; init; } = null!;
    public string? Description { get; set; }
}