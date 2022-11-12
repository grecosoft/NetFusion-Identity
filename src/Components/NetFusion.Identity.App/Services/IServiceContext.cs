using System.Diagnostics.CodeAnalysis;

namespace NetFusion.Identity.App.Services;

/// <summary>
/// Called by service implementations to record values during processing.  Only a Null
/// Implementation of this interface should be used in production code.  An implementation
/// of this interface can be used during unit-tests to validate recorded values.
/// </summary>
public interface IServiceContext
{
    public void RecordValue<TValue>(string source, string name, [DisallowNull] TValue value);

    public bool TryGetValue<TValue>(string source, string name, out TValue? value);
}

public static class ServiceContextKeys
{
    public const string AccountConfirmationToken = "AccountConfirmationToken";
    public const string RecoveryCodes = "RecoveryCodes";
    public const string PasswordRecoveryToken = "PasswordRecoveryToken";
    public const string AuthenticatorKey = "AuthenticatorKey";
}
