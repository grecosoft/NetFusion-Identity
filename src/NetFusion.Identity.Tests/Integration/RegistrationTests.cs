using FluentAssertions;
using NetFusion.Identity.Tests.Integration.Setup;
using NetFusion.Identity.Tests.Integration.Setup.Extensions;

namespace NetFusion.Identity.Tests.Integration;

/// <summary>
/// Tests the creation and confirmation of new accounts.
/// </summary>
public class RegistrationTests
{
    [Fact]
    public async Task UserProvidesValidInfo_SentConfirmationToken_NavigatesToConfirmView()
    {
        var fixture = WebAppTestFixture.Create();
        
        var response = await fixture.RegisterUser(Models.ValidRegistration);
        
        var confirmationToken = fixture.GetConfirmationToken(Models.ValidRegistration.Email);
        confirmationToken.Should().NotBeNullOrEmpty("confirmation token not sent");
        response.ShouldBeRedirectionToEmailConfirmation();
    }

    [Fact]
    public async Task UserProvidesInvalidInfo_RemainsOnRegistrationView()
    {
        var fixture = WebAppTestFixture.Create();
        
        var response = await fixture.RegisterUser(Models.InValidRegistration);
  
        response.ShouldRemainCurrentLocation();
    }

    [Fact]
    public async Task EmailConfirmation_Resent_NavigatesToConfirmView()
    {
        var fixture = WebAppTestFixture.Create();
        await fixture.RegisterUser(Models.ValidRegistration);
        
        var response = await fixture.ResendEmailConfirmation(Models.InValidRegistration.Email);
        
        var confirmationToken = fixture.GetConfirmationToken(Models.ValidRegistration.Email);
        confirmationToken.Should().NotBeNullOrEmpty("confirmation token not sent");
        response.ShouldRemainCurrentLocation();
    }
    
    [Fact]
    public async Task SuccessfulEmailConfirmation_NavigatesTo_LoginView()
    {
        var fixture = WebAppTestFixture.Create();
        await fixture.RegisterUser(Models.ValidRegistration);
        
        var confirmToken = fixture.GetConfirmationToken(Models.ValidRegistration.Email);
        
        var response = await fixture.ConfirmEmail(
            Models.ValidRegistration.Email, 
            confirmToken);
        
        response.ShouldBeRedirectionToLogin();
    }

    [Fact]
    public async Task UnsuccessfulEmailConfirmation_RemainsOnConfirmView()
    {
        var fixture = WebAppTestFixture.Create();
        await fixture.RegisterUser(Models.ValidRegistration);
        
        var confirmToken = fixture.GetConfirmationToken(Models.ValidRegistration.Email);
        
        var response = await fixture.ConfirmEmail(
            Models.ValidRegistration.Email, 
            confirmToken + "Invalid");
        
        response.ShouldRemainCurrentLocation();
    }
}