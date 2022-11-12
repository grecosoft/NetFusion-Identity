using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Identity.App.Implementations;
using NetFusion.Identity.Client.Models;
using NetFusion.Identity.Client.Models.Authentication;
using NetFusion.Identity.Domain.Authentication.Entities;
using NetFusion.Identity.Domain.Authentication.Services;
using NetFusion.Identity.Domain.Registration.Entities;
using NetFusion.Identity.Domain.TwoFactor.Entities;
using NetFusion.Identity.Domain.TwoFactor.Services;

namespace NetFusion.Identity.Client.Controllers;

/// <summary>
/// Responsible for logging in user with an email and password.
/// Also delegates to other services if two-factor authentication is enabled.
/// </summary>
[Authorize]
public class AuthenticationController : Controller
{
    private readonly IAuthenticationService _authenticationSrv;
    private readonly ITwoFactorService _twoFactorSrv;
    private readonly IAuthenticatorService _authenticatorSrv;
    private readonly UrlEncoderService _tokenEncoderSrv;

    public AuthenticationController(
        IAuthenticationService authenticationSrv,
        ITwoFactorService twoFactorSrv,
        IAuthenticatorService authenticatorSrv,
        UrlEncoderService tokenEncoderSrv)
    {
        _authenticationSrv = authenticationSrv;
        _twoFactorSrv = twoFactorSrv;
        _authenticatorSrv = authenticatorSrv;
        _tokenEncoderSrv = tokenEncoderSrv;
    }

    public const string Name = "Authentication";
    public const string LoginView = "Login";
    public const string TwoFactorLoginView = "TwoFactorLogin";
    public const string AccountLockedView = "AccountLocked";
    public const string SendPasswordRecoveryView = "SendPasswordRecovery";
    public const string ResetPasswordView = "ResetPassword";
    public const string TwoFactorLogoutView = "TwoFactorLogout";

    private IActionResult DisplayDashboard =>
        RedirectToAction(DashboardController.ApplicationsView, DashboardController.Name);


    [AllowAnonymous, HttpGet]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? string.Empty;
        return View(LoginView);
    }

    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var (domain, login) = UserLogin.Create(model.Email, model.Password, model.RememberClient);
        ModelState.AddValidations(domain);

        if (domain.Valid && login != null)
        {
            var result = await _authenticationSrv.LoginAsync(login);
            ModelState.AddValidations(result.Validations);

            return HandleLoginStatus(result, model.ReturnUrl, login.Email);
        }

        return View(LoginView);
    }

    [AllowAnonymous, HttpGet]
    public IActionResult TwoFactorLogin(
        [FromQuery] string? returnUrl = null,
        [FromQuery(Name = "recovery")] bool useRecoveryCode = false)
    {
        ViewBag.UseRecoveryCode = useRecoveryCode;
        ViewBag.ReturnUrl = returnUrl ?? string.Empty;
        return View(TwoFactorLoginView);
    }

    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> TwoFactorAuthenticatorCodeLogin(TwoFactorLoginModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var (domain, login) = AuthenticatorLogin.Create(model.AuthenticatorCode, model.RememberClient);
        ModelState.AddValidations(domain);

        if (domain.Valid && login != null)
        {
            var result = await _authenticatorSrv.ConfirmLoginTokenAsync(login);
            ModelState.AddValidations(result.Validations);

            if (result.Valid)
            {
                return string.IsNullOrEmpty(model.ReturnUrl) ? DisplayDashboard : Redirect(model.ReturnUrl);
            }
        }

        ViewBag.UseRecoveryCode = false;
        ViewBag.ReturnUrl = model.ReturnUrl;
        return View(TwoFactorLoginView);
    }

    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> TwoFactorRecoveryCodeLogin(TwoFactorLoginModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var result = await _twoFactorSrv.ConfirmLoginRecoveryTokenAsync(model.RecoveryCode);
        ModelState.AddValidations(result.Validations);
        
        ViewBag.UseRecoveryCode = true;
        return HandleLoginStatus(result, model.ReturnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmLogout()
    {
        var twoFactorConfig = await _twoFactorSrv.GetConfiguration();
        if (twoFactorConfig.IsEnabled && twoFactorConfig.IsMachineRemembered)
        {
            return View(TwoFactorLogoutView);
        }

        await _authenticationSrv.LogOutAsync();
        return RedirectToAction(LoginView);
    }

    [HttpPost]
    public async Task<IActionResult> Logout(LogoutModel model)
    {
        await _authenticationSrv.LogOutAsync(model.ForgetTwoFactorClient);
        return RedirectToAction(LoginView);
    }

    [AllowAnonymous, HttpGet]
    public IActionResult AccountLocked() => View(AccountLockedView);

    [AllowAnonymous, HttpGet]
    public IActionResult SendPasswordRecovery() => View(SendPasswordRecoveryView);

    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> SendPasswordRecovery([FromForm] string email)
    {
        var result = await _authenticationSrv.SendPasswordRecovery(email);
        ModelState.AddValidations(result.Validations);
        return View(SendPasswordRecoveryView);
    }

    [AllowAnonymous, HttpGet("Authentication/PasswordRecovery/{token}")]
    public IActionResult ResetPassword([FromRoute] string token)
    {
        return View(ResetPasswordView, new ResetPasswordModel());
    }

    [AllowAnonymous, HttpPost("Authentication/PasswordRecovery/{token}")]
    public async Task<IActionResult> ResetPassword([FromRoute] string token, [FromForm] ResetPasswordModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var resetToken = _tokenEncoderSrv.Decode(token);

        var (domain, recovery) = PasswordRecovery.Create(
            model.Email,
            new ConfirmedPassword(model.Password, model.ConfirmedPassword), resetToken);

        ModelState.AddValidations(domain);

        if (domain.Valid && recovery != null)
        {
            var result = await _authenticationSrv.ResetPasswordAsync(recovery);
            ModelState.AddValidations(result.Validations);

            if (result.Valid)
            {
                return RedirectToAction(LoginView);
            }
        }

        return View(ResetPasswordView);
    }

    private IActionResult HandleLoginStatus(LoginStatus loginStatus,
        string? returnUrl,
        string email) => loginStatus switch
    {
        { RequiredTwoFactor: true } => RedirectToAction(TwoFactorLoginView, new { returnUrl }),
        { LockedOut: true } => RedirectToAction(AccountLockedView),

        { EmailNotConfirmed: true } => RedirectToAction(
            RegistrationController.ResendEmailConfirmationView,
            RegistrationController.Name, new { email, returnUrl }),

        { Valid: true } => string.IsNullOrEmpty(returnUrl) ? DisplayDashboard : Redirect(returnUrl),
        _ => View(LoginView)
    };

    private IActionResult HandleLoginStatus(RecoveryLoginStatus loginStatus,
        string? returnUrl) => loginStatus switch
    {
        { Valid: true, LowRecoveryCodeCount: true } => RedirectToAction(
            AccountSettingsController.TwoFactorRecoveryCodesView,
            AccountSettingsController.Name),
        { Valid: true } => string.IsNullOrEmpty(returnUrl) ? DisplayDashboard : Redirect(returnUrl),
        _ => View(TwoFactorLoginView)
    };
}


