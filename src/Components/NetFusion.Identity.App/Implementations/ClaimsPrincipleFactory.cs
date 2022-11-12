using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NetFusion.Identity.App.Repositories;
using NetFusion.Identity.Domain;
using NetFusion.Identity.Domain.Claims.Entities;

namespace NetFusion.Identity.App.Implementations;

/// <summary>
/// Creates a ClaimsPrinciple containing a ClaimIdentity for each set of claims
/// associated with an added scope.  The claims associated with the Dashboard and
/// common across all applications are added.
/// </summary>
/// <typeparam name="TIdentity">Type containing information saved for a given user's registration.</typeparam>
public class ClaimsPrincipleFactory<TIdentity> : UserClaimsPrincipalFactory<TIdentity>
    where TIdentity : class, IUserIdentity
{
    private readonly IClaimsRepository _claimsRepository;
    
    public ClaimsPrincipleFactory(
        UserManager<TIdentity> userManager,
        IClaimsRepository claimsRepository,
        IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
    {
        _claimsRepository = claimsRepository;
    }

    public override async Task<ClaimsPrincipal> CreateAsync(TIdentity user)
    {
        var principal = await base.CreateAsync(user);

        await AddScopedUserClaims(principal, KnowClaimScopes.DashboardKey, user.Id);
        await AddScopedUserClaims(principal, KnowClaimScopes.ApplicationGlobalKey, user.Id);
        return principal;
    }
    
    private async Task AddScopedUserClaims(ClaimsPrincipal principal, string scopeKey, string userId)
    {
        var scopedClaims = (await _claimsRepository.ReadUserClaimsAsync(scopeKey, userId))
            .Select(cv => cv.Claim);
        
        principal.AddIdentity(new ClaimsIdentity(scopedClaims, scopeKey));
    }
}