using System.Net;
using FluentAssertions;
using ProductApi.Contracts;

namespace ProductApi.IntegrationTests.Products;

[Collection("Integration")]
public class ProductsIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    // --- Options ---

    [Fact]
    public async Task GetOptions_ValidRequest_ReturnsCategories()
    {
        var response = await GetAsync<ProductOptionsResponse>("/api/products/options");

        response.Should().NotBeNull();
        response!.Categories.Should().NotBeEmpty();
    }

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ValidRequest_ReturnsPagedShape()
    {
        var response = await GetAsync<PagedResponse<ProductResponse>>("/api/products");

        response.Should().NotBeNull();
        response!.Items.Should().NotBeNull();
        response.Page.Should().Be(1);
        response.PageSize.Should().BeGreaterThan(0);
        response.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    // --- Create ---

    [Fact]
    public async Task Create_WithValidInput_Returns201()
    {
        var input = new { Name = "Integration Test Product", Category = "Peripherals", Price = 99.99m, Stock = 10 };

        var createResponse = await PostAsync("/api/products", input);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ReadAsync<ProductResponse>(createResponse);
        created.Should().NotBeNull();
        created!.Name.Should().Be(input.Name);

        // Cleanup
        await DeleteAsync($"/api/products/{created.Id}");
    }

    [Fact]
    public async Task Create_WithMissingName_Returns400()
    {
        var input = new { Category = "Peripherals", Price = 99.99m, Stock = 10 };

        var response = await PostAsync("/api/products", input);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_NonExistentProduct_Returns404()
    {
        var response = await DeleteAsync("/api/products/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
