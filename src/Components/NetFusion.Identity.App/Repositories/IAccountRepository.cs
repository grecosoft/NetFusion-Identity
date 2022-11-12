using NetFusion.Identity.Domain;

namespace NetFusion.Identity.App.Repositories;

/// <summary>
/// Implements retrieving account information from data store. 
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Searches for account based on email address.
    /// </summary>
    /// <param name="email">The full or partial email address to be searched.</param>
    /// <returns>List of User Identities matching the search criteria.</returns>
    IEnumerable<IUserIdentity> Search(string email);
}