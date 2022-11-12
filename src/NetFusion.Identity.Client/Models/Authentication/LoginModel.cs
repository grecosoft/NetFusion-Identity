#pragma warning disable CS8618

namespace NetFusion.Identity.Client.Models.Authentication;

public class LoginModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public bool RememberClient { get; set; }
    public string ReturnUrl { get; set; }
}

