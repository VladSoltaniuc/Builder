// Shared layer
namespace ProductApi.Constants;

public static class ImageSettings
{
    public static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    public const long MaxImageSizeBytes = 5 * 1024 * 1024;
    public const long MaxInvoiceSizeBytes = 10 * 1024 * 1024;
}
