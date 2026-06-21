using Microsoft.AspNetCore.Mvc;
using ProductApi.Contracts;
using ProductApi.Services;

namespace ProductApi.Controllers;

/// <summary>
/// Controller-ul expune endpoint-urile HTTP pentru resursa "products".
/// Rolul lui: să traducă HTTP &lt;-&gt; apeluri către serviciu și să întoarcă codul de status corect.
/// NU conține logică de business - aceea stă în IProductService.
/// </summary>
[ApiController]                  // activează validarea automată a modelului + răspunsuri 400 standard
[Route("api/[controller]")]      // [controller] => "products" => ruta de bază este /api/products
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    // Dependency Injection: ASP.NET ne dă automat implementarea înregistrată în Program.cs.
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>GET /api/products?page=1&amp;pageSize=10 - returnează o pagină de produse.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductResponse>), StatusCodes.Status200OK)]
    public ActionResult<PagedResponse<ProductResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        return Ok(_productService.GetAll(page, pageSize));
    }

    /// <summary>GET /api/products/{id} - returnează un singur produs.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProductResponse> GetById(int id)
    {
        var product = _productService.GetById(id);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>POST /api/products - creează un produs nou.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ProductResponse> Create(CreateProductRequest request)
    {
        var created = _productService.Create(request);

        // 201 Created + header Location către noua resursă (best practice REST).
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>PUT /api/products/{id} - actualizează complet un produs existent.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<ProductResponse> Update(int id, UpdateProductRequest request)
    {
        var result = _productService.Update(id, request);
        if (result.IsConflict) return Conflict();
        return result.Product is null ? NotFound() : Ok(result.Product);
    }

    /// <summary>DELETE /api/products/{id} - șterge un produs.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var deleted = _productService.Delete(id);

        // 204 No Content = ștergere reușită, fără body de returnat.
        return deleted ? NoContent() : NotFound();
    }
}
