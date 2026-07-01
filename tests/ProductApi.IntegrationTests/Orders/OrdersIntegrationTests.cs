using System.Net;
using FluentAssertions;
using ProductApi.Contracts;
using ProductApi.Models;

namespace ProductApi.IntegrationTests.Orders;

[Collection("Integration")]
public class OrdersIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    // --- place_order stored function: stock handling ---

    [Fact]
    public async Task Create_DecrementsProductStock()
    {
        var user = await CreateUserAsync();
        var product = await CreateProductAsync(stock: 10);
        try
        {
            await CreateOrderAsync(user.Id, product.Id, quantity: 3);

            var after = await GetAsync<ProductResponse>($"/api/products/{product.Id}");
            after!.Stock.Should().Be(7); // 10 - 3, decremented atomically by place_order()
        }
        finally
        {
            await DeleteAsync($"/api/products/{product.Id}"); // cascades the order
            await DeleteAsync($"/api/users/{user.Id}");
        }
    }

    [Fact]
    public async Task Create_InsufficientStock_Returns400()
    {
        var user = await CreateUserAsync();
        var product = await CreateProductAsync(stock: 1);
        try
        {
            var resp = await PostAsync("/api/orders", new { UserId = user.Id, ProductId = product.Id, Quantity = 5 });

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var after = await GetAsync<ProductResponse>($"/api/products/{product.Id}");
            after!.Stock.Should().Be(1); // unchanged - the function rejected and rolled back
        }
        finally
        {
            await DeleteAsync($"/api/products/{product.Id}");
            await DeleteAsync($"/api/users/{user.Id}");
        }
    }

    [Fact]
    public async Task Create_NonExistentProduct_Returns404()
    {
        var user = await CreateUserAsync();
        try
        {
            var resp = await PostAsync("/api/orders", new { UserId = user.Id, ProductId = 999999, Quantity = 1 });

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            await DeleteAsync($"/api/users/{user.Id}");
        }
    }

    // --- AWB generation ---

    [Fact]
    public async Task GenerateAwb_AssignsAwb()
    {
        var user = await CreateUserAsync();
        var product = await CreateProductAsync(stock: 5);
        try
        {
            var order = await CreateOrderAsync(user.Id, product.Id, 1);
            var resp = await Client.PostAsync($"/api/orders/{order.Id}/awb", null);

            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await ReadAsync<OrderResponse>(resp);
            updated!.Awb.Should().NotBeNullOrEmpty();
        }
        finally
        {
            await DeleteAsync($"/api/products/{product.Id}");
            await DeleteAsync($"/api/users/{user.Id}");
        }
    }

    [Fact]
    public async Task GenerateAwb_NonExistentOrder_Returns404()
    {
        var resp = await Client.PostAsync("/api/orders/999999/awb", null);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Unique AWB + optimistic concurrency ---

    [Fact]
    public async Task Update_DuplicateAwb_Returns409()
    {
        var user = await CreateUserAsync();
        var product = await CreateProductAsync(stock: 10);
        try
        {
            var o1 = await CreateOrderAsync(user.Id, product.Id, 1);
            var o2 = await CreateOrderAsync(user.Id, product.Id, 1);
            var awb = $"AWB{Guid.NewGuid():N}"[..20];

            var put1 = await PutAsync($"/api/orders/{o1.Id}", new { Quantity = 1, Status = "Pending", Version = o1.Version, Awb = awb });
            put1.StatusCode.Should().Be(HttpStatusCode.OK);

            var put2 = await PutAsync($"/api/orders/{o2.Id}", new { Quantity = 1, Status = "Pending", Version = o2.Version, Awb = awb });
            put2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        finally
        {
            await DeleteAsync($"/api/products/{product.Id}");
            await DeleteAsync($"/api/users/{user.Id}");
        }
    }

    [Fact]
    public async Task Update_StaleVersion_Returns409()
    {
        var user = await CreateUserAsync();
        var product = await CreateProductAsync(stock: 10);
        try
        {
            var order = await CreateOrderAsync(user.Id, product.Id, 1);

            var resp = await PutAsync($"/api/orders/{order.Id}",
                new { Quantity = 2, Status = "Pending", Version = 999, Awb = (string?)null });

            resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        finally
        {
            await DeleteAsync($"/api/products/{product.Id}");
            await DeleteAsync($"/api/users/{user.Id}");
        }
    }

    // --- Options ---

    [Fact]
    public async Task GetOptions_ValidRequest_ReturnsStatuses()
    {
        var response = await GetAsync<OrderOptionsResponse>("/api/orders/options");

        response.Should().NotBeNull();
        response!.Statuses.Should().NotBeEmpty();
        response.Statuses.Should().Contain(OrderStatus.Pending);
        response.Statuses.Should().Contain(OrderStatus.Completed);
        response.Statuses.Should().Contain(OrderStatus.Cancelled);
    }

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ValidRequest_ReturnsPagedShape()
    {
        var response = await GetAsync<PagedResponse<OrderResponse>>("/api/orders");

        response.Should().NotBeNull();
        response!.Items.Should().NotBeNull();
        response.Page.Should().Be(1);
        response.PageSize.Should().BeGreaterThan(0);
        response.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_NonExistentOrder_Returns404()
    {
        var response = await DeleteAsync("/api/orders/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
