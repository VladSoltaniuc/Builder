// Domain layer
namespace ProductApi.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // BCrypt hash. Empty for accounts that only sign in via an external provider.
    public string PasswordHash { get; set; } = string.Empty;

    // External login (e.g. "Google") and the provider's stable user id. Null for
    // password-only accounts.
    public string? ExternalProvider { get; set; }
    public string? ExternalId { get; set; }

    public int Version { get; set; } = 1;
}
