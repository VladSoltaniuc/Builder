using System.Text.Json.Serialization;

namespace ProductApi.Contracts;

public record ErrorDetail(
    int Code,
    string Status,
    string Message,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Detail = null
);

public record ErrorResponse(ErrorDetail Error);
