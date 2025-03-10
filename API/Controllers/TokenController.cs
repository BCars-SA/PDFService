using API.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers;

[ApiController]
public class TokenController : BaseController
{
    private Authenticator _authenticator;

    public TokenController(IConfiguration configuration)
        : base(configuration)
    {
        _authenticator = new Authenticator(configuration);
    }

    [Route("token")]
    [HttpGet]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]    
    public IActionResult GetToken(TokenRequest tokenRequest)
    {        
        string? token = tokenRequest.Username != null ? _authenticator.Authenticate(tokenRequest.Username) : null;
        if (token != null)
            return Ok(new TokenResponse() { Token = token });
        else
            return BadRequestProblem("Username missing (must be any value)");
    }
}

public class TokenRequest
{
    [Required]
    public string? Username { get; set; }    
}

public class TokenResponse
{
    public string? Token { get; set; }
}