using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetFusion.Identity.Infra.Repositories.Entities;

namespace NetFusion.Identity.Infra;

public class ApplicationDbContext : IdentityDbContext<UserIdentity>
{
    public DbSet<ClaimScope> ClaimScopes { get; set; } = null!;
    public DbSet<ClaimType> ClaimTypes { get; set; } = null!;
    public DbSet<ClaimUserValue> ClaimUserValues { get; set; } = null!;
    public DbSet<RoleType> RoleTypes { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
           
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ClaimScope>().ToTable("ClaimScopes");
    }
}
