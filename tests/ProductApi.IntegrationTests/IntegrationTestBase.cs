// Infrastructure — shared setup for all integration test classes
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProductApi.Contracts;

namespace ProductApi.IntegrationTests;

public abstract class IntegrationTestBase
{
    // Match the API's serialization — enums travel as strings ("Pending"), not numbers.
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly HttpClient Client;

    protected IntegrationTestBase(IntegrationTestFactory factory)
    {
        Client = factory.CreateClient();
    }

    protected Task<T?> GetAsync<T>(string url) =>
        Client.GetFromJsonAsync<T>(url, Json);

    protected static Task<T?> ReadAsync<T>(HttpResponseMessage response) =>
        response.Content.ReadFromJsonAsync<T>(Json);

    protected Task<HttpResponseMessage> PostAsync<T>(string url, T body) =>
        Client.PostAsJsonAsync(url, body);

    protected Task<HttpResponseMessage> PutAsync<T>(string url, T body) =>
        Client.PutAsJsonAsync(url, body);

    protected Task<HttpResponseMessage> DeleteAsync(string url) =>
        Client.DeleteAsync(url);

    // --- Fixture helpers: create throwaway rows with unique values ---

    protected async Task<ProductResponse> CreateProductAsync(int stock, decimal price = 10m)
    {
        var input = new { Name = $"Test Product {Guid.NewGuid():N}", Category = "Peripherals", Price = price, Stock = stock };
        var resp = await PostAsync("/api/products", input);
        resp.EnsureSuccessStatusCode();
        return (await ReadAsync<ProductResponse>(resp))!;
    }

    protected async Task<UserResponse> CreateUserAsync()
    {
        var input = new { Name = "Test User", Email = $"{Guid.NewGuid():N}@test.com" };
        var resp = await PostAsync("/api/users", input);
        resp.EnsureSuccessStatusCode();
        return (await ReadAsync<UserResponse>(resp))!;
    }

    protected async Task<OrderResponse> CreateOrderAsync(int userId, int productId, int quantity)
    {
        var input = new { UserId = userId, ProductId = productId, Quantity = quantity };
        var resp = await PostAsync("/api/orders", input);
        resp.EnsureSuccessStatusCode();
        return (await ReadAsync<OrderResponse>(resp))!;
    }
}
