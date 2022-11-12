using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace NetFusion.Identity.Tests.Controllers.Setup.Extensions;

public static class AssertExtensions
{
    public static void ShouldBeRedirectToActionResult(this IActionResult actionResult, 
        string actionName, string? controllerName = null)
    {
        var redirectionAction = actionResult as RedirectToActionResult;
        redirectionAction.Should().NotBeNull($"Expected action result type {nameof(RedirectToActionResult)}");

        if (redirectionAction != null)
        {
            redirectionAction.ActionName.Should().Be(actionName);
            redirectionAction.ControllerName.Should().Be(controllerName);
        }
    }

    public static void ShouldBeViewResult(this IActionResult actionResult, 
        string viewName)
    {
        var viewResult = actionResult as ViewResult;
        viewResult.Should().NotBeNull($"Expected action result type {nameof(ViewResult)}");

        viewResult?.ViewName.Should().Be(viewName);
    }
}