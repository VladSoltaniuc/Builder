// Presentation layer
using Microsoft.AspNetCore.Mvc;
using ProductApi.Contracts;

namespace ProductApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ApiNotFound(string message = "The requested resource does not exist.")
        => NotFound(Err(404, "NOT_FOUND", message));

    protected ActionResult ApiBadRequest(string message)
        => BadRequest(Err(400, "INVALID_ARGUMENT", message));

    protected ActionResult ApiConflict(string message = "The resource was modified by another request.")
        => Conflict(Err(409, "CONFLICT", message));

    private static ErrorResponse Err(int code, string status, string message)
        => new(new ErrorDetail(code, status, message));
}
