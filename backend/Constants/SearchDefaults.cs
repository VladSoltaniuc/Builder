// Shared layer — substring-search constants
namespace ProductApi.Constants;

public static class SearchDefaults
{
    // Trigram search needs at least 3 characters to form a single trigram,
    // so we refuse shorter terms before hitting the database.
    public const int MinTermLength = 3;

    // Upper bound on rows returned by a search endpoint (guards against
    // a 2-char-ish term matching most of a large table).
    public const int MaxResults = 50;
}
