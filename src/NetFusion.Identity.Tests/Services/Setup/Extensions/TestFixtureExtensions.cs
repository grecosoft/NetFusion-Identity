using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Identity.App.Services;
using NetFusion.Identity.Domain.Registration.Entities;
using NetFusion.Identity.Domain.Registration.Services;
using NetFusion.Identity.Domain.TwoFactor.Services;
using TwoStepsAuthenticator;

namespace NetFusion.Identity.Tests.Services.Setup.Extensions;

public static class TestFixtureExtensions
{
    public static TService GetService<TService>(this ServiceTestFixture fixture) where TService : notnull =>
        fixture.Services.GetRequiredService<TService>();
    
    public static void SetPrinciple(this ServiceTestFixture fixture, RegistrationStatus registration)
    {
        if (registration.Email == null || registration.Id == null)
        {
            throw new InvalidOperationException(
                "Principle can only be set for a registration containing both and email and id.");
        }

        var identity = new GenericIdentity(registration.Email);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, registration.Id));

        fixture.HttpContext.User = new GenericPrincipal(identity, null);
    }

    public static string? GetAccountConfirmationToken(this ServiceTestFixture fixture, string email) =>
        fixture.ServiceContext.TryGetValue(email, ServiceContextKeys.AccountConfirmationToken, out string? token) ? token : null;
    
    public static string? GetAccountRecoveryToken(this ServiceTestFixture fixture, string email) =>
        fixture.ServiceContext.TryGetValue(email, ServiceContextKeys.PasswordRecoveryToken, out string? token) ? token : null;
    
    public static void ClearAccountConfirmation(this ServiceTestFixture fixture, string email) =>
        fixture.ServiceContext.DeleteValue(email, ServiceContextKeys.AccountConfirmationToken);

    public static async Task<RegistrationStatus> RegisterNewAccount(this ServiceTestFixture fixture, 
        UserRegistration registration,
        bool confirm = true)
    {
        var registrationService = fixture.GetService<IRegistrationService>();
        var registrationStatus = await registrationService.RegisterAsync(registration);

        if (!registrationStatus.Valid)
        {
            throw new InvalidOperationException("Account registration failed");
        }

        var confirmationToken = fixture.GetAccountConfirmationToken(registration.Email);
        if (confirmationToken == null)
        {
            throw new InvalidOperationException("Confirmation token not sent");
        }

        if (!confirm)
        {
            return registrationStatus;
        }
        
        var (domain, confirmation) = AccountConfirmation.Create(registration.Email, confirmationToken);
        if (domain.NotValid || confirmation == null)
        {
            throw new InvalidOperationException("Confirmation request not valid");
        }

        var confirmationStatus = await registrationService.ConfirmEmailAsync(confirmation);
        if (!confirmationStatus.Valid)
        {
            throw new InvalidOperationException("Account not confirmed");
        }

        return registrationStatus;
    }

    public static string GenerateAuthenticatorToken(this ServiceTestFixture fixture, string secret)
    {
        var authenticator = new TimeAuthenticator();
        return authenticator.GetCode(secret);
    }

    public static async Task ActivateAuthenticator(this ServiceTestFixture fixture)
    {
        var authenticatorSrv = fixture.GetService<IAuthenticatorService>();
        
        var authenticatorSetup = await authenticatorSrv.GetSetupInformationAsync();
        var setupToken = fixture.GenerateAuthenticatorToken(authenticatorSetup.AuthenticatorKey);
        var setupStatus = await authenticatorSrv.ConfirmSetupTokenAsync(setupToken);

        if (! setupStatus.Valid)
        {
            throw new InvalidOperationException("Authenticator not configured");
        }
    }
}