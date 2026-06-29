// Application layer
namespace ProductApi.Exceptions;

public class UserFriendlyException(string message, string errorCode = "INVALID_ARGUMENT", string? detail = null)
    : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
    public string? Detail   { get; } = detail;
}
