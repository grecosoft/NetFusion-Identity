#pragma warning disable CS8618

namespace NetFusion.Identity.Client.Models.AccountSettings;

public class TwoFactorConfigurationModel
{
    public bool HasAuthenticator { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsMachineRemembered { get; set; }
    public int RemainingRecoveryCodes { get; set; }
    public string[] RecoveryCodes { get; set; }
}


