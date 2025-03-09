using API.Authentication;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers;

[ApiController]
public class LoginController : BaseController
{
    private Authenticator _authenticator;

    public LoginController(IConfiguration configuration, BaseDBContext<User> userContext)
        : base(configuration, userContext)
    {
        _authenticator = new Authenticator(configuration);
    }

    [Route("login")]
    [HttpPost]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Login(LoginRequest loginData)
    {
        var token = _authenticator.Authenticate(_userContext?.Items.Find(loginData.Username), loginData.Password);
        if (token != null)
            return Ok(new TokenResponse(token));
        else
            return UnauthorizedProblem("Invalid username or password");
    }
}

public class LoginRequest
{
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? Password { get; set; }
}

public class TokenResponse
{
    public string Token { get; set; }

    public TokenResponse(string token)
    {
        Token = token;
    }
}