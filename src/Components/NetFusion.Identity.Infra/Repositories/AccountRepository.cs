using Microsoft.AspNetCore.Identity;
using NetFusion.Identity.App.Repositories;
using NetFusion.Identity.Domain;

namespace NetFusion.Identity.Infra.Repositories;

public class AccountRepository<TIdentity> : IAccountRepository
    where TIdentity : class, IUserIdentity
{
    private readonly UserManager<TIdentity> _userManager;

    public AccountRepository(UserManager<TIdentity> userManager)
    {
        _userManager = userManager;
    }

    public IEnumerable<IUserIdentity> Search(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Array.Empty<IUserIdentity>();
        }

        return _userManager.Users.Where(u => u.Email.Contains(email))
            .Cast<IUserIdentity>()
            .ToArray();
    }
}