using Microsoft.AspNetCore.Mvc.Rendering;
using NetFusion.Identity.Domain.Claims.Entities;

namespace NetFusion.Identity.Client.Models.AccountAdmin;

#pragma warning disable CS8618
public class AccountClaimsModel
{
    public string UserId { get; set; }
    public string? Email { get; set; }
    public string Criteria { get; set; }
    
    public SelectListItem[] Scopes { get; set; } = Array.Empty<SelectListItem>();
    public SelectListItem[] Claims { get; set; } = Array.Empty<SelectListItem>();
    public SelectListItem[] Roles { get; set; } = Array.Empty<SelectListItem>();
    public IdentityUserClaim[] UserClaims { get; set; } = Array.Empty<IdentityUserClaim>();
    
    // Model properties sent during editing:
    public string ClaimScopeId { get; set; }
    public int ClaimTypeId { get; set; }
    public int ClaimUserValueId { get; set; }
    public string ClaimValue { get; set; }
    
    public string RoleClaimScopeId { get; set; }
    public int RoleClaimTypeId { get; set; }
    public string RoleValue { get; set; }
   



}