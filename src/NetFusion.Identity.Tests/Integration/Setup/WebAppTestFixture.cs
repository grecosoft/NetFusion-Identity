using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetFusion.Identity.App.Implementations;
using NetFusion.Identity.App.Repositories;
using NetFusion.Identity.App.Services;
using NetFusion.Identity.App.Settings;
using NetFusion.Identity.Client.Controllers;
using NetFusion.Identity.Client.Services;
using NetFusion.Identity.Infra;
using NetFusion.Identity.Infra.Repositories.Entities;
using NetFusion.Identity.Tests.Mocks;

namespace NetFusion.Identity.Tests.Integration.Setup;

/// <summary>
/// Test fixture for configuring an instance of NetFusion.Identity.Client executing in
/// memory to which HTTP requests can be made.  These tests execute using an in-memory
/// database instance.
/// </summary>
public class WebAppTestFixture
{
    public IServiceProvider Services { get; private set; }
    public HttpClient HttpClient { get; private set; }

    private WebAppTestFixture(TestServer testServer)
    {
        Services = testServer.Services;
        HttpClient = testServer.CreateClient();
    }

    public IServiceContext ServiceContext => Services.GetRequiredService<IServiceContext>();
    
    public static WebAppTestFixture Create(Action<IServiceCollection>? services = null)
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder
            .ConfigureServices((_, serviceColl) =>
            {
                // Configurations:
                serviceColl.Configure<IdentitySettings>(options =>
                {
                    options.JwtExpireMinutes = 60;
                    options.JwtSecurityKey = "UnitTestKey";
                });
                
                // Authentication:
                serviceColl.AddAuthentication(o =>
                    {
                        o.DefaultScheme = IdentityConstants.ApplicationScheme;
                        o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                    })
                    .AddIdentityCookies();

                // In-Memory database context:
                serviceColl.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("UnitTests")
                        .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                );
                
                // ASP.NET Identity Core:
                serviceColl.AddIdentityCore<UserIdentity>(o =>
                    {
                        o.Stores.MaxLengthForKeys = 128;
                        o.SignIn.RequireConfirmedAccount = true;
                    })
                    .AddSignInManager()
                    .AddDefaultTokenProviders()
                    .AddEntityFrameworkStores<ApplicationDbContext>();
                
                // NetFusion Identity Service:
                serviceColl.RegisterIdentityServices<UserIdentity>();
                
                serviceColl.AddControllersWithViews()
                    .AddApplicationPart(typeof(AuthenticationController).Assembly);
                
                // Additional Services:
                serviceColl.AddSingleton<IServiceContext, ServiceContext>();
                serviceColl.AddScoped<ILoginInfoService, LoginInfoService<UserIdentity>>();

                // Mocked Services:
                var mockClaimsRepository = new Mock<IClaimsRepository>();
                serviceColl.AddSingleton(mockClaimsRepository.Object);

                var mockSender = new Mock<IConfirmationSender>();
                serviceColl.AddSingleton(mockSender.Object);
                
                // Unit-Test specific services:
                services?.Invoke(serviceColl);
            })
            .Configure(builder =>
            {
                builder.UseRouting();

                builder.UseAuthentication();
                builder.UseAuthorization();

                builder.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                });
            })
            .UseSetting(WebHostDefaults.ApplicationKey, typeof(WebAppTestFixture).Assembly.FullName);

        var testServer = new TestServer(hostBuilder);

        // Initialize in-memory database:
        using var context = testServer.Services.GetRequiredService<ApplicationDbContext>();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        return new WebAppTestFixture(testServer);
    }
    
    public string GetConfirmationToken(string email)
    {
        if (ServiceContext.TryGetValue(email, ServiceContextKeys.AccountConfirmationToken, out string? token))
        {
            var encoder = Services.GetRequiredService<UrlEncoderService>();
            return encoder.Encode(token!);
        }

        throw new InvalidOperationException($"Confirmation Token not found for email {email}");
    }
    
    public string GetPasswordRecoveryToken(string email)
    {
        if (ServiceContext.TryGetValue(email, ServiceContextKeys.PasswordRecoveryToken, out string? token))
        {
            var encoder = Services.GetRequiredService<UrlEncoderService>();
            return encoder.Encode(token!);
        }

        throw new InvalidOperationException($"Recovery Token not found for email {email}");
    }
}