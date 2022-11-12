using NetFusion.Identity.Domain.Claims.Entities;

namespace NetFusion.Identity.App.Repositories;

/// <summary>
/// Retrieves and updates claim associated data.
/// </summary>
public interface IClaimsRepository
{
    /// <summary>
    /// Returns all defined scopes to which claims can be associated.
    /// </summary>
    /// <returns>List of all defined scopes.</returns>
    Task<IdentityScope[]> ReadAllScopesAsync();
    
    /// <summary>
    /// Returns a list of all defined claims for which a value can be defined for
    /// a given user and scope.
    /// </summary>
    /// <returns>List of all possible claims.</returns>
    Task<IdentityClaim[]> ReadAllClaimsAsync();

    /// <summary>
    /// Returns a list of all claim values defined for a user within a given scope.
    /// </summary>
    /// <param name="scopeId">The scope associated with the claim.</param>
    /// <param name="userId">The user for which claims are to be returned.</param>
    /// <returns>List of claims with the user's associated value.</returns>
    Task<IdentityUserClaim[]> ReadUserClaimsAsync(Guid scopeId, string userId);
    
    /// <summary>
    /// Returns a list of all claim values defined for a user within a given scope.
    /// </summary>
    /// <param name="scopeKey">The scope associated with the claim.</param>
    /// <param name="userId">The user for which claims are to be returned.</param>
    /// <returns>List of claims with the user's associated value.</returns>
    Task<IdentityUserClaim[]> ReadUserClaimsAsync(string scopeKey, string userId);
    
    /// <summary>
    /// Returns a list of all user defined claims with their associated value.
    /// </summary>
    /// <param name="userId">The user for which claims are to be returned.</param>
    /// <returns>List of claims with the user's associated value.</returns>
    Task<IdentityUserClaim[]> ReadAllUserClaimsAsync(string userId);
    
    /// <summary>
    /// Returns all roles associated with a specific scope.
    /// </summary>
    /// <param name="scopeId">The scope associated with the role.</param>
    /// <returns>List of all roles defined for the scope.</returns>
    Task<IdentityRole[]> ReadRolesAsync(Guid scopeId);

    /// <summary>
    /// Records a user's specific claim value for a given scope.
    /// </summary>
    /// <param name="claimScopeId">The scope in which the claim applies.</param>
    /// <param name="claimTypeId">Indicates the type of claim for the value.</param>
    /// <param name="userId">The user associated with the claim value.</param>
    /// <param name="value">The value for the claim.</param>
    /// <returns>Future result.</returns>
    Task AddUserClaimAsync(string claimScopeId, int claimTypeId, string userId, string value);
    
    /// <summary>
    /// Updates an existing user defined claim value.
    /// </summary>
    /// <param name="claimUserValueId">The specific user claim value to be updated.</param>
    /// <param name="value">The new claim value.</param>
    /// <returns>Future result.</returns>
    Task UpdateUserClaimAsync(int claimUserValueId, string value);
    
    /// <summary>
    /// Deletes an existing user defined claim value.
    /// </summary>
    /// <param name="userClaimValueId">The specific user claim value to be updated.</param>
    /// <returns>Future result.</returns>
    Task DeleteUserClaimAsync(int userClaimValueId);
}