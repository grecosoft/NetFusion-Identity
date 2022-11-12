using NetFusion.Identity.Domain.Authentication.Entities;

namespace NetFusion.Identity.Domain.Authentication.Services;

/// <summary>
/// Responsible for generating application specific tokens scoped to a specific application.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a token scoped to a specific application.
    /// </summary>
    /// <param name="appScopeId">Identifies the scope of the application.</param>
    /// <returns>JWT</returns>
    Task<TokenStatus> CreateJwtToken(Guid appScopeId);
}