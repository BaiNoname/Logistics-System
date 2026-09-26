# TruckingSystem — Hệ thống Quản lý Vận tải và Kho Trung chuyển Hàng hóa Nội địa

## 1. Giới thiệu

TruckingSystem là hệ thống phần mềm quản lý vận tải đường bộ và kho trung chuyển (cross-docking) dành cho doanh nghiệp logistics vừa và nhỏ tại Việt Nam. Hệ thống hỗ trợ toàn bộ chuỗi nghiệp vụ từ lúc tiếp nhận đơn hàng vận chuyển, tự động phân bổ xe/tài xế, quản lý nhập-xuất kho, theo dõi hành trình qua GPS thời gian thực, cho đến việc tính toán chi phí vận hành thực tế của từng chuyến xe.

Đồ án được xây dựng theo mô hình 2 nhóm chức năng chính:
- **Vận hành vận tải** (đơn hàng, chia xe, đội xe, tài xế, kho bãi, GPS, chi phí nhiên liệu)
- **Quản trị doanh nghiệp** (khách hàng, hợp đồng, hóa đơn, phân quyền, chatbot hỗ trợ)

## 2. Mục tiêu đồ án

- Tự động hóa quy trình phân bổ xe và tài xế cho từng đơn hàng dựa trên năng lực chuyên chở thực tế (khối lượng, thể tích, số lượng), thay vì phân công thủ công.
- Số hóa việc quản lý kho trung chuyển theo đúng không gian vật lý thật (sơ đồ tỉ lệ mét), tự động đề xuất vị trí lưu trữ tối ưu theo thuật toán Best-Fit.
- Theo dõi hành trình xe thực tế qua GPS, đối chiếu với lộ trình lý thuyết để đánh giá hiệu quả vận hành.
- Tính toán chi phí nhiên liệu và các chi phí phát sinh khác cho từng chuyến, làm cơ sở đánh giá lợi nhuận theo từng đơn hàng.
- Cung cấp công cụ theo dõi công khai cho khách hàng (tra cứu trạng thái đơn hàng qua link, không cần đăng nhập).
- Đồng bộ lịch trình lên Google Calendar cho cả tài xế và khách hàng.

## 3. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Backend API | ASP.NET Core (.NET 8) |
| Kiến trúc | Clean Architecture (Domain → Application → Infrastructure → API) |
| Cơ sở dữ liệu | PostgreSQL, Entity Framework Core |
| Ứng dụng quản trị (Desktop) | Windows Forms (.NET 8) |
| Ứng dụng tài xế (Mobile) | React Native (Expo) |
| Xác thực | ASP.NET Core Identity + JWT |
| Bản đồ & định tuyến | VietMap API |
| Đồng bộ lịch | Google Calendar API |

## 4. Kiến trúc hệ thống

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Desktop (WinForms) │  │ Mobile App tài xế │     │ Trang công khai  │
│  (Điều phối viên)   │  │ (React Native)    │     │ (Khách theo dõi) │
└────────┬─────────┘     └────────┬─────────┘     └────────┬────────┘
         │                        │                          │
         └────────────────────────┼──────────────────────────┘
                                   ▼
                        ┌─────────────────────┐
                        │   ASP.NET Core API   │
                        │  (Clean Architecture) │
                        └──────────┬───────────┘
                                   ▼
                        ┌─────────────────────┐
                        │     PostgreSQL        │
                        └─────────────────────┘
```

Mỗi tầng trong Clean Architecture đảm nhiệm 1 vai trò riêng biệt:
- **Domain**: định nghĩa các thực thể nghiệp vụ (entities) và quy tắc cốt lõi, không phụ thuộc công nghệ nào.
- **Application**: chứa toàn bộ logic nghiệp vụ (services), điều phối luồng xử lý.
- **Infrastructure**: cài đặt kỹ thuật cụ thể (kết nối database, gọi API bên ngoài).
- **API**: tầng giao tiếp HTTP, expose các endpoint cho Desktop/Mobile/Web sử dụng.

## 5. Cơ sở dữ liệu

### 5.1. Sơ đồ tổng quan các nhóm bảng

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Khách hàng  │────▶│  Đơn vận chuyển │────▶│    Chuyến    │
│   (Customer)  │     │(TransportOrder) │     │    (Trip)    │
└──────────────┘     └───────┬──────┘     └───────┬──────┘
                              │                     │
                              ▼                     ▼
                      ┌──────────────┐     ┌──────────────┐
                      │   Kho trung   │     │   Đội xe &    │
                      │    chuyển     │     │   Tài xế      │
                      │  (Warehouse)  │     │(Vehicle/Driver)│
                      └──────────────┘     └──────────────┘
                              │                     │
                              ▼                     ▼
                      ┌──────────────┐     ┌──────────────┐
                      │  Tồn kho &    │     │  GPS & Xăng   │
                      │  Vị trí lưu   │     │  dầu          │
                      └──────────────┘     └──────────────┘
```

### 5.2. Chi tiết các bảng chính

#### Khách hàng & Đơn vận chuyển

**Customer** (Khách hàng)

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| Name | string | Tên khách hàng (cá nhân/doanh nghiệp) |
| CustomerType | enum | Individual / Company |
| Phone | string | Số điện thoại liên hệ |
| Address | string | Địa chỉ |
| TaxCode | string | Mã số thuế (nếu là doanh nghiệp) |
| CreditLimit | decimal | Hạn mức công nợ |
| GoogleCalendarId | string | ID lịch Google để đồng bộ thông báo giao hàng |

**TransportOrder** (Đơn vận chuyển)

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| Code | string | Mã đơn hàng (tự sinh) |
| CustomerId | int | Khách hàng đặt đơn |
| WarehouseId | int | Kho trung chuyển dùng cho đơn này |
| PickupPoint / DropoffPoint | string | Điểm lấy hàng / giao hàng (địa chỉ dạng chữ) |
| PickupLocationId / DropoffLocationId | int? | Liên kết tới địa điểm đã lưu tọa độ |
| PickupMethod | enum | CompanyPickup / SelfDelivery |
| CommittedDeliveryDate | DateTime? | Ngày cam kết giao hàng |
| Status | enum | Draft / Confirmed / InProgress / Done / Cancelled |
| ProgressPercent | decimal | Tiến độ hoàn thành (%) |
| PublicTrackingToken | string | Mã token theo dõi công khai |

**CargoLine** (Dòng hàng hóa trong đơn)

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| TransportOrderId | int | Thuộc đơn hàng nào |
| CargoType | string | Loại hàng hóa |
| PackageCount | int | Số kiện |
| WeightKg | decimal | Khối lượng (kg) |
| VolumeM3 | decimal | Thể tích (m³) |
| QtyTotal | decimal | Tổng số lượng cần vận chuyển |
| IsFragile | bool | Hàng dễ vỡ |
| IsHazmat | bool | Hàng nguy hiểm |

**Location** (Địa điểm đã lưu)

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| Name / Address | string | Tên / địa chỉ đầy đủ |
| Lat / Lng | double? | Tọa độ |

#### Kho trung chuyển

**Warehouse** (Kho)

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| Name / Address | string | Tên / địa chỉ kho |
| Lat / Lng | double? | Tọa độ kho |
| LengthMeters / WidthMeters | decimal? | Kích thước thật của kho |
| CapacityM3 | decimal | Sức chứa tổng |

**WarehouseZone** (Khu vực trong kho)

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| WarehouseId | int | Thuộc kho nào |
| Name | string | Tên khu vực |
| ZoneType | enum | Normal / Fragile / Hazmat |
| PosX / PosY | decimal | Tọa độ (tương đối so với kho) |
| WidthMeters / HeightMeters | decimal | Kích thước khu vực |

**StorageBin** (Vị trí lưu trữ cụ thể)

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| WarehouseZoneId | int | Thuộc khu vực nào |
| Code | string | Mã vị trí |
| PosX / PosY / WidthMeters / HeightMeters | decimal | Tọa độ + kích thước thật |
| CapacityM3 | decimal | Sức chứa |
| CurrentUsedM3 | decimal | Thể tích đang chiếm dụng |
| MaxWeightKg | decimal | Tải trọng tối đa |
| DistanceFromDoorMeters | decimal | Khoảng cách tới cửa kho |

**WarehouseStock** (Tồn kho tổng hợp)

| Field | Kiểu | Mô tả |
|---|---|---|
| WarehouseId / CargoLineId | int | Khóa tổng hợp |
| QtyOnHand | decimal | Số lượng đang tồn |

**StorageAssignment** (Gán lô hàng vào vị trí)

| Field | Kiểu | Mô tả |
|---|---|---|
| GoodsReceiptId | int | Phiếu nhập liên quan |
| StorageBinId | int | Vị trí được gán |
| QtyStored | decimal | Số lượng lưu tại vị trí này |

**GoodsReceipt / GoodsIssue** (Phiếu nhập / xuất kho)

| Field | Kiểu | Mô tả |
|---|---|---|
| TransportOrderId / CargoLineId | int | Đơn hàng và dòng hàng liên quan |
| WarehouseId | int | Kho thực hiện |
| TripId | int? | Chuyến gắn liền (null nếu thao tác thủ công) |
| QtyReceived / QtyIssued | decimal | Số lượng nhập/xuất |

#### Đội xe & Tài xế

**Vehicle** (Xe)

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| PlateNumber | string | Biển số |
| VehicleType | string | Loại xe |
| CapacityQty / CapacityKg / CapacityM3 | decimal | Năng lực chuyên chở |
| Status | enum | Available / OnTrip / UnderMaintenance / OutOfService |
| WarehouseId | int? | Kho đóng quân |
| FuelConsumptionPer100Km | decimal? | Định mức tiêu hao nhiên liệu |
| FuelType | string? | Loại nhiên liệu sử dụng |

**VehicleDocument** (Giấy tờ xe)

| Field | Kiểu | Mô tả |
|---|---|---|
| VehicleId | int | Thuộc xe nào |
| DocType | enum | Registration / Insurance / TransportPermit / Other |
| IssueDate / ExpiryDate | DateTime | Ngày cấp / hết hạn |
| FileUrl | string? | Đường dẫn ảnh scan giấy tờ |

**DriverProfile** (Hồ sơ tài xế)

| Field | Kiểu | Mô tả |
|---|---|---|
| EmployeeId | int | Nhân viên tương ứng |
| LicenseNumber / LicenseClass | string | Số / hạng bằng lái |
| LicenseExpiryDate | DateTime | Ngày hết hạn bằng lái |
| YearsOfExperience | int | Số năm kinh nghiệm |
| WarehouseId | int? | Kho đóng quân |
| GoogleCalendarId | string? | ID lịch Google |

**DriverSchedule** (Lịch làm việc tài xế)

| Field | Kiểu | Mô tả |
|---|---|---|
| DriverEmployeeId | int | Tài xế |
| ShiftDate | DateTime | Ngày làm việc |
| ShiftType | enum | Morning / Afternoon / Night / FullDay |

#### Chuyến & Hành trình

**Trip** (Chuyến — 1 chặng cụ thể)

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| TransportOrderId / VehicleId / DriverEmployeeId | int | Đơn hàng / Xe / Tài xế thực hiện |
| WarehouseId | int | Kho trung chuyển liên quan |
| LegType | enum | Pickup / Delivery |
| Status | enum | Planned / InProgress / Completed |
| StartTime / EndTime | DateTime? | Thời gian bắt đầu / kết thúc thực tế |
| EstimatedDistanceKm / EstimatedDurationMinutes | decimal? / int? | Ước tính từ hệ thống định tuyến |
| PlannedFuelCost / ActualFuelCost | decimal | Chi phí xăng ước tính / thực tế |

**TripLine**: số lượng hàng dự kiến chuyên chở trong 1 Trip (liên kết Trip + CargoLine).

**Waypoint** (Điểm GPS)

| Field | Kiểu | Mô tả |
|---|---|---|
| TripId | int | Chuyến liên quan |
| Lat / Lng | double | Tọa độ |
| Timestamp | DateTime | Thời điểm ghi nhận |
| SpeedKmh | decimal? | Tốc độ tại thời điểm đó |

**IncidentReport** (Sự cố)

| Field | Kiểu | Mô tả |
|---|---|---|
| TripId | int | Chuyến xảy ra sự cố |
| Type | enum | Accident / Breakdown / TrafficViolation / Other |
| Description | string? | Mô tả chi tiết |
| Cost | decimal? | Chi phí phát sinh (nếu có) |

**TripExpense** (Chi phí khác)

| Field | Kiểu | Mô tả |
|---|---|---|
| TripId | int | Chuyến liên quan |
| Type | enum | Toll / Parking / Loading / Fine / Other |
| Amount | decimal | Số tiền |

**TripCalendarEvent**: lưu vết đồng bộ Google Calendar cho từng Trip (theo từng Calendar cụ thể — tài xế hoặc khách hàng).

#### Nhiên liệu

| Bảng | Field chính | Mô tả |
|---|---|---|
| FuelProvider | Name, ContactPhone, Address | Hãng cung cấp xăng dầu |
| FuelPriceHistory | FuelType, Price, EffectiveDate, FuelProviderId | Lịch sử giá theo hãng (hoặc giá thị trường chung) |
| FuelLog | TripId, Liters, Cost, RecordedAt | Ghi nhận thực tế đổ nhiên liệu cho 1 chuyến |

#### Hạ tầng chung

**AuditLog**: ghi lại các hành động quan trọng trong hệ thống (ai làm gì, lúc nào, trên đối tượng nào).

**Notification**: thông báo tự động gửi tới người dùng (cảnh báo giấy tờ sắp hết hạn, thiếu lịch làm việc...).

## 6. Các dịch vụ bên ngoài (External APIs) sử dụng

| Dịch vụ | Mục đích sử dụng |
|---|---|
| **VietMap API** | Tính toán tuyến đường thực tế (khoảng cách, thời gian di chuyển) giữa các địa điểm; hiển thị bản đồ nền cho chức năng xem lộ trình và điều hướng trên app tài xế. |
| **Google Calendar API** | Đồng bộ lịch trình chuyến xe lên Google Calendar của tài xế và khách hàng, kèm liên kết theo dõi hành trình công khai. |

*(Hệ thống sử dụng khóa API riêng cho từng dịch vụ, được cấu hình trong file cấu hình môi trường của Backend — không public trong tài liệu này.)*

## 7. Các nghiệp vụ chính

### 7.1. Quản lý Đơn vận chuyển
Tạo đơn với đầy đủ thông tin khách hàng, hàng hóa, điểm lấy/giao và kho trung chuyển sử dụng. Hỗ trợ 2 hình thức lấy hàng (công ty cử xe đến lấy / khách tự mang tới kho).

### 7.2. Phân bổ xe & tài xế tự động (thuật toán Greedy)
Tự động chọn xe và tài xế phù hợp nhất cho từng đơn hàng dựa trên năng lực chuyên chở, tình trạng giấy tờ pháp lý, lịch làm việc và vị trí đóng quân — không cần điều phối viên chọn thủ công.

### 7.3. Quản lý Kho trung chuyển
Sơ đồ hóa kho theo đúng tỉ lệ không gian thật, phân chia khu vực theo tính chất hàng hóa (thường/dễ vỡ/nguy hiểm). Thuật toán Best-Fit tự động đề xuất vị trí lưu trữ tối ưu, có ưu tiên theo mức độ khẩn cấp của đơn hàng.

### 7.4. Nhập – Xuất kho (Cross-docking)
Tự động hóa toàn bộ luồng nhập hàng khi xe lấy hàng về tới kho, và xuất hàng khi xe bắt đầu chặng giao — đảm bảo đúng trình tự nghiệp vụ thực tế.

### 7.5. Quản lý Đội xe & Tài xế
Theo dõi tình trạng giấy tờ pháp lý của xe (đăng kiểm, bảo hiểm), lịch làm việc và hồ sơ tài xế (bằng lái, kinh nghiệm).

### 7.6. Theo dõi hành trình qua GPS
Ghi nhận vị trí thực tế của xe theo thời gian thực qua ứng dụng di động dành cho tài xế, hiển thị trực quan trên bản đồ, đối chiếu với lộ trình lý thuyết.

### 7.7. Quản lý chi phí vận hành
Tính toán và so sánh chi phí nhiên liệu ước tính (dựa trên quãng đường thực tế di chuyển) với chi phí thực tế đã ghi nhận, cùng các chi phí phát sinh khác.

### 7.8. Theo dõi công khai cho khách hàng
Khách hàng có thể tra cứu trạng thái đơn hàng của mình qua đường link công khai, không cần đăng nhập vào hệ thống.

### 7.9. Đồng bộ lịch trình
Tự động đưa lịch trình các chuyến xe lên Google Calendar của cả tài xế và khách hàng liên quan.

### 7.10. Thông báo & Nhật ký hệ thống
Cảnh báo chủ động các vấn đề cần chú ý; lưu vết các thao tác quan trọng phục vụ tra cứu, kiểm tra sau này.

## 8. Phân công nhóm

| Mảng nghiệp vụ | Người phụ trách |
|---|---|
| Phân quyền, Khách hàng, OCR danh thiếp, Thanh toán/Hóa đơn, Chatbot | Minh |
| Vận chuyển, Kho trung chuyển, Đội xe, Tài xế, Bản đồ/Định tuyến, Chuyến đi, Nhiên liệu, GPS, Ứng dụng tài xế | Nhân |
