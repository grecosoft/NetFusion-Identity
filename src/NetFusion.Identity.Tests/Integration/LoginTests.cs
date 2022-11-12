using System.Net;
using FluentAssertions;
using NetFusion.Identity.Client.Models.Authentication;
using NetFusion.Identity.Tests.Integration.Setup;
using NetFusion.Identity.Tests.Integration.Setup.Extensions;

namespace NetFusion.Identity.Tests.Integration;

/// <summary>
/// Tests logging in scenarios based on the credentials and current state of the account.
/// </summary>
public class LoginTests
{
    [Fact]
    public async Task LoginAttempt_ForNonConfirmedEmail_NavigatesToConfirmView()
    {
        // Arrange:
        var fixture = WebAppTestFixture.Create();
        await fixture.RegisterUser(Models.ValidRegistration);
        
        // Act:
        var response = await fixture.Login(Models.ValidLogin);
        
        // Assert:
        response.ShouldBeRedirectionToEmailConfirmation(Models.ValidLogin.Email);
    }
    
    [Fact]
    public async Task RegisteredUser_NavigatesToDashboard_WhenValidLogin()
    {
        var fixture = WebAppTestFixture.Create();
        
        // -- Register new user and confirm account:
        await fixture.RegisterUser(Models.ValidRegistration);
        
        var confirmToken = fixture.GetConfirmationToken(Models.ValidRegistration.Email);
        await fixture.ConfirmEmail(Models.ValidRegistration.Email, confirmToken);

        // -- Attempt to login:
        var loginResponse = await fixture.Login(Models.ValidLogin);
        loginResponse.ShouldBeRedirectionToDashboard();
    }

    [Fact]
    public async Task UserRemainsOnLoginView_UntilValidCredentials_Specified()
    {
        var fixture = WebAppTestFixture.Create();
        
        // -- Register new user and confirm account:
        await fixture.RegisterUser(Models.ValidRegistration);
        
        var confirmToken = fixture.GetConfirmationToken(Models.ValidRegistration.Email);
        await fixture.ConfirmEmail(Models.ValidRegistration.Email, confirmToken);
        
        // -- attempting to login within invalid password does not redirect user:
        var loginResult = await fixture.Login(Models.InvalidLogin);
        
        loginResult.Should().HaveStatusCode(HttpStatusCode.OK);
        loginResult.ShouldRemainCurrentLocation();
    }

    [Fact]
    public async Task AccountCanBeSent_RecoveryToken_UsedToResetPassword()
    {
        var fixture = WebAppTestFixture.Create();

        // -- create new account:
        await fixture.RegisterAccountAndLogin(Models.ValidRegistration);
        await fixture.Logout();

        // -- request a recovery token be sent:
        var recoverySendResult = await fixture.SendEmailRecovery(Models.ValidRegistration.Email);
        recoverySendResult.Should().HaveStatusCode(HttpStatusCode.OK);
        
        // -- get the sent recovery token and send request to change password:
        var passwordRecoverToken = fixture.GetPasswordRecoveryToken(Models.ValidRegistration.Email);

        var recoveryResult = await fixture.RecoverEmail(new ResetPasswordModel
        {
            Email = Models.ValidRegistration.Email,
            Password = Models.ValidRegistration.Password + "@@@",
            ConfirmedPassword = Models.ValidRegistration.ConfirmedPassword + "@@@"
        }, passwordRecoverToken);
        
        recoveryResult.ShouldBeRedirectionToLogin();
        
        // -- attempt to login with new password:
        var loginResult = await fixture.Login(new LoginModel
        {
            Email = Models.ValidLogin.Email,
            Password = Models.ValidLogin.Password + "@@@"
        });

        loginResult.ShouldBeRedirectionToDashboard();
    }
}