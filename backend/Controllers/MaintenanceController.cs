// Presentation layer
using Microsoft.AspNetCore.Mvc;
using ProductApi.Constants;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController(IMaintenanceService maintenanceService) : ApiControllerBase
{
    [HttpPost("purge-audit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PurgeAudit([FromQuery] int olderThanDays = AuditDefaults.DefaultPurgeDays)
    {
        if (olderThanDays < 0)
            return ApiBadRequest("olderThanDays must be zero or greater.");

        await maintenanceService.PurgeAudit(olderThanDays);
        return NoContent();
    }
}
