// Shared layer
namespace ProductApi.Constants;

public static class PaginationDefaults
{
    // Trigram index can't use lower than 3
    public const int MinTermLength = 3;
    public const int Page = 1;
    public static readonly int[] PageSizes = [10, 25, 50];
}
