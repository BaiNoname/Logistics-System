namespace TruckingSystem.Domain.Enums;

public enum EmployeeStatus { Active, OnLeave, Terminated }

// ---- Chấm công QR (Mobile) ----
public enum AttendanceScanAction { None, CheckIn, CheckOut }
public enum AttendanceScanResult { Accepted, Rejected }
public enum AttendanceAdjustmentReason { ForgotScan, GpsError, NetworkError, WrongRecord, OffSiteWork, Other }
public enum AttendanceAdjustmentStatus { Pending, Approved, Rejected }

public enum TransportOrderStatus { Draft, Confirmed, InProgress, Done, Cancelled }

public enum TripStatus { Planned, InProgress, Completed, Cancelled, PendingConfirmation }

public enum TripLineStatus { Planned, Picked, Delivered }

public enum InvoiceStatus { Unpaid = 0, Paid = 3 }

public enum CustomerType { Individual, Company }

public enum VehicleStatus { Available, OnTrip, UnderMaintenance, OutOfService }

public enum VehicleDocumentType { Registration, Insurance, TransportPermit, Other }

public enum EmployeeDocumentType { NationalId, DriverLicense, HealthCertificate, Other }

public enum IncidentType { Accident, Breakdown, TrafficViolation, Other }

public enum TripExpenseType { Toll, Parking, Loading, Fine, Other }

public enum PaymentMethod { Cash, BankTransfer, Card, Other }

public enum ShiftType { Morning, Afternoon, Night, FullDay }

/// <summary>Cách lấy hàng chặng 1: công ty cử xe đến lấy, hay khách tự mang tới kho.</summary>
public enum PickupMethod { CompanyPickup, SelfDelivery }

/// <summary>Phân biệt 1 Trip là chặng lấy hàng về kho hay chặng giao hàng đi.</summary>
public enum TripLegType
{
    Pickup,
    Delivery,
    Return   // MỚI — chuyến trả hàng về đối tác khi đơn bị hủy sau khi đã nhập kho
}

public enum WarehouseTxnType { Receipt, Issue }

/// <summary>Loại khu vực trong kho — quyết định hàng dễ vỡ/nguy hiểm phải xếp đúng khu riêng.</summary>
public enum ZoneType { Normal, Fragile, Hazmat }

/// <summary>Bức tường có cửa kho — cửa LUÔN nằm trên tường (không thể ở giữa kho).
/// Quy ước tọa độ sơ đồ: X chạy theo chiều DÀI kho, Y chạy theo chiều RỘNG kho,
/// gốc (0,0) ở góc trên-trái. Top: Y = 0 · Bottom: Y = chiều rộng ·
/// Left: X = 0 · Right: X = chiều dài.</summary>
public enum WallSide { Top, Bottom, Left, Right }
public enum DeadlineChangeRequester { Company, Customer }