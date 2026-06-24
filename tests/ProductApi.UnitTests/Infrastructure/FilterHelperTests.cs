using FluentAssertions;
using ProductApi.Infrastructure;

namespace ProductApi.UnitTests.Infrastructure;

public class FilterHelperTests
{
    // --- ParseFilter ---

    [Theory]
    [InlineData("$eq:Alice",   "$eq",   "Alice")]
    [InlineData("$ilike:bob",  "$ilike","bob")]
    [InlineData("$gt:100",     "$gt",   "100")]
    [InlineData("$btw:10,20",  "$btw",  "10,20")]
    [InlineData("$sw:Al",      "$sw",   "Al")]
    public void ParseFilter_WithOperator_ReturnsParsedOpAndVal(string raw, string expectedOp, string expectedVal)
    {
        var (op, val) = FilterHelper.ParseFilter(raw);

        op.Should().Be(expectedOp);
        val.Should().Be(expectedVal);
    }

    [Theory]
    [InlineData("Alice")]
    [InlineData("hello world")]
    [InlineData("123")]
    public void ParseFilter_WithNoOperator_DefaultsToEq(string raw)
    {
        var (op, val) = FilterHelper.ParseFilter(raw);

        op.Should().Be("$eq");
        val.Should().Be(raw);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseFilter_WithNullOrWhitespace_ReturnsEqAndEmpty(string? raw)
    {
        var (op, val) = FilterHelper.ParseFilter(raw!);

        op.Should().Be("$eq");
        val.Should().BeEmpty();
    }

    [Fact]
    public void ParseFilter_WithOperatorButNoColon_ReturnsOpAndEmptyVal()
    {
        var (op, val) = FilterHelper.ParseFilter("$not");

        op.Should().Be("$not");
        val.Should().BeEmpty();
    }

    // --- ParseBtw ---

    [Fact]
    public void ParseBtw_WithValidRange_ReturnsParsedMinMax()
    {
        var result = FilterHelper.ParseBtw("100,500");

        result.Should().NotBeNull();
        result!.Value.Min.Should().Be(100);
        result!.Value.Max.Should().Be(500);
    }

    [Theory]
    [InlineData("100")]
    [InlineData("abc,500")]
    [InlineData("100,xyz")]
    [InlineData("")]
    public void ParseBtw_WithInvalidInput_ReturnsNull(string val)
    {
        FilterHelper.ParseBtw(val).Should().BeNull();
    }

    // --- ParseInInt ---

    [Fact]
    public void ParseInInt_WithValidInts_ReturnsList()
    {
        var result = FilterHelper.ParseInInt("1,2,3");

        result.Should().Equal([1, 2, 3]);
    }

    [Fact]
    public void ParseInInt_WithSingleValue_ReturnsSingleElementList()
    {
        var result = FilterHelper.ParseInInt("42");

        result.Should().Equal([42]);
    }

    [Theory]
    [InlineData("1,abc,3")]
    [InlineData("foo")]
    public void ParseInInt_WithNonIntValues_ReturnsNull(string val)
    {
        FilterHelper.ParseInInt(val).Should().BeNull();
    }
}
