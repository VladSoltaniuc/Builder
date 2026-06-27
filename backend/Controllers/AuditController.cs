// Presentation layer — read-only history view
using Microsoft.AspNetCore.Mvc;
using ProductApi.Contracts;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController(IAuditService auditService) : ApiControllerBase
{
    private const int MaxLimit = 200;

    // GET /api/audit?table=Orders&rowId=5&limit=50
    [HttpGet]
    [ProducesResponseType(typeof(List<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditLogResponse>>> GetHistory(
        [FromQuery] string? table, [FromQuery] int? rowId, [FromQuery] int limit = 50)
    {
        limit = Math.Clamp(limit, 1, MaxLimit);
        return Ok(await auditService.GetHistory(table, rowId, limit));
    }
}
