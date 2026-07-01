using FluentAssertions;
using ProductApi.Infrastructure;

namespace ProductApi.UnitTests.Infrastructure;

// Covers the parts of QueryShaper that run in-memory: sort registration/direction and the
// no-op guards. The free-text search translates to EF.Functions.ILike, which only executes
// against a real provider, so its matching behaviour lives in SearchIntegrationTests.
public class QueryShaperTests
{
    private record Person(string Name, int Age);

    private static readonly QueryShaper<Person> Shaper = new QueryShaper<Person>()
        .Search(p => p.Name)
        .Sort("name", p => p.Name)
        .Sort("age", p => p.Age);

    // Deliberately unordered so a sort has to actually do something.
    private static IQueryable<Person> People() => new[]
    {
        new Person("Bob", 40),
        new Person("Ann", 25),
        new Person("Cy", 30),
    }.AsQueryable();

    [Fact]
    public void ApplySort_Ascending_OrdersByColumn()
    {
        var result = Shaper.ApplySort(People(), "name").Select(p => p.Name).ToList();

        result.Should().Equal("Ann", "Bob", "Cy");
    }

    [Fact]
    public void ApplySort_DescendingSuffix_OrdersDescending()
    {
        var result = Shaper.ApplySort(People(), "age:desc").Select(p => p.Name).ToList();

        result.Should().Equal("Bob", "Cy", "Ann"); // 40, 30, 25
    }

    [Fact]
    public void ApplySort_UnknownColumn_LeavesOrderUnchanged()
    {
        var result = Shaper.ApplySort(People(), "unknown").Select(p => p.Name).ToList();

        result.Should().Equal("Bob", "Ann", "Cy"); // original order
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ApplySort_BlankSortBy_ReturnsQueryUnchanged(string? sortBy)
    {
        var query = People();

        Shaper.ApplySort(query, sortBy).Should().BeSameAs(query);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ApplySearch_BlankTerm_ReturnsQueryUnchanged(string? term)
    {
        var query = People();

        Shaper.ApplySearch(query, term).Should().BeSameAs(query);
    }

    [Fact]
    public void ApplySearch_NoRegisteredSearchColumns_ReturnsQueryUnchanged()
    {
        var shaper = new QueryShaper<Person>().Sort("name", p => p.Name);
        var query = People();

        shaper.ApplySearch(query, "Ann").Should().BeSameAs(query);
    }
}
