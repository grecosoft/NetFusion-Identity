using NetFusion.Identity.App.Services;
using TwoStepsAuthenticator;

namespace NetFusion.Identity.Tests.Extensions;

/// <summary>
/// Contains methods that are based on the internal state recorded by services.
/// </summary>
public static class ServiceContextExtensions
{
    

    public static string[] GetRecoveryCodes(this IServiceContext context, string email)
    {
        if (context.TryGetValue(email, ServiceContextKeys.RecoveryCodes, out string[]? recoveryCodes))
        {
            return recoveryCodes!;
        }

        throw new InvalidOperationException($"Recovery Codes not found for email {email}");
    }
    
    public static string GetAuthenticatorToken(this IServiceContext context, string email)
    {
        if (context.TryGetValue(email, ServiceContextKeys.AuthenticatorKey, out string? secret))
        {
            var authenticator = new TimeAuthenticator();
            return authenticator.GetCode(secret!);
        }

        throw new InvalidOperationException(
            "Authenticator Token could not be generated");
    }

    public static string? GetAuthenticatorSecret(this IServiceContext context, string email) =>
        context.TryGetValue(email, ServiceContextKeys.AuthenticatorKey, out string? value) ? value : null;
}