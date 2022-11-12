using Microsoft.Net.Http.Headers;
using NetFusion.Identity.Client.Models.Authentication;
using NetFusion.Identity.Client.Models.Authenticator;
using NetFusion.Identity.Client.Models.Registration;

namespace NetFusion.Identity.Tests.Integration.Setup.Extensions;

/// <summary>
/// Contains methods for making requests to MVC controller actions.
/// </summary>
public static class RequestExtensions
{
    private const string RegisterNewAccountUrl = "Registration/NewAccount";
    private const string AuthenticationLoginUrl = "Authentication/Login";
    private const string ResendEmailConfirmationUrl = "Registration/SendEmailConfirmation";
    private const string AuthenticatorSetupUrl = "Authenticator/Setup";
    private const string AuthenticatorCodeLoginUrl = "Authentication/TwoFactorAuthenticatorCodeLogin";
    private const string RecoveryCodeLoginUrl = "Authentication/TwoFactorRecoveryCodeLogin";
    private const string AuthenticatorResetUrl = "Authenticator/Reset";
    private const string ResetRecoveryCodesUrl = "AccountSettings/ResetRecoveryCodes";
    private const string DisableTwoFactorUrl = "AccountSettings/Disable";
    private const string SendPasswordRecoveryUrl = "Authentication/SendPasswordRecovery";
    private const string ConfirmRegistrationUrl = "Registration/Confirm/{0}";
    private const string PasswordRecoveryUrl = "Authentication/PasswordRecovery/{0}";
    private const string LogoutUrl = "Authentication/Login";


    public static Task<HttpResponseMessage> RegisterUser(this WebAppTestFixture fixture,
        RegistrationModel registration) =>
        
        fixture.HttpClient.PostAsync(RegisterNewAccountUrl, 
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Email" , registration.Email },
                { "Password", registration.Password },
                { "ConfirmedPassword", registration.ConfirmedPassword }
            }));

    public static Task<HttpResponseMessage> ConfirmEmail(this WebAppTestFixture fixture,
        string email, string token) => fixture.HttpClient.PostAsync(string.Format(ConfirmRegistrationUrl, token), 
        new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email" , email }
        }));
    
    public static Task<HttpResponseMessage> ResendEmailConfirmation(this WebAppTestFixture fixture,
        string email) => fixture.HttpClient.PostAsync(ResendEmailConfirmationUrl, 
        new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email" , email }
        }));
    
    public static Task<HttpResponseMessage>SendEmailRecovery(this WebAppTestFixture fixture, 
        string email) => fixture.HttpClient.PostAsync(SendPasswordRecoveryUrl, 
        new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email" , email }
        }));
    
    public static Task<HttpResponseMessage> RecoverEmail(this WebAppTestFixture fixture, 
        ResetPasswordModel model,
        string token) => fixture.HttpClient.PostAsync(string.Format(PasswordRecoveryUrl, token), 
        new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email" , model.Email },
            { "Password", model.Password },
            { "ConfirmedPassword", model.ConfirmedPassword }
        }));
    
    public static async Task<HttpResponseMessage> Login(this WebAppTestFixture fixture,
        LoginModel login)
    {
        var response = await fixture.HttpClient.PostAsync(AuthenticationLoginUrl, 
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Email" , login.Email },
                { "Password", login.Password }
            }));

        if (response.Headers.TryGetValues(HeaderNames.SetCookie, out var values))
        {
            fixture.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.Cookie,
                values.FirstOrDefault());
        }
        
        return response;
    }

    public static async Task Logout(this WebAppTestFixture fixture)
    {
        await fixture.HttpClient.PostAsync(LogoutUrl, null);
        // fixture.HttpClient.DefaultRequestHeaders.Clear();
    }

    
    public static Task<HttpResponseMessage> GetAuthenticatorSetup(this WebAppTestFixture fixture) =>
        fixture.HttpClient.GetAsync(AuthenticatorSetupUrl);

    public static Task<HttpResponseMessage> ConfigureAuthenticator(this WebAppTestFixture fixture, 
        AuthenticatorSetupModel model) => fixture.HttpClient.PostAsync(AuthenticatorSetupUrl, 
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "SetupToken" , model.SetupToken }
            }));

    public static Task<HttpResponseMessage> TwoFactorAuthenticatorCodeLogin(this WebAppTestFixture fixture,
        TwoFactorLoginModel model) => fixture.HttpClient.PostAsync(AuthenticatorCodeLoginUrl, 
        new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "AuthenticatorCode" , model.AuthenticatorCode }
        }));
    
    public static Task ResetAuthenticator(this WebAppTestFixture fixture) => 
        fixture.HttpClient.PostAsync(AuthenticatorResetUrl, null);
    
    public static Task<HttpResponseMessage> ResetRecoveryCodes(this WebAppTestFixture fixture) =>
        fixture.HttpClient.PostAsync(ResetRecoveryCodesUrl, null);
    
    public static Task<HttpResponseMessage> TwoFactorRecoveryCodeLogin(this WebAppTestFixture fixture,
        TwoFactorLoginModel model) => fixture.HttpClient.PostAsync(RecoveryCodeLoginUrl, 
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "RecoveryCode" , model.RecoveryCode }
            }));
    
    public static Task<HttpResponseMessage> DisableTwoFactor(this WebAppTestFixture fixture) =>
        fixture.HttpClient.PostAsync(DisableTwoFactorUrl, null);
    
}