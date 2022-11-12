using FluentAssertions;
using NetFusion.Identity.Domain.TwoFactor.Services;
using NetFusion.Identity.Tests.Services.Setup;
using NetFusion.Identity.Tests.Services.Setup.Extensions;

namespace NetFusion.Identity.Tests.Services;

/// <summary>
/// Unit tests asserting the TwoFactorService implementation using mocked web-runtime
/// dependencies and in-memory database.
/// </summary>
public class TwoFactorServiceTests
{
    [Fact]
    public async Task InitialAccount_TwoFactorDisabled_NoAuthenticator()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var twoFactorSrv = fixture.GetService<ITwoFactorService>();
        
        var registrationStatus = await fixture.RegisterNewAccount(Requests.ValidRegistration, confirm: true);
        fixture.SetPrinciple(registrationStatus);

        // Act:
        var twoFactorConfig = await twoFactorSrv.GetConfiguration();
        
        // Assert:
        twoFactorConfig.IsEnabled.Should().BeFalse("two-factor authentication should be disabled for new account");
        twoFactorConfig.HasAuthenticator.Should().BeFalse("new account should not have configured authenticator");
        twoFactorConfig.RemainingRecoveryCodes.Should().Be(0, "new account should not have recovery codes");
    }

    [Fact]
    public async Task TwoFactor_CanBeDisabled()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();
        var twoFactorSrv = fixture.GetService<ITwoFactorService>();
        
        var registrationStatus = await fixture.RegisterNewAccount(Requests.ValidRegistration, confirm: true);
        fixture.SetPrinciple(registrationStatus);
        
        // -- Enable two-factor by configuring an authenticator:
        await fixture.ActivateAuthenticator();
        
        var twoFactorConfig = await twoFactorSrv.GetConfiguration();
        twoFactorConfig.HasAuthenticator.Should().BeTrue("account should not have configured authenticator");
        twoFactorConfig.IsEnabled.Should().BeTrue("two-factor authentication should be enabled");

        // Act:
        await twoFactorSrv.DisableAsync();
        twoFactorConfig = await twoFactorSrv.GetConfiguration();

        // [ASSERT]
        twoFactorConfig.HasAuthenticator.Should().BeTrue("account should still have an authenticator");
        twoFactorConfig.IsEnabled.Should().BeFalse("two-factor authentication should now be disabled");
    }

    [Fact]
    public async Task RecoveryCodes_CanBe_Generated()
    {
        // Arrange:
        var fixture = ServiceTestFixture.Create();

        var twoFactorSrv = fixture.GetService<ITwoFactorService>();
        fixture.IdentitySettings.NumberRecoveryCodes = 7;
        
        var registrationStatus = await fixture.RegisterNewAccount(Requests.ValidRegistration, confirm: true);
        fixture.SetPrinciple(registrationStatus);
        
        // -- Enable two-factor by configuring an authenticator:
        await fixture.ActivateAuthenticator();
        
        var twoFactorConfig = await twoFactorSrv.GetConfiguration(includeRecoveryCodes: false);
        twoFactorConfig.HasAuthenticator.Should().BeTrue("account should not have configured authenticator");
        twoFactorConfig.IsEnabled.Should().BeTrue("two-factor authentication should be enabled");
        twoFactorConfig.RecoveryCodes.Should().BeEmpty("no recovery codes generated");

        // Act:
        await twoFactorSrv.RegenerateRecoveryCodes();
        twoFactorConfig = await twoFactorSrv.GetConfiguration(includeRecoveryCodes: true);
        
        // Assert:
        twoFactorConfig.RecoveryCodes.Should().HaveCount(fixture.IdentitySettings.NumberRecoveryCodes);
    }
}