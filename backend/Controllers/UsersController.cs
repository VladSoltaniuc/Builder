// Presentation layer
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Constants;
using ProductApi.Contracts;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // any authenticated user may read; writes additionally require Admin
public class UsersController(IUserService userService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetAll([FromQuery] PageQuery query)
        => Ok(await userService.GetAll(query));

    [HttpGet("search")]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<UserResponse>>> Search([FromQuery] string term)
    {
        if (ValidateSearchTerm(term, out var trimmed) is { } error) return error;
        return Ok(await userService.Search(trimmed));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await userService.GetById(id);
        return user is null ? ApiNotFound() : Ok(user);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var created = await userService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Update(int id, UpdateUserRequest request)
    {
        if (id == GetCurrentUserId())
            return ApiBadRequest("You cannot edit your own account.");
        var result = await userService.Update(id, request);
        if (result.IsConflict) return ApiConflict();
        return result.User is null ? ApiNotFound() : Ok(result.User);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == GetCurrentUserId())
            return ApiBadRequest("You cannot delete your own account.");
        var deleted = await userService.Delete(id);
        return deleted ? NoContent() : ApiNotFound();
    }
}
