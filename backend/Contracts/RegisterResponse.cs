// Application layer
namespace ProductApi.Contracts;

// Registration no longer logs the user in — it asks them to verify their email first.
public record RegisterResponse(string Email, string Message);
