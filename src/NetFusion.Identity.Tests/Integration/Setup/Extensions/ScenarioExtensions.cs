using System.Net;
using NetFusion.Identity.Client.Models.Authentication;
using NetFusion.Identity.Client.Models.Authenticator;
using NetFusion.Identity.Client.Models.Registration;
using NetFusion.Identity.Tests.Extensions;

namespace NetFusion.Identity.Tests.Integration.Setup.Extensions;

/// <summary>
/// Contains higher-level scenarios for common related request sequences.
/// </summary>
public static class ScenarioExtensions
{
    public static async Task<HttpResponseMessage> RegisterAccountAndLogin(this WebAppTestFixture fixture,
        RegistrationModel registrationModel)
    {
        await fixture.RegisterUser(registrationModel);
        
        var confirmToken = fixture.GetConfirmationToken(registrationModel.Email);
        
        await fixture.ConfirmEmail(registrationModel.Email, confirmToken);
        
        return await fixture.Login(new LoginModel
        {
            Email = registrationModel.Email,
            Password = registrationModel.Password
        });
    }

    public static async Task<HttpResponseMessage> SetupAuthenticator(this WebAppTestFixture fixture)
    {
        var setupResponse = await fixture.GetAuthenticatorSetup();
        if (setupResponse.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException("authenticator setup information not returned");
        }
        
        var token = fixture.ServiceContext.GetAuthenticatorToken(Models.ValidRegistration.Email);
        return await fixture.ConfigureAuthenticator(new AuthenticatorSetupModel
        {
            SetupToken = token
        });
    }
}