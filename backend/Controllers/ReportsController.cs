// Presentation layer
using Microsoft.AspNetCore.Mvc;
using ProductApi.Contracts;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(IReportService reportService) : ApiControllerBase
{
    // Last week's audit metrics: creates/updates/deletes per audited table.
    [HttpGet("weekly-audit")]
    [ProducesResponseType(typeof(List<WeeklyAuditReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WeeklyAuditReportResponse>>> WeeklyAudit()
        => Ok(await reportService.GetWeeklyAuditReport());
}
