using NetFusion.Identity.Domain.Authentication.Entities;
using NetFusion.Identity.Domain.Registration.Entities;

namespace NetFusion.Identity.Tests.Services.Setup;

public static class Requests
{
    public static UserRegistration ValidRegistration { get; }
    public static UserRegistration UnknownRegistration { get; }
    public static UserLogin ValidUserLogin { get; }
    public static UserLogin InvalidUserLogin { get; }
    
    static Requests()
    {
        var (_, validReg) = UserRegistration.Create("user@mock.com", 
            new ConfirmedPassword("Volvo4Live99#", "Volvo4Live99#"));

        ValidRegistration = validReg!;
        
        var (_, unknownReg) = UserRegistration.Create("unexpected_user@mock.com", 
            new ConfirmedPassword("Volvo4Live99#", "Volvo4Live99#"));

        UnknownRegistration = unknownReg!;
        
        var (_, login) = UserLogin.Create("user@mock.com", "Volvo4Live99#", rememberClient: false);
        ValidUserLogin = login!;
        
        var (_, invalidLogin) = UserLogin.Create("user@mock.com", "Saab4Live99#", rememberClient: false);
        InvalidUserLogin = invalidLogin!;
    }
}