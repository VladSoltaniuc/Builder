using System.Net;
using FluentAssertions;
using ProductApi.Contracts;

namespace ProductApi.IntegrationTests.Orders;

[Collection("Integration")]
public class OrdersIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    // --- Options ---

    [Fact]
    public async Task GetOptions_ValidRequest_ReturnsStatuses()
    {
        var response = await GetAsync<OrderOptionsResponse>("/api/orders/options");

        response.Should().NotBeNull();
        response!.Statuses.Should().NotBeEmpty();
        response.Statuses.Should().Contain("Pending");
        response.Statuses.Should().Contain("Completed");
        response.Statuses.Should().Contain("Cancelled");
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
