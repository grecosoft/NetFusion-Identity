using System.Net;
using FluentAssertions;
using NetFusion.Identity.Client.Models.Authentication;
using NetFusion.Identity.Client.Models.Authenticator;
using NetFusion.Identity.Tests.Extensions;
using NetFusion.Identity.Tests.Integration.Setup;
using NetFusion.Identity.Tests.Integration.Setup.Extensions;

namespace NetFusion.Identity.Tests.Integration;

/// <summary>
/// Tests the configuration and logging in using an authenticator.
/// </summary>
public class AuthenticatorTests
{
    [Fact]
    public async Task LoggedInUser_CanActivateTwoFactor_UsingAuthenticator_MustProvidedToken_OnLogin()
    {
        var fixture = WebAppTestFixture.Create();
        
        // -- create new account, confirm email, and login:
        var loginResponse = await fixture.RegisterAccountAndLogin(Models.ValidRegistration);
        loginResponse.ShouldBeRedirectionToDashboard();
        
        // -- view authenticator setup - to generate secret:
        var setupResponse = await fixture.GetAuthenticatorSetup();
        setupResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        // -- create token from secret and configure:
        var token = fixture.ServiceContext.GetAuthenticatorToken(Models.ValidRegistration.Email);
        var configureResponse = await fixture.ConfigureAuthenticator(new AuthenticatorSetupModel { SetupToken = token });

        // -- When configured, user navigated back to Two-Factor Configuration:
        configureResponse.ShouldBeRedirectionToTwoFactorConfig();
        await fixture.Logout();

        // -- Attempt to login again:
        var twoFactorLoginResponse = await fixture.Login(Models.ValidLogin);
        
        // -- Now navigated to screen to enter Two-Factor token:
        twoFactorLoginResponse.ShouldBeRedirectionToTwoFactorLogin();
        
        // -- Generate code using Authenticator and enter:
        var loginToken = fixture.ServiceContext.GetAuthenticatorToken(Models.ValidLogin.Email);

        // -- Attempt login using valid token:
        var authenticatorLoginResponse = await fixture.TwoFactorAuthenticatorCodeLogin(
            new TwoFactorLoginModel { AuthenticatorCode = loginToken });

        // -- Now user should be redirected to dashboard:
        authenticatorLoginResponse.ShouldBeRedirectionToDashboard();
    }

    [Fact]
    public async Task ResettingAuthenticator_DisablesTwoFactor_UntilReconfigured()
    {
        var fixture = WebAppTestFixture.Create();

        // -- create new account, confirm email, and login:
        var loginResponse = await fixture.RegisterAccountAndLogin(Models.ValidRegistration);
        loginResponse.ShouldBeRedirectionToDashboard();

        // -- view authenticator setup - to generate secret:
        var setupResponse = await fixture.GetAuthenticatorSetup();
        setupResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        // -- create token from secret and configure:
        var configureResponse = await fixture.ConfigureAuthenticator(new AuthenticatorSetupModel
        {
            SetupToken = fixture.ServiceContext.GetAuthenticatorToken(Models.ValidRegistration.Email)
        });

        // -- When configured, user navigated back to Two-Factor Configuration:
        configureResponse.ShouldBeRedirectionToTwoFactorConfig();
        await fixture.Logout();

        // -- Attempt to login again:
        var twoFactorLoginResponse = await fixture.Login(Models.ValidLogin);
        
        // -- Now navigated to screen to enter Two-Factor token:
        twoFactorLoginResponse.ShouldBeRedirectionToTwoFactorLogin();
        
        // -- Generate code using Authenticator and enter:
        var currentAuthenticatorSecret = fixture.ServiceContext.GetAuthenticatorSecret(Models.ValidRegistration.Email);
        loginResponse = await fixture.TwoFactorAuthenticatorCodeLogin(new TwoFactorLoginModel
        {
            AuthenticatorCode = fixture.ServiceContext.GetAuthenticatorToken(Models.ValidLogin.Email)
        });
        
        // -- Now user should be redirected to dashboard:
        loginResponse.ShouldBeRedirectionToDashboard();
        
        // -- Rest Authenticator and verify new secret has been generated.
        await fixture.ResetAuthenticator();
        await fixture.GetAuthenticatorSetup();
        var newAuthenticatorSecret = fixture.ServiceContext.GetAuthenticatorSecret(Models.ValidRegistration.Email);

        currentAuthenticatorSecret.Should().NotBe(newAuthenticatorSecret, 
            "new authenticator token not generated");
        
        // -- Validate that two-factor authentication is disabled till authenticator is re-configured:
        await fixture.Logout();
        loginResponse = await fixture.Login(Models.ValidLogin);
        
        // -- Now user should be redirected to dashboard:
        loginResponse.ShouldBeRedirectionToDashboard();
        
        // -- Reconfigure the authenticator:
        await fixture.ConfigureAuthenticator(new AuthenticatorSetupModel
        {
            SetupToken = fixture.ServiceContext.GetAuthenticatorToken(Models.ValidLogin.Email)
        });
        
        // -- Validate two-factor configure enabled for login:
        loginResponse = await fixture.Login(Models.ValidLogin);
        loginResponse.ShouldBeRedirectionToTwoFactorLogin();
    }
}