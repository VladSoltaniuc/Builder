// Application layer
namespace ProductApi.Exceptions;

// Carries only a stable code + status. The client owns all user-facing text (via i18n),
// so no human-readable message lives here. The base message mirrors the code for logs.
public class UserFriendlyException(
    string errorCode,
    int statusCode = 400,
    string? detail = null) : Exception(errorCode)
{
    public string ErrorCode { get; } = errorCode;
    public int StatusCode  { get; } = statusCode;
    public string? Detail   { get; } = detail;
}
