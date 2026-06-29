// Infrastructure layer — bound from the "Smtp" configuration section.
namespace ProductApi.Reports;

public sealed class EmailOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "ProductApi";
    public int MaxRetries { get; set; } = 3;
}
