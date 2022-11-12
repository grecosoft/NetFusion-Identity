#pragma warning disable CS8618

namespace NetFusion.Identity.Client.Models.Registration;

public class RegistrationModel
{
    public string Email { get; set; }
    
    public string Password { get; set; }
    public string ConfirmedPassword { get; set; }
    
    public string? PhoneNumber { get; set; }
}


