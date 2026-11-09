using System.Security.Cryptography;
using TruckingSystem.Application.TripVideos;

namespace TruckingSystem.Infrastructure.Storage;

/// <summary>Lưu video trên ổ đĩa của máy chạy API — miễn phí, đủ cho demo.
/// Thư mục gốc cấu hình ở appsettings "VideoStorage:RootPath"; để trống thì
/// dùng thư mục "Storage/TripVideos" cạnh project API.
/// Thư mục này KHÔNG nằm trong wwwroot -> không ai tải trực tiếp được, chỉ đi
/// qua API (có kiểm tra chữ ký link).</summary>
public class LocalVideoStorage : IVideoStorage
{
    private readonly string _root;

    public LocalVideoStorage(string? rootPath)
    {
        _root = Path.GetFullPath(string.IsNullOrWhiteSpace(rootPath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "Storage", "TripVideos")
            : rootPath);
        Directory.CreateDirectory(_root);
    }

    public async Task<(long SizeBytes, string Sha256)> SaveAsync(string relativePath, Stream content, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        long total = 0;
        await using (var file = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, buffer.Length, useAsync: true))
        {
            int read;
            while ((read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                await file.WriteAsync(buffer.AsMemory(0, read), ct);
                sha.AppendData(buffer, 0, read);
                total += read;
            }
        }
        return (total, Convert.ToHexString(sha.GetHashAndReset()).ToLowerInvariant());
    }

    public string GetFullPath(string relativePath)
    {
        var full = Path.GetFullPath(Path.Combine(_root, relativePath));
        // Chặn đường dẫn kiểu "../../" thoát ra ngoài thư mục gốc
        if (!full.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Đường dẫn video không hợp lệ.");
        return full;
    }

    public bool Exists(string relativePath) => File.Exists(GetFullPath(relativePath));

    public async Task<string?> ComputeSha256Async(string relativePath, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath)) return null;
        await using var file = File.OpenRead(fullPath);
        var hash = await SHA256.HashDataAsync(file, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public void Delete(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}
