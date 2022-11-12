using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Identity.Client.Models;
using NetFusion.Identity.Client.Models.AccountSettings;
using NetFusion.Identity.Domain.Authentication.Entities;
using NetFusion.Identity.Domain.Authentication.Services;
using NetFusion.Identity.Domain.Registration.Entities;
using NetFusion.Identity.Domain.TwoFactor.Services;

namespace NetFusion.Identity.Client.Controllers;

[Authorize]
public class AccountSettingsController : Controller
{
    public const string Name = "AccountSettings";
    private const string ChangePasswordView = "ChangePassword";
    private const string OverviewView = "Overview";
    public const string TwoFactorConfigurationView = "TwoFactorConfiguration";
    public const string TwoFactorRecoveryCodesView = "TwoFactorRecoveryCodes";

    private readonly IAuthenticationService _loginSrv;
    private readonly ITwoFactorService _twoFactorSrv;

    public AccountSettingsController(
        IAuthenticationService loginSrv,
        ITwoFactorService twoFactorSrv)
    {
        _loginSrv = loginSrv;
        _twoFactorSrv = twoFactorSrv;
    }

    public IActionResult Overview() => View(OverviewView);
    
    public IActionResult ChangePassword() => View(ChangePasswordView);

    [HttpGet]
    public async Task<IActionResult> TwoFactorConfiguration()
    {
        return View(TwoFactorConfigurationView, await GetTwoFactorConfigurationModel());
    }
    
    private async Task<TwoFactorConfigurationModel> GetTwoFactorConfigurationModel()
    {
        var configuration = await _twoFactorSrv.GetConfiguration(includeRecoveryCodes: true);
        return new TwoFactorConfigurationModel
        {
            HasAuthenticator = configuration.HasAuthenticator,
            IsEnabled = configuration.IsEnabled,
            IsMachineRemembered = configuration.IsMachineRemembered,
            RemainingRecoveryCodes = configuration.RemainingRecoveryCodes,
            RecoveryCodes = configuration.RecoveryCodes
        };
    }
    
    [HttpPost]
    public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordModel model)
    {
        var (domain, changePassword) = UserChangePassword.Create(
            model.CurrentPassword,
            new ConfirmedPassword(model.NewPassword, model.ConfirmedPassword));
        
        ModelState.AddValidations(domain);

        if (domain.Valid && changePassword != null)
        {
            var result = await _loginSrv.ChangePassword(changePassword);
            ModelState.AddValidations(result.Validations);
            return RedirectToAction(TwoFactorConfigurationView);
        }

        return View(ChangePasswordView);
    }
    
    [HttpGet]
    public IActionResult TwoFactorConfirmDisable() => View();
    
    [HttpPost]
    public async Task<IActionResult> Disable()
    {
        var result = await _twoFactorSrv.DisableAsync();
        ModelState.AddValidations(result.Validations);

        return result.Valid ? await TwoFactorConfiguration() : View("UnexpectedResult");
    }
    
    [HttpGet]
    public async Task<IActionResult> TwoFactorRecoveryCodes()
    {
        return View(TwoFactorRecoveryCodesView, await GetTwoFactorConfigurationModel());
    }
    
    [HttpPost]
    public async Task<IActionResult> ResetRecoveryCodes()
    {
        await _twoFactorSrv.RegenerateRecoveryCodes();
        return RedirectToAction(TwoFactorRecoveryCodesView);
    }
}