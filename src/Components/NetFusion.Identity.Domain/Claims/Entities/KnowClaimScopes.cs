namespace NetFusion.Identity.Domain.Claims.Entities;

/// <summary>
/// Claim scopes defined by the identity client used to identity a set
/// of related claims.
/// </summary>
public static class KnowClaimScopes
{
    /// <summary>
    /// Identities the claims associated with the Identity Client.
    /// </summary>
    public const string DashboardKey = "dashboard";
    
    /// <summary>
    /// Identifies claims that are associated with all applications managed by the dashboard.
    /// </summary>
    public const string ApplicationGlobalKey = "application-global";
}