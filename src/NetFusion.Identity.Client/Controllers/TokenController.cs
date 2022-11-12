using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Identity.Domain.Authentication.Services;

namespace NetFusion.Identity.Client.Controllers;

[ApiController, Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private ITokenService _tokenSrv;
    
    public TokenController(
        ITokenService tokenSrv)
    {
        _tokenSrv = tokenSrv;
    }

    [Authorize]
    [HttpGet("jwt/{scopeId}")]
    public async Task<IActionResult> GetJwtToken(Guid scopeId)
    {
        return Ok(await _tokenSrv.CreateJwtToken(scopeId));
    }
}


