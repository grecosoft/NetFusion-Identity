using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NetFusion.Identity.App.Repositories;
using NetFusion.Identity.App.Settings;
using NetFusion.Identity.Domain;
using Serilog.Context;

namespace NetFusion.Identity.App.Implementations;

/// <summary>
/// Provides central access to ASP.NET Core Identity managers and other
/// services common to service implementations.
/// </summary>
/// <typeparam name="TIdentity">Type containing information saved for a given user's registration.</typeparam>
public class AuthenticationContext<TIdentity> : IAuthenticationContext<TIdentity>
    where TIdentity : class, IUserIdentity
{
    private readonly IHttpContextAccessor _contextAccessor;
    
    public AuthenticationContext(IHttpContextAccessor contextAccessor,
        IOptions<IdentitySettings> options,
        UserManager<TIdentity> userManager,
        SignInManager<TIdentity> signinManager,
        IClaimsRepository claimsRepository)
    {
        _contextAccessor = contextAccessor;
        
        Settings = options.Value;
        UserManager = userManager;
        SigninManager = signinManager;
        ClaimsRepository = claimsRepository;
    }
    
    public IdentitySettings Settings { get; }
    public SignInManager<TIdentity> SigninManager { get; }
    public UserManager<TIdentity> UserManager { get; }
    public IClaimsRepository ClaimsRepository { get; }

    public Task<TIdentity> GetUserIdentity()
    {
        return UserManager.GetUserAsync(GetUserPrinciple());
    }

    public ClaimsPrincipal GetUserPrinciple()
    {
        var user = _contextAccessor.HttpContext?.User;
        if (user == null)
        {
            throw new NullReferenceException("HttpContext user not set");
        }

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            throw new InvalidOperationException("HttpContext user not authenticated");
        }

        return user;
    }

    public IDisposable GetLogContext(IUserIdentity user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        
        var claimsPrincipal = _contextAccessor.HttpContext?.User;

        var identityLogProps = new
        {
            user.Id,
            user.Email,
            user.EmailConfirmed,
            user.TwoFactorEnabled,
            user.LockoutEnabled,
            user.LockoutEnd,
            user.AccessFailedCount,
            Claims = claimsPrincipal?.Claims.Select(c => new
            {
                c.Issuer,
                c.Type,
                c.Value,
            }).ToArray()
        };

        return LogContext.PushProperty("UserIdentity", identityLogProps, destructureObjects: true);
    }
}