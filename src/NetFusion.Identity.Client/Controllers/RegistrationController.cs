using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Identity.App.Implementations;
using NetFusion.Identity.Client.Models;
using NetFusion.Identity.Client.Models.Registration;
using NetFusion.Identity.Domain.Registration.Entities;
using NetFusion.Identity.Domain.Registration.Services;

namespace NetFusion.Identity.Client.Controllers;

/// <summary>
/// Responsible for creating new accounts and confirming they
/// are being created from valid email accounts.
/// </summary>
[Authorize]
public class RegistrationController : Controller
{
    private readonly IRegistrationService _registrationSrv;
    private readonly UrlEncoderService _tokenEncoderSrv;

    public RegistrationController(
        IRegistrationService registrationSrv,
        UrlEncoderService tokenEncoderSrv)
    {
        _registrationSrv = registrationSrv;
        _tokenEncoderSrv = tokenEncoderSrv;
    }

    public const string Name = "Registration";
    public const string ResendEmailConfirmationView = "ResendEmailConfirmation";
    private const string NewAccountView = "NewAccount";
    private const string EmailConfirmationView = "EmailConfirmation";


    [AllowAnonymous, HttpGet]
    public IActionResult NewAccount() => View(NewAccountView);

    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> NewAccount(RegistrationModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        
        var (domain, userReg) = UserRegistration.Create(
            model.Email,
            new ConfirmedPassword(model.Password, model.ConfirmedPassword));

        ModelState.AddValidations(domain);

        if (domain.Valid && userReg != null)
        {
            var result = await _registrationSrv.RegisterAsync(userReg);
            ModelState.AddValidations(result.Validations);

            if (result.Valid)
            {
                return RedirectToAction(ResendEmailConfirmationView);
            }
        }

        return View(NewAccountView);
    }
    
    [AllowAnonymous, HttpGet]
    public IActionResult ResendEmailConfirmation(string? email)
    {
        ViewBag.Email = email ?? string.Empty;
        return View(ResendEmailConfirmationView); 
    }
    
    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> SendEmailConfirmation([FromForm]string email)
    {
        var result = await _registrationSrv.ResendEmailConfirmationAsync(email);
        ModelState.AddValidations(result.Validations);

        ViewBag.Email = email;
        return View(ResendEmailConfirmationView);
    }
    
    [AllowAnonymous, HttpGet("Registration/Confirm/{token}")]
    public IActionResult EmailConfirmation([FromRoute] string token) => View(EmailConfirmationView);
    
    [AllowAnonymous, HttpPost("Registration/Confirm/{token}")]
    public async Task<IActionResult> EmailConfirmation([FromRoute]string token, [FromForm]string email)
    {
        var conformToken = _tokenEncoderSrv.Decode(token);
        
        var (domain, confirmation) = AccountConfirmation.Create(email, conformToken);
        ModelState.AddValidations(domain);

        if (domain.Valid && confirmation != null)
        {
            var result = await _registrationSrv.ConfirmEmailAsync(confirmation);
            ModelState.AddValidations(result.Validations);

            if (result.Valid)
            {
                return RedirectToAction(AuthenticationController.LoginView, AuthenticationController.Name);
            }
        }

        return View(EmailConfirmationView);
    }
}


