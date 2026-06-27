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
public class ProductsController(IProductService productService) : ApiControllerBase
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    [HttpGet("options")]
    [ProducesResponseType<ProductOptionsResponse>(StatusCodes.Status200OK)]
    public ActionResult GetOptions() => Ok(productService.GetOptions());

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll([FromQuery] ProductQuery query)
        => Ok(await productService.GetAll(query));

    [HttpGet("search")]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ProductResponse>>> Search([FromQuery] string term)
    {
        if (ValidateSearchTerm(term, out var trimmed) is { } error) return error;
        return Ok(await productService.Search(trimmed));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await productService.GetById(id);
        return product is null ? ApiNotFound() : Ok(product);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var created = await productService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request)
    {
        var result = await productService.Update(id, request);
        if (result.IsConflict) return ApiConflict();
        return result.Product is null ? ApiNotFound() : Ok(result.Product);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await productService.Delete(id);
        return deleted ? NoContent() : ApiNotFound();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:int}/image")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> UploadImage(int id, IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            return ApiBadRequest("Only JPG, PNG, and WebP images are allowed.");
        if (file.Length > 5 * 1024 * 1024)
            return ApiBadRequest("Image must be under 5 MB.");

        var result = await productService.UploadImage(id, file);
        return result is null ? ApiNotFound() : Ok(result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var deleted = await productService.DeleteImage(id);
        return deleted ? NoContent() : ApiNotFound();
    }

    [HttpGet("export")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Export([FromQuery] string[] columns)
    {
        if (columns.Length == 0) return ApiBadRequest("Specify at least one column.");
        var bytes = await productService.ExportToExcel(columns);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx");
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportProductResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportProductResult>> Import(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return ApiBadRequest("No file provided.");
        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return ApiBadRequest("Only .xlsx files are supported.");
        return Ok(await productService.ImportFromExcel(file));
    }
}
