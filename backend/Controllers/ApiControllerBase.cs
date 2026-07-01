// Presentation layer
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Contracts;

namespace ProductApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected ActionResult ApiBadRequest(string code)
        => BadRequest(Err(400, code));

    private static ErrorResponse Err(int statusCode, string code)
        => new(new ErrorDetail(statusCode, code));
}
