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
        return Ok(await productService.GetById(id));
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
        return Ok(await productService.Update(id, request));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await productService.Delete(id);
        return NoContent();
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

        return Ok(await productService.UploadImage(id, file));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int id)
    {
        await productService.DeleteImage(id);
        return NoContent();
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
