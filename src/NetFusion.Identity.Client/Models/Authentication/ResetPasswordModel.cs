#pragma warning disable CS8618

namespace NetFusion.Identity.Client.Models.Authentication;

public class ResetPasswordModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmedPassword { get; set; }
}