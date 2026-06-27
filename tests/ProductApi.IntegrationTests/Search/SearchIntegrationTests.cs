using System.Net;
using FluentAssertions;
using ProductApi.Contracts;

namespace ProductApi.IntegrationTests.Search;

[Collection("Integration")]
public class SearchIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    // --- Minimum-length enforcement (controller-level, before any query) ---

    [Theory]
    [InlineData("/api/products/search?term=ab")]
    [InlineData("/api/users/search?term=ab")]
    [InlineData("/api/orders/search?term=ab")]
    public async Task Search_TermTooShort_Returns400(string url)
    {
        var resp = await Client.GetAsync(url);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_TermMissing_Returns400()
    {
        var resp = await Client.GetAsync("/api/products/search");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- Substring matching (pg_trgm) ---

    [Fact]
    public async Task ProductSearch_ReturnsSubstringMatch()
    {
        var product = await CreateProductAsync(stock: 5);
        var token = product.Name.Split(' ').Last()[..8]; // unique chunk of the name's guid

        var results = await GetAsync<List<ProductResponse>>($"/api/products/search?term={token}");

        results.Should().NotBeNull();
        results!.Should().Contain(p => p.Id == product.Id);

        await DeleteAsync($"/api/products/{product.Id}");
    }

    [Fact]
    public async Task UserSearch_ByEmailSubstring_ReturnsMatch()
    {
        var user = await CreateUserAsync();
        var token = user.Email[..8]; // chunk of the email's guid

        var results = await GetAsync<List<UserResponse>>($"/api/users/search?term={token}");

        results.Should().NotBeNull();
        results!.Should().Contain(u => u.Id == user.Id);

        await DeleteAsync($"/api/users/{user.Id}");
    }

    [Fact]
    public async Task OrderSearch_ByAwbSubstring_ReturnsMatch()
    {
        var user = await CreateUserAsync();
        var product = await CreateProductAsync(stock: 5);
        var order = await CreateOrderAsync(user.Id, product.Id, 1);
        var awb = $"AWB{Guid.NewGuid():N}"[..20];

        var put = await PutAsync($"/api/orders/{order.Id}",
            new { Quantity = 1, Status = "Pending", Version = order.Version, Awb = awb });
        put.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await GetAsync<List<OrderResponse>>($"/api/orders/search?term={awb[..10]}");

        results.Should().NotBeNull();
        results!.Should().Contain(o => o.Id == order.Id);

        await DeleteAsync($"/api/products/{product.Id}");
        await DeleteAsync($"/api/users/{user.Id}");
    }
}
