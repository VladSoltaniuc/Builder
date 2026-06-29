// Presentation layer
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Contracts;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController(IAuditService auditService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditLogResponse>>> GetHistory(
        [FromQuery] string? table, [FromQuery] int? rowId, [FromQuery] int limit = 50)
        => Ok(await auditService.GetHistory(table, rowId, limit));
}
