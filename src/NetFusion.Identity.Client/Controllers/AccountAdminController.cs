using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetFusion.Identity.App.Repositories;
using NetFusion.Identity.Client.Models.AccountAdmin;

namespace NetFusion.Identity.Client.Controllers;

[Authorize(Roles = "Admin")]
public class AccountAdminController : Controller
{
    private readonly IAccountRepository _accountRepo;
    private readonly IClaimsRepository _claimsRepo;

    public AccountAdminController(
        IAccountRepository accountRepo,
        IClaimsRepository claimsRepo)
    {
        _accountRepo = accountRepo;
        _claimsRepo = claimsRepo;
    }

    [HttpGet]
    public IActionResult AccountSearch([FromQuery] string criteria)
    {
        var matchingAccounts = _accountRepo.Search(criteria);
        ViewBag.Criteria = criteria;
        return View(matchingAccounts);
    }
    
    [HttpGet]
    public async Task<IActionResult> Claims([FromQuery] string criteria, [FromQuery] string userId)
    {
        var model = await CreateClaimsModel(criteria, userId);
        return View("AccountClaims", model);
    }

    [HttpPost]
    public async Task<IActionResult> AddNewClaim([FromForm]AccountClaimsModel model)
    {
        await _claimsRepo.AddUserClaimAsync(model.ClaimScopeId, model.ClaimTypeId, model.UserId, model.ClaimValue);
        return RedirectToAction("Claims", new { model.UserId });
    }

    [HttpPost]
    public async Task<IActionResult> AddNewRole([FromForm] AccountClaimsModel model)
    {
        await _claimsRepo.AddUserClaimAsync(model.RoleClaimScopeId, model.RoleClaimTypeId, model.UserId, model.RoleValue);
        return RedirectToAction("Claims", new { model.UserId });
    }

    [HttpPost]
    public async Task<IActionResult> EditExistingClaim([FromForm] AccountClaimsModel model)
    {
        await _claimsRepo.UpdateUserClaimAsync(model.ClaimUserValueId, model.ClaimValue);
        return RedirectToAction("Claims", new { model.UserId });
    }
    
    [HttpPost]
    public async Task<IActionResult> DeleteExistingClaim([FromForm] AccountClaimsModel model)
    {
        await _claimsRepo.DeleteUserClaimAsync(model.ClaimUserValueId);
        return RedirectToAction("Claims", new { model.UserId });
    }

    [HttpPost]
    public async Task<IActionResult> ListRoles([FromForm] AccountClaimsModel model)
    {
        var updatedModel = await CreateClaimsModel(model.Criteria, model.UserId, Guid.Parse(model.RoleClaimScopeId));
        return View("AccountClaims", updatedModel);
    }
    
    private async Task<AccountClaimsModel> CreateClaimsModel(string criteria, string userId, Guid? claimScopeId = null)
    {
        var allClaims = await _claimsRepo.ReadAllClaimsAsync();
        var scopes = await _claimsRepo.ReadAllScopesAsync();

        claimScopeId ??= scopes.FirstOrDefault() == null ? Guid.Empty : scopes.First().ClaimScopeId;
        
        var model =  new AccountClaimsModel
        {
            UserId = userId,
            Criteria = criteria,
            Email = User.Identity?.Name,
            RoleClaimTypeId = allClaims.Single(c => c.Namespace == ClaimTypes.Role).ClaimTypeId,
            
            Scopes = scopes.OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.ClaimScopeId.ToString()))
                .ToArray(),
            
            Claims = allClaims.Where(c => c.Namespace != ClaimTypes.Role)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem(c.Name, c.ClaimTypeId.ToString()))
                .ToArray(),
            
            Roles = (await _claimsRepo.ReadRolesAsync(claimScopeId.Value))
                .OrderBy(r => r.Value)
                .Select(r => new SelectListItem(r.Value, r.Value))
                .ToArray(),
            
            UserClaims = (await _claimsRepo.ReadAllUserClaimsAsync(userId))
                .OrderBy(c => c.Scope).ThenBy(c => c.Name)
                .ToArray()
        };
        
        return model;
    }
}