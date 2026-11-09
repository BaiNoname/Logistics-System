using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TruckingSystem.API.Controllers;

/// <summary>File scan giấy tờ xe (Đăng kiểm, Bảo hiểm...): Desktop chọn file trên
/// máy -> UPLOAD lên server -> lưu đường dẫn server vào VehicleDocument.FileUrl.
/// Trước đây chỉ gõ đường dẫn file trên máy người nhập (VD C:\scan\dk.pdf) —
/// máy khác (laptop, máy kế toán) không mở được.
/// Thư mục lưu: appsettings "DocumentStorage:RootPath" (để trống = Storage/VehicleDocuments
/// cạnh API), KHÔNG lưu file vào database.</summary>
[ApiController]
[Route("api/vehicles/documents")]
[Authorize(Roles = "Admin,Dispatcher")]
public class VehicleDocumentFilesController : ControllerBase
{
    private const long MaxBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
    };

    private readonly string _root;

    public VehicleDocumentFilesController(IConfiguration config)
    {
        var configured = config["DocumentStorage:RootPath"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(Directory.GetCurrentDirectory(), "Storage", "VehicleDocuments")
            : configured;
    }

    public record UploadDocumentFileResult(bool Success, string? FileUrl, string? FileName, string? Message);

    [HttpPost("upload")]
    [RequestSizeLimit(MaxBytes + 1024 * 1024)]
    public async Task<ActionResult<UploadDocumentFileResult>> Upload(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return Ok(new UploadDocumentFileResult(false, null, null, "Chưa chọn file."));
        if (file.Length > MaxBytes)
            return Ok(new UploadDocumentFileResult(false, null, null, "File quá lớn (tối đa 10 MB)."));

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedTypes.ContainsKey(ext))
            return Ok(new UploadDocumentFileResult(false, null, null, "Chỉ nhận file PDF, JPG, PNG."));

        var folder = DateTime.UtcNow.AddHours(7).ToString("yyyyMM");
        var name = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var dir = Path.Combine(_root, folder);
        Directory.CreateDirectory(dir);
        await using (var stream = System.IO.File.Create(Path.Combine(dir, name)))
            await file.CopyToAsync(stream);

        return Ok(new UploadDocumentFileResult(true, $"/api/vehicles/documents/files/{folder}/{name}", file.FileName, null));
    }

    /// <summary>Tải file đã upload (Desktop dùng token đăng nhập để tải rồi mở).</summary>
    [HttpGet("files/{folder}/{name}")]
    public IActionResult GetFile(string folder, string name)
    {
        // Chống đọc file ngoài thư mục lưu trữ (../..)
        if (folder.Any(c => !char.IsDigit(c)) || name.Contains("..") || name.Contains('/') || name.Contains('\\'))
            return BadRequest();
        var ext = Path.GetExtension(name);
        if (!AllowedTypes.TryGetValue(ext, out var contentType)) return BadRequest();

        var full = Path.GetFullPath(Path.Combine(_root, folder, name));
        if (!full.StartsWith(Path.GetFullPath(_root), StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(full))
            return NotFound();
        return PhysicalFile(full, contentType, name);
    }
}
