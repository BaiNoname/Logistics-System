# TruckingSystem — Hệ thống Quản lý Vận tải và Kho Trung chuyển Hàng hóa Nội địa

Đồ án tốt nghiệp — Nhóm: **Nguyễn Tuấn Thiện Nhân**, **Nguyễn Hoàng Minh** — GVHD: **Lê Thọ**

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
14. [Làm mới dữ liệu & tài liệu kiểm thử](#14-làm-mới-dữ-liệu--tài-liệu-kiểm-thử)

---

## 1. Giới thiệu
TruckingSystem là hệ thống quản lý vận tải đường bộ và kho trung chuyển (cross-docking) cho doanh nghiệp logistics vừa và nhỏ tại Việt Nam. Hệ thống hỗ trợ cả chuỗi nghiệp vụ:
- tiếp nhận đơn hàng vận chuyển;
- tự động phân bổ xe và tài xế;
- quản lý nhập, xuất kho;
- theo dõi hành trình qua GPS và camera hành trình;
- tính chi phí vận hành và lãi/lỗ của từng đơn hàng.

Đồ án được xây dựng theo 2 nhóm chức năng chính:
- **Vận hành vận tải**: đơn hàng, chia xe, đội xe, tài xế, kho bãi, GPS, video hành trình, chi phí, lãi/lỗ, thông báo
- **Quản trị doanh nghiệp**: khách hàng, hợp đồng, bảng giá, hóa đơn, phân quyền, lương, chatbot hỗ trợ

## 2. Mục tiêu đồ án
- Tự động hóa việc phân bổ xe và tài xế cho từng đơn hàng theo năng lực chuyên chở thực tế (khối lượng, thể tích, số lượng), thay cho phân công thủ công.
- Số hóa việc quản lý kho trung chuyển theo đúng không gian vật lý thật: sơ đồ tỉ lệ mét, có vị trí cửa kho. Hệ thống tự đề xuất vị trí lưu trữ tối ưu bằng thuật toán Best-Fit.
- Theo dõi hành trình xe thực tế qua GPS, đối chiếu với lộ trình lý thuyết. Ghi video hành trình để đối chiếu minh bạch khi có tranh chấp.
- Tính chi phí nhiên liệu, chi phí phát sinh, chi phí sự cố cho từng chuyến, làm cơ sở đo lãi/lỗ theo từng đơn hàng.
- Cung cấp công cụ theo dõi công khai cho khách hàng: tra cứu trạng thái đơn qua link, không cần đăng nhập.
- Đồng bộ lịch trình lên Google Calendar cho cả tài xế và khách hàng.

---

## 3. Có gì mới trong phiên bản này

| Nhóm | Thay đổi |
|---|---|
| **Kho: nhập & xuất theo chuyến** | Duyệt chuyến Lấy hàng không còn tự nhập kho. Hàng chuyển sang **📦 Chờ nhập kho**, nhân viên kho nhập ở **Kho → Nhập hàng từ chuyến** (chọn ô thủ công hoặc Best-Fit). Chặng Giao chỉ chạy khi kho đã **📤 Xác nhận xuất kho theo chuyến**. Kho xuất đủ cả chuyến hoặc không xuất dòng nào, có thể **Hoàn tác xuất** khi xe chưa chạy. Không xuất lên xe đang Bảo trì / Ngưng hoạt động. Chỉ xuất sớm nhất **1 ngày trước** ngày xuất phát dự kiến. |
| **Đơn khách tự mang tới** | Thủ kho nhập đủ hàng khách mang tới (**Kho → Nhận hàng khách mang tới**) rồi mới Chia xe. Chia xe không tự nhập kho nữa. |
| **Đơn gắn hợp đồng** | Đơn chọn được hợp đồng của đúng khách. Hợp đồng phải đang hiệu lực lúc tạo, sửa và xác nhận đơn. Cột **Mã HĐ** trong danh sách đơn. |
| **Điểm đi / Điểm đến của chuyến** | Lấy hàng: điểm lấy → 🏬 kho. Giao: 🏬 kho → điểm giao. Trả hàng: 🏬 kho → điểm lấy. |
| **Đổi hạn giao & phí lưu kho** | Đổi hạn phải chọn **Bên yêu cầu** (Khách hàng / Công ty) và ghi lý do. Khách xin dời trễ hơn thì tính **phí lưu kho** = m³ hàng còn trong kho × số ngày × đơn giá kho (đ/m³/ngày). Có xem trước số tiền khi đổi, xem chi tiết ở **💰 Phí lưu kho**. Hạn mới làm chuyến không kịp thì hệ thống cảnh báo ngay. |
| **Thông báo kiểu "việc cần làm"** | 🔵 = chưa xử lý, bấm xem **không** làm mất. Việc thật (chờ duyệt, chờ nhập kho, chuyến bị chặn, hạn giao, giấy tờ, lịch tài xế) tự chuyển **Đã xử lý** khi làm xong. **Nhấp đúp** mở thẳng màn hình xử lý, lọc sẵn đúng chuyến / đơn / xe / tài xế / kho. |
| **Kiểm hàng khi giao** | App tài xế phải tick đủ từng dòng hàng và chụp ≥ 1 ảnh mới báo xong được. Bị từ chối thì phải chụp lại ảnh mới. |
| **Ràng buộc xe / tài xế** | Chia xe và Đổi xe bắt buộc: đúng kho, đủ 3 giấy tờ xe còn hạn tới hết chuyến, hạng bằng hợp với tải trọng / loại xe, giấy khám sức khỏe còn hạn, đăng ký đủ ngày làm. Sửa xe hoặc hồ sơ tài xế mà làm hỏng chuyến chưa chạy thì bị chặn. Xe / tài xế còn chuyến thì không đổi kho đóng quân. |
| **Đổi xe / tài xế** | Gỡ chuyến bị kẹt (xe hỏng, giấy tờ hết hạn): đổi xe hoặc tài xế cho cả cặp chuyến chưa chạy. Chuyến Trả hàng có thể **Trả khách tại kho**. |
| **Dữ liệu sạch hơn** | Không trùng tên kho, tên địa điểm. Tổng sức chứa các ô ≤ sức chứa kho. Ô tìm địa chỉ VietMap dùng chung, hiện rõ trạng thái đang tìm / không có kết quả, chỉ chọn khi bấm chuột hoặc Enter. |
| **Bảo mật API** | API Chuyến chỉ cho Admin, Dispatcher. API Kho và Nhập hàng từ chuyến chỉ cho Admin, Dispatcher, WarehouseStaff. Các API của app tài xế giữ nguyên. |
| **Giao diện** | Thẻ KPI có số đếm động, bấm để lọc. Nhãn trạng thái màu, thanh tiến độ. Màn danh sách cuộn được khi màn hình nhỏ. |
| **Dữ liệu mẫu** | API chỉ đổ dữ liệu mẫu khi bật `Seed:DemoData`. Có sẵn script xóa dữ liệu để test lại từ đầu (mục 14). |

<details>
<summary>Phiên bản trước</summary>

| Nhóm | Thay đổi |
|---|---|
| Hủy đơn | Không hủy lẻ từng chuyến, chỉ hủy theo đơn. Hủy đơn tự xử lý từng xe: chuyến chưa chạy thì hủy; hàng đã vào kho thì tạo chuyến Trả hàng (dùng lại đúng xe + tài xế, tính chi phí như chuyến thường); đã xuất kho đi giao thì chặn hủy. Lưu lý do hủy. |
| Duyệt hoàn thành 2 bước | Tài xế bấm "Đã xong" trên app → chuyến sang Chờ xác nhận → Vận hành Duyệt hoặc Từ chối (quay lại Đang chạy). GPS ngừng ghi ngay khi tài xế báo xong. |
| Chặng giao phụ thuộc chặng lấy hàng | Chuyến Giao chỉ bắt đầu khi mọi chuyến Lấy hàng của đơn đã Hoàn tất. |
| App tài xế: chặn chuyến đã xong | Chuyến đã báo xong / hoàn tất / hủy thì không bật GPS được nữa. GPS đầu tiên đi đúng quy trình bắt đầu chuyến. |
| Trạng thái tài xế | Module Tài xế riêng: Đang làm / Sẵn sàng / Không sẵn sàng. |
| Kho | Quy đổi số lượng ↔ m³; kiểm tra tải trọng ô kệ; 1/3 số ô gần cửa nhất ưu tiên cho hàng gấp; cửa kho khai báo theo tường + khoảng cách. |
| Bản đồ | Icon rõ ràng, đường đã đi tự mất dần, chỉ gọi lại VietMap khi lệch tuyến > 100 m. Desktop và app dùng chung kiểu hiển thị. |
| Tổng quan & Lãi/Lỗ | Doanh thu, chi phí, lãi/lỗ, biên lợi nhuận, đơn hủy, biểu đồ theo tháng, chi tiết từng khoản chi. |
| Video hành trình | Quay 480p, cắt đoạn 1 phút gửi về máy chủ; Desktop bấm 1 điểm trên đường đi → phát đúng lúc; kiểm tra toàn vẹn SHA-256. |
| Triển khai | Tự host + Cloudflare Tunnel với tên miền riêng có HTTPS. |
</details>

---

## 4. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Backend API | ASP.NET Core (.NET 8) |
| Kiến trúc | Clean Architecture (Domain → Application → Infrastructure → API) |
| Cơ sở dữ liệu | PostgreSQL, Entity Framework Core (Npgsql) |
| Ứng dụng quản trị (Desktop) | Windows Forms (.NET 8) + WebView2 (bản đồ Leaflet) |
| Ứng dụng tài xế (Mobile) | React Native (Expo SDK 57): expo-location, expo-camera, expo-file-system, WebView |
| Xác thực | ASP.NET Core Identity + JWT |
| Bản đồ & định tuyến | VietMap API (tìm địa chỉ, tuyến đường, ảnh nền bản đồ), Leaflet |
| Đồng bộ lịch | Google Calendar API (Service Account) |
| Đọc danh thiếp | Gemini (OpenAI-compatible chat completions) |
| Đưa API ra internet | Cloudflare Tunnel (cloudflared) + tên miền riêng |
| Build app Android | EAS Build (Expo) |

---

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
                 │  track.html         │─────▶│ Kho video + ảnh hàng      │
                 └─────────────────────┘      │ (ổ đĩa máy chủ)           │
                                              └───────────────────────────┘
```

```
src/
├─ TruckingSystem.Domain          # Entity, Enum — không phụ thuộc công nghệ
├─ TruckingSystem.Application     # Nghiệp vụ: *Service, DTO, quy tắc dùng chung (…Rules.cs),
│                                 #   IApplicationDbContext, IVideoStorage
├─ TruckingSystem.Infrastructure  # EF Core / PostgreSQL, Migrations, Identity, VietMap, Google Calendar,
│                                 #   lưu file video / ảnh (LocalVideoStorage)
├─ TruckingSystem.API             # Controllers, DbConflictMiddleware (lỗi trùng / xung đột → 409),
│                                 #   DbSeeder, wwwroot (navigate.html cho app, track.html cho khách)
└─ TruckingSystem.Desktop         # WinForms: GUI_*, DTO_*, SERVICE_* (gọi API), Common (Theme, FilterBar,
                                  #   Modern UI: KpiStrip, GridDecor, ScrollFit, PlaceSearchBinder)
```

**Mô hình tự host (self-hosted):**
- Một máy chủ đặt tại chỗ chạy cùng lúc API, PostgreSQL và kho video.
- Cloudflare Tunnel đưa API ra internet có HTTPS. Database không mở ra ngoài, chỉ API truy cập.
- Desktop ở máy khác (ví dụ laptop) nhập địa chỉ domain lúc đăng nhập là dùng chung dữ liệu của máy chủ.

---

## 6. Cơ sở dữ liệu

### 6.1. Sơ đồ tổng quan các nhóm bảng
```
┌──────────────┐     ┌────────────────┐     ┌──────────────┐     ┌──────────────────┐
│  Khách hàng  │────▶│ Đơn vận chuyển │────▶│    Chuyến    │────▶│ GPS (Waypoint)   │
│  Hợp đồng    │     │(TransportOrder)│     │    (Trip)    │     │ Video, Ảnh hàng  │
└──────────────┘     └───────┬────────┘     └───────┬──────┘     └──────────────────┘
                             │  Đổi hạn giao        │
                             ▼  (phí lưu kho)       ▼
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

**Customer (Khách hàng)**

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| Name | string | Tên khách hàng (cá nhân / doanh nghiệp) |
| CustomerType | enum | Individual / Company |
| Phone | string | Số điện thoại liên hệ |
| Address | string | Địa chỉ |
| TaxCode | string | Mã số thuế (nếu là doanh nghiệp) |
| CreditLimit | decimal | Hạn mức công nợ |
| GoogleCalendarId | string | ID lịch Google để đồng bộ thông báo giao hàng |

**TransportOrder (Đơn vận chuyển)**

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| Code | string | Mã đơn hàng (tự sinh) |
| CustomerId | int | Khách hàng đặt đơn |
| **ContractId** | int? | Hợp đồng của đơn (tuỳ chọn, phải đúng khách và đang hiệu lực) |
| WarehouseId | int | Kho trung chuyển dùng cho đơn này |
| PickupPoint / DropoffPoint | string | Điểm lấy hàng / giao hàng (địa chỉ dạng chữ) |
| PickupLocationId / DropoffLocationId | int? | Liên kết tới địa điểm đã lưu tọa độ |
| PickupMethod | enum | CompanyPickup / SelfDelivery |
| CommittedDeliveryDate | DateTime? | Hạn giao cam kết (bắt buộc với đơn mới) |
| Status | enum | Draft / Confirmed / InProgress / Done / Cancelled |
| ProgressPercent | decimal | Tiến độ giao (%) |
| PublicTrackingToken | string | Mã token theo dõi công khai |

**CargoLine (Dòng hàng hóa trong đơn)**

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

**DeliveryDeadlineChange (Lịch sử đổi hạn giao — mới)**

| Field | Kiểu | Mô tả |
|---|---|---|
| TransportOrderId | int | Đơn được đổi hạn |
| OldDate / NewDate | DateTime? / DateTime | Hạn cũ → hạn mới |
| RequestedBy | enum | Customer / Company — chỉ Customer dời trễ hơn mới tính phí |
| Reason / ChangedByUserName | string | Lý do, người đổi |
| DelayDays | int | Số ngày dời |
| StorageFeePerM3PerDay | decimal | Đơn giá lưu kho **chốt** tại lúc đổi |
| VolumeM3AtChange / EstimatedFee | decimal | m³ hàng trong kho lúc đổi / phí ước tính |

**Location (Địa điểm đã lưu)**

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| Name / Address | string | Tên (không trùng) / địa chỉ đầy đủ |
| Lat / Lng | double? | Tọa độ (bắt buộc với địa điểm mới, khác (0, 0)) |
| CustomerId | int? | Địa chỉ riêng của 1 khách (trống = dùng chung) |

#### Kho trung chuyển

**Warehouse (Kho)**

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| Name / Address | string | Tên kho (không trùng) / địa chỉ kho |
| Lat / Lng | double? | Tọa độ kho |
| LengthMeters / WidthMeters | decimal? | Kích thước thật của kho |
| CapacityM3 | decimal? | Sức chứa tổng — tổng sức chứa các ô không được vượt |
| DoorSide | enum? | Cửa kho nằm trên tường nào: Top / Bottom / Left / Right |
| DoorOffsetMeters | decimal? | Vị trí cửa tính từ đầu bức tường (m) |
| **StorageFeePerM3PerDay** | decimal | Phí lưu kho (đ/m³/ngày), 0 = không tính |

**WarehouseZone (Khu vực trong kho)**

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| WarehouseId | int | Thuộc kho nào |
| Name | string | Tên khu vực |
| ZoneType | enum | Normal / Fragile / Hazmat |
| PosX / PosY | decimal | Tọa độ (tương đối so với kho) |
| WidthMeters / HeightMeters | decimal | Kích thước khu vực |

**StorageBin (Vị trí lưu trữ cụ thể)**

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| WarehouseZoneId | int | Thuộc khu vực nào |
| Code | string | Mã vị trí (không trùng trong cả kho) |
| PosX / PosY / WidthMeters / HeightMeters | decimal | Tọa độ + kích thước thật |
| CapacityM3 | decimal | Sức chứa |
| CurrentUsedM3 | decimal | Thể tích đang chiếm dụng |
| MaxWeightKg | decimal | Tải trọng tối đa (0 = không giới hạn) |
| DistanceFromDoorMeters | decimal | Khoảng cách tới cửa kho, hệ thống tự tính (khoảng cách Manhattan từ tâm ô tới cửa) |

**WarehouseStock (Tồn kho tổng hợp)**

| Field | Kiểu | Mô tả |
|---|---|---|
| WarehouseId / CargoLineId | int | Khóa tổng hợp |
| QtyOnHand | decimal | Số lượng đang tồn (≥ 0) |

**StorageAssignment (Gán lô hàng vào vị trí)**

| Field | Kiểu | Mô tả |
|---|---|---|
| GoodsReceiptId | int | Phiếu nhập liên quan |
| StorageBinId | int | Vị trí được gán |
| QtyStored | decimal | Số lượng lưu tại vị trí này |

**GoodsReceipt / GoodsIssue (Phiếu nhập / xuất kho)**

| Field | Kiểu | Mô tả |
|---|---|---|
| TransportOrderId / CargoLineId | int | Đơn hàng và dòng hàng liên quan |
| WarehouseId | int | Kho thực hiện |
| TripId | int? | Chuyến gắn liền (null nếu thao tác thủ công) |
| QtyReceived / QtyIssued | decimal | Số lượng nhập / xuất |
| PerformedByUserName | string? | Người thao tác |
| ReversedAt (phiếu xuất) | DateTime? | Thời điểm **hoàn tác** — phiếu giữ lại để minh bạch lịch sử |

#### Đội xe & Tài xế

**Vehicle (Xe)**

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| PlateNumber | string | Biển số (in hoa, không trùng) |
| VehicleType | string | Loại xe |
| CapacityQty / CapacityKg / CapacityM3 | decimal | Năng lực chuyên chở |
| Status | enum | Available / OnTrip / UnderMaintenance / OutOfService |
| WarehouseId | int? | Kho đóng quân (trống = dùng được mọi kho) |
| FuelConsumptionPer100Km | decimal? | Định mức tiêu hao nhiên liệu |
| FuelType | string? | Loại nhiên liệu sử dụng |

**VehicleDocument (Giấy tờ xe)**

| Field | Kiểu | Mô tả |
|---|---|---|
| VehicleId | int | Thuộc xe nào |
| DocType | enum | Registration / Insurance / TransportPermit / Other — 3 loại đầu bắt buộc còn hạn |
| IssueDate / ExpiryDate | DateTime | Ngày cấp / hết hạn (gia hạn = thêm bản mới) |
| FileUrl | string? | Đường dẫn ảnh scan giấy tờ |

**DriverProfile (Hồ sơ tài xế)**

| Field | Kiểu | Mô tả |
|---|---|---|
| EmployeeId | int | Nhân viên tương ứng (chức danh Tài xế) |
| LicenseNumber / LicenseClass | string | Số / hạng bằng lái (B2, C1, C, CE, FC…) |
| LicenseExpiryDate | DateTime | Ngày hết hạn bằng lái |
| **HealthCertExpiryDate** | DateTime? | Ngày hết hạn giấy khám sức khỏe |
| YearsOfExperience | int | Số năm kinh nghiệm |
| WarehouseId | int? | Kho đóng quân |
| GoogleCalendarId | string? | ID lịch Google |

**DriverSchedule (Ngày làm của tài xế)**

| Field | Kiểu | Mô tả |
|---|---|---|
| DriverEmployeeId | int | Tài xế |
| ShiftDate | DateTime | Ngày làm (duy nhất theo tài xế + ngày) |
| ShiftType | enum | FullDay (dữ liệu cũ có thể có Morning / Afternoon / Night) |

#### Chuyến & Hành trình

**Trip (Chuyến — 1 chặng cụ thể)**

| Field | Kiểu | Mô tả |
|---|---|---|
| Id | int | Khóa chính |
| TransportOrderId / VehicleId / DriverEmployeeId | int | Đơn hàng / Xe / Tài xế thực hiện |
| WarehouseId | int? | Kho trung chuyển liên quan |
| LegType | enum | Pickup (lấy hàng về kho) / Delivery (xuất kho đi giao) / Return (trả hàng về đối tác khi đơn bị hủy) |
| Status | enum | Planned / InProgress / PendingConfirmation (tài xế đã báo xong, chờ duyệt) / Completed / Cancelled |
| **PlannedStartDate** | DateTime? | Ngày xuất phát dự kiến (chọn lúc Chia xe) |
| StartTime / EndTime | DateTime? | Thời gian bắt đầu / kết thúc thực tế |
| CancelReason | string? | Lý do hủy (khi chuyến bị hủy theo đơn) |
| ConfirmRejectedAt | DateTime? | Lần bị từ chối gần nhất (ảnh hàng trước đó không còn tính) |
| EstimatedDistanceKm / EstimatedDurationMinutes | decimal? / int? | Ước tính từ hệ thống định tuyến |
| FuelTypeAtStart / FuelConsumptionPer100KmAtStart / FuelUnitPriceAtStart | | Số liệu nhiên liệu **chốt lúc xuất phát** (báo cáo không đổi khi sửa xe / bảng giá) |
| PlannedFuelCost / ActualFuelCost | decimal | Chi phí xăng ước tính / thực tế |

**TripLine**: số lượng hàng dự kiến chở trong 1 Trip (liên kết Trip + CargoLine).

**Waypoint (Điểm GPS)**

| Field | Kiểu | Mô tả |
|---|---|---|
| TripId | int | Chuyến liên quan |
| Lat / Lng | double | Tọa độ |
| Timestamp | DateTime | Thời điểm ghi nhận |
| SpeedKmh | decimal? | Tốc độ tại thời điểm đó |

**IncidentReport (Sự cố)**

| Field | Kiểu | Mô tả |
|---|---|---|
| TripId | int | Chuyến xảy ra sự cố |
| Type | enum | Accident / Breakdown / TrafficViolation / Other |
| Description | string | Mô tả (≥ 5 ký tự) |
| Cost | decimal? | Chi phí phát sinh (nếu có) |

**TripExpense (Chi phí khác)**

| Field | Kiểu | Mô tả |
|---|---|---|
| TripId | int | Chuyến liên quan |
| Type | enum | Toll / Parking / Loading / Fine / Other (Other bắt buộc ghi chú) |
| Amount | decimal | Số tiền |

**TripCalendarEvent**: lưu vết đồng bộ Google Calendar cho từng Trip, theo từng Calendar cụ thể (tài xế hoặc khách hàng).

**TripCargoPhoto (Ảnh hàng tài xế chụp lúc báo xong — mới)**

| Field | Kiểu | Mô tả |
|---|---|---|
| TripId | int | Chuyến |
| StoragePath / ContentType / SizeBytes | | File ảnh trên ổ đĩa máy chủ (JPG / PNG / WEBP ≤ 15 MB, tối đa 10 ảnh / lần) |
| Sha256 | string | Mã băm chống chỉnh sửa |
| TakenAt | DateTime | Giờ chụp |

#### Nhiên liệu

| Bảng | Field chính | Mô tả |
|---|---|---|
| FuelProvider | Name, ContactPhone, Address | Hãng cung cấp xăng dầu |
| FuelPriceHistory | FuelType, Price, EffectiveDate, FuelProviderId | Lịch sử giá theo hãng (hoặc giá thị trường chung) |
| FuelLog | TripId, Liters, Cost, RecordedAt | Ghi nhận thực tế đổ nhiên liệu cho 1 chuyến |

#### Hạ tầng chung
- **AuditLog**: ghi lại các hành động quan trọng (ai làm gì, lúc nào, trên đối tượng nào), ví dụ AllocateVehicles, ChangeDeliveryDeadline.
- **Notification**: thông báo tự động (cảnh báo giấy tờ, lịch tài xế, hạn giao, chuyến bị chặn, tài xế báo xong, chờ nhập kho…). `UserId` trống là thông báo chung cho mọi người dùng Desktop. Cột `IsRead` mang nghĩa **"Đã xử lý"**. Loại thông báo được nhận ra theo biểu tượng đầu tiêu đề: 🟢 🔴 🟡 📅 🪪 ⏰ 🚫 📦.

#### Video hành trình
**TripVideoSegment**: 1 đoạn video khoảng 1 phút của chuyến. Database chỉ lưu thông tin mô tả, file video nằm trên ổ đĩa máy chủ.

| Field | Kiểu | Mô tả |
|---|---|---|
| TripId | int | Chuyến liên quan |
| StoragePath | string | Đường dẫn tương đối tới file, VD `trip-12/20261006-083015-ab12cd34.mp4` |
| ContentType | string | video/mp4… |
| StartedAt / EndedAt | DateTime (UTC) | Giờ bắt đầu / kết thúc quay, cùng đồng hồ với GPS nên khớp video với vị trí xe |
| SizeBytes | long | Dung lượng file |
| Sha256 | string | Mã băm tính lúc nhận file, dùng để chứng minh video chưa bị chỉnh sửa |

---

## 7. Các dịch vụ bên ngoài (External APIs)

| Dịch vụ | Mục đích sử dụng |
|---|---|
| VietMap API | Gợi ý địa chỉ + tọa độ (thêm kho, địa điểm); tính tuyến đường thực tế (khoảng cách, thời gian); ảnh nền bản đồ cho màn xem lộ trình (Desktop) và điều hướng (app tài xế) |
| Google Calendar API | Đồng bộ lịch chuyến lên Google Calendar của tài xế và khách hàng, kèm link theo dõi công khai |
| Gemini API | Đọc danh thiếp khách hàng (OCR + trích xuất trường) |
| Cloudflare Tunnel | Đưa API từ máy chủ tại chỗ ra internet qua tên miền có HTTPS (app tài xế, trang theo dõi công khai) |

Khóa API cấu hình trong `appsettings.json` của API, không công khai trong tài liệu này.

---

## 8. Các nghiệp vụ chính

### 8.1. Quản lý Đơn vận chuyển
- Tạo đơn với khách hàng, hợp đồng (tuỳ chọn), hàng hóa, điểm lấy / giao, kho trung chuyển và **hạn giao cam kết**.
- Chỉ sửa được khi đơn còn **Nháp**. Lúc **Xác nhận**, hệ thống kiểm tra lại hạn giao, địa điểm, khu kho đúng loại hàng và hiệu lực hợp đồng.
- Có 2 hình thức lấy hàng:
  - **Công ty cử xe tới lấy**: có chặng Lấy hàng.
  - **Khách tự mang tới kho**: thủ kho nhập đủ hàng trước, Chia xe chỉ tạo chặng Giao.
- **Đổi hạn giao** (đơn Đã xác nhận / Đang chạy):
  - bắt buộc chọn Bên yêu cầu, ghi lý do ≥ 5 ký tự, có ghi nhật ký;
  - hạn mới làm chuyến đã chia không kịp thì cảnh báo ngay;
  - khách xin dời trễ hơn thì tính phí lưu kho (mục 8.5).

### 8.2. Phân bổ xe & tài xế tự động (Greedy)
- Thuật toán chọn xe lớn trước.
- Điều kiện xe: rảnh, đúng kho của đơn (hoặc không cố định kho), đủ Đăng kiểm + Bảo hiểm + Phù hiệu còn hạn tới **hết ngày cuối chuyến**.
- Điều kiện tài xế:
  - đang làm việc, chức danh Tài xế, đúng kho;
  - hạng bằng hợp với tải trọng / loại xe;
  - bằng lái và giấy khám còn hạn;
  - **đăng ký đủ ngày làm** trong cả thời gian chuyến.
- Xe / tài xế đang có chuyến (kể cả đang Chờ xác nhận) không được chọn trùng.
- Chọn được ngày xuất phát dự kiến, tối đa vài ngày tới.
- Chia trễ hạn giao thì phải xác nhận, hệ thống ghi nhật ký người chấp nhận trễ.

### 8.3. Vòng đời 1 chuyến
```
Đã lên kế hoạch ──(Bắt đầu / GPS đầu tiên)──▶ Đang chạy ──(tài xế báo xong)──▶ Chờ xác nhận
      │                                          ▲                               │
      │ (hủy theo đơn)                           └──────── Từ chối ──────────────┤
      ▼                                                                          ▼ Duyệt
    Đã hủy                                                                   Hoàn tất
```
- **Bắt đầu**: kiểm tra lại toàn bộ điều kiện xe + tài xế (chuyến có thể chia từ vài ngày trước).
- **Lấy hàng** hoàn tất / được duyệt: hàng sang **📦 Chờ nhập kho**. Nhân viên kho nhập ở **Kho → Nhập hàng từ chuyến**.
- **Giao hàng**: chỉ bắt đầu được khi mọi chặng Lấy hàng của đơn đã Hoàn tất, hàng đã nhập kho, **và kho đã xác nhận xuất** cho đúng chuyến này.
- **Trả hàng** (đơn bị hủy khi hàng đã vào kho): kho phải còn đủ hàng mới chạy được; hoàn tất thì hàng xuất kho trả về đối tác. Chưa chạy thì có thể **Trả khách tại kho**.
- **Đổi xe / tài xế**: chỉ cho chuyến chưa chạy; nếu kho đã xuất hàng lên xe cũ thì phải hoàn tác xuất trước.
- Vận hành tự bấm Hoàn tất trên Desktop thì coi như duyệt luôn, không qua bước chờ.
- Chi phí, xăng, sự cố khóa sau **7 ngày** kể từ khi chuyến đóng.

### 8.4. Hủy đơn

| Tình trạng hàng khi hủy | Hệ thống xử lý |
|---|---|
| Các chuyến còn "Đã lên kế hoạch" / hàng chưa vào kho | Hủy chuyến, trả xe / tài xế về rảnh |
| Hàng đã lấy về kho (đã nhập kho) | Tạo chuyến **Trả hàng** dùng lại đúng xe + tài xế (hoặc Trả khách tại kho) |
| Đơn khách tự mang tới, hàng trong kho | Xuất trả khách tại kho ngay khi hủy |
| Chặng Lấy hàng đang Chờ xác nhận / hàng còn **chờ nhập kho** | **Chặn** — duyệt + nhập kho trước rồi hủy |
| Kho đã xuất hàng lên xe (xe chưa chạy) | **Chặn** — Hoàn tác xuất trước |
| Đã xuất kho đi giao (chuyến giao đang chạy / chờ duyệt / xong) | **Không cho hủy** |

### 8.5. Kho trung chuyển
- **Sơ đồ & vị trí**:
  - Sơ đồ kho đúng tỉ lệ mét; khu vực theo tính chất hàng (thường / dễ vỡ / nguy hiểm). Cửa kho khai báo theo tường + khoảng cách và vẽ trên sơ đồ; khoảng cách từng ô tới cửa tự tính.
  - Tổng sức chứa các ô không vượt sức chứa kho. Không thu nhỏ kho / khu làm tràn khu / ô bên trong.
- **Best-Fit**: chọn ô theo thể tích và tải trọng còn lại. 1/3 số ô gần cửa nhất ưu tiên cho hàng gấp. Lô lớn được chia qua nhiều ô.
- **Nhập hàng từ chuyến**: nhập từng phần hoặc cả chuyến, thủ công hoặc Best-Fit. Không nhập vượt số khai báo, không nhập vào kho khác với kho của đơn.
- **Xuất kho theo chuyến** (📤):
  - xuất đủ cả chuyến hoặc không xuất dòng nào;
  - không xuất lên xe đang Bảo trì / Ngưng hoạt động;
  - chỉ xuất từ 1 ngày trước ngày chạy;
  - có thể **Hoàn tác xuất** khi xe chưa chạy.
- **Nhập / xuất tay**:
  - Nhập tay: chỉ cho đơn đang xử lý, chừa phần các chuyến lấy hàng sẽ mang về.
  - Xuất tay: chỉ cho đơn Hoàn tất / Đã hủy, không lấy phần đã giữ cho chuyến trả hàng.
  - Phiếu xuất tay hoàn tác được.
- **Phí lưu kho**:
  - Đơn giá đặt ở **Sửa kho → Phí lưu kho** (đ/m³/ngày).
  - Chỉ tính khi **khách** xin dời hạn trễ hơn.
  - Mỗi ngày tính = m³ hàng của đơn còn trong kho cuối ngày × đơn giá **chốt lúc đổi hạn**.
  - Chỉ tính những ngày hàng thực sự còn nằm trong kho, trong khoảng khách xin dời. Mỗi ngày chỉ tính 1 lần.
  - Xem ở **Chi tiết đơn → 💰 Phí lưu kho** (tạm tính khi hàng còn trong kho, chốt khi hàng đã xuất).

### 8.6. Đội xe & Tài xế
- **Giấy tờ xe**: Đăng kiểm, Bảo hiểm, Phù hiệu bắt buộc. Gia hạn bằng cách thêm bản mới; không xóa bản đang là giấy tờ hợp lệ duy nhất cho chuyến chưa chạy.
- **Hồ sơ tài xế**: bằng lái, hạng, giấy khám sức khỏe, kinh nghiệm, kho đóng quân.
- **Ngày làm**: đăng ký theo khoảng ngày, tối đa 62 ngày / lần. Không xóa ngày nằm trong chuyến chưa xong.
- Trạng thái tài xế tự tính: Đang làm / Sẵn sàng / Không sẵn sàng.
- **Ràng buộc khi sửa**:
  - Xe / tài xế còn chuyến thì không đổi kho đóng quân.
  - Sửa tải trọng / loại xe hoặc hạng bằng / hạn bằng / giấy khám mà làm chuyến chưa chạy không còn hợp lệ thì bị chặn (đổi xe / tài xế cho chuyến đó trước).
  - Trạng thái "Đang chạy" do hệ thống gán; không đặt "Rảnh" / "Ngưng hoạt động" khi xe còn chuyến.

### 8.7. GPS & bản đồ
- App tài xế gửi GPS mỗi 30 giây, kể cả khi chạy nền.
- Bản đồ trên app và Desktop dùng chung quy ước màu, cắt dần phần đường đã đi, tính lại tuyến khi lệch > 100 m.
- Chuyến đã xong chỉ hiện đường GPS thật đã đi (màu cam).

### 8.8. Video hành trình & ảnh hàng
- Tài xế bật / tắt quay video trên app. Video quay theo đoạn 1 phút, lưu tạm trên điện thoại rồi gửi về máy chủ qua domain.
- Gửi xong thì xóa khỏi điện thoại; mất mạng thì giữ lại và gửi tiếp sau.
- Máy chủ lưu file trên ổ đĩa, database chỉ lưu đường dẫn + giờ + mã SHA-256. Desktop xem lại video khớp với vị trí GPS.
- Khi báo xong chặng, tài xế phải tick đủ dòng hàng và chụp ≥ 1 ảnh. Desktop xem ảnh ở **📷 Ảnh hàng**.

### 8.9. Chi phí & Lãi/Lỗ
- Ghi nhận đổ xăng thực tế, chi phí phát sinh (cầu đường, bến bãi, bốc xếp, phạt…) và chi phí sự cố.
- Màn Tổng quan tổng hợp lãi/lỗ = doanh thu (hóa đơn không hủy) − chi phí, theo kỳ và theo từng đơn.
- Kiểm soát nhiên liệu theo 3 con số: dự toán (km VietMap), định mức (km GPS), thực tế.

### 8.10. Theo dõi công khai, lịch, thông báo
- **Theo dõi công khai**: khách tra cứu đơn qua link công khai.
- **Lịch Google**: lịch chuyến đồng bộ lên Google Calendar.
  - Chặng Lấy / Trả chưa chạy: theo ngày xuất phát dự kiến.
  - Chặng Giao: theo hạn giao.
  - Lịch tự cập nhật khi chuyến bắt đầu / hoàn tất / đổi hạn giao.
- **🔔 Thông báo kiểu việc cần làm**:
  - 🔵 = chưa xử lý; bấm xem không làm mất.
  - Thông báo gắn việc cụ thể tự chuyển Đã xử lý (hoặc tự biến mất) khi việc xong. Chỉ loại "Khác" đánh dấu tay được.
  - **Nhấp đúp** hoặc **↗ Mở màn xử lý** để mở thẳng màn hình liên quan.
- **Nhật ký thao tác** cho các hành động quan trọng.

### 8.11. Phân quyền

| Màn hình | Admin | Dispatcher | WarehouseStaff | Accountant |
|---|:-:|:-:|:-:|:-:|
| Tổng quan | ✔ | ✔ | ✔ | ✔ |
| Đơn vận chuyển, Chuyến, Địa điểm, Đội xe, Tài xế, Nguồn lực theo kho | ✔ | ✔ | | |
| Kho trung chuyển | ✔ | ✔ | ✔ | |
| Nhiên liệu | ✔ | | | ✔ |
| Khách hàng, Hợp đồng, Bảng giá | ✔ | ✔ | | ✔ |
| Hóa đơn | ✔ | | | ✔ |
| Nhân viên, Nhật ký hệ thống | ✔ | ✔ | | |
| Quản lý tài khoản, Hệ thống | ✔ | | | |

- API giới hạn quyền đúng như bảng trên; sai vai trò trả **403**.
- Các API của app tài xế không cần đăng nhập: GPS, trạng thái chuyến, báo đến nơi, danh sách kiểm hàng, ảnh, video.
- Trang `track.html` cũng không cần đăng nhập.

---

## 9. Cài đặt & chạy hệ thống

### 9.1. Yêu cầu
- Windows 10/11, .NET 8 SDK, Visual Studio 2022
- PostgreSQL 14+ và pgAdmin
- Microsoft Edge WebView2 Runtime (thường có sẵn trên Windows 11)
- Node.js 20+, tài khoản Expo (build app tài xế bằng EAS)
- (Tuỳ chọn) tài khoản Cloudflare + tên miền, để app tài xế dùng được từ xa
- Khóa VietMap (routing + tile)

### 9.2. Cấu hình API — `TruckingSystem.API/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=trucking_system;Username=postgres;Password=MAT_KHAU"
  },
  "Jwt": { "Issuer": "TruckingSystemApi", "Audience": "TruckingSystemClients", "Key": "CHUOI_BI_MAT_TOI_THIEU_32_KY_TU" },
  "VietMap": { "RouteApiKey": "KEY_ROUTING", "MapApiKey": "KEY_TILE" },
  "GoogleCalendar": {
    "ServiceAccountKeyPath": "D:\\Keys\\trucking-calendar-sa.json",
    "ApplicationName": "TruckingSystem"
  },
  "VideoStorage": { "RootPath": "D:\\TruckingStorage" },
  "Seed": { "DemoData": false },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Model": "gemini-3.5-flash-lite",
    "ApiUrl": "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions"
  }
}
```

| Khóa | Ghi chú |
|---|---|
| `GoogleCalendar:ServiceAccountKeyPath` | Đường dẫn tuyệt đối tới file JSON Service Account. Calendar của tài xế / khách phải được chia sẻ quyền "Make changes to events" cho email Service Account |
| `VideoStorage:RootPath` | Thư mục lưu video + ảnh hàng. Để trống thì dùng `Storage/TripVideos` cạnh API. **Không** đặt trong OneDrive |
| `Seed:DemoData` | `true` = khi DB chưa có chuyến nào thì API đổ dữ liệu mẫu. Mặc định **tắt**, để xóa dữ liệu xong không bị đổ lại |

**Quét danh thiếp (Gemini)**

Tính năng này dùng Gemini, được copy từ AI_Vision.

- **Đặt khóa trên máy chạy API**: dùng `Gemini:ApiKey` hoặc biến môi trường `GEMINI_API_KEY` / `Gemini__ApiKey`. Không đặt khóa trong Desktop.
- **Ghi đè model / URL**: dùng `GEMINI_MODEL` và `GEMINI_API_URL`. URL phải đầy đủ `/chat/completions`, không chỉ là base URL.
- **Không dùng nguồn khác**: API không đọc file `.env` của AI_Vision và không dùng khóa NVIDIA thay cho Gemini.
- **Khởi động lại API** sau khi thay cấu hình.

Cách dùng trên Desktop: **Khách hàng → Thêm → Quét danh thiếp**, chọn ảnh JPG/PNG tối đa 10 MB.
- Màn hình hiển thị ảnh và các trường đọc được, kể cả khi AI trả về JSON bị cắt hoặc danh sách Markdown. Kiểm tra cảnh báo và sửa thông tin trước khi lưu.
- Họ tên liên hệ, công ty, chức danh, email, website ở cột đối chiếu chỉ để tham khảo. Nút lưu vẫn lưu các trường khách hàng hiện có ở cột bên trái.
- Kết quả quét không tự tạo khách hàng hoặc người liên hệ.

Endpoint `/api/customers/read-business-card`:
- Giữ nguyên phương thức POST, file `image`, phân quyền và năm trường kết quả cũ. Bổ sung `fullName`, `company`, `title`, `email`, `website`, `rawResponse`, `errorMessage`.
- Mã trả về:
  - **200** kèm cảnh báo: AI chỉ đọc được một phần.
  - **502**: phản hồi lỗi, không có trường đọc được.
  - **400**: ảnh sai định dạng.
  - **504**: dịch vụ quá thời gian chờ.
- API vẫn trả `rawResponse` để giữ tương thích, nhưng Desktop không hiển thị dữ liệu gốc.
- Kiểm thử: `dotnet test tests/TruckingSystem.UnitTests --filter FullyQualifiedName~CustomerBusinessCardReaderTests`. Kiểm tra giao diện: `dotnet run --project tests/TruckingSystem.CustomerFormChecks`.

> ⚠️ Không commit key thật / mật khẩu thật lên Git, kể cả trong `appsettings.json.example`.

### 9.3. Tạo / cập nhật database
Trong Visual Studio → Package Manager Console, chọn Default project = `TruckingSystem.Infrastructure`, Startup project = `TruckingSystem.API`:

```powershell
Update-Database
```

Khi đổi entity, tạo migration mới:

```powershell
Add-Migration <TenMigration> -Project TruckingSystem.Infrastructure -StartupProject TruckingSystem.API
```

- Các migration của phiên bản này: `AddOrderContract` (đơn gắn hợp đồng) và `AddStorageFee` (phí lưu kho + lịch sử đổi hạn giao). Thiếu migration nào thì tạo bằng lệnh trên rồi `Update-Database`.
- Database cũ thì sau đó chạy thêm `TinhLai_SucChua_OKho.sql` trong pgAdmin (tính lại sức chứa ô kho theo cấu trúc mới).
- Lần chạy đầu, API tự tạo các vai trò và tài khoản `admin` / `driver1`. Dữ liệu nghiệp vụ mẫu chỉ được tạo khi bật `Seed:DemoData`.
- `Update-Database` báo lỗi "relation … already exists" hoặc "duplicate key": xem mục 11.

### 9.4. Chạy API
- Chọn `TruckingSystem.API` làm Startup Project → Run.
- Mặc định chạy ở `https://localhost:8080`, Swagger tại `/swagger`.
- Lần đầu, nếu trình duyệt / WebView2 báo lỗi chứng chỉ localhost: `dotnet dev-certs https --trust`.

### 9.5. Chạy Desktop
Run `TruckingSystem.Desktop`. Ở màn đăng nhập, ô **Server URL**:
- `https://localhost:8080`: khi Desktop chạy cùng máy với API (nhanh nhất, xem video mượt nhất)
- `https://<tên-miền>`: khi Desktop chạy ở máy khác (dùng chung dữ liệu của máy chủ qua Cloudflare)

| Tài khoản mặc định | Mật khẩu | Vai trò |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `driver1` | `Driver@123` | Driver |

### 9.6. Đưa API ra internet (Cloudflare Tunnel)
1. **Tạo tunnel**: Cloudflare Zero Trust → Networks → Tunnels → tạo tunnel. Cài `cloudflared` trên máy chủ bằng lệnh / token được cấp, chạy dạng dịch vụ Windows để tự chạy lại khi bật máy.
2. **Public Hostname**: trỏ tên miền → `https://localhost:8080`, bật **No TLS Verify** (vì chứng chỉ localhost tự ký).
3. **Kiểm tra**: mở `https://<tên-miền>/track.html`, mở được là xong.

> ⚠️ Chỉ 1 máy được chạy tunnel này. Nếu 2 máy cùng chạy, Cloudflare chia yêu cầu ngẫu nhiên cho cả 2, lúc gặp code mới lúc gặp code cũ (lỗi 401/404 khó hiểu).

### 9.7. App tài xế (Android)
```bash
cd driver-gps-app
npm install
npx expo install expo-camera expo-keep-awake expo-file-system
eas build -p android --profile preview
```
- Tải APK từ link EAS và cài lên điện thoại.
- Cập nhật app thì **cài đè**, không gỡ app cũ (gỡ sẽ mất các đoạn video chưa gửi).
- Trang bản đồ điều hướng (`navigate.html`) nằm trên API (`wwwroot`), nên sửa giao diện bản đồ không cần build lại APK.

---

## 10. Hướng dẫn sử dụng

### 10.1. Quy trình nghiệp vụ chuẩn (Desktop)
1. **Chuẩn bị dữ liệu nền**
   - **Kho**: tạo kho (tên không trùng; gõ địa chỉ rồi **bấm chọn 1 dòng gợi ý** để có tọa độ; kích thước; tường đặt cửa + khoảng cách; sức chứa; phí lưu kho), rồi tạo khu vực và ô kệ. Rê chuột lên ô để xem khoảng cách tới cửa.
   - **Xe**: biển số, năng lực (kg / m³ / số lượng), định mức nhiên liệu, **kho đóng quân**, đủ 3 giấy tờ còn hạn.
   - **Tài xế**: hồ sơ (bằng lái, hạng, giấy khám, kho đóng quân) và **đăng ký ngày làm**. Cột trạng thái tự hiện Đang làm / Sẵn sàng / Không sẵn sàng.
   - **Khách hàng, hợp đồng, địa điểm**: địa điểm chọn từ gợi ý VietMap để có tọa độ.
2. **Tạo đơn**: chọn khách, hợp đồng (nếu có), hàng hóa, điểm lấy / giao, kho, hình thức lấy hàng, hạn giao → **Xác nhận**.
3. **Chia xe**: chọn ngày xuất phát dự kiến. Hệ thống tự chọn xe + tài xế và tạo chuyến Lấy hàng + Giao hàng.
4. **Gửi Trip ID cho tài xế** (màn Chuyến). Tài xế dùng app (mục 10.2).
5. **Duyệt chuyến**: khi tài xế báo xong, 🔔 hiện "🟢 Tài xế báo xong chuyến #… — chờ duyệt". **Nhấp đúp** thông báo để mở thẳng chuyến → xem lộ trình, video, 📷 ảnh hàng → **Duyệt** hoặc **Từ chối**.
6. **Nhập kho**: duyệt chuyến Lấy hàng xong thì 🔔 hiện "📦 Chuyến #… đã về kho … — chờ nhập kho". Nhấp đúp để mở **Kho → Nhập hàng từ chuyến**, rồi nhập thủ công hoặc Best-Fit.
7. **Xuất kho**: **Kho → 📤 Xuất kho theo chuyến** → soạn hàng theo cột "Lấy ở ô" → **Xác nhận xuất** (từ 1 ngày trước ngày chạy). Màn Chuyến hiện "📤 Chờ kho xuất" cho tới khi kho xuất xong.
8. **Ghi chi phí** trong lúc / sau chuyến: Đổ xăng, Chi phí phát sinh, Sự cố.
9. **Đổi hạn giao** (nếu cần): Chi tiết đơn → 📅 Đổi hạn giao → chọn Bên yêu cầu, ngày mới, lý do. Form hiện trước phí lưu kho ước tính. Xem tổng phí ở **💰 Phí lưu kho**.
10. **Gỡ chuyến bị kẹt**: xe hỏng hoặc giấy tờ hết hạn thì dùng **🔁 Đổi xe / tài xế**. Chuyến Trả hàng chưa chạy có thể **Trả khách tại kho**.
11. **Hủy đơn** (nếu cần): Chi tiết đơn → Hủy đơn. Hệ thống tự hủy chuyến, tạo chuyến Trả hàng, hoặc chặn hủy tùy tình trạng hàng (mục 8.4).
12. **Theo dõi lãi/lỗ**: màn 📊 Tổng quan → chọn kỳ → xem thẻ số liệu, biểu đồ. Double-click 1 đơn để xem từng khoản chi.

### 10.2. App tài xế
1. Nhập **Server URL** (tên miền) và **Trip ID** do Vận hành gửi.
2. (Tuỳ chọn) Bật 🎥 **Quay video hành trình** → cấp quyền Camera + Micro. Tắt thì chuyến chỉ có GPS.
3. Bấm **Start GPS Tracking** → cấp quyền vị trí "Luôn cho phép".
4. App hỏi máy chủ trước khi bật:
   - Chuyến đã xong / chờ duyệt / đã hủy: báo "Chuyến đã hoàn thành" (hoặc đã hủy), không bật.
   - Chuyến Giao mà chặng Lấy chưa xong, hàng chưa nhập kho hoặc kho chưa xuất: báo "Chưa thể bắt đầu chuyến" kèm lý do.
5. **Open Navigation** → bản đồ chỉ đường: mũi tên xám là xe, đường xanh là đoạn còn phải đi (tự ngắn lại khi chạy). Kéo bản đồ ra chỗ khác thì bấm 🎯 **Về vị trí xe** để bám lại.
6. Nếu bật video: khung camera nhỏ ở góc màn hình (dấu ● REC). Giữ app mở khi chạy (màn hình tự sáng). Dòng trạng thái cho biết đã gửi / chờ gửi bao nhiêu đoạn; lỗi gửi hiện chữ đỏ, bấm 🔄 **Gửi lại ngay** để thử lại.
7. Xong việc:
   - **Tick đủ từng dòng hàng** và **chụp ít nhất 1 ảnh hàng**.
   - Bấm ✅ **Xác nhận đã LẤY / GIAO / TRẢ HÀNG xong**.
   - GPS dừng, chuyến chuyển "Chờ xác nhận". Thiếu hàng thì **không** xác nhận, gọi Điều phối.

### 10.3. Xem lại lộ trình & video (Desktop)
Màn Chuyến → chọn chuyến → **Xem lộ trình**:
- **Chuyến chưa có GPS**: hiện đường lý thuyết nét đứt để xem trước.
- **Chuyến đang chạy**: tự cập nhật mỗi 15 giây; mũi tên xám là vị trí xe, đường đã đi tự mất.
- **Chuyến Chờ xác nhận / Hoàn tất**: hiện đường GPS thật (cam). Nếu có video thì đoạn có video tô tím, khung video bên phải.
- **Phát video theo vị trí**: bấm vào đường trên bản đồ, video phát đúng lúc xe đi qua chỗ đó. Video chạy thì mũi tên chạy theo, kèm giờ và tốc độ.
- **Thanh thời gian**: khối tím là đoạn có video; bấm để tua.
- **Kiểm tra từng đoạn**: ✅ = video trùng khớp bản đã nhận (chưa bị sửa); ⚠️ = file đã bị thay đổi.

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

### 10.5. Quản lý hợp đồng (Desktop)
- **Thêm hợp đồng**: mở **Hợp đồng → Thêm mới**, nhập mã, chọn khách hàng, ngày bắt đầu, ngày kết thúc (tuỳ chọn), bảng giá (tuỳ chọn) và điều khoản. Mã không được trùng; ngày kết thúc được phép bằng ngày bắt đầu.
- **Tìm và lọc**: ô tìm kiếm lọc theo mã hoặc tên khách hàng, kết hợp bộ lọc trạng thái Chưa hiệu lực / Đang hiệu lực / Hết hạn. Trạng thái tính theo ngày Việt Nam (UTC+7), bao gồm cả ngày kết thúc.
- **Xem, sửa, xóa**: nhấp đúp một dòng hoặc bấm Chi tiết để xem toàn bộ nội dung; bấm Sửa để cập nhật. Xóa phải xác nhận. Hợp đồng đã được đơn hàng sử dụng thì không xóa được và không đổi được khách hàng.

### 10.6. Thông báo (🔔)
- **Trạng thái**: mặc định chỉ hiện việc **Chưa xử lý** (🔵). Lọc thêm theo Loại / Trạng thái / từ khóa.
- **Nhấp đúp hoặc ↗ Mở màn xử lý** mở thẳng màn hình liên quan:

| Thông báo | Mở tới |
|---|---|
| Chuyến chờ duyệt, chuyến bị chặn, hạn giao theo chuyến | Màn Chuyến, lọc đúng #id |
| Hạn giao theo đơn | Màn Đơn vận chuyển, lọc mã đơn |
| Giấy tờ xe | Đội xe, lọc biển số |
| Giấy tờ / lịch tài xế | Màn Tài xế, lọc mã tài xế |
| Chờ nhập kho | Màn Kho, chọn đúng kho và mở luôn Nhập hàng từ chuyến |

- **Đánh dấu tay**: nút **✔ Đánh dấu đã xử lý** chỉ dùng cho thông báo loại "Khác". Các loại còn lại tự chuyển Đã xử lý khi việc làm xong.

---

## 11. Xử lý sự cố thường gặp

| Hiện tượng | Nguyên nhân & cách xử lý |
|---|---|
| `Update-Database` báo relation "…" already exists | Database có bảng nhưng `__EFMigrationsHistory` thiếu dòng. Kiểm tra bảng / cột nào đã có, INSERT tên các migration đã có sẵn vào `__EFMigrationsHistory`, rồi chạy lại `Update-Database` cho phần còn thiếu. |
| `Update-Database` báo duplicate key `IX_DriverSchedules_DriverEmployeeId_ShiftDate` | Dữ liệu cũ có 2 dòng lịch cùng tài xế, cùng ngày. Xóa trùng rồi chạy lại: `DELETE FROM "DriverSchedules" d USING "DriverSchedules" k WHERE d."DriverEmployeeId" = k."DriverEmployeeId" AND d."ShiftDate" = k."ShiftDate" AND d."Id" > k."Id";` |
| "Unable to create a 'DbContext'…" khi Add-Migration | Chọn đúng Startup project = `TruckingSystem.API`, build lại solution không lỗi rồi chạy lại. |
| `dotnet ef` không chạy được | Dùng lệnh của Package Manager Console: `Add-Migration`, `Update-Database`. |
| Xóa dữ liệu xong, mở API lại thấy dữ liệu mẫu quay về | `Seed:DemoData` đang bật → đặt `false`. |
| Gõ địa chỉ kho / địa điểm mà không có gợi ý | Xem dòng chữ trong khung gợi ý: "Không tìm thấy" → gõ thêm phường / quận / tỉnh; "Không gọi được dịch vụ bản đồ" → kiểm tra `VietMap:RouteApiKey`, mạng, hạn mức key. |
| Lưu kho / địa điểm báo "chưa có địa chỉ / tọa độ" | Chưa **bấm chọn** 1 dòng gợi ý (gõ tay không có tọa độ). Chọn xong phải thấy "✅ … (Lat, Lng)". |
| Gọi API báo 403 | Tài khoản không đúng vai trò của màn hình đó (mục 8.11). |
| Qua tên miền gặp 401/404 hoặc giao diện cũ, localhost thì đúng | Có máy khác (chạy code cũ) cũng đang chạy cùng tunnel → tắt cloudflared ở máy đó. |
| App: video "chờ gửi" không giảm | Xem dòng lỗi đỏ: 404 = máy chủ chưa có chức năng video / tên miền trỏ nhầm máy; lỗi mạng = kiểm tra kết nối. Video vẫn được giữ trên máy tới khi gửi được. |
| Xem video bị quay vòng liên tục | Desktop đang đi qua tên miền (băng thông upload mạng nhà) hoặc thư mục video nằm trong OneDrive → dùng `https://localhost:8080` khi ngồi tại máy chủ, đặt `VideoStorage:RootPath` ngoài OneDrive. |
| Video báo "không phát được" | Link hết hạn (6 giờ) → đóng và mở lại màn lộ trình; hoặc chứng chỉ localhost chưa tin cậy → `dotnet dev-certs https --trust`. |
| Bản đồ trắng / lỗi tải ảnh nền | Kiểm tra `VietMap:MapApiKey`; mạng có chặn VietMap không. |
| Không bắt đầu được chuyến Giao | Đúng quy tắc: còn chuyến Lấy hàng chưa Hoàn tất, hàng chưa nhập kho, hoặc kho chưa **Xác nhận xuất** (màn Chuyến hiện "📤 Chờ kho xuất"). Làm đủ các bước ở mục 10.1. |
| Không xuất kho được cho chuyến | Xe đang Bảo trì (đổi xe trước), hoặc còn hơn 1 ngày mới tới ngày chạy, hoặc kho không đủ hàng. |
| Không sửa được xe / hồ sơ tài xế | Thay đổi làm chuyến chưa chạy không hợp lệ (bằng lái / tải trọng / kho). Đổi xe / tài xế cho chuyến đó trước. |

---

## 12. Hạn chế đã biết & hướng mở rộng

**Hạn chế**
- **App tài xế chưa có đăng nhập**: ai biết Trip ID đều gửi được GPS / video / báo đến nơi cho chuyến đó (đánh đổi để đơn giản hóa demo).
- **Máy chủ tự host**: máy tắt / mất mạng thì app và Desktop từ xa không dùng được. Dữ liệu và video nằm trên 1 máy duy nhất, cần tự sao lưu (backup database bằng pgAdmin + chép thư mục video).
- **Video chỉ quay khi app đang mở** (hệ điều hành không cho dùng camera khi app chạy nền); chưa tự động xóa video cũ.
- **iPhone có thể quay ra định dạng HEVC**, trình phát trong Desktop không phát được → demo bằng Android.
- **Chưa giao từng phần**: chặng Giao chờ toàn bộ chặng Lấy hàng xong, chưa giao theo số hàng đã về kho.
- **Xe / tài xế "không cố định kho"** được chia cho đơn của mọi kho; muốn ràng buộc chặt thì phải gán kho cho tất cả.
- **Chỉ số "Giao đúng hạn"** trong báo cáo so với hạn giao **cuối cùng**, kể cả khi công ty tự dời hạn.

**Hướng mở rộng**
- Đăng nhập cho tài xế, gắn chuyến với tài khoản.
- Đưa database và kho video lên đám mây (PostgreSQL managed; Cloudflare R2 / S3). Code đã tách `IVideoStorage` nên chỉ cần thêm 1 lớp cài đặt mới.
- Chính sách lưu video có thời hạn (VD 30 ngày, giữ lại chuyến có sự cố); chỉ gửi video khi có Wi-Fi hoặc chỉ gửi đoạn quanh thời điểm sự cố.
- Giao từng phần theo tồn kho thực tế; tối ưu ghép đơn cho cùng 1 xe.
- Đưa phí lưu kho vào hóa đơn tự động; tính KPI đúng hạn chỉ theo các lần khách xin dời hạn.

---

## 13. Phân công nhóm

| Mảng nghiệp vụ | Người phụ trách |
|---|---|
| Phân quyền, Nhân viên, Khách hàng, OCR danh thiếp, Hợp đồng, Bảng giá cứng, Thanh toán/Hóa đơn, Lương, Chatbot | Nguyễn Hoàng Minh |
| Vận chuyển, Chia xe, Kho trung chuyển (nhập/xuất theo chuyến, phí lưu kho), Đội xe, Tài xế, Địa điểm, Bản đồ/Định tuyến, Chuyến đi, GPS, Video hành trình, Nhiên liệu & Chi phí, Tổng quan Lãi/Lỗ, Ứng dụng tài xế, Triển khai (Cloudflare Tunnel), Thông báo, Nhật ký hệ thống | Nguyễn Tuấn Thiện Nhân |

---

## 14. Làm mới dữ liệu & tài liệu kiểm thử

**Script SQL** (chạy trong pgAdmin → Query Tool). Sao lưu trước khi chạy: `pg_dump -U postgres -d <ten_db> -F c -f backup.backup`

| Script | Tác dụng |
|---|---|
| `XoaDuLieu_Nhan.sql` | Xóa dữ liệu nghiệp vụ phần vận hành: đơn, chuyến, kho, xe, địa điểm, xăng, lịch tài xế, thông báo, hóa đơn gắn đơn. Giữ tài khoản, nhân viên, hồ sơ tài xế (bỏ trống kho), khách hàng, hợp đồng, bảng giá, lương |
| `XoaToanBo.sql` | Xóa toàn bộ dữ liệu, chỉ giữ `__EFMigrationsHistory`, vai trò, phân quyền. Lần chạy API kế tiếp tự tạo lại `admin` / `driver1` |
| `TaoTaiKhoan_Nhan.sql` | Tạo tài khoản Admin `nhan / Nhan@123` |

Sau khi xóa: để `Seed:DemoData = false`, rồi chọn lại kho đóng quân cho từng tài xế.

**Tài liệu kiểm thử**
- `TestCase_Sprint1.docx`: Xe, Nhiên liệu, Tài xế, Địa điểm (129 test case)
- `TestCase_Sprint2.docx`: Đơn vận chuyển, Chia xe, Chuyến, Bản đồ, GPS & video, Kho, Thông báo (268 test case)
- `TestCase_Sprint2_Nhan_DinhDangBaoCao.docx`: Sprint 2 theo định dạng báo cáo (Test Case ID / Name / Test Data / Expected Result, có mã FR)
