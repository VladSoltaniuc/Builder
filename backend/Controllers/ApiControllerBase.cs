// Presentation layer
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Constants;
using ProductApi.Contracts;

namespace ProductApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    // Enforces the minimum search length before any query fires. Returns a 400
    // ActionResult when too short, or null with the trimmed term when valid.
    protected int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected ActionResult? ValidateSearchTerm(string? term, out string trimmed)
    {
        trimmed = term?.Trim() ?? string.Empty;
        return trimmed.Length < SearchDefaults.MinTermLength
            ? ApiBadRequest($"Search term must be at least {SearchDefaults.MinTermLength} characters.")
            : null;
    }

    protected ActionResult ApiNotFound(string message = "The requested resource does not exist.")
        => NotFound(Err(404, "NOT_FOUND", message));

    protected ActionResult ApiBadRequest(string message)
        => BadRequest(Err(400, "INVALID_ARGUMENT", message));

    protected ActionResult ApiConflict(string message = "The resource was modified by another request.")
        => Conflict(Err(409, "CONFLICT", message));

    private static ErrorResponse Err(int code, string status, string message)
        => new(new ErrorDetail(code, status, message));
}
