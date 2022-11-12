using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using NetFusion.Identity.App.Extensions;
using NetFusion.Identity.App.Repositories;
using NetFusion.Identity.Client.Services;
using NetFusion.Identity.Infra;
using NetFusion.Identity.Infra.Repositories;
using NetFusion.Identity.Infra.Repositories.Entities;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configure Logging:
InitializeLogger(builder.Configuration);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);
builder.Host.UseSerilog();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(o =>
    {
        o.DefaultScheme = IdentityConstants.ApplicationScheme;
        o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<UserIdentity>(o =>
    {
        o.Stores.MaxLengthForKeys = 128;
        o.SignIn.RequireConfirmedAccount = true;
    })
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.RegisterIdentityConfigurations(builder.Configuration);
builder.Services.RegisterIdentityServices<UserIdentity>();

builder.Services.AddScoped<ILoginInfoService, LoginInfoService<UserIdentity>>();
builder.Services.AddScoped<IClaimsRepository, ClaimsRepository>();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews(options =>
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddScoped(col => {
    var actionContext = col.GetRequiredService<IActionContextAccessor>().ActionContext!;
    var factory = col.GetRequiredService<IUrlHelperFactory>();
    return factory.GetUrlHelper(actionContext);
});

var app = builder.Build();

app.ConfigureDashboardCors();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void InitializeLogger(IConfiguration configuration)
{
    // Send any Serilog configuration issues logs to console.
    Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
    Serilog.Debugging.SelfLog.Enable(Console.Error);

    var logConfig = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext();

    logConfig.WriteTo.Console(theme: AnsiConsoleTheme.Literate);

    var seqUrl = configuration.GetValue<string>("logging:seqUrl");
    if (!string.IsNullOrEmpty(seqUrl))
    {
        logConfig.WriteTo.Seq(seqUrl);
    }

    Log.Logger = logConfig.CreateLogger();
}
