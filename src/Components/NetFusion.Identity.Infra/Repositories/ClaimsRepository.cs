using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NetFusion.Identity.App.Repositories;
using NetFusion.Identity.Domain.Claims.Entities;
using NetFusion.Identity.Infra.Repositories.Entities;

namespace NetFusion.Identity.Infra.Repositories;

public class ClaimsRepository : IClaimsRepository
{
   private readonly ApplicationDbContext _dbContext;

   public ClaimsRepository(ApplicationDbContext dbContext)
   {
      _dbContext = dbContext;
   }

   public async Task<IdentityScope[]> ReadAllScopesAsync()
   {
      return (await _dbContext.ClaimScopes.ToArrayAsync())
         .Select(ct => new IdentityScope {
            ClaimScopeId = ct.ClaimScopeId,
            Name = ct.Name, 
            Key = ct.Key,
            Description = ct.Description})
         .ToArray();
   }

   public async Task<IdentityClaim[]> ReadAllClaimsAsync()
   {
      return (await _dbContext.ClaimTypes.ToArrayAsync())
         .Select(ct => new IdentityClaim { 
            ClaimTypeId = ct.ClaimTypeId, 
            Name = ct.Name, 
            Namespace = ct.Namespace, 
            Description = ct.Description})
         .ToArray();
   }
   
   public async Task<IdentityUserClaim[]> ReadAllUserClaimsAsync(string userId)
   {
      if (string.IsNullOrWhiteSpace(userId))
         throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
      
      return (await _dbContext.ClaimUserValues
            .Include(cv => cv.ClaimType)
            .Include(cv => cv.ClaimScope)
            .Where(cv => cv.UserId == userId)
            .ToArrayAsync())
         .Select(MapToEntity)
         .ToArray();
   }

   public async Task<IdentityUserClaim[]> ReadUserClaimsAsync(Guid scopeId, string userId)
   {
      if (string.IsNullOrWhiteSpace(userId))
         throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
      
      if (string.IsNullOrWhiteSpace(userId))
         throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));

      return (await _dbContext.ClaimUserValues
            .Include(cv => cv.ClaimType)
            .Include(cv => cv.ClaimScope)
            .Where(cv => cv.UserId == userId && cv.ClaimScopeId == scopeId)
            .ToArrayAsync())
            .Select(MapToEntity)
            .ToArray();
   }

   public async Task<IdentityRole[]> ReadRolesAsync(Guid claimScopeId)
   {
      return (await _dbContext.RoleTypes
         .Where(rt => rt.ClaimScopeId == claimScopeId)
         .ToArrayAsync())
         .Select(rt => new IdentityRole
         {
            RoleTypeId = rt.RoleTypeId,
            Value = rt.Value,
            Description = rt.Description
         })
         .ToArray();
   }

   public async Task<IdentityUserClaim[]> ReadUserClaimsAsync(string scopeKey, string userId)
   {
      if (scopeKey == null) throw new ArgumentNullException(nameof(scopeKey));
      
      if (string.IsNullOrWhiteSpace(userId))
         throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
      
      return (await _dbContext.ClaimUserValues
            .Include(cv => cv.ClaimType)
            .Include(cv => cv.ClaimScope)
            .Where(cv => cv.UserId == userId && cv.ClaimScope.Key == scopeKey)
            .ToArrayAsync())
         .Select(MapToEntity)
         .ToArray();
   }
   
   private static IdentityUserClaim MapToEntity(ClaimUserValue claimValue) => new()
   {
      ClaimUserValueId = claimValue.ClaimUserValueId,
      ClaimTypeId = claimValue.ClaimTypeId,
      Scope = claimValue.ClaimScope.Name,
      Name = claimValue.ClaimType.Name,
      Namespace = claimValue.ClaimType.Namespace,
      Value = claimValue.Value,
      Claim = new Claim(claimValue.ClaimType.Namespace, claimValue.Value)
   };

   public Task DeleteUserClaimAsync(int userClaimValueId)
   {
      _dbContext.ClaimUserValues.Remove(new ClaimUserValue { ClaimUserValueId = userClaimValueId });
      return _dbContext.SaveChangesAsync();
   }

   public async Task AddUserClaimAsync(string claimScopeId, int claimTypeId, string userId, string value)
   {
      if (string.IsNullOrWhiteSpace(claimScopeId))
         throw new ArgumentException("Value cannot be null or empty.", nameof(claimScopeId));
      
      if (string.IsNullOrWhiteSpace(userId))
         throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
      
      if (string.IsNullOrWhiteSpace(value))
         throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));


      var claimUserValue = new ClaimUserValue
      {
         UserId = userId,
         ClaimScopeId = Guid.Parse(claimScopeId),
         ClaimTypeId = claimTypeId,
         Value = value
      };

      await _dbContext.ClaimUserValues.AddAsync(claimUserValue).AsTask();
      await _dbContext.SaveChangesAsync();
   }

   public async Task UpdateUserClaimAsync(int claimUserValueId, string value)
   {
      if (string.IsNullOrWhiteSpace(value))
         throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
      
      var claimUserValue = await _dbContext.ClaimUserValues
         .FirstOrDefaultAsync(uv => uv.ClaimUserValueId == claimUserValueId);

      if (claimUserValue == null)
      {
         return;
      }

      claimUserValue.Value = value;

      _dbContext.ClaimUserValues.Update(claimUserValue);
      await _dbContext.SaveChangesAsync();
   }
}