// Presentation layer
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProductApi.Constants;
using ProductApi.Contracts;
using ProductApi.Models;
using ProductApi.Hubs;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(IOrderService orderService, IHubContext<OrderHub> hub) : ApiControllerBase
{
    [HttpGet("options")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetOptions() => Ok(orderService.GetOptions());

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> GetAll([FromQuery] PageQuery query)
        => Ok(await orderService.GetAll(query));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        return Ok(await orderService.GetById(id));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var created = await orderService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponse>> Update(int id, UpdateOrderRequest request)
    {
        var order = await orderService.Update(id, request);
        await hub.Clients.All.SendAsync("OrderStatusChanged", order);
        return Ok(order);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await orderService.Delete(id);
        return NoContent();
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id:int}/awb")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GenerateAwb(int id)
    {
        return Ok(await orderService.AssignGeneratedAwb(id));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id:int}/invoice")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> UploadInvoice(int id, IFormFile file)
    {
        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return ApiBadRequest("INVOICE_TYPE_INVALID");
        if (file.Length > ImageSettings.MaxInvoiceSizeBytes)
            return ApiBadRequest("INVOICE_TOO_LARGE");

        return Ok(await orderService.UploadInvoice(id, file));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:int}/invoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        await orderService.DeleteInvoice(id);
        return NoContent();
    }

    [HttpGet("{id:int}/invoice")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadInvoice(int id)
    {
        var path = await orderService.GetInvoicePath(id);
        return PhysicalFile(path, "application/pdf", $"invoice-{id}.pdf");
    }
}
