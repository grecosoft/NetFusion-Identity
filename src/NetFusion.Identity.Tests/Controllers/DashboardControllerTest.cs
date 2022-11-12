using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Identity.App.Settings;
using NetFusion.Identity.Client.Controllers;
using NetFusion.Identity.Client.Models.Dashboard;
using NetFusion.Identity.Tests.Controllers.Setup;

namespace NetFusion.Identity.Tests.Controllers;

public class DashboardControllerTest
{

    [Fact]
    public void DashboardView_OnlyListsApplications_UserHasRoleFor()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.Settings.Title = "MockDashboard";
            fixture.Settings.ApplicationConfigs = new[]
            {
                new ApplicationConfig { Name = "TestAppOne", RequiredRoleName = "AppOneRole" },
                new ApplicationConfig { Name = "TestAppTwo", RequiredRoleName = "AppTwoRole" },
                new ApplicationConfig { Name = "TestAppThree", RequiredRoleName = "AppThreeRole" }
            };
        
            fixture.SetUser("mock.user@mock.com", "AppOneRole", "AppTwoRole");
        });
        
        // Act:
        var viewResult = fixture.DashboardController.Applications() as ViewResult;

        // Assert:
        viewResult.Should().NotBeNull("expected view result");
        viewResult!.ViewName.Should().Be(DashboardController.ApplicationsView, "wrong view selected");
        
        var model = viewResult.Model as DashboardModel;
        model.Should().NotBeNull("incorrect model returned");
        model!.Title.Should().Be("MockDashboard");
        model.Applications.Should().HaveCount(2, "user should have access to two applications");
        model.Applications.Should().ContainSingle(a => a.Name == "TestAppOne", "missing application");
        model.Applications.Should().ContainSingle(a => a.Name == "TestAppTwo", "missing application");
    }

    [Fact]
    public void DashboardView_ListsApplication_IfRoleNotSpecified()
    {
        // Arrange:
        var fixture = ControllerTestFixture.Arrange(fixture =>
        {
            fixture.Settings.ApplicationConfigs = new[]
            {
                new ApplicationConfig { Name = "TestAppOne", RequiredRoleName = "AppOneRole" },
                new ApplicationConfig { Name = "TestAppTwo", RequiredRoleName = "AppTwoRole" },
                new ApplicationConfig { Name = "TestAppThree", RequiredRoleName = "" }
            };
            
            fixture.SetUser("mock.user@mock.com", "AppOneRole");
        });
        
        // Act:
        var viewResult = fixture.DashboardController.Applications() as ViewResult;

        // Assert:
        viewResult.Should().NotBeNull("expected view result");
        viewResult!.ViewName.Should().Be(DashboardController.ApplicationsView, "wrong view selected");
        
        var model = viewResult.Model as DashboardModel;
        model.Should().NotBeNull("incorrect model returned");
        model!.Applications.Should().HaveCount(2, "user should have access to two applications");
        model.Applications.Should().ContainSingle(a => a.Name == "TestAppOne", "missing application");
        model.Applications.Should().ContainSingle(a => a.Name == "TestAppThree", "missing application");
    }
}