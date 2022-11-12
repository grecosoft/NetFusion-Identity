using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetFusion.Identity.App.Settings;
using NetFusion.Identity.Domain;
using NetFusion.Identity.Domain.Authentication.Entities;
using NetFusion.Identity.Domain.Authentication.Services;
using NetFusion.Identity.Domain.Claims.Entities;

namespace NetFusion.Identity.App.Implementations;

/// <summary>
/// Adds to the authenticated identity claims for a given application's scope.
/// </summary>
/// <typeparam name="TIdentity">Type containing information saved for a given user's registration.</typeparam>
public class TokenService<TIdentity> : ITokenService
    where TIdentity : class, IUserIdentity
{
    private readonly ILogger _logger;
    private readonly IAuthenticationContext<TIdentity> _authentication;

    public TokenService(
        ILoggerFactory loggerFactory, 
        IAuthenticationContext<TIdentity> authentication)
    {
        _logger = loggerFactory.CreateLogger("AuthenticationService");
        _authentication = authentication;
    }
    
    public async Task<TokenStatus> CreateJwtToken(Guid appScopeId)
    {
        if (appScopeId == Guid.Empty)
        {
            throw new ArgumentException("Value not specified", nameof(appScopeId));
        }
        
        IdentitySettings settings = _authentication.Settings;
        ClaimsPrincipal principal = _authentication.GetUserPrinciple();

        ClaimsIdentity appClaimsIdentity = await CreateApplicationScopedIdentity(principal, appScopeId);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = appClaimsIdentity,
            Expires = DateTime.UtcNow.AddMinutes(settings.JwtExpireMinutes),
            SigningCredentials = new SigningCredentials(
                GetSecurityKey(settings), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
       _logger.LogInformation("JWT token generated for {Email}", appClaimsIdentity.Name);

        return new TokenStatus(GenerateToken(tokenDescriptor));
    }

    private async Task<ClaimsIdentity> CreateApplicationScopedIdentity(ClaimsPrincipal principal, Guid appScopeId)
    {
        string authenticationType = appScopeId.ToString();
        
        string? userId = GetNamedIdentifier(principal);
        if (userId == null)
        {
            return new ClaimsIdentity(authenticationType);
        }

        // All claims for non-dashboard principle identities, are added
        // to the generated application specific claims-identity.
        var baseClaims = principal.Identities
            .Where(i => i.AuthenticationType != KnowClaimScopes.DashboardKey)
            .SelectMany(i => i.Claims)
            .ToList();
        
        // Read application specific claims and merge with the base set of claims:
        var applicationClaims = (await _authentication.ClaimsRepository.ReadUserClaimsAsync(appScopeId, userId))
            .Select(uc => uc.Claim);
        
        baseClaims.AddRange(applicationClaims);
        
        return new ClaimsIdentity(baseClaims, authenticationType);
    }
    
    private string? GetNamedIdentifier(ClaimsPrincipal principal)
    {
        ClaimsIdentity? identity = principal.Identities.FirstOrDefault(
            i => i.AuthenticationType == IdentityConstants.ApplicationScheme);

        if (identity == null)
        {
            _logger.LogError("Could not find Claims Identity for {ApplicationType}", IdentityConstants.ApplicationScheme);
            return null;
        }
        
        Claim? namedClaim =  identity.FindFirst(ClaimTypes.NameIdentifier);
        if (namedClaim == null)
        {
            _logger.LogError("The claim {ClaimType} not found on Claims Identity for {ApplicationType}.", 
                ClaimTypes.NameIdentifier,
                IdentityConstants.ApplicationScheme);
        }

        return namedClaim?.Value;
    }

    private static SymmetricSecurityKey GetSecurityKey(IdentitySettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.JwtSecurityKey))
        {
            throw new InvalidOperationException("Security key not configured");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtSecurityKey));
    }

    private static string GenerateToken(SecurityTokenDescriptor tokenDescriptor)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        SecurityToken secToken = jwtTokenHandler.CreateToken(tokenDescriptor);
        return jwtTokenHandler.WriteToken(secToken);
    }
}