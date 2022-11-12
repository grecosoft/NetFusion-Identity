using FluentAssertions;
using NetFusion.Identity.Domain.TwoFactor.Services;
using NetFusion.Identity.Tests.Services.Setup;
using NetFusion.Identity.Tests.Services.Setup.Extensions;

namespace NetFusion.Identity.Tests.Services;

/// <summary>
/// Unit tests asserting the AuthenticatorService implementation using mocked web-runtime
/// dependencies and in-memory database.
/// </summary>
public class AuthenticatorServiceTests
{
    [Fact]
    public async Task LoggedInUser_CanRequest_ConfigurationInformation()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var authenticatorSrv = fixture.GetService<IAuthenticatorService>();
        var twoFactorSrv = fixture.GetService<ITwoFactorService>();

        // -- Create new account and set the principle to simulate logging in:
        var registrationStatus = await fixture.RegisterNewAccount(Requests.ValidRegistration);
        fixture.SetPrinciple(registrationStatus);
        
        // -- Two-factor configuration and authenticator should not be configured:
        var twoFactorConfig = await twoFactorSrv.GetConfiguration();
        twoFactorConfig.IsEnabled.Should().BeFalse("two-factor authentication should not be configured for new account");
        twoFactorConfig.HasAuthenticator.Should().BeFalse("authenticator should not be configured for new account");

        // Act:
        
        // -- Request the information
        var authenticatorSetup = await authenticatorSrv.GetSetupInformationAsync();
        
        // Assert:
        authenticatorSetup.Should().NotBeNull("authenticator setup information not returned");
        authenticatorSetup.AuthenticatorKey.Should().NotBeEmpty("key used to setup authenticator not returned");
        authenticatorSetup.Email.Should().NotBeEmpty("email associated with authenticator not returned");
        
        // -- Two-factor configuration should remain disabled but account should not have authenticator: 
        twoFactorConfig = await twoFactorSrv.GetConfiguration();
        twoFactorConfig.IsEnabled.Should().BeFalse("two-factor authentication should not be configured for new account");
        twoFactorConfig.HasAuthenticator.Should().BeTrue("authenticator should be associated with account");
    }

    [Fact]
    public async Task AuthenticatorActivated_UsingGeneratedToken_CreatedFromSecret()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var authenticatorSrv = fixture.GetService<IAuthenticatorService>();
        var twoFactorSrv = fixture.GetService<ITwoFactorService>();
        
        // -- Create new account and set the principle to simulate logging in:
        var registrationStatus = await fixture.RegisterNewAccount(Requests.ValidRegistration);
        fixture.SetPrinciple(registrationStatus);

        // -- Request the information and generate token to setup authenticator:
        var authenticatorSetup = await authenticatorSrv.GetSetupInformationAsync();
        
        // Act:
        var token = fixture.GenerateAuthenticatorToken(authenticatorSetup.AuthenticatorKey);
        var resultStatus = await authenticatorSrv.ConfirmSetupTokenAsync(token);
        
        // Assert:
        resultStatus.Valid.Should().BeTrue("authenticator should have been activated");
        
        // -- Confirm two-factor configuration:
        var twoFactorConfig = await twoFactorSrv.GetConfiguration();
        twoFactorConfig.IsEnabled.Should().BeTrue("Two-factor authentication should now be enabled");
        twoFactorConfig.HasAuthenticator.Should().BeTrue("authenticator should now be enabled");
    }

    [Fact]
    public async Task ResettingAuthenticator_DisablesTwoFactor_GeneratesNewSecret()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var authenticatorSrv = fixture.GetService<IAuthenticatorService>();
        var twoFactorSrv = fixture.GetService<ITwoFactorService>();

        // -- Create new account and set the principle to simulate logging in:
        var registrationStatus = await fixture.RegisterNewAccount(Requests.ValidRegistration);
        fixture.SetPrinciple(registrationStatus);
        
        // -- Request the information and generate token to setup authenticator:
        var authenticatorSetup = await authenticatorSrv.GetSetupInformationAsync();
        var token = fixture.GenerateAuthenticatorToken(authenticatorSetup.AuthenticatorKey);
        var resultStatus = await authenticatorSrv.ConfirmSetupTokenAsync(token);
        resultStatus.Valid.Should().BeTrue("authenticator should have been activated");
        
        // Act:
        await authenticatorSrv.Reset();
        
        // Assert:
        var authenticatorResetSetup = await authenticatorSrv.GetSetupInformationAsync();
        authenticatorResetSetup.AuthenticatorKey.Should().NotBe(authenticatorSetup.AuthenticatorKey, 
            "new authenticator secret key should have been generated");
        
        var twoFactorConfig = await twoFactorSrv.GetConfiguration();
        twoFactorConfig.IsEnabled.Should().BeFalse("two-factor authentication should now be disabled");
        twoFactorConfig.HasAuthenticator.Should().BeTrue("account should still have an authenticator");
    }
}