using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NetFusion.Identity.App.Extensions;
using NetFusion.Identity.Domain;
using NetFusion.Identity.Domain.Registration.Entities;
using NetFusion.Identity.Domain.Registration.Services;
using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.App.Implementations;

/// <summary>
/// Implements the creating and confirmation of accounts associated with a given email address
/// by delegating to ASP.NET Identity managers.
/// </summary>
/// <typeparam name="TIdentity">Type containing information saved for a given user's registration.</typeparam>
public class RegistrationService<TIdentity> : IRegistrationService
    where TIdentity : class, IUserIdentity, new()
{
    private readonly ILogger _logger;
    private readonly IAuthenticationContext<TIdentity> _authentication;
    private readonly IConfirmationService _confirmationSrv;

    public RegistrationService(
        ILoggerFactory loggerFactory,
        IAuthenticationContext<TIdentity> authentication,
        IConfirmationService confirmationSrv)
    {
        _logger = loggerFactory.CreateLogger("RegistrationService");
        _authentication = authentication;
        _confirmationSrv = confirmationSrv;
    }

    public async Task<RegistrationStatus> RegisterAsync(UserRegistration registration)
    {
        if (registration == null) throw new ArgumentNullException(nameof(registration));
        
        TIdentity identity = await _authentication.UserManager.FindByEmailAsync(registration.Email);

        var registrationStatus = new RegistrationStatus(
            existingUser: identity != null, 
            pendingConfirmation: !identity?.EmailConfirmed ?? false);
        
        registrationStatus.Validations.ValidateFalse(registrationStatus.ExistingUser, ValidationLevel.Error, 
            $"Email address {registration.Email} is already registered.");

        if (registrationStatus.Valid)
        {
            await RegisterUser(registration, registrationStatus);
        }
        
        _logger.LogValidations(registration.Email, registrationStatus);
        return registrationStatus;
    }
    
    public async Task<ConfirmEmailStatus> ConfirmEmailAsync(AccountConfirmation confirmation)
    {
        if (confirmation == null) throw new ArgumentNullException(nameof(confirmation));

        TIdentity user = await _authentication.UserManager.FindByEmailAsync(confirmation.Email);

        var registrationStatus = new ConfirmEmailStatus(
            existingUser: user != null, 
            pendingConfirmation: !user?.EmailConfirmed ?? false);
        
        if (! registrationStatus.ExistingUser)
        {
            registrationStatus.Validations.Add(ValidationLevel.Error, 
                $"User with email: {confirmation.Email} not found.");
            
            _logger.LogValidations(confirmation.Email, registrationStatus);
            return registrationStatus;
        }

        if (user == null)
        {
            throw new InvalidOperationException("User identity required to confirm account.");
        }

        using var _ = _authentication.GetLogContext(user);
        
        if (registrationStatus.PendingConfirmation)
        {
            IdentityResult confirmResult = await _authentication.UserManager.ConfirmEmailAsync(
                user, confirmation.Token);
            
            registrationStatus.AddValidations(confirmResult);
        }
        else
        {
            registrationStatus.Validations.Add(ValidationLevel.Error, 
                $"The account for email: {user.Email} has already been confirmed.");
        }
        
        _logger.LogValidations(user.Email, registrationStatus);
        return registrationStatus;
    }

    public async Task<ConfirmEmailStatus> ResendEmailConfirmationAsync(string email)
    {
        var status = new ConfirmEmailStatus();
       
        status.Validations.ValidateFalse(string.IsNullOrWhiteSpace(email), ValidationLevel.Error, "Email required");
        if (status.NotValid)
        {
            return status;
        }

        TIdentity user = await _authentication.UserManager.FindByEmailAsync(email);

        var registrationStatus = new ConfirmEmailStatus(
            existingUser: user != null, 
            pendingConfirmation: !user?.EmailConfirmed ?? false);
        
        if (registrationStatus.ExistingUser && registrationStatus.PendingConfirmation)
        {
            await _confirmationSrv.SendAccountConfirmationAsync(user!);
        }

        var knownEmail = registrationStatus.Validations.ValidateTrue(registrationStatus.ExistingUser, 
            ValidationLevel.Error, $"Account for email {email} not found.");

        if (knownEmail)
        {
            registrationStatus.Validations.ValidateTrue(registrationStatus.PendingConfirmation, 
                ValidationLevel.Error, $"Account form email {email} already confirmed.");
        }

        _logger.LogValidations(email, registrationStatus);
        return registrationStatus;
    }

    private async Task RegisterUser(UserRegistration registration, RegistrationStatus registrationStatus)
    {
        var user = CreateUserFromRegistration(registration);
        
        IdentityResult createResult = await _authentication.UserManager.CreateAsync(user);
        if (! createResult.Succeeded)
        {
            registrationStatus.AddValidations(createResult);
            return;
        }

        IdentityResult passwordResult = await _authentication.UserManager.AddPasswordAsync(
            user, registration.ConformedPassword.Chosen);
        
        if (! passwordResult.Succeeded)
        {
            registrationStatus.AddValidations(passwordResult);
            await _authentication.UserManager.DeleteAsync(user);
            return;
        }

        registrationStatus.SetUserIdentity(user.Id, user.Email);
        await _confirmationSrv.SendAccountConfirmationAsync(user);
    }
    
    private static TIdentity CreateUserFromRegistration(UserRegistration registration)
    {
        TIdentity user = CreateUser();
        user.UserName = registration.Email;
        user.Email = registration.Email;

        return user;
    }

    private static TIdentity CreateUser()
    {
        try
        {
            return Activator.CreateInstance<TIdentity>();
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Can't create an instance of: {nameof(TIdentity)}", ex);
        }
    }
}