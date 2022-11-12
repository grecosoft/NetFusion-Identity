using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetFusion.Identity.Client.Controllers;
using NetFusion.Identity.Client.Models.Authentication;
using NetFusion.Identity.Domain;
using NetFusion.Identity.Domain.Authentication.Entities;
using NetFusion.Identity.Domain.TwoFactor.Entities;
using NetFusion.Identity.Tests.Controllers.Setup;
using NetFusion.Identity.Tests.Integration.Setup;

namespace NetFusion.Identity.Tests.Controllers;

public class AuthenticationControllerTests
{
    [Fact]
    public void ReturnUrl_PassedToView_WhenPageNeedsAuthentication()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange();
        
        // Act:
        var viewResult = fixture.AuthenticationController.Login("http://unauthorized-page.html") as ViewResult;
        
        // Assert:
        viewResult.Should().NotBeNull("view action result expected");
        viewResult!.ViewData.Should().ContainKey("ReturnUrl");
        viewResult.ViewData["ReturnUrl"].Should().Be("http://unauthorized-page.html");
    }

    [Fact]
    public async Task LoginReturnUrlSpecified_NavigatesToUrl_AfterSuccessfulLogin()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticationSrv.Setup(m => m.LoginAsync(It.IsAny<UserLogin>()))
                .ReturnsAsync(new LoginStatus());
        });
        
        var model = Models.ValidLogin;
        model.ReturnUrl = "http://unauthorized-page.html";

        // Act:
        var viewResult = await fixture.AuthenticationController.Login(model) as RedirectResult;
        viewResult.Should().NotBeNull("expected redirection result");
        viewResult!.Url.Should().Be("http://unauthorized-page.html", "return url not specified");
        
        fixture.MockAuthenticationSrv.Verify(
            m => m.LoginAsync(It.Is<UserLogin>(e => e.Email == model.Email && e.Password == model.Password)),
            Times.Once);
    }
    
    
    [Fact]
    public async Task LoginReturnUrlNotSpecified_NavigatesToDashboard_AfterSuccessfulLogin()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticationSrv.Setup(m => m.LoginAsync(It.IsAny<UserLogin>()))
                .ReturnsAsync(new LoginStatus());
        });
        
        var model = Models.ValidLogin;

        // Act:
        var viewResult = await fixture.AuthenticationController.Login(model) as RedirectToActionResult;
        viewResult.Should().NotBeNull("expected redirection action result");
        viewResult!.ActionName.Should().Be(DashboardController.ApplicationsView, "expected application view");
        viewResult.ControllerName.Should().Be(DashboardController.Name, "expected dashboard controller");

        fixture.MockAuthenticationSrv.Verify(
            m => m.LoginAsync(It.Is<UserLogin>(e => e.Email == model.Email && e.Password == model.Password)),
            Times.Once);
    }

    [Fact]
    public async Task InvalidLogin_Remains_CurrentView()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticationSrv.Setup(m => m.LoginAsync(It.IsAny<UserLogin>()))
                .ReturnsAsync(new LoginStatus(
                    succeeded: false, 
                    lockedOut: false, 
                    requiredTwoFactor: false, 
                    notAllowed: false, 
                    emailNotConfirmed: false));
        });

        // Act:
        var viewResult = await fixture.AuthenticationController.Login(Models.InvalidLogin) as ViewResult;
        viewResult.Should().NotBeNull("expected view action result");
        viewResult!.ViewName.Should().Be(AuthenticationController.LoginView);
    }

    [Fact]
    public async Task LoginNavigatesTo_TwoFactorLogin_WhenConfigured()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticationSrv.Setup(m => m.LoginAsync(It.IsAny<UserLogin>()))
                .ReturnsAsync(new LoginStatus(
                    succeeded: false, 
                    lockedOut: false, 
                    requiredTwoFactor: true, 
                    notAllowed: false, 
                    emailNotConfirmed: false));
        });
        
        // Act:
        var viewResult = await fixture.AuthenticationController.Login(Models.ValidLogin) as RedirectToActionResult;
        viewResult.Should().NotBeNull("expected view action result");
        viewResult!.ActionName.Should().Be(AuthenticationController.TwoFactorLoginView);
    }

    [Fact]
    public async Task LoginNavigatesTo_LockOutView_WhenAccountLocked()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticationSrv.Setup(m => m.LoginAsync(It.IsAny<UserLogin>()))
                .ReturnsAsync(new LoginStatus(
                    succeeded: false, 
                    lockedOut: true, 
                    requiredTwoFactor: false, 
                    notAllowed: false, 
                    emailNotConfirmed: false));
        });
        
        // Act:
        var viewResult = await fixture.AuthenticationController.Login(Models.ValidLogin) as RedirectToActionResult;
        viewResult.Should().NotBeNull("expected view action result");
        viewResult!.ActionName.Should().Be(AuthenticationController.AccountLockedView);
    }

    [Fact]
    public async Task LoginNavigatesTo_EmailConformation_WhenAccountNotConfirmed()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticationSrv.Setup(m => m.LoginAsync(It.IsAny<UserLogin>()))
                .ReturnsAsync(new LoginStatus(
                    succeeded: false, 
                    lockedOut: false, 
                    requiredTwoFactor: false, 
                    notAllowed: false, 
                    emailNotConfirmed: true));
        });
        
        // Act:
        var viewResult = await fixture.AuthenticationController.Login(Models.ValidLogin) as RedirectToActionResult;
        viewResult.Should().NotBeNull("expected view action result");
        viewResult!.ActionName.Should().Be(RegistrationController.ResendEmailConfirmationView);
        viewResult.ControllerName.Should().Be(RegistrationController.Name);
    }

    [Fact]
    public async Task InvalidTwoFactor_AuthenticatorCode_RemainsAtTwoFactorLogin()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticatorSrv.Setup(m => m.ConfirmLoginTokenAsync(It.IsAny<AuthenticatorLogin>()))
                .ReturnsAsync(new ResultStatus(isSuccess: false));
        });
        
        // Act:
        var model = new TwoFactorLoginModel { AuthenticatorCode = "MOCK_CODE" };
        var actionResult = await fixture.AuthenticationController.TwoFactorAuthenticatorCodeLogin(model) as ViewResult;
        
        // Arrange:
        actionResult.Should().NotBeNull("expected view result");
        actionResult!.ViewName.Should().Be(AuthenticationController.TwoFactorLoginView, 
            "should remain on two-factor login view");

        actionResult.ViewData.Should().ContainKey("UseRecoveryCode");
        actionResult.ViewData["UseRecoveryCode"].Should().Be(false, "recovery-code should not be displayed");
        
        fixture.MockAuthenticatorSrv.Verify(
            m => m.ConfirmLoginTokenAsync(It.Is<AuthenticatorLogin>(v => v.Token == "MOCK_CODE")),
            Times.Once);
    }

    [Fact]
    public async Task ValidTwoFactorAuthenticatorCode_Navigates_ToReturnUrl()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticatorSrv.Setup(m => m.ConfirmLoginTokenAsync(It.IsAny<AuthenticatorLogin>()))
                .ReturnsAsync(new ResultStatus(isSuccess: true));
        });
        
        // Act:
        var model = new TwoFactorLoginModel { AuthenticatorCode = "MOCK_CODE", ReturnUrl = "http://original-page.html"};
        var actionResult = await fixture.AuthenticationController.TwoFactorAuthenticatorCodeLogin(model) as RedirectResult;
        
        // Arrange:
        actionResult.Should().NotBeNull("expected redirect result");
        actionResult!.Url.Should().Be("http://original-page.html");
        
        fixture.MockAuthenticatorSrv.Verify(
            m => m.ConfirmLoginTokenAsync(It.Is<AuthenticatorLogin>(v => v.Token == "MOCK_CODE")),
            Times.Once);
    }

    [Fact]
    public async Task ValidTwoFactorAuthenticatorCode_NoReturnUrl_NavigatesToDashboard()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticatorSrv.Setup(m => m.ConfirmLoginTokenAsync(It.IsAny<AuthenticatorLogin>()))
                .ReturnsAsync(new ResultStatus(isSuccess: true));
        });
        
        // Act:
        var model = new TwoFactorLoginModel { AuthenticatorCode = "MOCK_CODE" };
        var actionResult = await fixture.AuthenticationController.TwoFactorAuthenticatorCodeLogin(model) as RedirectToActionResult;
        
        // Arrange:
        actionResult.Should().NotBeNull("expected redirect to action result");
        actionResult!.ActionName.Should().Be(DashboardController.ApplicationsView);
        actionResult.ControllerName.Should().Be(DashboardController.Name);
        
        fixture.MockAuthenticatorSrv.Verify(
            m => m.ConfirmLoginTokenAsync(It.Is<AuthenticatorLogin>(v => v.Token == "MOCK_CODE")),
            Times.Once);
    }

    [Fact]
    public async Task InvalidRecoveryCode_NavigationRemains_AtRecoveryCodeView()
    {
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockTwoFactorSrv.Setup(m => m.ConfirmLoginRecoveryTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new RecoveryLoginStatus(succeeded: false, lowRecoveryCodeCount: false));
        });

        var actionResult = await  fixture.AuthenticationController.TwoFactorRecoveryCodeLogin(
            new TwoFactorLoginModel { RecoveryCode = "MOCK_RECOVERY_CODE"}) as ViewResult;

        actionResult.Should().NotBeNull("view action result expected");
        actionResult!.ViewName.Should().Be(AuthenticationController.TwoFactorLoginView);
        actionResult.ViewData.Should().ContainKey("UseRecoveryCode");
        actionResult.ViewData["UseRecoveryCode"].Should().Be(true, "recovery-code should be displayed");
        
        fixture.MockTwoFactorSrv.Verify(
            m => m.ConfirmLoginRecoveryTokenAsync(It.Is<string>(v => v == "MOCK_RECOVERY_CODE")),
            Times.Once);
    }

    [Fact]
    public async Task ValidRecoverCode_NavigatesToReturnUrl_WhenSpecified()
    {
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockTwoFactorSrv.Setup(m => m.ConfirmLoginRecoveryTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new RecoveryLoginStatus(succeeded: true, lowRecoveryCodeCount: false));
        });

        var actionResult = await  fixture.AuthenticationController.TwoFactorRecoveryCodeLogin(
            new TwoFactorLoginModel { RecoveryCode = "MOCK_RECOVERY_CODE", ReturnUrl = "original-page.com"}) as RedirectResult;

        actionResult.Should().NotBeNull("redirection result expected");
        actionResult!.Url.Should().Be("original-page.com");
        
        fixture.MockTwoFactorSrv.Verify(
            m => m.ConfirmLoginRecoveryTokenAsync(It.Is<string>(v => v == "MOCK_RECOVERY_CODE")),
            Times.Once);
    }

    [Fact]
    public async Task ValidRecoveryCode_NavigatesToDashboard_WhenReturnUrlNotSpecified()
    {
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockTwoFactorSrv.Setup(m => m.ConfirmLoginRecoveryTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new RecoveryLoginStatus(succeeded: true, lowRecoveryCodeCount: false));
        });

        var actionResult = await  fixture.AuthenticationController.TwoFactorRecoveryCodeLogin(
            new TwoFactorLoginModel { RecoveryCode = "MOCK_RECOVERY_CODE" }) as RedirectToActionResult;

        actionResult.Should().NotBeNull("redirection to action result expected");
        actionResult!.ActionName.Should().Be(DashboardController.ApplicationsView);
        actionResult.ControllerName.Should().Be(DashboardController.Name);
        
        fixture.MockTwoFactorSrv.Verify(
            m => m.ConfirmLoginRecoveryTokenAsync(It.Is<string>(v => v == "MOCK_RECOVERY_CODE")),
            Times.Once);
    }
    
    [Fact]
    public async Task ValidRecoveryCode_NavigatesToRecoveryCodeView_WhenCodesRemainingLow()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockTwoFactorSrv
                .Setup(m => m.ConfirmLoginRecoveryTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new RecoveryLoginStatus(succeeded: true, lowRecoveryCodeCount: true));
        });

        var model = new TwoFactorLoginModel
        {
            RecoveryCode = "MOCK_RECOVERY_CODE"
        };

        // Act:
        var actionResult = await fixture.AuthenticationController.TwoFactorRecoveryCodeLogin(model) as RedirectToActionResult;
        
        // Assert:
        actionResult.Should().NotBeNull("expected redirection to controller action");
        actionResult!.ActionName.Should().Be(AccountSettingsController.TwoFactorRecoveryCodesView);
        actionResult.ControllerName.Should().Be(AccountSettingsController.Name);
        
        fixture.MockTwoFactorSrv.Verify(
            m => m.ConfirmLoginRecoveryTokenAsync(It.Is<string>(v => v == "MOCK_RECOVERY_CODE")),
            Times.Once);
    }

    [Fact]
    public async Task WhenTwoFactorMachineRemembered_NavigatesTo_LogoutConfirm()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockTwoFactorSrv.Setup(m => m.GetConfiguration(It.IsAny<bool>()))
                .ReturnsAsync(new Configuration { IsEnabled = true, IsMachineRemembered = true});
        });

        // Act:
        var actionResult = await fixture.AuthenticationController.ConfirmLogout() as ViewResult;
        
        // Assert:
        actionResult.Should().NotBeNull("expected view result");
        actionResult!.ViewName.Should().Be(AuthenticationController.TwoFactorLogoutView);
        
        fixture.MockAuthenticationSrv.Verify(m => m.LogOutAsync(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task WhenTwoFactorMachineNotRemembered_Navigates_ToLoginView()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockTwoFactorSrv.Setup(m => m.GetConfiguration(It.IsAny<bool>()))
                .ReturnsAsync(new Configuration { IsEnabled = false, IsMachineRemembered = false});
        });

        // Act:
        var actionResult = await fixture.AuthenticationController.ConfirmLogout() as RedirectToActionResult;
        
        // Assert:
        actionResult.Should().NotBeNull("expected redirection to action");
        actionResult!.ActionName.Should().Be(AuthenticationController.LoginView);
        
        fixture.MockAuthenticationSrv.Verify(m => m.LogOutAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task PasswordRest_ValidCredentials_NavigatesToLogin()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticationSrv.Setup(m => m.ResetPasswordAsync(It.IsAny<PasswordRecovery>()))
                .ReturnsAsync(new ResultStatus(isSuccess: true));
        });

        // Act:
        var actionResult = await fixture.AuthenticationController.ResetPassword(
            "RECOVERY_TOKEN", new ResetPasswordModel
            {
                Email = Models.ValidLogin.Email,
                Password = Models.ValidLogin.Password,
                ConfirmedPassword = Models.ValidRegistration.ConfirmedPassword
            }) as RedirectToActionResult;

        // Assert:
        actionResult.Should().NotBeNull("expected redirection to action");
        actionResult!.ActionName.Should().Be(AuthenticationController.LoginView);

        fixture.MockAuthenticationSrv.Verify(m => m.ResetPasswordAsync(It.IsAny<PasswordRecovery>()), Times.Once());
    }

    [Fact]
    public async Task PasswordResetWith_InvalidCredentials_RemainsAtPasswordRestView()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticationSrv.Setup(m => m.ResetPasswordAsync(It.IsAny<PasswordRecovery>()))
                .ReturnsAsync(new ResultStatus(isSuccess: false));
        });

        // Act:
        var actionResult = await fixture.AuthenticationController.ResetPassword(
            "RECOVERY_TOKEN", new ResetPasswordModel
            {
                Email = Models.ValidLogin.Email,
                Password = Models.ValidLogin.Password,
                ConfirmedPassword = Models.ValidRegistration.ConfirmedPassword
            }) as ViewResult;

        // Assert:
        actionResult.Should().NotBeNull("expected view action");
        actionResult!.ViewName.Should().Be(AuthenticationController.ResetPasswordView);

        fixture.MockAuthenticationSrv.Verify(m => m.ResetPasswordAsync(It.IsAny<PasswordRecovery>()), Times.Once());
    }
}