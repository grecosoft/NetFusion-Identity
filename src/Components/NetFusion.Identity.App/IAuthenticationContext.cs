using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using NetFusion.Identity.App.Repositories;
using NetFusion.Identity.App.Settings;
using NetFusion.Identity.Domain;

namespace NetFusion.Identity.App;

/// <summary>
/// Service providing access to common authentication services and properties.
/// </summary>
/// <typeparam name="TIdentity"></typeparam>
public interface IAuthenticationContext<TIdentity>
    where TIdentity : class, IUserIdentity
{
    /// <summary>
    /// Settings provided by the host application.
    /// </summary>
    IdentitySettings Settings { get; }

    /// <summary>
    /// Reference to ASP.NET Core Identity's sign-in manager.
    /// </summary>
    SignInManager<TIdentity> SigninManager { get; }
    
    /// <summary>
    /// Reference to ASP.NET Core Identity's user manager.
    /// </summary>
    UserManager<TIdentity> UserManager { get; }

    /// <summary>
    /// Returns reference to the logged in user's claim principle.
    /// If the user is not authenticated, an exception is raised.
    /// </summary>
    /// <returns>Claim Principle of authenticated user.</returns>
    ClaimsPrincipal GetUserPrinciple();
    
    /// <summary>
    /// Returns the identity information stored for the currently
    /// authenticated user.  If the user is not authenticated, an
    /// exception is raised.
    /// </summary>
    /// <returns>Stored identity data for the authenticated user.</returns>
    Task<TIdentity> GetUserIdentity();

    /// <summary>
    /// Returns a log context containing information from the passed user
    /// identity.  This is used to associate details with log messages
    /// written by Serilog.
    /// </summary>
    /// <param name="user">The current user-identity.</param>
    /// <returns>Disposable reference used to add log details to messages
    /// written within the context of the disposable object.</returns>
    IDisposable GetLogContext(IUserIdentity user);
    
    /// <summary>
    /// Reference to the repository used to query user's claims.
    /// </summary>
    IClaimsRepository ClaimsRepository { get; }
}