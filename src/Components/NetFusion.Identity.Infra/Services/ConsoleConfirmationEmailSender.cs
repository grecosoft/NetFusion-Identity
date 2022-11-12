using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Identity.App.Implementations;
using NetFusion.Identity.App.Services;
using NetFusion.Identity.Domain;

namespace NetFusion.Identity.Infra.Services;

public class ConsoleConfirmationEmailSender : IConfirmationSender
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUrlHelper _urlHelper;
    private readonly UrlEncoderService _tokenEncoderSrv;

    public ConsoleConfirmationEmailSender(
        IHttpContextAccessor contextAccessor,
        IUrlHelper urlHelper,
        UrlEncoderService tokenEncoderSrv)
    {
        _contextAccessor = contextAccessor;
        _urlHelper = urlHelper;
        _tokenEncoderSrv = tokenEncoderSrv;
    }
    
    public Task SendAccountConfirmationAsync(IUserIdentity userIdentity, string confirmationToken)
    {
        string? url = GetUrl(confirmationToken, "Registration", "EmailConfirmation");
        if (url != null)
        {
            WriteEmail(
                userIdentity.Email, 
                "Complete Your Account Setup",
                $"Please set up your account by <a href={url}>clicking here</a>.");  
        }
        
        return Task.CompletedTask;
    }

    public Task SendPasswordRecoveryAsync(IUserIdentity userIdentity, string recoveryToken)
    {
        string? url = GetUrl(recoveryToken, "Authentication", "ResetPassword");
        if (url != null)
        {
            WriteEmail(
                userIdentity.Email, 
                "Complete Your Account Setup",
                $"Please set up your account by <a href={url}>clicking here</a>.");
        }
        
        return Task.CompletedTask;
    }

    private string? GetUrl(string token, string controllerName, string actionName)
    {
        var currentRequest = _contextAccessor.HttpContext?.Request;
        if (currentRequest == null)
        {
            return null;
        }

        string? host = currentRequest.Host.Value;
        string protocol = currentRequest.IsHttps ? "https" : "http";
        string safeToken = _tokenEncoderSrv.Encode(token);

        
        return _urlHelper.ActionLink(actionName, controllerName, new { token = safeToken } , protocol, host);
    }

    private void WriteEmail(string emailAddress, string subject, string htmlMessage)
    {
        Console.WriteLine("---New Email----");
        Console.WriteLine($"To: {emailAddress}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine(HttpUtility.HtmlDecode(htmlMessage));
        Console.WriteLine("-------");
    }
}