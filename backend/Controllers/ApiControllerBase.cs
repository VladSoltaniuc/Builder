// Presentation layer
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Contracts;

namespace ProductApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected ActionResult ApiNotFound(string code = "RESOURCE_NOT_FOUND")
        => NotFound(Err(404, code));

    protected ActionResult ApiBadRequest(string code)
        => BadRequest(Err(400, code));

    protected ActionResult ApiConflict(string code = "RESOURCE_CONFLICT")
        => Conflict(Err(409, code));

    private static ErrorResponse Err(int statusCode, string code)
        => new(new ErrorDetail(statusCode, code));
}
