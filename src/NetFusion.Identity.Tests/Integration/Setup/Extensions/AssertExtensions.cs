using System.Net;
using FluentAssertions;

namespace NetFusion.Identity.Tests.Integration.Setup.Extensions;

/// <summary>
/// Extension methods for asserting responses from ASP.NET controller action methods.
/// </summary>
public static class AssertExtensions
{
    private const string LoginUrl = "/Authentication/Login";
    private const string DashboardApplicationsUrl = "/Dashboard/Applications";
    private const string TwoFactorConfigurationUrl = "/AccountSettings/TwoFactorConfiguration";
    private const string TwoFactorLoginUrl = "/Authentication/TwoFactorLogin";
    private const string EmailConfirmationUrl = "/Registration/ResendEmailConfirmation";
    private const string TwoFactorRecoveryCodesUrl = "/AccountSettings/TwoFactorRecoveryCodes";
    
    public static void ShouldBeRedirection(this HttpResponseMessage response, string url)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Redirect, "should be redirection status code");
        response.Headers.Location.Should().Be(url, $"should be redirection to {url}");
    }

    public static void ShouldRemainCurrentLocation(this HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK, "should be redirection status code");
        response.Headers.Location.Should().BeNull("server should not have specified location");
    }

    public static void ShouldBeRedirectionToLogin(this HttpResponseMessage response) =>
        response.ShouldBeRedirection(LoginUrl);
    
    public static void ShouldBeRedirectionToDashboard(this HttpResponseMessage response) =>
        response.ShouldBeRedirection(DashboardApplicationsUrl);
    
    public static void ShouldBeRedirectionToTwoFactorConfig(this HttpResponseMessage response) =>
        response.ShouldBeRedirection(TwoFactorConfigurationUrl);
    
    public static void ShouldBeRedirectionToTwoFactorLogin(this HttpResponseMessage response) =>
        response.ShouldBeRedirection(TwoFactorLoginUrl);
    
    public static void ShouldBeRedirectionToEmailConfirmation(this HttpResponseMessage response, string email) =>
        response.ShouldBeRedirection(EmailConfirmationUrl + $"?email={email}");
    
    public static void ShouldBeRedirectionToEmailConfirmation(this HttpResponseMessage response) =>
        response.ShouldBeRedirection(EmailConfirmationUrl);
    
    public static void ShouldBeRedirectionToTwoFactoryRecoveryCodes(this HttpResponseMessage response) =>
        response.ShouldBeRedirection(TwoFactorRecoveryCodesUrl);
}