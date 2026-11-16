namespace TruckingSystem.Application.Common;

/// <summary>"CÁCH SỬA NHANH" gửi kèm câu báo lỗi khi 1 thao tác bị chặn (Bắt đầu chuyến, Chia xe, Nhập kho…):
/// Desktop hiện thành nút dẫn thẳng tới đúng chỗ sửa, điền sẵn đúng tài xế / xe / ngày — không phải đoán theo chữ.
/// Kind (Desktop biết mở gì):
///  • DriverWorkDays — đăng ký ngày làm cho tài xế Id (Name), điền sẵn From..To
///  • DriverProfile — hồ sơ tài xế Id (bằng lái, giấy khám sức khỏe)
///  • VehicleDocs — giấy tờ xe Id (Name = biển số)
///  • Reassign — đổi xe / tài xế cho chuyến Id (Name = "#12")
///  • PendingReceipt / TripIssue — nhập hàng từ chuyến / xuất kho theo chuyến ở kho WarehouseId (Name = tên kho)
///  • WarehouseLayout / EditWarehouse — sơ đồ kho (thêm khu, vị trí) / sửa thông tin kho WarehouseId
///  • ManualReceive — nhận hàng khách mang tới ở kho WarehouseId
///  • Vehicle / Driver — mở màn Đội xe / Tài xế lọc đúng Name (biển số / mã tài xế)
///  • VehiclesBlocked — màn Đội xe lọc sẵn xe thiếu / hết hạn giấy tờ
///  • Contracts — mở màn Hợp đồng</summary>
public record FixHint(string Kind, string Text, int? Id = null, string? Name = null,
    DateTime? From = null, DateTime? To = null, int? WarehouseId = null, string? Note = null);
