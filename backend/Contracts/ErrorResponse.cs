// Application layer
using System.Text.Json.Serialization;

namespace ProductApi.Contracts;

public record FieldError(string Field, string Code);

public record ErrorDetail(
    int Code,
    string Status,
    string Message,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Detail = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<FieldError>? Errors = null
);

public record ErrorResponse(ErrorDetail Error);
