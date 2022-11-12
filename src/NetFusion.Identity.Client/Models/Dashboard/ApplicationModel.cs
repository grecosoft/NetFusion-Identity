using NetFusion.Identity.App.Settings;

namespace NetFusion.Identity.Client.Models.Dashboard;

public class ApplicationModel
{
    private readonly ApplicationConfig _applicationConfig;
    
    public ApplicationModel(ApplicationConfig applicationConfig)
    {
        _applicationConfig = applicationConfig;
    }

    public string Name => _applicationConfig.Name;
    public string Url => _applicationConfig.Url;
    public string Color => _applicationConfig.Color;
    public string Description => _applicationConfig.Description ?? string.Empty;
    public string? IconName => _applicationConfig.IconName;
}