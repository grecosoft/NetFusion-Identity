using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NetFusion.Identity.App.Implementations;
using NetFusion.Identity.App.Settings;
using NetFusion.Identity.Client.Controllers;
using NetFusion.Identity.Domain.Authentication.Services;
using NetFusion.Identity.Domain.TwoFactor.Services;

namespace NetFusion.Identity.Tests.Controllers.Setup;

public class ControllerTestFixture
{
    public DashboardSettings Settings { get; } = new();
    public DefaultHttpContext HttpContext { get; } = new();

    public Mock<IHttpContextAccessor>  MockContextAccessor { get; } = new();
    public Mock<IAuthenticatorService> MockAuthenticatorSrv { get; } = new();
    public Mock<IAuthenticationService> MockAuthenticationSrv { get; } = new();
    public Mock<ITwoFactorService> MockTwoFactorSrv { get; } = new();

    private ControllerTestFixture()
    {
        MockContextAccessor.Setup(m => m.HttpContext).Returns(HttpContext);
    }

    public static ControllerTestFixture Arrange(Action<ControllerTestFixture>? fixture = null)
    {
        var fixtureObj = new ControllerTestFixture();
        fixture?.Invoke(fixtureObj);

        return fixtureObj;
    }
        
    public void SetUser(string name, params string[] roles)
    {
        var identity = new GenericIdentity(name);
        HttpContext.User = new GenericPrincipal(identity, roles);
    }

    public DashboardController DashboardController => new(
        Options.Create(Settings), 
        MockContextAccessor.Object);
    
    public AuthenticatorController AuthenticatorController => new(
        Options.Create(Settings), 
        MockAuthenticatorSrv.Object);

    public AuthenticationController AuthenticationController => new(
        MockAuthenticationSrv.Object,
        MockTwoFactorSrv.Object, 
        MockAuthenticatorSrv.Object, 
        new UrlEncoderService());
}