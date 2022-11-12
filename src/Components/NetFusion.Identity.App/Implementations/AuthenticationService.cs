using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NetFusion.Identity.App.Extensions;
using NetFusion.Identity.Domain;
using NetFusion.Identity.Domain.Authentication.Entities;
using NetFusion.Identity.Domain.Authentication.Services;
using NetFusion.Identity.Domain.Registration.Services;
using NetFusion.Identity.Domain.Validation;
using Serilog.Context;

namespace NetFusion.Identity.App.Implementations;

/// <summary>
/// Implements user login by delegating to ASP.NET Identity managers.
/// </summary>
/// <typeparam name="TIdentity">Type containing information saved for a given user's registration.</typeparam>
public class AuthenticationService<TIdentity> : IAuthenticationService
    where TIdentity : class, IUserIdentity
{
    private readonly ILogger _logger;
    private readonly IAuthenticationContext<TIdentity> _authentication;
    private readonly IConfirmationService _confirmationSrv;

    public AuthenticationService(
        ILoggerFactory loggerFactory, 
        IAuthenticationContext<TIdentity> authenticationContext,
        IConfirmationService confirmationSrv)
    {
        _logger = loggerFactory.CreateLogger("AuthenticationService");
        _authentication = authenticationContext;
        _confirmationSrv = confirmationSrv;
    }

    public async Task<LoginStatus> LoginAsync(UserLogin login)
    {
        if (login == null) throw new ArgumentNullException(nameof(login));

        var resultStatus = new LoginStatus();
        TIdentity? user = await _authentication.UserManager.FindByEmailAsync(login.Email);
        
        resultStatus.Validations.ValidateNotNull(user, ValidationLevel.Error, 
            $"Email address {login.Email} not registered.");
        
        if (resultStatus.NotValid)
        {
            _logger.LogValidations(nameof(LoginAsync), resultStatus);
            return resultStatus;
        }
        
        using var _ = _authentication.GetLogContext(user);
    
        SignInResult signinResult = await _authentication.SigninManager.PasswordSignInAsync(
            user, login.Password, login.RememberClient, true);
        
        var loginStatus = new LoginStatus(
            signinResult.Succeeded,         
            signinResult.IsLockedOut,
            signinResult.RequiresTwoFactor,
            signinResult.IsNotAllowed,
            !user.EmailConfirmed);
        
        loginStatus.Validations.ValidateFalse(loginStatus.InvalidCredentials, ValidationLevel.Error, 
            "Invalid Credentials.  Please try again.");

        using (LogContext.PushProperty("Details", loginStatus, destructureObjects: true))
        {
            _logger.LogInformation("Login details for {Email}", user.Email);
        }
        
        return loginStatus;
    }

    public async Task LogOutAsync(bool forgetTwoFactorClient = false)
    {
        var user = await _authentication.GetUserIdentity();

        if (forgetTwoFactorClient)
        {
            await _authentication.SigninManager.ForgetTwoFactorClientAsync();
        }
        
        await _authentication.SigninManager.SignOutAsync();
        
        _logger.LogInformation("User {Email} logged out", user.Email);
    }

    public async Task<ResultStatus> ChangePassword(UserChangePassword changedPassword)
    {
        if (changedPassword == null) throw new ArgumentNullException(nameof(changedPassword));

        TIdentity user = await _authentication.GetUserIdentity();
        using var _ = _authentication.GetLogContext(user);
        
        IdentityResult result = await _authentication.UserManager.ChangePasswordAsync(user,
            changedPassword.CurrentPassword,
            changedPassword.ConfirmedPassword.Chosen);

        var resultStatus = new ResultStatus(result.Succeeded);
        resultStatus.AddValidations(result);
        
        _logger.LogValidations(nameof(ChangePassword), resultStatus);
        return resultStatus;
    }

    public async Task<ResultStatus> SendPasswordRecovery(string emailAddress)
    {
        var resultStatus = new ResultStatus();
        
        resultStatus.Validations.ValidateFalse(string.IsNullOrWhiteSpace(emailAddress), ValidationLevel.Error, 
            "Email address required.");

        if (resultStatus.NotValid)
        {
            _logger.LogValidations(emailAddress, resultStatus);
            return resultStatus;
        }
        
        IUserIdentity user = await _authentication.UserManager.FindByEmailAsync(emailAddress);
        
        resultStatus.Validations.ValidateNotNull(user, ValidationLevel.Error, 
            "Account for {emailAddress} not registered.");
        
        if (resultStatus.NotValid)
        {
            _logger.LogValidations(emailAddress, resultStatus);
            return resultStatus;
        }
        
        using var _ = _authentication.GetLogContext(user);
        
        resultStatus.Validations.ValidateTrue(user.EmailConfirmed, 
            ValidationLevel.Error, 
            "Password recovery can't be resent for non-confirmed account.");
        
        if (resultStatus.Valid)
        {
            await _confirmationSrv.SendPasswordRecoveryAsync(user);
            _logger.LogInformation("Password recovery sent to {Email}", user.Email);
        }

        _logger.LogValidations(user.Email, resultStatus);
        return resultStatus;
    }

    public async Task<ResultStatus> ResetPasswordAsync(PasswordRecovery recovery)
    {
        if (recovery == null) throw new ArgumentNullException(nameof(recovery));

        var resultStatus = new ResultStatus();
        TIdentity user = await _authentication.UserManager.FindByEmailAsync(recovery.Email);
        
        resultStatus.Validations.ValidateNotNull(user, ValidationLevel.Error, 
            $"Account for {recovery.Email} not registered.");
        
        if (resultStatus.NotValid)
        {
            _logger.LogValidations(nameof(ResetPasswordAsync), resultStatus);
            return resultStatus;
        }
        
        using var _ = _authentication.GetLogContext(user);
        
        IdentityResult result = await _authentication.UserManager.ResetPasswordAsync(
            user, recovery.Token, recovery.Password.Chosen);
            
        resultStatus.AddValidations(result);
        
        _logger.LogValidations(user.Email, resultStatus);
        return resultStatus;
    }
}