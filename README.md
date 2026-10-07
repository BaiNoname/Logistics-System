# TruckingSystem — Hệ thống Quản lý Vận tải và Kho Trung chuyển Hàng hóa Nội địa

> Đồ án tốt nghiệp — Nhóm: **Nguyễn Tuấn Thiện Nhân**, **Nguyễn Hoàng Minh** — GVHD: **Lê Thọ**

## Mục lục

1. [Giới thiệu](#1-giới-thiệu)
2. [Mục tiêu đồ án](#2-mục-tiêu-đồ-án)
3. [Có gì mới trong phiên bản này](#3-có-gì-mới-trong-phiên-bản-này)
4. [Công nghệ sử dụng](#4-công-nghệ-sử-dụng)
5. [Kiến trúc & mô hình triển khai](#5-kiến-trúc--mô-hình-triển-khai)
6. [Cơ sở dữ liệu](#6-cơ-sở-dữ-liệu)
7. [Các dịch vụ bên ngoài](#7-các-dịch-vụ-bên-ngoài-external-apis)
8. [Các nghiệp vụ chính](#8-các-nghiệp-vụ-chính)
9. [Cài đặt & chạy hệ thống](#9-cài-đặt--chạy-hệ-thống)
10. [Hướng dẫn sử dụng](#10-hướng-dẫn-sử-dụng)
11. [Xử lý sự cố thường gặp](#11-xử-lý-sự-cố-thường-gặp)
12. [Hạn chế đã biết & hướng mở rộng](#12-hạn-chế-đã-biết--hướng-mở-rộng)
13. [Phân công nhóm](#13-phân-công-nhóm)

---

## 1. Giới thiệu

TruckingSystem là hệ thống phần mềm quản lý vận tải đường bộ và kho trung chuyển (cross-docking) dành cho doanh nghiệp logistics vừa và nhỏ tại Việt Nam. Hệ thống hỗ trợ toàn bộ chuỗi nghiệp vụ từ lúc tiếp nhận đơn hàng vận chuyển, tự động phân bổ xe/tài xế, quản lý nhập-xuất kho, theo dõi hành trình qua GPS và **camera hành trình**, cho đến việc tính toán chi phí vận hành và **lãi/lỗ** của từng đơn hàng.

Đồ án được xây dựng theo 2 nhóm chức năng chính:
- **Vận hành vận tải** (đơn hàng, chia xe, đội xe, tài xế, kho bãi, GPS, video hành trình, chi phí, lãi/lỗ)
- **Quản trị doanh nghiệp** (khách hàng, hợp đồng, hóa đơn, phân quyền, chatbot hỗ trợ)

## 2. Mục tiêu đồ án

- Tự động hóa quy trình phân bổ xe và tài xế cho từng đơn hàng dựa trên năng lực chuyên chở thực tế (khối lượng, thể tích, số lượng), thay vì phân công thủ công.
- Số hóa việc quản lý kho trung chuyển theo đúng không gian vật lý thật (sơ đồ tỉ lệ mét, có vị trí cửa kho), tự động đề xuất vị trí lưu trữ tối ưu theo thuật toán Best-Fit.
- Theo dõi hành trình xe thực tế qua GPS, đối chiếu với lộ trình lý thuyết; ghi **video hành trình** để đối chiếu minh bạch khi có tranh chấp.
- Tính toán chi phí nhiên liệu, chi phí phát sinh, chi phí sự cố cho từng chuyến, làm cơ sở **đo lường lãi/lỗ theo từng đơn hàng**.
- Cung cấp công cụ theo dõi công khai cho khách hàng (tra cứu trạng thái đơn hàng qua link, không cần đăng nhập).
- Đồng bộ lịch trình lên Google Calendar cho cả tài xế và khách hàng.

## 3. Có gì mới trong phiên bản này

| Nhóm | Thay đổi |
|---|---|
| **Hủy đơn** | Chuyến không hủy lẻ được nữa — chỉ hủy **theo đơn**. Hủy đơn tự xử lý từng xe: chuyến chưa chạy → hủy; hàng đã vào kho → tạo **chuyến Trả hàng** (dùng lại đúng xe + tài xế, tính chi phí như chuyến thường); đã xuất kho đi giao → **chặn hủy**. Lưu lý do hủy. |
| **Duyệt hoàn thành 2 bước** | Tài xế bấm "Đã xong" trên app → chuyến sang **Chờ xác nhận**, Vận hành nhận thông báo → **Duyệt** (mới nhập/xuất kho, trả xe rảnh) hoặc **Từ chối** (quay lại Đang chạy). GPS ngừng ghi ngay khi tài xế báo xong. |
| **Chặng giao phụ thuộc chặng lấy hàng** | Chuyến Giao chỉ bắt đầu được khi **mọi chuyến Lấy hàng của đơn đã Hoàn tất** (đã duyệt, hàng đã nhập kho) — áp dụng cả Desktop lẫn app. Desktop hiện "⏳ Chờ lấy hàng xong". |
| **App tài xế: chặn chuyến đã xong** | Chuyến đã báo xong / hoàn tất / hủy thì không bật GPS được nữa, app báo rõ "Chuyến đã hoàn thành". GPS đầu tiên của chuyến giờ đi đúng quy trình bắt đầu (kiểm tra + xuất kho). |
| **Trạng thái tài xế** | Module Tài xế riêng (tách khỏi Nhân viên): **Đang làm** (đang chạy chuyến) / **Sẵn sàng** (có ca, rảnh) / **Không sẵn sàng** (không có ca). Thêm/sửa hồ sơ, lịch làm việc. |
| **Kho** | Quy đổi số lượng ↔ m³ đúng theo hàng; kiểm tra **tải trọng tối đa** của ô kệ; **vùng gần cửa** = 1/3 số ô gần cửa nhất, dành ưu tiên cho hàng gấp; **cửa kho** khai báo theo tường + khoảng cách, hiển thị trên sơ đồ; khoảng cách ô→cửa **tự tính**. |
| **Bản đồ** | Icon rõ ràng: **khởi hành xanh lá**, **kho vàng**, **điểm đến đỏ**, **vị trí xe = mũi tên xám** xoay theo hướng chạy; bảng chú thích. Đường đã đi **tự mất dần** như Google Maps; chỉ gọi lại VietMap khi lệch tuyến > 100 m. Desktop và app tài xế dùng chung 1 kiểu hiển thị. |
| **Tổng quan & Lãi/Lỗ** | Dashboard: doanh thu, chi phí (nhiên liệu / phát sinh / sự cố), lãi/lỗ, biên lợi nhuận, đơn hủy & chi phí mất do hủy, biểu đồ theo tháng, cơ cấu chi phí, bảng lãi/lỗ từng đơn, xem chi tiết từng khoản chi. |
| **Video hành trình** | App tài xế quay video (480p, có tiếng, **bật/tắt tùy chọn**, độc lập với GPS), tự cắt đoạn 1 phút và gửi về máy chủ. Desktop: bấm 1 điểm trên đường đi → phát video đúng lúc xe đi qua; kiểm tra toàn vẹn bằng SHA-256. |
| **Triển khai** | Tự host trên máy tại chỗ + **Cloudflare Tunnel** với tên miền riêng có HTTPS — không cần IP tĩnh, không mở cổng router, không thuê host database. |

## 4. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Backend API | ASP.NET Core (.NET 8) |
| Kiến trúc | Clean Architecture (Domain → Application → Infrastructure → API) |
| Cơ sở dữ liệu | PostgreSQL, Entity Framework Core (Npgsql) |
| Ứng dụng quản trị (Desktop) | Windows Forms (.NET 8) + WebView2 (bản đồ Leaflet) |
| Ứng dụng tài xế (Mobile) | React Native (Expo SDK 57) — expo-location, expo-camera, expo-file-system, WebView |
| Xác thực | ASP.NET Core Identity + JWT |
| Bản đồ & định tuyến | VietMap API (tuyến đường + ảnh nền bản đồ), Leaflet |
| Đồng bộ lịch | Google Calendar API |
| Đưa API ra internet | Cloudflare Tunnel (`cloudflared`) + tên miền riêng |
| Build app Android | EAS Build (Expo) |

## 5. Kiến trúc & mô hình triển khai

```
 ┌──────────────────────┐   ┌──────────────────────┐   ┌──────────────────────┐
 │ Desktop (WinForms)   │   │ App tài xế (Expo)    │   │ Trang công khai      │
 │ Điều phối / Vận hành │   │ GPS + video + bản đồ │   │ track.html (khách)   │
 └──────────┬───────────┘   └──────────┬───────────┘   └──────────┬───────────┘
            │  localhost hoặc domain   │  domain (HTTPS)          │  domain
            └──────────────┬───────────┴──────────────────────────┘
                           ▼
                 ┌─────────────────────┐
                 │  Cloudflare Tunnel  │   https://trucking.logisticssystem.dpdns.org
                 └─────────┬───────────┘
                           ▼   (máy chủ tại chỗ — PC chạy cloudflared)
                 ┌─────────────────────┐      ┌───────────────────────────┐
                 │  ASP.NET Core API   │─────▶│ PostgreSQL (trên cùng máy)│
                 │ (Clean Architecture)│      └───────────────────────────┘
                 │  wwwroot: navigate, │      ┌───────────────────────────┐
                 │  track.html         │─────▶│ Kho video (ổ đĩa máy chủ) │
                 └─────────────────────┘      └───────────────────────────┘
```

- **Domain**: thực thể nghiệp vụ (entities) và enum, không phụ thuộc công nghệ.
- **Application**: logic nghiệp vụ (services), interface `IApplicationDbContext`, `IVideoStorage`.
- **Infrastructure**: EF Core/PostgreSQL, VietMap, Google Calendar, lưu file video (`LocalVideoStorage`).
- **API**: controller HTTP + trang tĩnh `wwwroot` (`navigate.html` cho app tài xế, `track.html` cho khách).

**Mô hình tự host (self-hosted):** máy chủ đặt tại chỗ chạy cùng lúc API, PostgreSQL và kho video. Cloudflare Tunnel đưa API ra internet có HTTPS; database **không** mở ra ngoài (chỉ API truy cập). Desktop ở máy khác (VD laptop) nhập địa chỉ domain lúc đăng nhập là dùng chung dữ liệu của máy chủ.

## 6. Cơ sở dữ liệu

### 6.1. Sơ đồ tổng quan các nhóm bảng

```
┌──────────────┐     ┌────────────────┐     ┌──────────────┐     ┌──────────────────┐
│  Khách hàng  │────▶│ Đơn vận chuyển │────▶│    Chuyến    │────▶│ GPS (Waypoint)   │
│  (Customer)  │     │(TransportOrder)│     │    (Trip)    │     │ Video (TripVideo │
└──────────────┘     └───────┬────────┘     └───────┬──────┘     │   Segment)       │
                             │                      │            └──────────────────┘
                             ▼                      ▼
                     ┌──────────────┐       ┌────────────────┐
                     │ Kho trung    │       │ Đội xe &       │
                     │ chuyển (+cửa)│       │ Tài xế         │
                     └──────┬───────┘       └───────┬────────┘
                            ▼                       ▼
                     ┌──────────────┐       ┌────────────────────────┐
                     │ Tồn kho &    │       │ Chi phí: xăng, phát    │
                     │ vị trí lưu   │       │ sinh, sự cố → Lãi/Lỗ   │
                     └──────────────┘       └────────────────────────┘
```

### 6.2. Chi tiết các bảng chính

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
| DoorSide | enum? | Cửa kho nằm trên tường nào: Top / Bottom / Left / Right |
| DoorOffsetMeters | decimal? | Vị trí cửa tính từ đầu bức tường (m) — dùng tính khoảng cách ô kệ tới cửa |

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
| DistanceFromDoorMeters | decimal | Khoảng cách tới cửa kho — **hệ thống tự tính** (khoảng cách Manhattan từ tâm ô tới cửa), không nhập tay |

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
| LegType | enum | Pickup (lấy hàng về kho) / Delivery (xuất kho đi giao) / Return (trả hàng về đối tác khi đơn bị hủy) |
| Status | enum | Planned / InProgress / Completed / Cancelled / PendingConfirmation (tài xế đã báo xong, chờ Vận hành duyệt) |
| CancelReason | string? | Lý do hủy (khi chuyến bị hủy theo đơn) |
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

**Notification**: thông báo tự động gửi tới người dùng (cảnh báo giấy tờ sắp hết hạn, thiếu lịch làm việc...). `UserId` để trống = thông báo **chung** cho mọi người dùng Desktop (VD "Tài xế báo xong chuyến #12 — chờ duyệt").

#### Video hành trình (mới)

**TripVideoSegment** (1 đoạn video ~1 phút của chuyến) — **database chỉ lưu thông tin mô tả, file video nằm trên ổ đĩa máy chủ**

| Field | Kiểu | Mô tả |
|---|---|---|
| TripId | int | Chuyến liên quan |
| StoragePath | string | Đường dẫn tương đối tới file, VD `trip-12/20261006-083015-ab12cd34.mp4` |
| ContentType | string | `video/mp4`... |
| StartedAt / EndedAt | DateTime (UTC) | Giờ bắt đầu/kết thúc quay — cùng đồng hồ với GPS nên khớp video với vị trí xe |
| SizeBytes | long | Dung lượng file |
| Sha256 | string | Mã băm tính lúc nhận file — đối chiếu lại để chứng minh video chưa bị chỉnh sửa |

## 7. Các dịch vụ bên ngoài (External APIs)

| Dịch vụ | Mục đích sử dụng |
|---|---|
| **VietMap API** | Tính tuyến đường thực tế (khoảng cách, thời gian) giữa các địa điểm; ảnh nền bản đồ cho màn xem lộ trình (Desktop) và điều hướng (app tài xế). |
| **Google Calendar API** | Đồng bộ lịch chuyến lên Google Calendar của tài xế và khách hàng, kèm link theo dõi công khai. |
| **Cloudflare Tunnel** | Đưa API từ máy chủ tại chỗ ra internet qua tên miền có HTTPS (app tài xế, trang theo dõi công khai). |

*(Khóa API cấu hình trong `appsettings.json` của API — không public trong tài liệu này.)*

## 8. Các nghiệp vụ chính

### 8.1. Quản lý Đơn vận chuyển
Tạo đơn với đầy đủ thông tin khách hàng, hàng hóa, điểm lấy/giao và kho trung chuyển. Hỗ trợ 2 hình thức lấy hàng: **công ty cử xe tới lấy** (có chặng Lấy hàng) hoặc **khách tự mang tới kho** (nhập kho ngay lúc chia xe, không có chặng Lấy hàng).

### 8.2. Phân bổ xe & tài xế tự động (Greedy)
Tự chọn xe và tài xế phù hợp theo năng lực chuyên chở, giấy tờ pháp lý còn hạn, lịch làm việc, vị trí đóng quân; xe/tài xế đang có chuyến (kể cả đang Chờ xác nhận) không được chọn trùng.

### 8.3. Vòng đời 1 chuyến

```
Đã lên kế hoạch ──(Bắt đầu / GPS đầu tiên)──▶ Đang chạy ──(tài xế báo xong)──▶ Chờ xác nhận
      │                                          ▲                               │
      │ (hủy theo đơn)                           └──────── Từ chối ──────────────┤
      ▼                                                                          ▼ Duyệt
    Đã hủy                                                                   Hoàn tất
```

- **Lấy hàng** hoàn tất (được duyệt) → hàng **nhập kho** (Best-Fit chọn ô kệ).
- **Giao hàng** chỉ được bắt đầu khi **mọi chặng Lấy hàng của đơn đã Hoàn tất**; lúc bắt đầu thì hàng **xuất kho**.
- **Trả hàng** (đơn bị hủy khi hàng đã vào kho) → hoàn tất thì hàng xuất kho trả về đối tác.
- Vận hành tự bấm **Hoàn tất** trên Desktop = duyệt luôn, không qua bước chờ.

### 8.4. Hủy đơn
| Tình trạng hàng khi hủy | Hệ thống xử lý |
|---|---|
| Các chuyến còn "Đã lên kế hoạch" | Hủy chuyến, trả xe/tài xế về rảnh |
| Hàng đã lấy về kho | Tạo **chuyến Trả hàng** dùng lại đúng xe + tài xế |
| Đã xuất kho đi giao (chuyến giao đang chạy / chờ duyệt / xong) | **Không cho hủy** |

### 8.5. Kho trung chuyển
Sơ đồ kho đúng tỉ lệ mét, khu vực theo tính chất hàng (thường / dễ vỡ / nguy hiểm). **Cửa kho** khai báo theo tường + khoảng cách, vẽ trên sơ đồ; khoảng cách từng ô tới cửa tự tính. Best-Fit chọn ô theo thể tích **và** tải trọng còn lại; **1/3 số ô gần cửa nhất** dành ưu tiên cho hàng gấp (xuất sớm).

### 8.6. Đội xe & Tài xế
Giấy tờ xe (đăng kiểm, bảo hiểm...), hồ sơ tài xế (bằng lái, kinh nghiệm), lịch làm việc theo ca. Trạng thái tài xế tự tính: **Đang làm / Sẵn sàng / Không sẵn sàng**.

### 8.7. GPS & bản đồ
App tài xế gửi GPS mỗi 30 giây (chạy nền). Bản đồ (app và Desktop) dùng chung quy ước màu, cắt dần phần đường đã đi, tính lại tuyến khi lệch > 100 m. Chuyến đã xong: chỉ hiện đường GPS thật đã đi (màu cam).

### 8.8. Video hành trình
Tài xế bật/tắt quay video trên app. Video quay theo đoạn 1 phút, lưu tạm trên điện thoại, gửi về máy chủ qua domain; gửi xong thì xóa khỏi điện thoại, mất mạng thì giữ lại và gửi tiếp sau. Máy chủ lưu file trên ổ đĩa, database chỉ lưu đường dẫn + giờ + mã SHA-256. Desktop xem lại video **khớp với vị trí GPS**.

### 8.9. Chi phí & Lãi/Lỗ
Ghi nhận đổ xăng thực tế, chi phí phát sinh (cầu đường, bến bãi, bốc xếp, phạt...), chi phí sự cố. Màn **Tổng quan** tổng hợp doanh thu (hóa đơn không hủy) − chi phí = lãi/lỗ theo kỳ và theo từng đơn.

### 8.10. Theo dõi công khai, lịch, thông báo
Khách tra cứu đơn qua link công khai; lịch chuyến đồng bộ Google Calendar; thông báo chủ động (giấy tờ sắp hết hạn, tài xế báo xong chuyến...) và nhật ký thao tác.

## 9. Cài đặt & chạy hệ thống

### 9.1. Yêu cầu
- Windows 10/11, **.NET 8 SDK**, **Visual Studio 2022**
- **PostgreSQL** (14+) + pgAdmin
- **Microsoft Edge WebView2 Runtime** (thường có sẵn trên Windows 11)
- Node.js 20+, tài khoản **Expo** (build app tài xế bằng EAS)
- (Tùy chọn) tài khoản **Cloudflare** + tên miền, để app tài xế dùng được từ xa
- Khóa **VietMap** (routing + tile)

### 9.2. Cấu hình API — `TruckingSystem.API/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=trucking_system;Username=postgres;Password=MAT_KHAU"
  },
  "Jwt": { "Issuer": "TruckingSystemApi", "Audience": "TruckingSystemClients", "Key": "CHUOI_BI_MAT_TOI_THIEU_32_KY_TU" },
  "VietMap": { "RouteApiKey": "KEY_ROUTING", "MapApiKey": "KEY_TILE" },
}
```

### 9.3. Tạo / cập nhật database
Trong Visual Studio → **Package Manager Console**, chọn *Default project* = `TruckingSystem.Infrastructure`, *Startup project* = `TruckingSystem.API`:
```
Update-Database
```
Sau đó chạy script `TinhLai_SucChua_OKho.sql` trong pgAdmin (tính lại sức chứa ô kho theo cấu trúc mới). Lần chạy đầu, API tự tạo dữ liệu mẫu (vai trò, tài khoản, chuyến mẫu).

> Nếu `Update-Database` báo *"relation ... already exists"*: database đã có bảng nhưng bảng `__EFMigrationsHistory` thiếu dòng tương ứng — xem mục [11](#11-xử-lý-sự-cố-thường-gặp).

### 9.4. Chạy API
Chọn `TruckingSystem.API` làm Startup Project → Run. Mặc định: `https://localhost:8080` (Swagger: `/swagger`).
Lần đầu nếu trình duyệt/WebView2 báo lỗi chứng chỉ localhost: `dotnet dev-certs https --trust`.

### 9.5. Chạy Desktop
Run `TruckingSystem.Desktop`. Ở màn đăng nhập, ô **Server URL**:
- `https://localhost:8080` — khi Desktop chạy **cùng máy** với API (nhanh nhất, xem video mượt nhất)
- `https://<tên-miền>` — khi Desktop chạy ở **máy khác** (dùng chung dữ liệu của máy chủ qua Cloudflare)

Tài khoản mẫu: `admin` / `Admin@123`.

### 9.6. Đưa API ra internet (Cloudflare Tunnel)
1. Cloudflare Zero Trust → *Networks → Tunnels* → tạo tunnel, cài `cloudflared` trên **máy chủ** bằng lệnh/token được cấp (chạy dạng dịch vụ Windows, tự chạy lại khi bật máy).
2. *Public Hostname*: tên miền → `https://localhost:8080`, bật **No TLS Verify** (chứng chỉ localhost tự ký).
3. Kiểm tra: `https://<tên-miền>/track.html` mở được.

> ⚠️ Chỉ **1 máy** được chạy tunnel này. Nếu 2 máy cùng chạy, Cloudflare chia yêu cầu ngẫu nhiên cho cả 2 → lúc gặp code mới, lúc gặp code cũ (lỗi 401/404 khó hiểu).

### 9.7. App tài xế (Android)
```
cd driver-gps-app
npm install
npx expo install expo-camera expo-keep-awake expo-file-system
eas build -p android --profile preview
```
Tải APK từ link EAS và cài lên điện thoại. **Cập nhật app: cài đè, không gỡ app cũ** (gỡ sẽ mất các đoạn video chưa gửi).
Trang bản đồ điều hướng (`navigate.html`) nằm trên API (`wwwroot`) — sửa giao diện bản đồ **không cần** build lại APK.

## 10. Hướng dẫn sử dụng

### 10.1. Quy trình nghiệp vụ chuẩn (Desktop)

1. **Chuẩn bị dữ liệu nền**
   - *Kho*: tạo kho (kích thước, tọa độ, **tường đặt cửa + khoảng cách cửa**), tạo khu vực và ô kệ. Sơ đồ hiện cửa kho; rê chuột lên ô để xem khoảng cách tới cửa.
   - *Xe*: biển số, năng lực (kg / m³ / số lượng), định mức nhiên liệu, giấy tờ.
   - *Tài xế*: màn **Tài xế** → **Thêm tài xế** (chọn từ nhân viên), **Hồ sơ** (bằng lái...), **Lịch làm việc** (ca). Cột trạng thái tự hiện Đang làm / Sẵn sàng / Không sẵn sàng.
   - *Khách hàng, địa điểm*: chọn địa điểm từ gợi ý VietMap để có tọa độ.
2. **Tạo đơn** → chọn khách, hàng hóa, điểm lấy/giao, kho, hình thức lấy hàng, mức độ gấp.
3. **Chia xe** → hệ thống tự chọn xe + tài xế, tạo chuyến Lấy hàng và Giao hàng.
4. **Gửi Trip ID cho tài xế** (màn **Chuyến**). Tài xế dùng app (mục 10.2).
5. **Duyệt chuyến**: khi tài xế báo xong, chuông thông báo hiện "🟢 Tài xế báo xong chuyến #… — chờ duyệt". Vào **Chuyến** → dòng "🟡 Chờ xác nhận" → **Xem lộ trình** (kiểm tra đường đi + video) → **Duyệt** hoặc **Từ chối**.
   - Duyệt chuyến Lấy hàng → hàng nhập kho tự động.
   - Chuyến Giao hiện "⏳ Chờ lấy hàng xong" cho tới khi mọi chuyến Lấy hàng của đơn được duyệt; sau đó tài xế mới bắt đầu được.
6. **Ghi chi phí** trong lúc/sau chuyến: **Đổ xăng**, **Chi phí phát sinh**, **Sự cố**.
7. **Hủy đơn** (nếu cần): màn chi tiết đơn → **Hủy đơn** → nhập lý do. Hệ thống tự hủy chuyến / tạo chuyến Trả hàng / chặn hủy tùy tình trạng hàng (mục 8.4).
8. **Theo dõi lãi/lỗ**: màn **📊 Tổng quan** → chọn kỳ (tháng này, 3 tháng, năm nay, tùy chọn) → xem thẻ số liệu, biểu đồ; **double-click** 1 đơn để xem từng khoản chi.

### 10.2. App tài xế

1. Nhập **Server URL** (tên miền) và **Trip ID** do Vận hành gửi.
2. (Tùy chọn) Bật **🎥 Quay video hành trình** → cấp quyền Camera + Micro. Tắt thì chuyến chỉ có GPS.
3. Bấm **Start GPS Tracking** → cấp quyền vị trí "Luôn cho phép".
   - App hỏi máy chủ trước: chuyến đã xong / chờ duyệt / đã hủy → báo **"Chuyến đã hoàn thành"** (hoặc đã hủy), không bật. Chuyến Giao khi chuyến Lấy hàng chưa xong → báo **"Chưa thể bắt đầu chuyến"** kèm lý do.
4. **Open Navigation** → bản đồ chỉ đường: mũi tên xám là xe, đường xanh là đoạn còn phải đi (tự ngắn lại khi chạy). Kéo bản đồ ra chỗ khác → bấm **🎯 Về vị trí xe** để bám lại.
5. Nếu bật video: khung camera nhỏ ở góc màn hình (dấu ● REC). **Giữ app mở** khi chạy (màn hình tự sáng). Dòng trạng thái cho biết đã gửi / chờ gửi bao nhiêu đoạn; lỗi gửi hiện chữ đỏ, bấm **🔄 Gửi lại ngay** để thử lại.
6. Xong việc → bấm **✅ Xác nhận đã LẤY/GIAO/TRẢ HÀNG xong** → xác nhận. GPS dừng, chuyến chuyển "Chờ xác nhận" chờ Vận hành duyệt.

### 10.3. Xem lại lộ trình & video (Desktop)

Màn **Chuyến** → chọn chuyến → **Xem lộ trình**:
- *Chuyến chưa có GPS*: đường lý thuyết nét đứt để xem trước.
- *Chuyến đang chạy*: tự cập nhật mỗi 15 giây, mũi tên xám là vị trí xe, đường đã đi tự mất.
- *Chuyến Chờ xác nhận / Hoàn tất*: đường GPS thật (cam). Nếu có video: đoạn có video tô **tím**, khung video bên phải.
  - **Bấm vào đường** trên bản đồ → video phát đúng lúc xe đi qua chỗ đó; video chạy thì mũi tên chạy theo (kèm giờ, tốc độ).
  - **Thanh thời gian**: khối tím = có video; bấm để tua.
  - **Kiểm tra** ở từng đoạn: ✅ = video trùng khớp bản đã nhận (chưa bị sửa); ⚠️ = file đã bị thay đổi.

### 10.4. Quy ước màu bản đồ

| Ký hiệu | Ý nghĩa |
|---|---|
| 🟢 Ghim xanh lá | Điểm khởi hành |
| 🟡 Ghim vàng | Kho |
| 🔴 Ghim đỏ | Điểm đến / điểm lấy-trả hàng của đối tác |
| Ghim 2 màu | 2 điểm trùng chỗ (VD xuất phát từ kho và quay về đúng kho đó) |
| ➤ Mũi tên xám | Vị trí xe, xoay theo hướng chạy |
| Đường xanh | Tuyến còn phải đi |
| Đường cam | Đường GPS thật đã đi |
| Đường tím | Đoạn có video hành trình |

## 11. Xử lý sự cố thường gặp

| Hiện tượng | Nguyên nhân & cách xử lý |
|---|---|
| `Update-Database` báo *relation "…" already exists* | Database có bảng nhưng `__EFMigrationsHistory` thiếu dòng. Kiểm tra bảng/cột nào đã có, `INSERT` tên các migration đã có sẵn vào `__EFMigrationsHistory`, rồi chạy lại `Update-Database` cho phần còn thiếu. |
| `dotnet ef` không chạy được | Dùng lệnh của Package Manager Console: `Add-Migration`, `Update-Database`. |
| Qua tên miền gặp **401/404** hoặc giao diện cũ, localhost thì đúng | Có máy khác (chạy code cũ) cũng đang chạy cùng tunnel → tắt `cloudflared` ở máy đó. |
| App: video "chờ gửi" không giảm | Xem dòng lỗi đỏ: *404* = máy chủ chưa có chức năng video / tên miền trỏ nhầm máy; lỗi mạng = kiểm tra kết nối. Video vẫn được giữ lại trên máy cho tới khi gửi được. |
| Xem video bị quay vòng liên tục | Desktop đang đi qua tên miền (băng thông upload mạng nhà) hoặc thư mục video nằm trong OneDrive → dùng `https://localhost:8080` khi ngồi tại máy chủ, đặt `VideoStorage:RootPath` ngoài OneDrive. |
| Video báo "không phát được" | Link hết hạn (6 giờ) → đóng và mở lại màn lộ trình; hoặc chứng chỉ localhost chưa tin cậy → `dotnet dev-certs https --trust`. |
| Bản đồ trắng / lỗi tải ảnh nền | Kiểm tra `VietMap:MapApiKey`; mạng có chặn VietMap không. |
| Không bắt đầu được chuyến Giao | Đúng quy tắc: còn chuyến Lấy hàng chưa Hoàn tất (xem thông báo kèm số chuyến). Duyệt chuyến Lấy hàng trước. |

## 12. Hạn chế đã biết & hướng mở rộng

**Hạn chế**
- App tài xế **chưa có đăng nhập**: ai biết Trip ID đều gửi được GPS/video cho chuyến đó (đánh đổi để đơn giản hóa demo).
- Máy chủ tự host: máy tắt / mất mạng thì app và Desktop từ xa không dùng được; dữ liệu và video nằm trên 1 máy duy nhất → cần tự sao lưu (Backup database bằng pgAdmin + chép thư mục video).
- Video chỉ quay khi app đang mở (hệ điều hành không cho dùng camera khi app chạy nền); **chưa có tự động xóa video cũ**.
- iPhone có thể quay ra định dạng HEVC mà trình phát trong Desktop không phát được → demo bằng Android.
- Chặng Giao chờ **toàn bộ** chặng Lấy hàng xong (chưa hỗ trợ giao từng phần theo số hàng đã về kho).

**Hướng mở rộng**
- Đăng nhập cho tài xế, gắn chuyến với tài khoản.
- Đưa database và kho video lên đám mây (PostgreSQL managed; Cloudflare R2 / S3) — code đã tách `IVideoStorage` nên chỉ cần thêm 1 lớp cài đặt mới.
- Chính sách lưu video có thời hạn (VD 30 ngày, giữ lại chuyến có sự cố); chỉ gửi video khi có Wi-Fi hoặc chỉ gửi đoạn quanh thời điểm sự cố.
- Giao từng phần theo tồn kho thực tế; tối ưu ghép đơn cho cùng 1 xe.

## 13. Phân công nhóm

| Mảng nghiệp vụ | Người phụ trách |
|---|---|
| Phân quyền, Nhân viên, Khách hàng, OCR danh thiếp, Hợp đồng, Bảng giá cứng, Thanh toán/Hóa đơn, Chatbot | Nguyễn Hoàng Minh |
| Vận chuyển, Chia xe, Kho trung chuyển, Đội xe, Tài xế, Bản đồ/Định tuyến, Chuyến đi, GPS, Video hành trình, Nhiên liệu & Chi phí, Tổng quan Lãi/Lỗ, Ứng dụng tài xế, Triển khai (Cloudflare Tunnel), Thông báo, Nhật ký hệ thống | Nguyễn Tuấn Thiện Nhân |
