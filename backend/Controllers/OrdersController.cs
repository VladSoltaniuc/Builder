// Presentation layer
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Auth;
using ProductApi.Contracts;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // any authenticated user may read; writes additionally require Admin
public class OrdersController(IOrderService orderService) : ApiControllerBase
{
    [HttpGet("options")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetOptions() => Ok(orderService.GetOptions());

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> GetAll([FromQuery] OrderQuery query)
        => Ok(await orderService.GetAll(query));

    [HttpGet("search")]
    [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<OrderResponse>>> Search([FromQuery] string term)
    {
        if (ValidateSearchTerm(term, out var trimmed) is { } error) return error;
        return Ok(await orderService.Search(trimmed));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var order = await orderService.GetById(id);
        return order is null ? ApiNotFound() : Ok(order);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var created = await orderService.Create(request);
        if (created is null) return ApiNotFound();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponse>> Update(int id, UpdateOrderRequest request)
    {
        var result = await orderService.Update(id, request);
        if (result.IsConflict) return ApiConflict();
        return result.Order is null ? ApiNotFound() : Ok(result.Order);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await orderService.Delete(id);
        return deleted ? NoContent() : ApiNotFound();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:int}/awb")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GenerateAwb(int id)
    {
        var result = await orderService.AssignGeneratedAwb(id);
        return result is null ? ApiNotFound() : Ok(result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:int}/invoice")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> UploadInvoice(int id, IFormFile file)
    {
        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return ApiBadRequest("Only PDF files are allowed.");
        if (file.Length > 10 * 1024 * 1024)
            return ApiBadRequest("Invoice must be under 10 MB.");

        var result = await orderService.UploadInvoice(id, file);
        return result is null ? ApiNotFound() : Ok(result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}/invoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var deleted = await orderService.DeleteInvoice(id);
        return deleted ? NoContent() : ApiNotFound();
    }

    [HttpGet("{id:int}/invoice")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadInvoice(int id)
    {
        var path = await orderService.GetInvoicePath(id);
        if (path is null) return ApiNotFound();
        return PhysicalFile(path, "application/pdf", $"invoice-{id}.pdf");
    }
}
