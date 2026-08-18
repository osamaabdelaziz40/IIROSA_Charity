using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Base API Controller with common functionality
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public abstract class ApiController : Controller
{
    private readonly ILogger<ApiController> _logger;

    protected ApiController(ILogger<ApiController> logger)
    {
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation($"Executing {context.ActionDescriptor.DisplayName}");
        base.OnActionExecuting(context);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation($"Executed {context.ActionDescriptor.DisplayName}");
        base.OnActionExecuted(context);
    }

    protected string? CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    protected string? CurrentUserName => User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    protected string? CurrentUserEmail => User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
}
