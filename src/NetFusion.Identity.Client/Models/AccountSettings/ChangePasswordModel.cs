#pragma warning disable CS8618

namespace NetFusion.Identity.Client.Models.AccountSettings;

public class ChangePasswordModel
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmedPassword { get; set; }
}
