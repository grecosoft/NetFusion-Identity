namespace NetFusion.Identity.Client.Services;

public interface ILoginInfoService
{
    bool IsLoggedIn { get; }
    string Name { get; set; }
    
    public bool IsAdmin { get; }
}