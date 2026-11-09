namespace TruckingSystem.Domain.Common;

/// <summary>
/// Lớp cơ sở cho mọi entity: Id tự tăng + audit thời gian tạo/sửa.
/// Kế thừa lớp này thay vì viết lặp lại 3 field ở từng entity.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
