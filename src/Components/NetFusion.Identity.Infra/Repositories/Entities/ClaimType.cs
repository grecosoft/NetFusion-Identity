namespace NetFusion.Identity.Infra.Repositories.Entities;

public class ClaimType
{
    public int ClaimTypeId { get; set; }
    public string Name { get; set; } = null!;
    public string Namespace { get; set; } = null!;
    public string? Description { get; set; }
}