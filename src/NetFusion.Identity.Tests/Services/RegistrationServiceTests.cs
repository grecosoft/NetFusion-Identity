using FluentAssertions;
using NetFusion.Identity.Domain.Registration.Entities;
using NetFusion.Identity.Domain.Registration.Services;
using NetFusion.Identity.Tests.Services.Setup;
using NetFusion.Identity.Tests.Services.Setup.Extensions;

namespace NetFusion.Identity.Tests.Services;

/// <summary>
/// Unit tests asserting the RegistrationService implementation using mocked web-runtime
/// dependencies and in-memory database.
/// </summary>
public class RegistrationServiceTests
{
    [Fact]
    public void EmailRequired_ToCreate_Account()
    {
        var (domain, registration) = UserRegistration.Create(string.Empty, 
            new ConfirmedPassword("A", "A"));

        domain.Valid.Should().BeFalse("email must be specified to create account");
        domain.Items.Should().HaveCount(1);
        registration.Should().BeNull("registration crated for invalid email");
    }

    [Fact]
    public void MatchingPasswordRequired_ToCreate_Account()
    {
        var (domain, registration) = UserRegistration.Create("user@mock.com", 
            new ConfirmedPassword("Password1", "Password2"));

        domain.Valid.Should().BeFalse("matching passwords must be specified");
        domain.Items.Should().HaveCount(1);
        registration.Should().BeNull("registration created for non-matching passwords");
    }
    
    [Fact]
    public async Task AccountCreated_ConfirmationSet_WhenEmailNotAlreadyUsed()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var registrationSrv = fixture.GetService<IRegistrationService>();

        // Act:
        var registrationStatus = await registrationSrv.RegisterAsync(Requests.ValidRegistration);
        var confirmationToken = fixture.GetAccountConfirmationToken(Requests.ValidRegistration.Email);
        
        // Assert:
        registrationStatus.Should().NotBeNull("registration status should be returned");
        registrationStatus.Valid.Should().BeTrue("should have success status");
        registrationStatus.ExistingUser.Should().BeFalse("existing user should not be reported");
        registrationStatus.PendingConfirmation.Should().BeFalse("existing pending confirmation should not be reported");
        registrationStatus.Validations.Valid.Should().BeTrue("should have no failed validations");
        registrationStatus.Id.Should().NotBeNull("user assigned identifier should be returned");
        registrationStatus.Email.Should().NotBeEmpty("email address used for registration should be returned");
        confirmationToken.Should().NotBeEmpty("confirmation token should have been sent");
    }

    [Fact]
    public async Task AccountNotCreated_NoConfirmationSent_WhenEmailAlreadyUsed()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var registrationSrv = fixture.GetService<IRegistrationService>();
        
        // -- Create first account for email:
        await registrationSrv.RegisterAsync(Requests.ValidRegistration);
        fixture.ClearAccountConfirmation(Requests.ValidRegistration.Email);
        
        // Act:
        // -- Attempt to created another account using same email:
        var registrationStatus = await registrationSrv.RegisterAsync(Requests.ValidRegistration);
        var confirmationToken = fixture.GetAccountConfirmationToken(Requests.ValidRegistration.Email);
        
        // Assert:
        registrationStatus.Should().NotBeNull("registration status should be returned");
        registrationStatus.Valid.Should().BeFalse("should have unsuccessful status");
        registrationStatus.ExistingUser.Should().BeTrue("existing user should have been reported");
        registrationStatus.PendingConfirmation.Should().BeTrue("existing pending confirmation should have been reported");
        registrationStatus.Validations.Valid.Should().BeFalse("should have failed validations");
        registrationStatus.Id.Should().BeNull("user identifier should not be returned for non-created account");
        registrationStatus.Email.Should().BeNull("email address should not be returned for non-created account");
        confirmationToken.Should().BeNull("confirmation should not be sent for non-created account");
    }

    [Fact]
    public async Task AccountConfirmed_ForMatching_EmailAndToken()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var registrationSrv = fixture.GetService<IRegistrationService>();
        
        await registrationSrv.RegisterAsync(Requests.ValidRegistration);
        var confirmationToken = fixture.GetAccountConfirmationToken(Requests.ValidRegistration.Email);

        confirmationToken.Should().NotBeNull("confirmation token not issued");
        
        // Act:
        var (domain, confirmation) = AccountConfirmation.Create(Requests.ValidRegistration.Email, confirmationToken!);

        domain.Valid.Should().BeTrue("confirmation should be valid");
        confirmation.Should().NotBeNull("confirmation not created");
        
        var confirmEmailStatus = await registrationSrv.ConfirmEmailAsync(confirmation!);
        
        // Assert:
        confirmEmailStatus.Valid.Should().BeTrue("account should have been successfully confirmed");
        confirmEmailStatus.ExistingUser.Should().BeTrue("account should have existed");
        confirmEmailStatus.PendingConfirmation.Should().BeTrue("account should have pending confirmation");
        confirmEmailStatus.Validations.Valid.Should().BeTrue("no validations should have been reported");
    }

    [Fact]
    public async Task AccountNotConfirmed_ForInvalidEmail()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var registrationSrv = fixture.GetService<IRegistrationService>();
        
        // Act:
        var (domain, confirmation) = AccountConfirmation.Create(Requests.UnknownRegistration.Email, "Unknown-Token");
        domain.Valid.Should().BeTrue("confirmation should be valid");
        confirmation.Should().NotBeNull("confirmation not created");
        
        var confirmEmailStatus = await registrationSrv.ConfirmEmailAsync(confirmation!);
        
        // Assert:
        confirmEmailStatus.Valid.Should().BeFalse("confirmation should not succeed for unknown email");
        confirmEmailStatus.ExistingUser.Should().BeFalse("existing user should not be found");
        confirmEmailStatus.PendingConfirmation.Should().BeFalse("pending confirmation should not exist.");
        confirmEmailStatus.Validations.Valid.Should().BeFalse("should have failed validations");
    }

    [Fact]
    public async Task AccountNotConfirmed_ForInvalidToken()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var registrationSrv = fixture.GetService<IRegistrationService>();
        
        var registrationStatus = await registrationSrv.RegisterAsync(Requests.ValidRegistration);
        registrationStatus.Valid.Should().BeTrue("account registration failed");
        
        // Act:
        var (domain, confirmation) = AccountConfirmation.Create(Requests.ValidRegistration.Email, "Unknown-Token");
        domain.Valid.Should().BeTrue("confirmation should be valid");
        confirmation.Should().NotBeNull("confirmation not created");
        
        var confirmEmailStatus = await registrationSrv.ConfirmEmailAsync(confirmation!);
        
        // Assert:
        confirmEmailStatus.Valid.Should().BeFalse("confirmation should fail for invalid token");
        confirmEmailStatus.Validations.Valid.Should().BeFalse("failed validation should be reported");
        confirmEmailStatus.ExistingUser.Should().BeTrue("there should have been an user for account");
        confirmEmailStatus.PendingConfirmation.Should().BeTrue("there should currently be a pending confirmation");
    }

    [Fact]
    public async Task AccountNotConfirmed_WhenAlreadyConfirmed()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var registrationSrv = fixture.GetService<IRegistrationService>();
        
        // -- Create and confirm account:
        await fixture.RegisterNewAccount(Requests.ValidRegistration, confirm: true);
        
        var confirmationToken = fixture.GetAccountConfirmationToken(Requests.ValidRegistration.Email);
        confirmationToken.Should().NotBeNull("confirmation should have been sent");
        
        // Act:
        var (domain, confirmation) = AccountConfirmation.Create(Requests.ValidRegistration.Email, confirmationToken!);
        domain.Valid.Should().BeTrue("confirmation should be valid");
        confirmation.Should().NotBeNull("confirmation not created");

        // -- Attempt to confirm the account again:
        var confirmEmailStatus = await registrationSrv.ConfirmEmailAsync(confirmation!);
        
        // Assert:
        confirmEmailStatus.Valid.Should().BeFalse("account can't be confirmed twice");
        confirmEmailStatus.ExistingUser.Should().BeTrue("account should have existed for email");
        confirmEmailStatus.PendingConfirmation.Should().BeFalse("account should have already been confirmed");
    }

    [Fact]
    public async Task RegisteredEmail_WithPendingConfirmation_CanBeResent()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var registrationSrv = fixture.GetService<IRegistrationService>();
        
        await fixture.RegisterNewAccount(Requests.ValidRegistration, confirm: false);
        
        // -- Clear current confirmation and resend:
        fixture.ClearAccountConfirmation(Requests.ValidRegistration.Email);
        
        // Act:
        var confirmEmailStatus = await registrationSrv.ResendEmailConfirmationAsync(Requests.ValidRegistration.Email);
        var confirmationToken = fixture.GetAccountConfirmationToken(Requests.ValidRegistration.Email);
        
        // Assert:
        confirmEmailStatus.Valid.Should().BeTrue("confirmation should have been resent");
        confirmationToken.Should().NotBeNull("confirmation token should have been resent");
    }

    [Fact]
    public async Task AlreadyConfirmedAccount_CannotHaveConfirmationResent()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var registrationSrv = fixture.GetService<IRegistrationService>();
        
        await fixture.RegisterNewAccount(Requests.ValidRegistration, confirm: true);
        
        // -- Clear current confirmation and resend:
        fixture.ClearAccountConfirmation(Requests.ValidRegistration.Email);
        
        // Act:
        var confirmEmailStatus = await registrationSrv.ResendEmailConfirmationAsync(Requests.ValidRegistration.Email);
        
        // Assert:
        confirmEmailStatus.Valid.Should().BeFalse("already confirmed account can't have confirmation resent");
        confirmEmailStatus.ExistingUser.Should().BeTrue("existing user should have been reported");
        confirmEmailStatus.PendingConfirmation.Should().BeFalse("account should have already been confirmed");
    }
}
