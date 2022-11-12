using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NetFusion.Identity.App.Settings;
using NetFusion.Identity.Client.Extensions;
using NetFusion.Identity.Client.Models;
using NetFusion.Identity.Client.Models.Authenticator;
using NetFusion.Identity.Domain.TwoFactor.Services;

namespace NetFusion.Identity.Client.Controllers;

/// <summary>
/// Responsible for enabling two-factor authentication by configuring
/// an authenticator. 
/// </summary>
[Authorize]
public class AuthenticatorController : Controller
{
    private readonly DashboardSettings _dashboardSettings;
    private readonly IAuthenticatorService _authenticatorSrv;

    public AuthenticatorController(
        IOptions<DashboardSettings> options,
        IAuthenticatorService authenticatorSrv)
    {
        _dashboardSettings = options.Value;
        _authenticatorSrv = authenticatorSrv;
        
    }

    public const string Name = "Authenticator";
    public const string SetupView = "Setup";
    public const string ConfirmResetView = "ConfirmReset";
    

    [HttpGet]
    public async Task<IActionResult> Setup()
    {
        var result = await _authenticatorSrv.GetSetupInformationAsync();
        var authenticatorName = _dashboardSettings.AuthenticatorLabel ?? _dashboardSettings.Title ?? "Dashboard";
        
        return View(SetupView, new AuthenticatorSetupModel
        {
            Key = result.AuthenticatorKey.SplitIntoParts(4).ToLowerInvariant(),
            QrCodeUrl = $"otpauth://totp/{authenticatorName}:{result.Email}?secret={result.AuthenticatorKey}"
        });
    }

    [HttpPost]
    public async Task<IActionResult> Setup(AuthenticatorSetupModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        
        var result = await _authenticatorSrv.ConfirmSetupTokenAsync(model.SetupToken);
        ModelState.AddValidations(result.Validations);

        if (result.Valid)
        {
            return RedirectToAction(AccountSettingsController.TwoFactorConfigurationView, AccountSettingsController.Name); 
        }
        
        return View(SetupView, model);
    }

    [HttpGet]
    public IActionResult ConfirmReset() => View(ConfirmResetView);
    
    [HttpPost]
    public async Task<IActionResult> Reset()
    {
        await _authenticatorSrv.Reset();
        return RedirectToAction(SetupView);
    }
}


