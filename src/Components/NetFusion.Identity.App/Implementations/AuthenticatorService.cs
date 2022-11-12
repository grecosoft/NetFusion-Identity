using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NetFusion.Identity.App.Extensions;
using NetFusion.Identity.App.Services;
using NetFusion.Identity.Domain;
using NetFusion.Identity.Domain.TwoFactor.Entities;
using NetFusion.Identity.Domain.TwoFactor.Services;
using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.App.Implementations;

/// <summary>
/// Implements the configuring of an Authenticator to support two-factor authentication.
/// </summary>
/// <typeparam name="TIdentity">Type containing information saved for a given user's registration.</typeparam>
public class AuthenticatorService<TIdentity> : IAuthenticatorService
    where TIdentity : class, IUserIdentity
{
    private readonly ILogger _logger;
    private readonly IAuthenticationContext<TIdentity> _authentication;
    private readonly IServiceContext _serviceContext;

    public AuthenticatorService(
        ILoggerFactory loggerFactory,
        IAuthenticationContext<TIdentity> authentication,
        IServiceContext serviceContext)
    {
        _logger = loggerFactory.CreateLogger("AuthenticatorService");
        _authentication = authentication;
        _serviceContext = serviceContext;
    }

    public async Task<AuthenticatorSetup> GetSetupInformationAsync()
    {
        ThrowIfNotSupported();
        
        TIdentity user = await _authentication.GetUserIdentity();
        using var _ = _authentication.GetLogContext(user);

        string key = await _authentication.UserManager.GetAuthenticatorKeyAsync(user);
        if (key == null)
        {
            _logger.LogInformation("Authentication key being reset for {Email}", user.Email);
            
            await _authentication.UserManager.ResetAuthenticatorKeyAsync(user);
            key = await _authentication.UserManager.GetAuthenticatorKeyAsync(user);
            
            await _authentication.SigninManager.RefreshSignInAsync(user);
        }
        
        _serviceContext.RecordValue(user.Email, "AuthenticatorKey", key);
        return new AuthenticatorSetup(user.Email, key);
    }

    public async Task<ResultStatus> ConfirmSetupTokenAsync(string setupToken)
    {
        if (string.IsNullOrWhiteSpace(setupToken))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(setupToken));

        ThrowIfNotSupported();

        TIdentity user = await _authentication.GetUserIdentity();
        using var _ = _authentication.GetLogContext(user);
        
        bool isActivated = await _authentication.UserManager.VerifyTwoFactorTokenAsync(user,
            _authentication.SigninManager.Options.Tokens.AuthenticatorTokenProvider, setupToken);

        var resultStatus = new ResultStatus(isActivated);

        resultStatus.Validations.ValidateTrue(isActivated, ValidationLevel.Error, 
            "Authenticator could not be activated.");
        
        if (resultStatus.NotValid)
        {
            _logger.LogValidations(user.Email, resultStatus);
            return resultStatus;
        }
        
        // Enable two-factor authentication:
        var enabledResult = await _authentication.UserManager.SetTwoFactorEnabledAsync(user, true);
        var enabledStatus = new ResultStatus(enabledResult.Succeeded);
        
        enabledStatus.AddValidations(enabledResult);

        await _authentication.SigninManager.RefreshSignInAsync(user);
        
        _logger.LogValidations(user.Email, enabledStatus);
        return enabledStatus;
    }

    public async Task<ResultStatus> ConfirmLoginTokenAsync(AuthenticatorLogin login)
    {
        if (login == null) throw new ArgumentNullException(nameof(login));
        
        ThrowIfNotSupported();

        var statusResult = new ResultStatus();
        TIdentity user = await _authentication.SigninManager.GetTwoFactorAuthenticationUserAsync();
        
        statusResult.Validations.ValidateNotNull(user, ValidationLevel.Error, 
            "Two Factor Authentication failed.");

        if (statusResult.NotValid)
        {
            _logger.LogValidations("ConfirmLoginTokenAsync", statusResult);
            return statusResult;
        }
        
        using var _ = _authentication.GetLogContext(user);

        SignInResult signinResult = await _authentication.SigninManager.TwoFactorAuthenticatorSignInAsync(
            login.Token, true, login.RememberClient);

        var signInStatus = new ResultStatus();

        signInStatus.Validations.ValidateTrue(signinResult.Succeeded, ValidationLevel.Warning, 
            "Two Factor Authentication failed.");

        _logger.LogValidations(user.Email, signInStatus);
        return signInStatus;
    }

    public async Task Reset()
    {
        ThrowIfNotSupported();
        
        var domain = DomainValidations.Empty;
        
        TIdentity user = await _authentication.GetUserIdentity();
        using var _ = _authentication.GetLogContext(user);
        
        var enableResult = await _authentication.UserManager.SetTwoFactorEnabledAsync(user, false);
        var resetResult = await _authentication.UserManager.ResetAuthenticatorKeyAsync(user);

        domain.AddValidations(enableResult, resetResult);
        if (domain.Valid)
        {
            _logger.LogInformation("Reset Authenticator for {Email} completed", user.Email);
        }
        _logger.LogValidations(user.Email, domain);
    }

    private void ThrowIfNotSupported()
    {
        if (!_authentication.UserManager.SupportsUserTwoFactor 
            || !_authentication.UserManager.SupportsUserAuthenticatorKey)
        {
            throw new InvalidOperationException("Two Factor Authenticator no supported");
        }
    }
}
