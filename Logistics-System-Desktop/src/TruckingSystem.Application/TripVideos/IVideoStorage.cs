namespace TruckingSystem.Application.TripVideos;

/// <summary>Kho lưu file video — Application chỉ biết interface này, không
/// biết file nằm ở ổ đĩa server hay dịch vụ đám mây (Cloudflare R2, S3...).
/// Hiện cài đặt bằng ổ đĩa (Infrastructure/Storage/LocalVideoStorage); muốn
/// chuyển lên đám mây chỉ cần viết thêm 1 lớp cài đặt khác, không sửa nghiệp vụ.</summary>
public interface IVideoStorage
{
    /// <summary>Ghi stream thành file, đồng thời tính SHA-256 trong lúc ghi (không phải đọc lại file).</summary>
    Task<(long SizeBytes, string Sha256)> SaveAsync(string relativePath, Stream content, CancellationToken ct = default);

    /// <summary>Đường dẫn vật lý để API trả file (hỗ trợ tua — HTTP Range).</summary>
    string GetFullPath(string relativePath);

    bool Exists(string relativePath);

    /// <summary>Tính lại SHA-256 của file đang lưu — null nếu file không còn.</summary>
    Task<string?> ComputeSha256Async(string relativePath, CancellationToken ct = default);

    void Delete(string relativePath);
}
