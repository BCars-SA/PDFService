using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace API.Controllers;

[ApiController]
abstract public class BaseController : ControllerBase
{
    protected readonly IConfiguration _configuration;
    protected readonly Logger _logger;

    public static readonly int LIST_LIMIT = 500;

    protected readonly BaseDBContext<User>? _userContext;

    public BaseController(IConfiguration configuration, BaseDBContext<User>? userContext = null)
    {
        _configuration = configuration;
        _logger = NLog.LogManager.GetCurrentClassLogger();
        _userContext = userContext;
    }

    [NonAction]
    public virtual ObjectResult NotFoundProblem(string? detail = null)
    {
        return Problem(
            detail: detail,
            title: "Not Found",
            statusCode: StatusCodes.Status404NotFound,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        );
    }

    [NonAction]
    public virtual ObjectResult UnauthorizedProblem(string? detail = null)
    {
        return Problem(
            detail: detail,
            title: "Unauthorized",
            statusCode: StatusCodes.Status401Unauthorized,
            type: "https://tools.ietf.org/html/rfc7235#section-3.1"
        );
    }

    [NonAction]
    public virtual ObjectResult BadRequestProblem(string? detail = null)
    {
        return Problem(
            detail: detail,
            title: "Bad Request",
            statusCode: StatusCodes.Status400BadRequest,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        );
    }
}

public interface IBaseResponse
{

}

public class SuccessResponse : IBaseResponse
{
    public bool Success { get; set; }
}
