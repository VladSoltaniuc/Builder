// Application layer
using System.Text.Json.Serialization;

namespace ProductApi.Contracts;

public record ErrorResponse(ErrorDetail Error);

// The client owns all user-facing text: we ship a stable Status code (and any
// interpolation Detail / per-field codes), never a human-readable message
public record ErrorDetail(
    int Code,
    string Status,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Detail = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<FieldError>? Errors = null
);

public record FieldError(string Field, string Code);
