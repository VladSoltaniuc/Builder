// Presentation layer
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Constants;
using ProductApi.Contracts;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(IProductService productService, ProductExcel productExcel) : ApiControllerBase
{

    [HttpGet("options")]
    [ProducesResponseType<ProductOptionsResponse>(StatusCodes.Status200OK)]
    public ActionResult GetOptions() => Ok(productService.GetOptions());

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll([FromQuery] PageQuery query)
        => Ok(await productService.GetAll(query));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await productService.GetById(id);
        return product is null ? ApiNotFound() : Ok(product);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var created = await productService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
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

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await productService.Delete(id);
        return deleted ? NoContent() : ApiNotFound();
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id:int}/image")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> UploadImage(int id, IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageSettings.AllowedExtensions.Contains(ext))
            return ApiBadRequest("IMAGE_TYPE_INVALID");
        if (file.Length > ImageSettings.MaxImageSizeBytes)
            return ApiBadRequest("IMAGE_TOO_LARGE");

        var result = await productService.UploadImage(id, file);
        return result is null ? ApiNotFound() : Ok(result);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
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
        if (columns.Length == 0) return ApiBadRequest("EXPORT_COLUMNS_REQUIRED");
        var bytes = await productExcel.Export(columns);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx");
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportProductResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportProductResult>> Import(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return ApiBadRequest("IMPORT_FILE_REQUIRED");
        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return ApiBadRequest("IMPORT_FILE_TYPE_INVALID");
        return Ok(await productExcel.Import(file));
    }
}
