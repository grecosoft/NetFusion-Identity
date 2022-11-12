using NetFusion.Identity.Client.Models.Authentication;
using NetFusion.Identity.Client.Models.Registration;

namespace NetFusion.Identity.Tests.Integration.Setup;

/// <summary>
/// Common test models.
/// </summary>
public class Models
{
    public static RegistrationModel ValidRegistration => new()
    {
        Email = "mark.twain@gmail.com",
        Password = "Test$Password99",
        ConfirmedPassword = "Test$Password99"
    };
    
    public static LoginModel ValidLogin => new()
    {
        Email = "mark.twain@gmail.com",
        Password = "Test$Password99"
    };
    
    public static LoginModel InvalidLogin => new()
    {
        Email = "mark.twain@gmail.com",
        Password = "Test$_Invalid_Password99"
    };

    public static RegistrationModel InValidRegistration => new()
    {
        Email = "mark.twain@gmail.com",
        Password = "invalid-password",
        ConfirmedPassword = "invalid-password"
    };
}