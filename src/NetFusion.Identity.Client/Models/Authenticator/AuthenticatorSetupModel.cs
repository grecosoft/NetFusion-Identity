#pragma warning disable CS8618

namespace NetFusion.Identity.Client.Models.Authenticator;

public class AuthenticatorSetupModel
{
    public string? Key { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? Message { get; set; }

    public string SetupToken { get; set; }
}


