using System.Net;
using FluentAssertions;
using ProductApi.Contracts;

namespace ProductApi.IntegrationTests.Users;

[Collection("Integration")]
public class UsersIntegrationTests(IntegrationTestFactory factory) : IntegrationTestBase(factory)
{
    // --- GetAll ---

    [Fact]
    public async Task GetAll_ValidRequest_ReturnsPagedShape()
    {
        var response = await GetAsync<PagedResponse<UserResponse>>("/api/users");

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
        var input = new { Name = "Integration Test User", Email = $"{Guid.NewGuid():N}@test.com" };

        var createResponse = await PostAsync("/api/users", input);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ReadAsync<UserResponse>(createResponse);
        created.Should().NotBeNull();
        created!.Name.Should().Be(input.Name);
        created.Email.Should().Be(input.Email);

        // Cleanup
        await DeleteAsync($"/api/users/{created.Id}");
    }

    // --- Unique email (case-insensitive) ---

    [Fact]
    public async Task Create_DuplicateEmail_DifferentCase_Returns409()
    {
        var email = $"{Guid.NewGuid():N}@dup.com";
        var first = await PostAsync("/api/users", new { Name = "First", Email = email });
        first.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ReadAsync<UserResponse>(first);

        // Same email, different casing → still a duplicate (stored lower-cased)
        var dup = await PostAsync("/api/users", new { Name = "Second", Email = email.ToUpperInvariant() });
        dup.StatusCode.Should().Be(HttpStatusCode.Conflict);

        await DeleteAsync($"/api/users/{created!.Id}");
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_NonExistentUser_Returns404()
    {
        var response = await DeleteAsync("/api/users/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
