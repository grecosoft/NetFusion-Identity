#pragma warning disable CS8618

namespace NetFusion.Identity.Client.Models.Authentication;

public class TwoFactorLoginModel
{
    public bool RememberClient { get; set; }
    public string AuthenticatorCode { get; set; }
    public string RecoveryCode { get; set; }
    public string ReturnUrl { get; set; }
}