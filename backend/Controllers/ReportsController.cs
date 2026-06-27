// Presentation layer
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Contracts;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController(IReportService reportService) : ApiControllerBase
{
    // Last week's audit metrics: creates/updates/deletes per audited table.
    [HttpGet("weekly-audit")]
    [ProducesResponseType(typeof(List<WeeklyAuditReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WeeklyAuditReportResponse>>> WeeklyAudit()
        => Ok(await reportService.GetWeeklyAuditReport());

    // Opt the signed-in user in/out of the weekly report email.
    [HttpPut("subscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetSubscription(ReportSubscriptionRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await reportService.SetSubscription(userId, request.Subscribed);
        return NoContent();
    }
}
