using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NetFusion.Identity.App.Repositories;
using NetFusion.Identity.App.Services;
using NetFusion.Identity.App.Settings;
using NetFusion.Identity.Infra;
using NetFusion.Identity.Infra.Repositories.Entities;
using NetFusion.Identity.Tests.Mocks;

namespace NetFusion.Identity.Tests.Services.Setup;

/// <summary>
/// Creates an environment under which services can be tested. This consists of an
/// in-memory database and a mocked HttpContext and Principle.
/// </summary>
public class ServiceTestFixture
{
    public IServiceProvider Services { get; }
    public ServiceContext ServiceContext { get; }
    public DefaultHttpContext HttpContext { get; }
    public IdentitySettings IdentitySettings { get; }
    
    public Mock<IClaimsRepository> MockClaimsRepository { get; init; } = null!;

    private ServiceTestFixture(IServiceProvider services, ServiceContext serviceContext, 
        DefaultHttpContext httpContext,
        IdentitySettings identitySettings)
    {
        Services = services;
        ServiceContext = serviceContext;
        HttpContext = httpContext;
        IdentitySettings = identitySettings;
    }
    
    public static ServiceTestFixture Create(Action<IServiceCollection>? services = null)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.Configure<IdentitySettings>(options =>
        {
            options.JwtExpireMinutes = 30;
            options.JwtSecurityKey = "UnitTestKey";
        });
        
        AddAuthentication(serviceCollection);
        AddInMemoryDatabase(serviceCollection);
        AddIdentity(serviceCollection);
        
        var serviceContext = new ServiceContext();
        var httpContext = AddMockedHttpContext(serviceCollection);
        var mockClaimsRepository = new Mock<IClaimsRepository>();
        var identityOptions = new IdentitySettings();
        
        serviceCollection.AddSingleton<IServiceContext>(serviceContext);
        serviceCollection.AddSingleton(mockClaimsRepository.Object);

        serviceCollection.AddSingleton(Options.Create(identityOptions));
        
        // Added needed mocks:
        var mockSender = new Mock<IConfirmationSender>();
        serviceCollection.AddSingleton(mockSender.Object);

        services?.Invoke(serviceCollection);
        
        var servicesProvider = serviceCollection.BuildServiceProvider();
        httpContext.RequestServices = servicesProvider;
        
        InitializeInMemoryDatabase(servicesProvider);

        return new ServiceTestFixture(servicesProvider, serviceContext, httpContext, identityOptions)
        {
            MockClaimsRepository = mockClaimsRepository
        };
    }

    private static void AddAuthentication(IServiceCollection serviceCollection)
    {
        serviceCollection.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();
    }

    private static void AddInMemoryDatabase(IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("UnitTests")
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        );
    }
    
    private static void AddIdentity(IServiceCollection serviceCollection)
    {
        serviceCollection.AddIdentityCore<UserIdentity>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
                o.SignIn.RequireConfirmedAccount = true;
            })
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        
        serviceCollection.RegisterIdentityServices<UserIdentity>();
    }

    private static DefaultHttpContext AddMockedHttpContext(IServiceCollection serviceCollection)
    {
        var defaultContext = new DefaultHttpContext();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        
        mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(defaultContext);
        serviceCollection.AddSingleton(mockHttpContextAccessor.Object);
        return defaultContext;
    }
    
    private static void InitializeInMemoryDatabase(IServiceProvider servicesProvider)
    {
        var context = servicesProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}