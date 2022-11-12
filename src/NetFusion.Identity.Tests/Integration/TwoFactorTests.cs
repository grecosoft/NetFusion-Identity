using System.Net;
using FluentAssertions;
using NetFusion.Identity.Client.Models.Authentication;
using NetFusion.Identity.Tests.Extensions;
using NetFusion.Identity.Tests.Integration.Setup;
using NetFusion.Identity.Tests.Integration.Setup.Extensions;

namespace NetFusion.Identity.Tests.Integration;

/// <summary>
/// Tests Two-Factor account settings.
/// </summary>
public class TwoFactorTests
{
    [Fact]
    public async Task AccountWithTwoFactorEnabled_UserCanUseOneTimeRecoverCode_ToLogin()
    {
        var fixture = WebAppTestFixture.Create();
        
        // -- create new account, confirm email, and login:
        var loginResponse = await fixture.RegisterAccountAndLogin(Models.ValidRegistration);
        loginResponse.ShouldBeRedirectionToDashboard();
        
        // -- setup authenticator to activate two-factor authentication:
        var configureResponse = await fixture.SetupAuthenticator();

        // -- When configured, user navigated back to Two-Factor Configuration:
        configureResponse.ShouldBeRedirectionToTwoFactorConfig();
        
        // -- Generate list of recovery codes:
        var resetResponse = await fixture.ResetRecoveryCodes();
        resetResponse.ShouldBeRedirectionToTwoFactoryRecoveryCodes();

        var recoveryCodes = fixture.ServiceContext.GetRecoveryCodes(Models.ValidLogin.Email);
        recoveryCodes.Should().NotBeNullOrEmpty("Recovery codes not generated");
        
        // -- attempt to login again and two-factor login will be requested:
        await fixture.Logout();
        loginResponse = await fixture.Login(Models.ValidLogin);
        loginResponse.ShouldBeRedirectionToTwoFactorLogin();
        
        // -- login using recovery code:
        loginResponse = await fixture.TwoFactorRecoveryCodeLogin(new TwoFactorLoginModel
        {
            RecoveryCode = recoveryCodes.First()
        });
        
        loginResponse.ShouldBeRedirectionToDashboard();
        
        // -- attempt to login again and reuse the recovery code:
        await fixture.Logout();
        loginResponse = await fixture.Login(Models.ValidLogin);
        loginResponse.ShouldBeRedirectionToTwoFactorLogin();
        
        // -- login using same recovery code:
        loginResponse = await fixture.TwoFactorRecoveryCodeLogin(new TwoFactorLoginModel
        {
            RecoveryCode = recoveryCodes.First()
        });

        loginResponse.Should().HaveStatusCode(HttpStatusCode.OK, 
            "should remain on two-factor page with no redirection");
    }

    [Fact]
    public async Task TwoFactorAuthentication_CanBeDisabled()
    {
        var fixture = WebAppTestFixture.Create();
        
        // -- create new account, confirm email, and login:
        var loginResponse = await fixture.RegisterAccountAndLogin(Models.ValidRegistration);
        loginResponse.ShouldBeRedirectionToDashboard();
        
        // -- setup authenticator to activate two-factor authentication:
        var configureResponse = await fixture.SetupAuthenticator();

        // -- When configured, user navigated back to Two-Factor Configuration:
        configureResponse.ShouldBeRedirectionToTwoFactorConfig();
        await fixture.Logout();
        
        // -- attempt to login to validate two-factor enabled:
        loginResponse = await fixture.Login(Models.ValidLogin);
        loginResponse.ShouldBeRedirectionToTwoFactorLogin();

        // -- login with authentication token:
        var loginToken = fixture.ServiceContext.GetAuthenticatorToken(Models.ValidLogin.Email);
        loginResponse = await fixture.TwoFactorAuthenticatorCodeLogin(new TwoFactorLoginModel
        {
            AuthenticatorCode = loginToken
        });
        loginResponse.ShouldBeRedirectionToDashboard();
        
        // -- disable two-factor and validate only email and password required to login:
        var disableResponse = await fixture.DisableTwoFactor();
        disableResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        await fixture.Logout();

        loginResponse = await fixture.Login(Models.ValidLogin);
        loginResponse.ShouldBeRedirectionToDashboard();
    }

    [Fact]
    public async Task RecoveryCodes_CanBeRegenerated()
    {
        var fixture = WebAppTestFixture.Create();
        
        // -- create new account, confirm email, and login:
        var loginResponse = await fixture.RegisterAccountAndLogin(Models.ValidRegistration);
        loginResponse.ShouldBeRedirectionToDashboard();
        
        // -- setup authenticator to activate two-factor authentication:
        var configureResponse = await fixture.SetupAuthenticator();

        // -- When configured, user navigated back to Two-Factor Configuration:
        configureResponse.ShouldBeRedirectionToTwoFactorConfig();
        await fixture.ResetRecoveryCodes();

        var currentRecoverCodes = fixture.ServiceContext.GetRecoveryCodes(Models.ValidLogin.Email);
        await fixture.ResetRecoveryCodes();

        var newRecoverCodes = fixture.ServiceContext.GetRecoveryCodes(Models.ValidLogin.Email);
        currentRecoverCodes.Should().NotBeEquivalentTo(newRecoverCodes);
    }
}