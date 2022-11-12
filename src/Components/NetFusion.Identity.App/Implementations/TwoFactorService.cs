using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NetFusion.Identity.App.Extensions;
using NetFusion.Identity.App.Services;
using NetFusion.Identity.Domain;
using NetFusion.Identity.Domain.TwoFactor.Entities;
using NetFusion.Identity.Domain.TwoFactor.Services;
using NetFusion.Identity.Domain.Validation;
using Serilog.Context;

namespace NetFusion.Identity.App.Implementations;

/// <summary>
/// Implements two-factor authentication related services.
/// </summary>
/// <typeparam name="TIdentity">Type containing information saved for a given user's registration.</typeparam>
public class TwoFactorService<TIdentity> : ITwoFactorService
    where TIdentity : class, IUserIdentity
{
    private readonly ILogger _logger;
    private readonly IAuthenticationContext<TIdentity> _authentication;
    private readonly IServiceContext _serviceContext;

    public TwoFactorService(
        ILoggerFactory loggerFactory,
        IAuthenticationContext<TIdentity> authentication,
        IServiceContext serviceContext)
    {
        _logger = loggerFactory.CreateLogger("TwoFactorService");
        _authentication = authentication;
        _serviceContext = serviceContext;
    }

    public async Task<Configuration> GetConfiguration(bool includeRecoveryCodes)
    {
        ThrowIfNotSupported();
        
        TIdentity user = await _authentication.GetUserIdentity();
        using var _ = _authentication.GetLogContext(user);

        var config = new Configuration {
            IsEnabled = user.TwoFactorEnabled,
            HasAuthenticator = await _authentication.UserManager.GetAuthenticatorKeyAsync(user) != null,
            IsMachineRemembered = await _authentication.SigninManager.IsTwoFactorClientRememberedAsync(user),
            RemainingRecoveryCodes = await _authentication.UserManager.CountRecoveryCodesAsync(user)
        };

        using (LogContext.PushProperty("TwoFactorConfig", config, destructureObjects: true))
        {
            _logger.LogInformation("Two-Factor Configuration for {Email}", user.Email);
        }

        if (includeRecoveryCodes && _authentication.UserManager.SupportsUserTwoFactorRecoveryCodes)
        {
            config.RecoveryCodes = await GetCurrentRecoveryCodes(user);
        }

        return config;
    }
    
    public async Task<ResultStatus> DisableAsync()
    {
        ThrowIfNotSupported();

        TIdentity user = await _authentication.GetUserIdentity();
        using var _ = _authentication.GetLogContext(user);
        
        var disabledResult = await _authentication.UserManager.SetTwoFactorEnabledAsync(user, false);
        var resultStatus = new ResultStatus(disabledResult.Succeeded);

        await _authentication.SigninManager.ForgetTwoFactorClientAsync();
        await _authentication.SigninManager.RefreshSignInAsync(user);
        
        resultStatus.AddValidations(disabledResult);
        _logger.LogValidations(user.Email, resultStatus);
        
        return resultStatus;
    }
    
    public async Task<RecoveryLoginStatus> ConfirmLoginRecoveryTokenAsync(string recoveryCode)
    {
        ThrowIfNotSupported();
        
        var resultStatus = new RecoveryLoginStatus();
        TIdentity user = await _authentication.SigninManager.GetTwoFactorAuthenticationUserAsync();

        resultStatus.Validations.ValidateNotNull(user, ValidationLevel.Error, 
            "Two Factor Authentication cannot be completed.");

        if (resultStatus.NotValid)
        {
            _logger.LogInformation("Two-Factor Authentication failed.  User unknown.");
            return resultStatus;
        }
        
        resultStatus.Validations.ValidateFalse(string.IsNullOrWhiteSpace(recoveryCode), ValidationLevel.Error,
            "Recovery code not specified.");

        if (resultStatus.NotValid)
        {
            _logger.LogValidations(user.Email, resultStatus);
            return resultStatus;
        }

        SignInResult result = await _authentication.SigninManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
        int remainingCodes = await _authentication.UserManager.CountRecoveryCodesAsync(user);
        bool lowRemainingCodes = remainingCodes <= _authentication.Settings.MinNumberRecoveryCodesWarning;
        
        _logger.LogInformation("Two-Factory recovery code for {Email} succeeded", user.Email);

        return new RecoveryLoginStatus(result.Succeeded, lowRemainingCodes);
    }

    private async Task<string[]> GetCurrentRecoveryCodes(TIdentity user)
    {
        string value = await _authentication.UserManager
            .GetAuthenticationTokenAsync(user, "[AspNetUserStore]", "RecoveryCodes");

        return value == null ? Array.Empty<string>() : value.Split(';');
    }

    public async Task RegenerateRecoveryCodes()
    {
        ThrowIfNotSupported(recoveryCodesRequired: true);

        int numberCodesToGenerate = _authentication.Settings.NumberRecoveryCodes;

        TIdentity user = await _authentication.GetUserIdentity();
        if (user.TwoFactorEnabled)
        {
            var codes = await _authentication.UserManager.GenerateNewTwoFactorRecoveryCodesAsync(
                user, numberCodesToGenerate);

            _serviceContext.RecordValue(user.Email, ServiceContextKeys.RecoveryCodes, codes.ToArray());
            _logger.LogInformation("Two-Factor Recovery Codes generated for {Email}", user.Email);
        }
    }
    
    private void ThrowIfNotSupported(bool recoveryCodesRequired = false)
    {
        if (!_authentication.UserManager.SupportsUserTwoFactor || 
            (recoveryCodesRequired && !_authentication.UserManager.SupportsUserTwoFactorRecoveryCodes))
        {
            throw new InvalidOperationException("Two Factor Authentication no supported");
        }
    }
}





