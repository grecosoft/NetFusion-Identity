using Microsoft.AspNetCore.Identity;
using NetFusion.Identity.App.Services;
using NetFusion.Identity.Domain;
using NetFusion.Identity.Domain.Registration.Services;

namespace NetFusion.Identity.App.Implementations;

/// <summary>
/// Implements sending confirmations to an user.
/// </summary>
/// <typeparam name="TIdentity">Type containing information saved for a given user's registration.</typeparam>
public class ConfirmationService<TIdentity> : IConfirmationService
    where TIdentity : class, IUserIdentity
{
    private readonly UserManager<TIdentity> _userManager;
    private readonly IConfirmationSender _confirmationSender;
    private readonly IServiceContext _serviceContext;

    public ConfirmationService(
        UserManager<TIdentity> userManager,
        IConfirmationSender confirmationSender,
        IServiceContext serviceContext)
    {
        _userManager = userManager;
        _confirmationSender = confirmationSender;
        _serviceContext = serviceContext;
    }
    
    public async Task SendAccountConfirmationAsync(IUserIdentity userIdentity)
    {
        var user = (TIdentity)userIdentity;

        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _confirmationSender.SendAccountConfirmationAsync(userIdentity, token);
        
        _serviceContext.RecordValue(user.Email, ServiceContextKeys.AccountConfirmationToken, token);
    }

    public async Task SendPasswordRecoveryAsync(IUserIdentity userIdentity)
    {
        var user = (TIdentity)userIdentity;

        string token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _confirmationSender.SendPasswordRecoveryAsync(userIdentity, token);
        
        _serviceContext.RecordValue(user.Email, ServiceContextKeys.PasswordRecoveryToken, token);
    }
}