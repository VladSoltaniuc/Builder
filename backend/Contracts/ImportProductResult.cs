// Application layer
namespace ProductApi.Contracts;

public record ImportProductResult(int Imported, int Failed, IReadOnlyList<string> Errors);
