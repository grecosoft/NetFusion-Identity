namespace NetFusion.Identity.Domain.Claims.Entities
{
    /// <summary>
    /// Entity used to scope claims and roles.  A scope can be related to claims and roles associated
    /// with the dashboard, those that pertain globally to all dashboard applications and roles and claims
    /// that are application specific.
    /// </summary>
    public class IdentityScope
    {
        /// <summary>
        /// Identities the scope.
        /// </summary>
        public Guid ClaimScopeId { get; init; }
        
        /// <summary>
        /// The readable name of the scope.
        /// </summary>
        public string Name { get; init; } = null!;
        
        /// <summary>
        /// A key value used to identify the scope.
        /// </summary>
        public string? Key { get; init; }
        
        /// <summary>
        /// Description of the scope.
        /// </summary>
        public string? Description { get; init; }
    }
}