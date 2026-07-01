// Infrastructure layer
namespace ProductApi.Infrastructure;

// One place that knows how bytes are persisted. Services own the path convention
// (e.g. "uploads/invoices/{id}.pdf"); this just stores/removes/resolves them
// Swapping local disk for S3/Blob later means changing only the implementation
public interface IFileStorage
{
    /// <summary>Writes the uploaded file at the given app-relative path (creating folders) and returns that path.</summary>
    Task<string> Save(IFormFile file, string relativePath);

    /// <summary>Deletes the file at the app-relative path. No-op when it doesn't exist.</summary>
    void Delete(string relativePath);

    /// <summary>Deletes the file when a path is given; no-op when it's null.</summary>
    void DeleteIfPresent(string? relativePath)
    {
        if (relativePath is not null) Delete(relativePath);
    }

    /// <summary>Removes a file that has been replaced by another. Skips deletion when the
    /// replacement reused the same path (Save already overwrote it) or there was none.</summary>
    void DeleteReplaced(string? previousPath, string? newPath)
    {
        if (previousPath is not null && previousPath != newPath) Delete(previousPath);
    }

    /// <summary>Resolves an app-relative path to its absolute location on disk.</summary>
    string ResolvePath(string relativePath);
}

public sealed class LocalFileStorage(IWebHostEnvironment env) : IFileStorage
{
    public async Task<string> Save(IFormFile file, string relativePath)
    {
        var fullPath = ResolvePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream);
        return relativePath;
    }

    public void Delete(string relativePath)
    {
        var fullPath = ResolvePath(relativePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    public string ResolvePath(string relativePath) => Path.Combine(env.WebRootPath, relativePath);
}
