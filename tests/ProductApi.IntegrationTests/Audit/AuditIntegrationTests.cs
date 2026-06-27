using FluentAssertions;
using ProductApi.Contracts;

namespace ProductApi.IntegrationTests.Audit;

[Collection("Integration")]
public class AuditIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreatingOrder_WritesInsertAuditRow()
    {
        var user = await CreateUserAsync();
        var product = await CreateProductAsync(stock: 5);
        var order = await CreateOrderAsync(user.Id, product.Id, 1);

        // The DB trigger should have logged the insert automatically.
        var history = await GetAsync<List<AuditLogResponse>>($"/api/audit?table=Orders&rowId={order.Id}");

        history.Should().NotBeNull();
        history!.Should().Contain(a => a.Action == "INSERT" && a.RowId == order.Id);

        await DeleteAsync($"/api/products/{product.Id}");
        await DeleteAsync($"/api/users/{user.Id}");
    }

    [Fact]
    public async Task UpdatingOrder_WritesUpdateAuditRowWithBeforeAndAfter()
    {
        var user = await CreateUserAsync();
        var product = await CreateProductAsync(stock: 10);
        var order = await CreateOrderAsync(user.Id, product.Id, 1);

        var put = await PutAsync($"/api/orders/{order.Id}",
            new { Quantity = 4, Status = "Completed", Version = order.Version, Awb = (string?)null });
        put.EnsureSuccessStatusCode();

        var history = await GetAsync<List<AuditLogResponse>>($"/api/audit?table=Orders&rowId={order.Id}");

        history.Should().NotBeNull();
        var update = history!.FirstOrDefault(a => a.Action == "UPDATE");
        update.Should().NotBeNull();              // UPDATE captured
        update!.OldData.Should().NotBeNull();     // before-image present
        update.NewData.Should().NotBeNull();      // after-image present

        await DeleteAsync($"/api/products/{product.Id}");
        await DeleteAsync($"/api/users/{user.Id}");
    }
}
