// Domain layer
namespace ProductApi.Models;

// Coarse-grained access level. ReadOnly may read; Admin may also mutate.
public enum UserRole
{
    ReadOnly,
    Admin,
}
