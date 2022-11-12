namespace NetFusion.Identity.Infra.Repositories.Entities;

public class ClaimScope
{
    public Guid ClaimScopeId { get; init; }
    public string Name { get; init; } = null!;
    public string? Key { get; set; }
    public string? Description { get; set; }
}