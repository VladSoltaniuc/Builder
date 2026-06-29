// Configuration layer
namespace ProductApi.Configuration;

public class AdminSeedOptions
{
    public bool Enabled { get; set; } = false;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = "Administrator";
}
