// Domain layer
namespace ProductApi.Models;

// Coarse-grained access level. Operator is the base staff role; Admin may also
// mutate and manage users. (Per-permission handling comes later.)
public enum UserRole
{
    Operator,
    Admin,
}
