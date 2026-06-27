// Domain layer
namespace ProductApi.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // BCrypt hash. Empty for accounts that only sign in via an external provider.
    public string PasswordHash { get; set; } = string.Empty;

    public int Version { get; set; } = 1;
}
