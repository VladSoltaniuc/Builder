// Infrastructure layer — bound from the "AdminSeed" configuration section.
namespace ProductApi.Auth;

// The founder Admin account, provisioned from trusted config at startup rather than
// via public registration. Leave Email/Password blank to disable seeding.
public class AdminSeedOptions
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = "Administrator";
}
