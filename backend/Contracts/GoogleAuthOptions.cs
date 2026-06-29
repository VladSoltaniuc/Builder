// Infrastructure layer
namespace ProductApi.Auth;

// Bound from the "Authentication:Google" configuration section.
public class GoogleAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
}
