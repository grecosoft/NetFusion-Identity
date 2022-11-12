using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NetFusion.Identity.App.Settings;
using NetFusion.Identity.Client.Models.Dashboard;

namespace NetFusion.Identity.Client.Controllers;

[Authorize]
public class DashboardController : Controller
{
    public const string Name = "Dashboard";
    public const string ApplicationsView = "Applications";
    private const string AccessDeniedView = "AccessDenied";

    private readonly DashboardSettings _dashboardSettings;
    private readonly IHttpContextAccessor _contextAccessor;
    
    public DashboardController(
        IOptions<DashboardSettings> dashboardSettings,
        IHttpContextAccessor contextAccessor)
    {
        _dashboardSettings = dashboardSettings.Value;
        _contextAccessor = contextAccessor;
    }

    [HttpGet]
    public IActionResult Applications()
    {
        var title = string.IsNullOrEmpty(_dashboardSettings.Title) ? "Dashboard" : _dashboardSettings.Title;
        
        var user = _contextAccessor.HttpContext?.User;
        if (user == null)
        {
            return View(ApplicationsView, new DashboardModel(title));
        }
        
        var userAccessibleApps = _dashboardSettings.ApplicationConfigs
            .Where(a => UserHasAccessToApplication(a, user) || ApplicationAccessibleToAllUsers(a))
            .Select(a => new ApplicationModel(a))
            .OrderBy(a => a.Name)
            .ToArray();

        var model = new DashboardModel(title, userAccessibleApps);
        return View(ApplicationsView, model);
    }

    private bool UserHasAccessToApplication(ApplicationConfig appConfig, ClaimsPrincipal user) =>
        !string.IsNullOrEmpty(appConfig.RequiredRoleName) && user.IsInRole(appConfig.RequiredRoleName);

    private bool ApplicationAccessibleToAllUsers(ApplicationConfig appConfig) =>
        string.IsNullOrEmpty(appConfig.RequiredRoleName);

    public IActionResult AccessDenied() => View(AccessDeniedView);
}