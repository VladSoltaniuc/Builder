// Presentation layer
using Microsoft.AspNetCore.Mvc;
using ProductApi.Contracts;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet("options")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetOptions() => Ok(orderService.GetOptions());

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> GetAll([FromQuery] OrderQuery query)
        => Ok(await orderService.GetAll(query));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var order = await orderService.GetById(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var created = await orderService.Create(request);
        if (created is null) return NotFound();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponse>> Update(int id, UpdateOrderRequest request)
    {
        var result = await orderService.Update(id, request);
        if (result.IsConflict) return Conflict();
        return result.Order is null ? NotFound() : Ok(result.Order);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await orderService.Delete(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/invoice")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> UploadInvoice(int id, IFormFile file)
    {
        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only PDF files are allowed.");
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("Invoice must be under 10 MB.");

        var result = await orderService.UploadInvoice(id, file);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:int}/invoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var deleted = await orderService.DeleteInvoice(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/invoice")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadInvoice(int id)
    {
        var path = await orderService.GetInvoicePath(id);
        if (path is null) return NotFound();
        return PhysicalFile(path, "application/pdf", $"invoice-{id}.pdf");
    }
}
