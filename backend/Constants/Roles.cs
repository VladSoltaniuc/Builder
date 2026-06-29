// Infrastructure layer
namespace ProductApi.Auth;

// Role names as they appear in the JWT role claim and [Authorize(Roles = ...)].
// Kept in sync with the UserRole enum names.
public static class Roles
{
    public const string Admin = "Admin";
    public const string Operator = "Operator";
}
