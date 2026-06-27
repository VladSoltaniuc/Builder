// Infrastructure layer — bound from the "App" configuration section.
namespace ProductApi.Auth;

public class AppOptions
{
    // Base URL of the SPA, used to build links emailed to users
    // (e.g. the email-verification page).
    public string FrontendUrl { get; set; } = "http://localhost:5173";
}
