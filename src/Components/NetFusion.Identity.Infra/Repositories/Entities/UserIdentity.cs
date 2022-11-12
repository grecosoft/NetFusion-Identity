using Microsoft.AspNetCore.Identity;
using NetFusion.Identity.Domain;

namespace NetFusion.Identity.Infra.Repositories.Entities;

/// <summary>
/// Confirms the existing Entity Framework defined user class to
/// the generic IUserIdentity interface used when exposing a user
/// from a service to eliminate the dependency of Entity Framework.
/// </summary>
public class UserIdentity : IdentityUser,
    IUserIdentity
    
{
    
}