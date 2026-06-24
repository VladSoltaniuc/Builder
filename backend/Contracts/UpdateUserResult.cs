// Application layer
namespace ProductApi.Contracts;

public record UpdateUserResult(UserResponse? User, bool IsConflict)
{
    public static UpdateUserResult Success(UserResponse user) => new(user, false);
    public static UpdateUserResult NotFound() => new(null, false);
    public static UpdateUserResult Conflict() => new(null, true);
}
