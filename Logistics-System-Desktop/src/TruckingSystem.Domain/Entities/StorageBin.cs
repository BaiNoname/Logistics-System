using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Từng vị trí lưu trữ cụ thể (ô/kệ/pallet) trong 1 khu vực — đơn vị nhỏ nhất để xếp hàng.</summary>
public class StorageBin : BaseEntity
{
    public int WarehouseZoneId { get; set; }
    public WarehouseZone WarehouseZone { get; set; } = null!;

    public string Code { get; set; } = string.Empty; // vd: "A-01-01"
    public decimal CapacityM3 { get; set; }
    public decimal MaxWeightKg { get; set; }

    /// <summary>Thể tích đang chiếm dụng — cập nhật mỗi khi có StorageAssignment mới/gỡ bỏ.</summary>
    public decimal CurrentUsedM3 { get; set; }

    // Tọa độ + kích thước THẬT (mét), tính TƯƠNG ĐỐI so với góc trên-trái của
    // Khu vực (WarehouseZone) chứa nó — CÙNG cơ chế với cách Khu vực định vị
    // trong Kho. Trước đây dùng Row/Column (chỉ số lưới trừu tượng, không
    // phản ánh không gian thật) — đổi sang mét thật để kiểm tra được: 1) vị
    // trí có nằm gọn trong khu vực hay không, 2) 2 vị trí có đè lên nhau
    // không — đúng thực tế vật lý của 1 kho hàng thật.
    public decimal PosX { get; set; }
    public decimal PosY { get; set; }
    public decimal WidthMeters { get; set; }
    public decimal HeightMeters { get; set; }

    /// <summary>Khoảng cách thật (mét) từ vị trí này tới cửa kho — dùng cho
    /// thuật toán tối ưu theo độ khẩn cấp: đơn hàng SẮP tới hạn giao thì ưu
    /// tiên xếp vào vị trí GẦN CỬA nhất, giảm thời gian lấy hàng đi sau này.</summary>
    public decimal DistanceFromDoorMeters { get; set; }

    public ICollection<StorageAssignment> Assignments { get; set; } = new List<StorageAssignment>();
}