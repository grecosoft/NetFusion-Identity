using Microsoft.AspNetCore.Mvc;
using Moq;
using NetFusion.Identity.App.Implementations;
using NetFusion.Identity.Client.Controllers;
using NetFusion.Identity.Client.Models.Registration;
using NetFusion.Identity.Domain.Registration.Entities;
using NetFusion.Identity.Domain.Registration.Services;
using NetFusion.Identity.Tests.Controllers.Setup.Extensions;

namespace NetFusion.Identity.Tests.Controllers;

public class RegistrationControllerTests
{
    [Fact]
    public async Task AccountCreated_NonExistentEmail()
    {
        // Arrange:
        var mockRegistrationSrv = new Mock<IRegistrationService>();
        var controller = new RegistrationController(mockRegistrationSrv.Object, new UrlEncoderService());

        mockRegistrationSrv.Setup(m => m.RegisterAsync(It.IsAny<UserRegistration>()))
            .ReturnsAsync(new RegistrationStatus( existingUser: false, pendingConfirmation: false ));

        // Act:
        IActionResult result = await controller.NewAccount(new RegistrationModel
        {
            Email = "json.mock@test.com",
            Password = "!Password99_xyz",
            ConfirmedPassword = "!Password99_xyz"
        });
        
        // Assert:
        result.ShouldBeRedirectToActionResult("ResendEmailConfirmation");
    }

    [Fact]
    public async Task AccountNotCreated_AlreadyRegisteredEmail()
    {
        // Arrange:
        var mockRegistrationSrv = new Mock<IRegistrationService>();
        var controller = new RegistrationController(mockRegistrationSrv.Object, new UrlEncoderService());

        mockRegistrationSrv.Setup(m => m.RegisterAsync(It.IsAny<UserRegistration>()))
            .ReturnsAsync(new RegistrationStatus( existingUser: true, pendingConfirmation: false ));

        // Act:
        IActionResult result = await controller.NewAccount(new RegistrationModel
        {
            Email = "json.mock@test.com",
            Password = "!Password99_xyz",
            ConfirmedPassword = "!Password99_xyz"
        });
        
        // Assert:
        result.ShouldBeViewResult("NewAccount");
    }

    [Fact]
    public async Task AccountNotCreated_PendingEmailConfirmation()
    {
        // Arrange:
        var mockRegistrationSrv = new Mock<IRegistrationService>();
        var controller = new RegistrationController(mockRegistrationSrv.Object, new UrlEncoderService());

        mockRegistrationSrv.Setup(m => m.RegisterAsync(It.IsAny<UserRegistration>()))
            .ReturnsAsync(new RegistrationStatus( existingUser: true, pendingConfirmation: true ));

        // Act:
        IActionResult result = await controller.NewAccount(new RegistrationModel
        {
            Email = "json.mock@test.com",
            Password = "!Password99_xyz",
            ConfirmedPassword = "!Password99_xyz"
        });
        
        // Assert:
        result.ShouldBeViewResult("NewAccount");
    }
}

