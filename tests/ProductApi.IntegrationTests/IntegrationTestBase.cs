// Infrastructure — shared setup for all integration test classes
using System.Net.Http.Json;

namespace ProductApi.IntegrationTests;

public abstract class IntegrationTestBase
{
    protected readonly HttpClient Client;

    protected IntegrationTestBase(IntegrationTestFactory factory)
    {
        Client = factory.CreateClient();
    }

    protected Task<T?> GetAsync<T>(string url) =>
        Client.GetFromJsonAsync<T>(url);

    protected Task<HttpResponseMessage> PostAsync<T>(string url, T body) =>
        Client.PostAsJsonAsync(url, body);

    protected Task<HttpResponseMessage> DeleteAsync(string url) =>
        Client.DeleteAsync(url);
}
