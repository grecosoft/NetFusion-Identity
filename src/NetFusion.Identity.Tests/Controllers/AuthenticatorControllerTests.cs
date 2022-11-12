using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetFusion.Identity.Client.Controllers;
using NetFusion.Identity.Client.Models.Authenticator;
using NetFusion.Identity.Domain;
using NetFusion.Identity.Domain.TwoFactor.Entities;
using NetFusion.Identity.Domain.Validation;
using NetFusion.Identity.Tests.Controllers.Setup;

namespace NetFusion.Identity.Tests.Controllers;

public class AuthenticatorControllerTests
{

    [Fact]
    public async Task SettingUpAuthenticator_DisplaysView_WithKeyAndQrCodeUrl()
    {
        // Arrange:
        var setup = new AuthenticatorSetup("mock.user@test.com", "AAAABBBBCCCC");
        
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.Settings.AuthenticatorLabel = "TestApp";
            fixture.MockAuthenticatorSrv.Setup(m => m.GetSetupInformationAsync()).ReturnsAsync(setup);
        });
        
        // Act:
        var viewResult = await fixture.AuthenticatorController.Setup() as ViewResult;
        
        // Assert:
        viewResult.Should().NotBeNull("expected view result not returned");

        var model = viewResult!.Model as AuthenticatorSetupModel;
        model.Should().NotBeNull("expected view model not returned");

        model!.Key.Should().Be("aaaa bbbb cccc", 
            "expected key to be split into parts each containing 4 characters.");
        
        model.QrCodeUrl.Should().Be(
            $"otpauth://totp/{fixture.Settings.AuthenticatorLabel}:{setup.Email}?secret={setup.AuthenticatorKey}",
            "QR Code Url was not correctly built");
    }

    [Fact]
    public async Task ValidAuthenticatorSetup_RedirectsTo_TwoFactorConfigurationView()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticatorSrv
                .Setup(m => m.ConfirmSetupTokenAsync(It.Is<string>(v => v == "GeneratedCode")))
                .ReturnsAsync(new ResultStatus());
        });
        
        // Act:
        var model = new AuthenticatorSetupModel { SetupToken = "GeneratedCode"};
        var redirectActionResult = await fixture.AuthenticatorController.Setup(model) as RedirectToActionResult;
        
        // Assert:
        redirectActionResult.Should().NotBeNull("expected redirection to action");
        redirectActionResult!.ActionName.Should().Be(AccountSettingsController.TwoFactorConfigurationView);
        redirectActionResult.ControllerName.Should().Be(AccountSettingsController.Name);
    }

    [Fact]
    public async Task NotValidAuthenticatorSetup_Displays_SetupView()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.MockAuthenticatorSrv
                .Setup(m => m.ConfirmSetupTokenAsync(It.Is<string>(v => v == "GeneratedCode")))
                .ReturnsAsync(() =>
                {
                    var result = new ResultStatus();
                    result.Validations.Add(ValidationLevel.Error, "Invalid setup token.");

                    return result;
                });
        });
        
        // Act:
        var model = new AuthenticatorSetupModel { SetupToken = "GeneratedCode"};
        var viewResult = await fixture.AuthenticatorController.Setup(model) as ViewResult;
        
        // Assert:
        viewResult.Should().NotBeNull("expected view result");
        viewResult!.ViewName.Should().Be(AuthenticatorController.SetupView);
    }

    [Fact]
    public async Task ResettingAuthenticator_RedirectsTo_SetupView()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange();

        // Act:
        var redirectActionResult = await fixture.AuthenticatorController.Reset() as RedirectToActionResult;
        
        // Assert:
        fixture.MockAuthenticatorSrv.Verify(m => m.Reset(), Times.Once, "service not invoked to rest");
        redirectActionResult.Should().NotBeNull("expected view result");
        redirectActionResult!.ActionName.Should().Be(AuthenticatorController.SetupView);
        redirectActionResult.ControllerName.Should().BeNull("should be action of current controller");
    }
}