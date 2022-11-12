using Microsoft.AspNetCore.Identity;
using NetFusion.Identity.Domain;

namespace NetFusion.Identity.Client.Services;

public class LoginInfoService<TIdentity> : ILoginInfoService
    where TIdentity : class, IUserIdentity
{
    public bool IsLoggedIn { get; }
    public string Name { get; set; }
    
    public bool IsAdmin { get; }
    
    public LoginInfoService(
        IHttpContextAccessor contextAccessor,
        SignInManager<TIdentity> signInManager)
    {
        var user = contextAccessor.HttpContext?.User;
        IsLoggedIn = user != null && signInManager.IsSignedIn(user);
        Name = user == null ? string.Empty : user.Identity?.Name ?? string.Empty;
        IsAdmin = IsLoggedIn && (user?.IsInRole("Admin") ?? false);
    }
}