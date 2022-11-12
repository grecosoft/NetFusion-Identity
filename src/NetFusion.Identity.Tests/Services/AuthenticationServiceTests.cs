using FluentAssertions;
using NetFusion.Identity.Domain.Authentication.Entities;
using NetFusion.Identity.Domain.Authentication.Services;
using NetFusion.Identity.Domain.Registration.Entities;
using NetFusion.Identity.Tests.Services.Setup;
using NetFusion.Identity.Tests.Services.Setup.Extensions;

namespace NetFusion.Identity.Tests.Services;

/// <summary>
/// Unit tests asserting the AuthenticationService implementation using mocked web-runtime
/// dependencies and in-memory database.
/// </summary>
public class AuthenticationServiceTests
{
    [Fact]
    public void EmailAndPassword_Required_ToLogin()
    {
        var (domain, login) = UserLogin.Create("user@mock.com", "auto-detailing99$", rememberClient: false);
        domain.Valid.Should().BeTrue("validations should not have been recorded");
        login.Should().NotBeNull("valid login should have been created");
    }

    [Fact]
    public void Email_Required_ToLogin()
    {
        var (domain, login) = UserLogin.Create("", "auto-detailing99$", rememberClient: false);
        domain.Valid.Should().BeFalse("validations should have been recorded");
        login.Should().BeNull("login should not be created for invalid credentials");
    }

    [Fact]
    public void Password_Required_ToLogin()
    {
        var (domain, login) = UserLogin.Create("user@mock.com", "", rememberClient: false);
        domain.Valid.Should().BeFalse("validations should have been recorded");
        login.Should().BeNull("login should not be created for invalid credentials");
    }
    
    [Fact]
    public async Task OnlyRegistered_Users_CanLogin()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var loginSrv = fixture.GetService<IAuthenticationService>();

        // Act:
        var loginStatus = await loginSrv.LoginAsync(Requests.ValidUserLogin);
        
        // Assert:
        loginStatus.Valid.Should().BeFalse("user without registered can't login");
        loginStatus.Validations.NotValid.Should().BeTrue("validations should have been recorded");
    }

    [Fact]
    public async Task ValidCredentials_Required_ToLogin()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var loginSrv = fixture.GetService<IAuthenticationService>();
        
        await fixture.RegisterNewAccount(Requests.ValidRegistration);
        
        // Act:
        var loginResult = await loginSrv.LoginAsync(Requests.InvalidUserLogin);
        
        // Assert:
        loginResult.Valid.Should().BeFalse("invalid credentials should not allow login");
        loginResult.Validations.Items.Should().NotBeEmpty("validations should have been recorded");
    }

    [Fact]
    public async Task Account_MustBeConfirmed_ToLogin()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var loginSrv = fixture.GetService<IAuthenticationService>();
        
        await fixture.RegisterNewAccount(Requests.ValidRegistration, confirm: false);
        
        // Act:
        var loginResult = await loginSrv.LoginAsync(Requests.ValidUserLogin);
        
        // Assert:
        loginResult.Valid.Should().BeFalse("should not be allowed to log into non-confirmed account");
        loginResult.EmailNotConfirmed.Should().BeTrue("result should be account not confirmed");
    }

    [Fact]
    public async Task ConfirmationToken_CanBeSent_ToResetPassword()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var loginSrv = fixture.GetService<IAuthenticationService>();
        
        await fixture.RegisterNewAccount(Requests.ValidRegistration);
        
        // Act:
        var sendResetStatus = await loginSrv.SendPasswordRecovery(Requests.ValidRegistration.Email);
        
        // Assert:
        var resetToken = fixture.GetAccountRecoveryToken(Requests.ValidRegistration.Email);

        sendResetStatus.Should().NotBeNull("response not returned");
        sendResetStatus.Valid.Should().BeTrue("issue sending account recovery");
        resetToken.Should().NotBeNull("recovery token not sent");
    }

    [Fact]
    public async Task AccountCanHavePassword_Reset_UsingConfirmationToken()
    {
        var fixture = ServiceTestFixture.Create();
        var loginSrv = fixture.GetService<IAuthenticationService>();

        // Arrange:
        await fixture.RegisterNewAccount(Requests.ValidRegistration);
        await loginSrv.SendPasswordRecovery(Requests.ValidRegistration.Email);
        var resetToken = fixture.GetAccountRecoveryToken(Requests.ValidRegistration.Email);
        
        // Act:
        // -- Using the reset token, create a request to change account's password:
        var newPassword = new ConfirmedPassword("NewPassword@99", "NewPassword@99");
        
        var (domain, recovery) = PasswordRecovery.Create(
            Requests.ValidUserLogin.Email, 
            newPassword, 
            resetToken!);

        domain.Valid.Should().BeTrue("recovery request not valid");
        recovery.Should().NotBeNull("recovery request not created");

        var resetStatus = await loginSrv.ResetPasswordAsync(recovery!);
        
        // Asset:
        resetStatus.Should().NotBeNull("response not returned");
        resetStatus.Valid.Should().BeTrue("account password was not reset");
        
        // -- Attempt to login with new password:
        var (_, login) = UserLogin.Create(Requests.ValidRegistration.Email, newPassword.Chosen, rememberClient: false);
        login.Should().NotBeNull("login was not created");

        var loginResult = await loginSrv.LoginAsync(login!);
        loginResult.Valid.Should().BeTrue("login with reset password didn't work");
    }

    [Fact]
    public async Task NonConfirmedAccount_CannotHavePassword_Reset()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var loginSrv = fixture.GetService<IAuthenticationService>();
        
        await fixture.RegisterNewAccount(Requests.ValidRegistration, confirm: false);
        
        // Act:
        var sendResetStatus = await loginSrv.SendPasswordRecovery(Requests.ValidRegistration.Email);
        
        // Assert:
        var resetToken = fixture.GetAccountRecoveryToken(Requests.ValidRegistration.Email);

        sendResetStatus.Valid.Should().BeFalse("non-confirmed account can't have password reset");
        resetToken.Should().BeNull("reset token should not be sent for non-conformed account");
    }

    [Fact]
    public async Task NonRegisteredAccount_CannotHave_PasswordReset()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var loginSrv = fixture.GetService<IAuthenticationService>();

        // Act:
        var resetResult = await loginSrv.SendPasswordRecovery(Requests.ValidRegistration.Email);
        
        // Assert:
        resetResult.Valid.Should().BeFalse("non-registered account can't have password reset");
    }

    [Fact]
    public async Task LoggedInUser_CanChange_Password()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var loginSrv = fixture.GetService<IAuthenticationService>();
        
        var registrationStatus = await fixture.RegisterNewAccount(Requests.ValidRegistration);
        fixture.SetPrinciple(registrationStatus);
        
        // Act:
        var newPassword = new ConfirmedPassword("NewPassword%34", "NewPassword%34");
        
        var (_, changePassword) = UserChangePassword.Create(
            Requests.UnknownRegistration.ConformedPassword.Chosen,
            newPassword);

        changePassword.Should().NotBeNull("change password request not created");
        var resultStatus = await loginSrv.ChangePassword(changePassword!);
        
        // Assert:
        resultStatus.Valid.Should().BeTrue("password should have been reset");

        var (_, newLogin) = UserLogin.Create(Requests.ValidRegistration.Email, newPassword.Chosen, rememberClient: false);
        newLogin.Should().NotBeNull("login request was not created");

        var loginResult = await loginSrv.LoginAsync(newLogin!);
        loginResult.Valid.Should().BeTrue("login using new password didn't work");
    }
}