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
| VietMap API | Gợi ý địa chỉ + tọa độ (thêm kho, địa điểm; ô Địa chỉ của khách hàng và nhân viên); tính tuyến đường thực tế (khoảng cách, thời gian); ảnh nền bản đồ cho màn xem lộ trình (Desktop) và điều hướng (app tài xế) |
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
  - Khi lập hóa đơn, hệ thống cộng tổng **Chi phí khác** của các chuyến thuộc đơn và **Phí lưu kho** khách chịu. Mỗi khoản có tiền hiện thành một dòng trong bảng tính và chi tiết hóa đơn. Nhiên liệu ước tính chỉ dùng để đối chiếu.
  - Nếu phí lưu kho còn tạm tính, chờ phí chốt rồi chọn **Tính lại** để lập hóa đơn. Hóa đơn đã lưu giữ nguyên số tiền tại thời điểm lập.
  - Admin/Kế toán chọn hóa đơn rồi bấm **Đánh dấu đã thanh toán**, xác nhận để đổi trạng thái sang `Paid`. Chỉ còn **Chưa thanh toán** (`Unpaid`) và **Đã thanh toán** (`Paid`); hóa đơn mới mặc định chưa thanh toán. Hóa đơn đã thanh toán không cần đánh dấu lại. Thao tác chỉ đổi trạng thái, không tạo giao dịch thu tiền hoặc gọi VNPAY. API: `POST /api/invoices/{id}/mark-paid`.

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
- Màn Tổng quan tổng hợp lãi/lỗ = doanh thu (tất cả hóa đơn của đơn hàng) − chi phí, theo kỳ và theo từng đơn.
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
File `src/TruckingSystem.API/appsettings.json.example` chứa cấu hình mẫu; điền khóa và thông tin kết nối trên máy chạy API. File `appsettings.json` cục bộ được Git bỏ qua nên cập nhật mã nguồn không tự cập nhật cấu hình trên máy chủ khác.

`VideoStorage:RootPath` và `DocumentStorage:RootPath` mặc định lần lượt là `Storage/TripVideos` và `Storage/VehicleDocuments`, tính từ thư mục làm việc của API. Đổi đường dẫn trên máy đã có dữ liệu cần chuyển cả các tệp hiện có sang nơi mới. `VideoStorage:SigningKey` để trống sẽ dùng `Jwt:Key`; thay khóa sẽ làm các liên kết video đã ký bằng khóa cũ hết hiệu lực. Phần `Nvidia` là cấu hình cũ, không được luồng quét danh thiếp hiện tại sử dụng.

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

Màn Khách hàng và form thêm/sửa không còn loại khách hàng, mã số thuế hoặc hạn mức công nợ; bỏ cột/bộ lọc loại và tìm kiếm theo mã số thuế. Quét danh thiếp không điền/hiển thị loại hoặc mã số thuế. Chuột phải vào dòng để **Xem người liên hệ** hoặc **Sửa**, dùng `ModernMenu` theo `design.md`; chỉ Admin/Điều phối được sửa. API/schema giữ tương thích: tạo mới gửi mặc định Individual với mã số thuế/hạn mức null, sửa giữ nguyên ba giá trị lịch sử. Không xóa dữ liệu cũ, không cần migration. Kiểm tra cục bộ: build `tools/BusinessDesignSelfCheck`, rồi chạy `dotnet run --project tools/BusinessDesignSelfCheck -c Release --no-build -- --customers-only` (tools bị ignore).

- Màn hình hiển thị ảnh và các trường đọc được, kể cả khi AI trả về JSON bị cắt hoặc danh sách Markdown. Kiểm tra cảnh báo và sửa thông tin trước khi lưu.
- Họ tên liên hệ, công ty, chức danh, email, website ở cột đối chiếu chỉ để tham khảo. Nút lưu vẫn lưu các trường khách hàng hiện có ở cột bên trái.
- Kết quả quét không tự tạo khách hàng hoặc người liên hệ.
- AI kiểm tra ảnh có phải danh thiếp trước khi trích xuất (cùng một lần gọi Gemini). Ảnh hóa đơn, giấy tờ, ảnh chụp màn hình, phong cảnh… bị từ chối kèm lý do, không điền trường nào; Desktop hiện cảnh báo "Không phải danh thiếp".
- Khi Gemini quá tải tạm thời (500/502/503/504), API tự gửi lại tối đa 2 lần (chờ 2 giây, rồi 4 giây). 429 (hết hạn mức) không gửi lại.

Endpoint `/api/customers/read-business-card`:
- Giữ nguyên phương thức POST, file `image`, phân quyền và năm trường kết quả cũ. Bổ sung `fullName`, `company`, `title`, `email`, `website`, `rawResponse`, `errorMessage`, `isNotBusinessCard`.
- Mã trả về:
  - **200** kèm cảnh báo: AI chỉ đọc được một phần.
  - **422**: ảnh không phải danh thiếp; body có `message` (kèm lý do) và `isNotBusinessCard: true`.
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

### 9.5a. Chạy ChatBot tra cước (gói riêng)
Toàn bộ chatbot (API, app khung chat, use case `FreightChat`, AI/vector store, quy tắc tính cước báo giá) nằm trong thư mục riêng **`Logistics-System-ChatBot/`** của repo này (cạnh `Logistics-System-Desktop/`), với solution `LogisticsSystem.ChatBot.sln`. Solution hệ thống `TruckingSystem.sln` không chứa mã chatbot; hướng dẫn chạy/đóng gói chatbot xem `Logistics-System-ChatBot/README.md`.

- Gói chatbot tham chiếu `TruckingSystem.Domain`/`Application`/`Infrastructure` (DbContext, bảng giá, VietMap) và link `Common` của Desktop từ repo này qua thuộc tính `TruckingSystemSrc` (mặc định `..\Logistics-System-Desktop\src\`). Đổi tên/di chuyển các file đó cần build lại cả gói chatbot.
- ChatBot API dùng chung DB với API chính; phải migrate DB bằng `TruckingSystem.API` trước. API chính vẫn giữ `Gemini`/`Nvidia` cho đọc danh thiếp nhưng không còn dùng `FreightChat`.
- Đóng gói hệ thống:

```bash
dotnet publish Logistics-System-Desktop/src/TruckingSystem.Desktop -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/Desktop
```

```bash
dotnet publish Logistics-System-Desktop/src/TruckingSystem.API -c Release -o publish/Api
```


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
- **Thêm hợp đồng**: mở **Hợp đồng → Thêm mới**, nhập mã, chọn khách hàng, ngày bắt đầu, ngày kết thúc (tuỳ chọn), loại hợp đồng, bảng giá đang áp dụng và điều khoản. Hợp đồng dài hạn cần nhập ưu đãi 0–100% (tối đa 2 chữ số lẻ); hợp đồng một lần vô hiệu ô ưu đãi. Chưa có bảng giá đang áp dụng thì không tạo được hợp đồng. Mã không được trùng; ngày kết thúc được phép bằng ngày bắt đầu.
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
| Ô Địa chỉ khách hàng / nhân viên không hiện gợi ý | Gõ ít nhất 3 ký tự rồi chờ một chút. Ô này vẫn cho gõ tay khi VietMap lỗi hoặc tài khoản không có quyền tìm địa chỉ (Admin, Điều phối). Kiểm tra `VietMap:RouteApiKey` và mạng. |
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
- Tính KPI đúng hạn chỉ theo các lần khách xin dời hạn.

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


## Bảng lương tháng — quy trình mới

**Lương trên hồ sơ nhân sự (Admin):** thêm nhân sự bắt buộc nhập lương tháng là số nguyên dương, tối đa 1.000 tỷ đồng; mức đầu áp dụng từ ngày vào làm, căn cứ tự ghi “Thiết lập lương khi tạo nhân sự”. Form sửa tải mức lương gần nhất và ngày hiệu lực, kể cả mức đã lên lịch. Giữ nguyên số tiền chỉ lưu hồ sơ; đổi số tiền phải chọn ngày áp dụng và nhập căn cứ để tạo lịch sử mới. Nhân sự cũ chưa có lương được giữ nguyên hoặc bổ sung tại form sửa. Ngày áp dụng thuộc thời gian làm việc, từ ngày 1 (mức đầu có thể từ ngày vào làm), sau mốc lương gần nhất. Lỗi tải lương khóa Lưu và cho tải lại; hồ sơ, mức lương và audit lưu cùng giao dịch. Không đổi bảng lương đã chốt, không cần migration. API tạo nhân sự yêu cầu thêm `monthlySalary`; API sửa nhận tùy chọn `monthlySalary`, `salaryEffectiveFrom`, `salaryReason`. Cần cập nhật API và Desktop cùng nhau; client cũ tạo nhân sự thiếu lương sẽ bị từ chối.

Chọn tháng để tự tải dữ liệu. **Tạo / tính lại bảng lương** tính toàn bộ nhân viên trong một transaction, tự tạo kỳ nếu chưa có. Thiếu dữ liệu của bất kỳ người nào thì trả danh sách lỗi và giữ nguyên toàn bộ bảng cũ. **Chốt lương** là bước riêng (Admin); bảng đã chốt không tính lại.

- Nhân viên thường: giờ vào/ra hiệu lực từ module chấm công (điều chỉnh đã duyệt ưu tiên giờ gốc). Đủ 8 giờ = 1 công; từ 4 đến dưới 8 giờ = 0,5 công; dưới 4 giờ = 0. Mỗi ngày tối đa 1 công; chưa tự trừ giờ nghỉ trưa hoặc cộng tăng ca. Các ngày có bản ghi hợp lệ đều tính, ngày không có bản ghi không tự sinh công; không có dữ liệu cả tháng, thiếu lượt quét hoặc điều chỉnh đang chờ thì chặn tính. Lương = LCB/công chuẩn × công hưởng lương; công chuẩn là số ngày T2–T7 trong tháng.
- Tài xế: **LCB × (P / điểm chuẩn tháng)**. P = tổng `(0,5 + km chuyến/K) × H loại xe của chuyến`. Tỷ lệ không giới hạn 100%; không chia LCB theo ngày làm/ngày nghỉ việc, không lấy lịch đăng ký công. Chỉ tính chuyến Completed có StartTime, EndTime trong tháng theo giờ VN, dùng EstimatedDistanceKm. Chưa có km hợp lệ hoặc H tương ứng thì báo lỗi. Không có chuyến hợp lệ ⇒ P=0. Tổng điểm làm tròn 8 số lẻ, tiền lương cuối cùng làm tròn đến đồng.
- **Cấu hình KPI** (Admin): K chung, điểm chuẩn chung, H theo loại xe; hiệu lực từ ngày 1. Chọn lịch sử để xem/sao chép sang ngày hiệu lực mới; lưu cùng ngày là cập nhật cấu hình có audit. Chưa có điểm chuẩn thì không tự quy đổi cấu hình đồng/điểm cũ. Thay cấu hình không đổi phiếu đã chốt.
- **Cộng / trừ tháng** là danh sách riêng theo nhân viên và tháng, có loại, số tiền dương và lý do. Có thể nhập trước khi tạo bảng; tính lại không xóa danh sách. Các khoản là số tiền thực tế, không tự chia theo công/KPI. Sau thay đổi phải tính lại nháp trước khi chốt. Tháng đã chốt khóa thêm/xóa khoản.

Bảng lương chọn nhân viên trực tiếp trên dòng dữ liệu, không còn ComboBox nhân viên. Nhân viên thuộc kỳ nhưng chưa có phiếu vẫn hiện với số liệu trống để có thể nhập mức lương và khoản cộng/trừ trước khi tính lương. Chuột phải vào dòng để mở **Mức lương / lịch sử** hoặc **Cộng / trừ tháng**. Cửa sổ cộng/trừ cố định theo nhân viên và tháng đã chọn; chuột phải vào khoản để **Xóa khoản**. Giữ quyền Admin/Kế toán và khóa thêm/xóa sau khi chốt. Màn Nhân viên chuyển **Sửa** sang menu chuột phải (Admin); màn Hóa đơn chuyển **Xem chi tiết** sang menu chuột phải và vẫn hỗ trợ nhấp đúp.
- Trước chốt, hệ thống đối chiếu nguồn công, chuyến, cấu hình, LCB và danh sách cộng/trừ với bản chụp khi tính. Có thay đổi thì yêu cầu tính lại. Chỉ chi trả phiếu đã chốt; vẫn giữ mức lương/lịch sử, xem chi tiết, xuất HTML và ghi nhận chi trả.
- Đã bỏ Mở kỳ/Tải kỳ, nhập/sửa/xác nhận công trong module lương, lấy công tài xế, cấu hình đồng/điểm và tạo từng phiếu. Menu lương dành Admin/Accountant; Dispatcher xử lý công ở module Chấm công QR.
- Phiếu nháp cũ không tự chuyển/xóa: xuất đối chiếu, chuyển khoản tay sang danh sách tháng, dùng **Xóa nháp cũ**, rồi tạo lại tháng. Phiếu đã chốt và bảng dữ liệu cũ được giữ; không chạy lại công thức mới lên lịch sử.

### Triển khai và kiểm chứng lương

Migration mới `20261102115836_RebuildMonthlyPayroll` chỉ thêm bảng cấu hình, bảng khoản tháng và cột bản chụp/điểm KPI trên phiếu; không xóa dữ liệu cũ. Chưa apply vào database trong phiên triển khai. Cần kiểm tra/backup/apply các migration pending trong môi trường test, triển khai API/Desktop cùng phiên bản và cấu hình điểm chuẩn trước khi vận hành. Down migration sẽ xóa các thông tin mới; không rollback tùy tiện.

API mới: POST `api/payroll/months/generate` (Year, Month, Revision; 0 nếu chưa có kỳ), GET/PUT `kpi-policies`, GET/POST/DELETE `monthly-adjustments`. Các endpoint nhập/lấy/xác nhận công và mở kỳ cũ đã gỡ; lập từng phiếu/cấu hình đồng-điểm trả lỗi hướng dẫn chuyển luồng mới. Không dùng client cũ cho luồng này.

```powershell
dotnet run --project tools/PayrollSelfCheck/PayrollSelfCheck.csproj --no-restore -p:UseSharedCompilation=false -p:BuildInParallel=false
```

Self-check dùng EF InMemory, không thay thế PostgreSQL transaction/constraint hay HTTP authorization/end-to-end. Ảnh UI và SQL migration offline nằm trong thư mục build của harness. Solution vẫn tham chiếu UnitTests không có trong checkout; build API/Desktop trực tiếp.

### Bổ sung loại hợp đồng và ưu đãi (docs/HopDong.md)

- Loại hợp đồng tách riêng khỏi bảng giá công ty. Chọn một lần hoặc dài hạn, rồi chọn bảng giá Active. Form hợp đồng không chọn dòng chi tiết giá. Bảng đang áp dụng được tính theo ngày Việt Nam: chọn ngày hiệu lực mới nhất đã đến, nếu trùng ngày chọn bảng có Id lớn hơn. Bảng chưa đến ngày hoặc đã hủy không được chọn cho hợp đồng/phụ lục mới.
- API `/api/contracts` giữ routes và quyền Admin/Dispatcher/Accountant. Request/response thêm `Type` (`OneTime`/`LongTerm`) và `DiscountPercent` (số JSON). POST bắt buộc Type và PriceListId hợp lệ/Active; LongTerm bắt buộc DiscountPercent từ 0–100, tối đa 2 chữ số lẻ. OneTime chỉ nhận null/0 và lưu null. PUT bỏ cả hai trường mới giữ nguyên giá trị cũ; gửi LongTerm phải kèm phần trăm.
- Chỉ bảng giá **Chưa áp dụng** được sửa tên, ngày hiệu lực và chi tiết giá. Bảng **Đang áp dụng**, **Ngưng áp dụng**, **Đã hủy** chỉ xem; trạng thái do server xác định, không sửa thủ công. Có thể sao chép để tạo bảng giá mới; API từ chối sửa bảng cũ bằng HTTP 409.
- Hợp đồng cũ giữ loại/ưu đãi null, hiển thị chưa phân loại; không suy đoán loại từ ngày hoặc tên bảng giá. Khi sửa được giữ bảng giá lịch sử, kể cả đã ngừng áp dụng. Không đổi khách hàng/xóa hợp đồng đã được đơn bán hàng hoặc đơn vận chuyển sử dụng.
- Hợp đồng vận chuyển một lần bắt buộc có ngày kết thúc khi tạo/sửa, kể cả PUT không gửi Type nhưng hợp đồng đã lưu là OneTime. Khi tạo mới, ngày bắt đầu không được trước hôm nay theo giờ Việt Nam; ngày kết thúc không được trước ngày bắt đầu. Hợp đồng dài hạn có thể không thời hạn. Khi sửa được giữ ngày lịch sử; dữ liệu cũ không tự được điền hoặc cập nhật trong database.
- Menu chuột phải Hợp đồng dùng `ModernMenu`: Xem chi tiết (nhấp đúp), Sửa, Thêm phụ lục và Xóa theo nhóm trong `design.md`. Thêm phụ lục chỉ bật cho hợp đồng đang hiệu lực, mở form chọn sẵn và cố định hợp đồng đã chọn; nếu hợp đồng không còn trong lookup đang hiệu lực thì khóa lưu, không tự chuyển sang hợp đồng khác. Lưu thành công làm mới danh sách. Server vẫn kiểm tra lại điều kiện tạo phụ lục.
- Bỏ mục Điều khoản khỏi form thêm/sửa/chi tiết; tạo mới gửi null, sửa giữ Terms đã lưu để tương thích API và bảo toàn dữ liệu lịch sử. Không cần migration cho thay đổi ngày/menu/điều khoản. Kiểm tra cục bộ: `dotnet run --project tools/ContractPricingSelfCheck -c Release --no-build -- --contract-rules-only` sau khi build harness (tools bị ignore).
- **Phần trăm hiện chỉ được lưu trong hợp đồng/phụ lục**, chưa tự trừ tiền đơn/hóa đơn. Đặc tả chưa xác định cơ sở giảm, phụ phí, làm tròn hay thời điểm chốt. Không tự giới hạn số đơn của loại một lần.
- Cần áp dụng migration `AddContractTypeAndDiscount` qua quy trình triển khai đã kiểm tra backup/môi trường trước khi chạy API mới; migration chỉ thêm hai cột nullable, không cập nhật lịch sử. Chưa áp dụng trong phiên này. Rollback Down xóa hai cột và mất metadata mới; không chạy tùy tiện. Cập nhật Desktop cùng API vì client cũ thiếu Type sẽ không tạo mới được.

Kiểm tra độc lập, không truy cập database thật hoặc chạy seed:

```powershell
dotnet run --project tools/ContractSelfCheck/ContractSelfCheck.csproj -p:UseSharedCompilation=false -p:BuildInParallel=false
```

Công cụ dùng EF InMemory cho service và Npgsql chỉ để kiểm tra model/sinh SQL offline, render form WinForms và kiểm tra lỗi nhập liệu. SQL và ảnh nằm trong thư mục build công cụ. Không thay thế PostgreSQL integration hoặc HTTP có xác thực.


## Chấm công QR qua điện thoại (TruckingSystem.EmployeeApp)

Ứng dụng .NET MAUI (Android) **nằm ngoài repo này** và không có trong `TruckingSystem.sln` (build solution không cần workload MAUI). Mã nguồn hiện để tại `D:\04. Other\TruckingSystem.EmployeeApp` trên máy người phụ trách app. App gồm 5 chức năng: đăng nhập, trang chủ/trạng thái hôm nay, quét QR để Check-in/Check-out, lịch sử chấm công, gửi yêu cầu điều chỉnh. Ứng dụng chỉ gọi `TruckingSystem.API`, không truy cập PostgreSQL. Giao diện theo DESIGN.md (Apple): một màu nhấn Action Blue `#0066CC`, nền parchment, nút pill, thẻ bo 18 px, không đổ bóng.

**Quy tắc đã chốt**
- Địa điểm chấm công là **Kho** hiện có (cần Lat/Lng). Mỗi kho có bán kính cho phép (mặc định 150 m) và sai số GPS tối đa (mặc định 100 m); Admin sửa trên Desktop.
- Một QR cho mỗi kho mỗi ngày (giờ VN), nhiều nhân viên quét chung. Nội dung QR `TSATT:<token>` ngẫu nhiên; backend tra ra kho/ngày.
- Mỗi lượt quét gửi QR + vị trí GPS (độ chính xác, cờ vị trí giả lập). Backend kiểm tra tài khoản → hồ sơ nhân viên (`Employee.UserId`, không nhận EmployeeId từ thiết bị), QR đúng ngày, kho có tọa độ, khoảng cách/sai số, vị trí giả lập và trạng thái.
- 1 ca/ngày, không ca đêm: lần hợp lệ đầu = Check-in; lần sau (cách ≥ 1 phút) ứng dụng hỏi xác nhận rồi mới Check-out. Được Check-out ở kho khác. Sau Check-out không quét thêm. **Giờ ghi nhận do máy chủ quyết định.**
- Mọi lượt quét (kể cả bị từ chối kèm lý do) lưu vào nhật ký gốc `AttendanceScans`, không sửa/xóa.
- Điều chỉnh: nhân viên đề xuất giờ vào và/hoặc giờ ra kèm lý do, giải thích (không đính kèm), chỉ cho ngày trong 7 ngày gần nhất, mỗi ngày tối đa 1 yêu cầu chờ duyệt. Admin/Dispatcher duyệt/từ chối (từ chối phải ghi lý do) trên Desktop. Duyệt ghi giờ vào `AdjustedCheckInAt/AdjustedCheckOutAt`, giữ nguyên giờ quét gốc, lưu người/thời điểm duyệt.
- Tài xế kết thúc chuyến ngoài kho: gửi yêu cầu điều chỉnh (lý do "Làm việc ngoài cơ sở").
- Dữ liệu QR/GPS **không tự thành công tính lương**: bảng công lương vẫn nhập/xác nhận ở "Công & lương".
- Tài khoản đăng nhập được Mobile nếu đang hoạt động **và** gắn với hồ sơ nhân viên chưa nghỉ việc. Admin gán hoặc bỏ gán qua ô **Gán nhân viên** trên form thêm/sửa tài khoản; danh sách tài khoản hiển thị nhân viên đã liên kết. Mỗi tài khoản gán tối đa một nhân viên và không được lấy nhân viên của tài khoản khác. Chọn **Không gán nhân viên** để bỏ liên kết; việc này không xóa hồ sơ nhân viên. Cập nhật API và Desktop cùng đợt để sử dụng ô chọn mới.
- Tự kiểm tra gán nhân viên (nghiệp vụ, JSON tương thích client cũ, form thật và HTTP giả): `dotnet run --project tools/AccountEmployeeSelfCheck -c Release -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false`. Thư mục `tools` giữ cục bộ theo `.gitignore` hiện có; kiểm tra này không thay thế thử transaction trên PostgreSQL thật.

**API** (`api/attendance`, cần JWT)
- Mobile: `GET me`, `POST scan`, `GET me/history?from=&to=`, `GET me/adjustments`, `POST me/adjustments`, `GET adjustment-reasons`.
- Desktop (Admin/Dispatcher): `GET sites`, `PUT sites/{warehouseId}` (chỉ Admin), `GET sites/{warehouseId}/qr?date=`, `GET records?from=&to=`, `GET scans?date=`, `GET adjustments?status=`, `POST adjustments/{id}/review`.
- Giờ trong request/response là giờ Việt Nam không kèm múi giờ; DB lưu UTC.

**Desktop** → menu **Nhân sự › Chấm công QR**: phát/lưu PNG/in QR theo kho và ngày (từ hôm nay tới 30 ngày sau), cấu hình bán kính/sai số, duyệt yêu cầu điều chỉnh, xem bảng chấm công, nhật ký quét.

**Database**: migration `20261102111437_AddMobileQrAttendance` chỉ thêm 4 bảng (`AttendanceQrCodes`, `AttendanceScans`, `AttendanceRecords`, `AttendanceAdjustmentRequests`) và 2 cột của `Warehouses` có giá trị mặc định 150/100 cho kho cũ. Sao lưu rồi chạy:
```bash
dotnet ef database update --project src/TruckingSystem.Infrastructure --startup-project src/TruckingSystem.API
```

**Chạy ứng dụng điện thoại**

1. Chạy API cho điện thoại nhận được (ở thư mục gốc repo, cmd; không chạy API bằng VS cùng lúc). WinForms trong lúc này đăng nhập bằng `http://localhost:8081`; chạy API bằng VS như thường thì quay lại `https://localhost:8080`:
```bash
dotnet run --project src/TruckingSystem.API --urls http://0.0.0.0:8081
```
2. Mở firewall cổng 8081 một lần (cmd quyền Administrator):
```bash
netsh advfirewall firewall add rule name="TruckingSystem API 8081" dir=in action=allow protocol=TCP localport=8081
```
3. Cắm điện thoại (đã bật Gỡ lỗi USB), rồi ở **thư mục của app** build và cài:
```bash
dotnet build -f net10.0-android -t:Run
```
- Cần .NET 10 SDK + workload `maui`/`android`. Bản `net10.0-windows` chỉ để xem giao diện (không quét camera; có ô dán nội dung QR để thử).
- Ở màn đăng nhập bấm **Máy chủ** để nhập địa chỉ API: máy ảo Android `http://10.0.2.2:8081`; điện thoại thật dùng IP LAN của máy chạy API (API phải lắng nghe trên địa chỉ đó, mở firewall). `usesCleartextTraffic` đang bật cho http nội bộ; triển khai thật nên dùng https và tắt cờ này.
- Quyền cần cấp: Camera, Vị trí.

**Kiểm tra**: `dotnet run --project tools/AttendanceSelfCheck/AttendanceSelfCheck.csproj -p:UseSharedCompilation=false` (80 kiểm tra: quy tắc GPS/QR, Check-in/out, xác nhận, quét lặp, điều chỉnh/duyệt, migration offline, render màn Desktop). EF InMemory không chứng minh ràng buộc SQL, tranh chấp đồng thời hay phân quyền HTTP.

**Giới hạn**: QR theo ngày có thể bị chụp/chuyển tiếp; GPS có sai số và có thể bị giả mạo (chỉ chặn cờ mock của Android). MVP không chống dùng chung tài khoản. Chưa có ca đêm/nhiều ca, chưa nối dữ liệu chấm công vào bảng công lương.


## Freight chatbot (Desktop, Gemini/OpenAI-compatible + vector RAG)

Packaged separately in the repo folder `Logistics-System-ChatBot/` (section 9.5a): run `TruckingSystem.ChatBot.API` (default `https://localhost:8090`) and open the `TruckingSystem.ChatBot.Desktop` app (`TruckingChatBot.exe`, server URL in `chatbot.settings.json`). The system Desktop app and main API no longer contain the chatbot. The window is a conversation view: customer messages appear as right-aligned bubbles, assistant replies on the left with option chips (vehicle, address, confirm) and a quote card. Each quote is one shipment (one pickup place, one delivery place). Describe the goods, weight, pickup and delivery places and whether the warehouse should collect the goods or you bring them to the warehouse; answer the bot's questions in short replies or click its option chips. The welcome screen offers chips for the price list, the pricing formula and a usage guide. Enter sends, Shift+Enter adds a line. Chips from earlier turns are disabled. Right-click a message to copy it (tables are copied tab-separated). **Cuộc trò chuyện mới** starts over. Confirm the order summary before quoting. Changing the request clears previous confirmations. Conversations expire after 30 minutes and are not stored in the database.

- Chat provider: `FreightChat:Provider` = `Gemini` (default) or `OpenAICompatible` (any `/chat/completions` API: NVIDIA NIM, OpenAI, Groq...). Changing provider only needs configuration; Application/API/Desktop are unchanged.
  - `Gemini`: model `FreightChat:Model` (default `gemini-3.8-flash`); key priority `FreightChat:ApiKey`, `GEMINI_API_KEY`, then `Gemini:ApiKey`.
  - `OpenAICompatible`: `FreightChat:BaseUrl`, `FreightChat:Model`, `FreightChat:ApiKey` (HTTPS only), each falling back to the existing `Nvidia:BaseUrl`/`Nvidia:Model`/`Nvidia:ApiKey`. Tested with `nvidia/nemotron-3.5-lightning-30b-a3b`. `FreightChat:DisableThinking` sends `chat_template_kwargs.enable_thinking=false`; it defaults to true for `*.nvidia.com` because reasoning mode took 24–31 s and could exhaust tokens before returning JSON. Other providers may reject that field, so leave it false for them. Vietnamese is not an officially listed Nemotron language; run `--live-ai` after changing model.
  - Environment overrides use `FreightChat__ApiKey` etc. Do not commit keys. The example JSON is documentation only; merge settings into local configuration or use environment variables.
- Natural replies (server controls, AI phrases): the server still decides every step (missing data, vehicle/place options, confirmation, pricing). After that, a second AI call writes a short opening that answers the customer's side question from the active public price list, politely declines out-of-scope requests, or acknowledges the message. The server then appends its own required question/summary/quote verbatim, so a required step can never be dropped. Sentences in the opening that ask the customer a question, copy earlier server lines/option labels, or repeat a greeting after the first turn are removed, and at most 3 sentences are kept. The opening is discarded (server text only) when it contains a number absent from the server facts or the customer's messages, a question mark, markup/JSON/links, exceeds 1,000 characters, or the AI call fails. Disable with `FreightChat:NaturalReplies=false` to halve AI usage. Reply calls time out after 20 s.
- Embeddings: `gemini-embedding-001`, 768 normalized dimensions; document/query task types are distinct. Embeddings always use Gemini; with `OpenAICompatible` chat they read only `GEMINI_API_KEY`/`Gemini:ApiKey`, never the chat provider key. Switching embedding provider needs a new migration if dimensions change, plus a re-index.
- Reuse existing `VietMap:RouteApiKey` and `VietMap:MapApiKey` settings. Quotes use car-route distances, not truck-restriction routing.
- Requires PostgreSQL with the **pgvector server extension installed**. Review and apply migration `20261102160000_AddFreightVectors` in a backed-up test environment first. It adds the extension and an Infrastructure-owned SQL vector table; the table is accessed through parameterized Npgsql commands, not Domain vector types. Down drops only the index table and preserves the shared extension. No existing tariff data is modified.
- The background worker synchronizes at startup and every minute, embedding changed documents only. Missing/obsolete index records cannot produce quotes. Initial synchronization requires working AI credentials and quota. No vector-specific client package is needed: existing Npgsql passes parameterized vector text to PostgreSQL casts.
- Public endpoint: `POST /api/public/freight-chat`, body `{ "message": "...", "context": null, "selection": null }`. Carry the returned opaque protected context into the next request; use returned option IDs as selection. Never construct context or supply prices. Limit: 2,000 characters/message, 20 context messages, 10 requests/minute/IP. Persist Data Protection keys across deployments if conversations must survive restarts; multiple API instances must share keys. Behind a proxy, configure trusted forwarding explicitly; otherwise clients share the proxy IP limit.
- Conversation flow (server decides, AI phrases): the server fills slots in a fixed order — cargo, weight, cargo type (fragile/hazardous/ordinary), pickup place, delivery place, pickup method (`company` = warehouse collects, `self` = customer brings goods), vehicle, place confirmation (VietMap), nearest warehouse, order summary, confirm, quote. The AI extracts `{ cargo, weight, unit, isFragile, isHazmat, from, to, pickupMethod, vehicle, asks }` and receives `lastQuestion` (the slot and text the bot just asked) so short replies fill the right slot. It infers fragile/hazardous from the cargo description; generic cargo ("hàng", "đồ") is always asked. Pickup method from the AI is accepted only when the customer mentioned it, when the bot is asking that slot, or when it was already known; bare replies ("không", "ok") never fill it, and an explicit keyword ("tự mang", "kho tới lấy") overrides the AI. Option ids: `pickup:company|self`, `cargo:normal|fragile|hazmat`, `vehicle:{id}`, `place:{i}`, `confirm`; ids not offered in the current step return 400. The vehicle is suggested from the weight (smallest vehicle whose name states a capacity ≥ the weight, then vehicles without a stated capacity); a vehicle the customer names is matched through the vector index.
- Response `tables`: list of `{ kind: priceList|formula|guide|summary, title, subtitle, columns, numericColumns, rows, note }` with cells already formatted vi-VN (max 30 rows). Price list / formula / guide appear when the latest message asks for them (AI `asks` plus diacritic-insensitive keywords); the summary appears at the confirmation step. Response `quote`: `{ priceList, effectiveDate, quotedAtUtc, vehicleType, pickupMethod, warehouse, legs: [{ name, from, to, distanceKm }], lines: [{ name, detail, amount }], total, notice }`. Context protection purpose is `FreightChat.v2`; conversations started before this version must start over. The earlier `priceTable` field and multi-leg itineraries were removed.
- The latest active effective price list is public to this endpoint. Highest ID breaks same-date ties. Warehouse pickup: km(pickup place → nearest warehouse) + km(warehouse → delivery place); self delivery: km(warehouse → delivery place). The nearest warehouse with coordinates is chosen, preferring one with a zone (with at least one bin) suitable for the cargo type; otherwise the nearest one with a note that staff will confirm. Total km × price/km, total tons × price/ton once, fragile/hazardous surcharge once as % of (km + ton charges) when a global surcharge period applies, otherwise the fixed fee of the vehicle — the same rules as invoices. Quotes exclude VAT, tolls, loading and fees outside the tariff; no contract discounts, orders or invoices are created.

Verification (build projects directly; the solution references a missing UnitTests project):

```powershell
dotnet build src/TruckingSystem.API/TruckingSystem.API.csproj --no-restore -m:1 -p:UseSharedCompilation=false
dotnet build src/TruckingSystem.Desktop/TruckingSystem.Desktop.csproj --no-restore -m:1 -p:UseSharedCompilation=false
dotnet run --project tools/FreightQuoteSelfCheck/FreightQuoteSelfCheck.csproj -p:UseSharedCompilation=false -p:BuildInParallel=false
```

Add `-- --live-ai` to send two real Vietnamese test messages (round trip with kg/tấn and fragile cargo; prompt injection with hazardous cargo) to the configured chat provider. It uses quota and prints the extracted itinerary, never keys.

The self-check covers pricing and the conversation with fake AI/maps/vector retrieval, plus protected-context integrity. `--probe-db` performs only a read-only pgvector availability check using configured connection settings. It does not install extensions or apply migrations. Real PostgreSQL vector retrieval, HTTP rate limiting, live Gemini/VietMap and interactive Desktop need a configured integration environment.

### Bảng phụ phí

Trạng thái danh sách phụ phí: trong các bảng đã xác nhận và **đã đến ngày hiệu lực theo giờ Việt Nam**, bảng có ID lớn nhất là **Đang áp dụng**. Bảng đã xác nhận có ngày tương lai là **Chưa áp dụng**, chưa thay trạng thái bảng hiện hành; các bảng hiệu lực cũ là **Ngưng áp dụng**. Đến ngày hiệu lực, trạng thái được tính lại khi tải/làm mới danh sách, không cần công bố lại hoặc tác vụ nền. Nếu chỉ có bảng tương lai thì chưa có bảng đang áp dụng. Bản nháp cũ không tự được công bố. API giữ IsActive/Published và bổ sung IsPending để Desktop hiển thị trạng thái chờ theo ngày của server; cách chọn ID lớn nhất khớp tra phụ phí cho hóa đơn/chatbot. Cần cập nhật API và Desktop cùng đợt, không cần migration cho sửa lỗi này.

Mục **Phụ phí** trong nhóm Kinh doanh quản lý các bảng độc lập với bảng giá xe. Admin/Kế toán nhập ngày và hai dòng tỷ lệ (dễ vỡ, nguy hiểm); tên được server tự sinh PP-[năm Việt Nam hiện tại]-[STT tối thiểu 3 số], ví dụ PP-2026-004, chọn **Lưu và áp dụng** (ngày hôm nay) hoặc **Lưu và chờ áp dụng** (ngày tương lai) để xác nhận và khóa bảng ngay trong một lần lưu; bảng tương lai chỉ được tính phí từ ngày hiệu lực. Đã bỏ chức năng sao chép bảng phụ phí trên giao diện và API. Điều phối chỉ xem. Ngày hiệu lực từ hôm nay theo giờ Việt Nam, chỉ chặn ngày quá khứ; được thêm nhiều bảng cùng ngày. Khi tính phí, chọn bảng thêm sau đã đến ngày hiệu lực. Bản nháp cũ được giữ nguyên; nhấp đúp vào dòng để mở **Chi tiết** (đã bỏ nút trên thanh công cụ), kiểm tra thông tin và chọn **Lưu và áp dụng** để xác nhận, không tự động kích hoạt dữ liệu cũ.

Hóa đơn chọn bảng đã công bố theo ngày bắt đầu chuyến đầu của đơn; báo giá chọn theo ngày tra cứu. Mỗi loại phí bằng tỷ lệ nhân tổng cước km và tấn trước chiết khấu, làm tròn hai chữ số và thu một lần trên toàn đơn. Trước kỳ đầu, hệ thống dùng phí cố định lịch sử. Chi tiết bảng giá xe chỉ quản lý giá/km và giá/tấn.

Áp migration **SplitSurchargeLists** trước khi khởi động bản API mới và cập nhật Desktop cùng đợt. Migration chuyển mỗi kỳ cũ thành một bảng đã công bố với hai dòng, bảo toàn hóa đơn đã lưu. Nếu chưa có kỳ nào, người quản lý tạo và lưu kỳ đầu. API dùng GET/POST `/api/surcharge-lists`, PUT theo ID và POST theo ID với hậu tố `/copy`; POST, PUT và sao chép đều xác nhận bảng trong cùng lần lưu, trả `Published = true`. Giữ nguyên cấu trúc request/response và endpoint `/publish` cho bản nháp cũ, nhưng Desktop không còn bước công bố riêng. Thay đổi luồng này không cần migration mới. Endpoint đọc kỳ cũ tiếp tục trả bảng đã công bố; endpoint ghi cũ trả HTTP 410.

Kiểm tra luồng lưu phụ phí: `dotnet run --project tools/SurchargeSelfCheck -c Release`. Kiểm tra form tạo/nháp cũ, tên tự sinh, lỗi API và quyền chỉ xem: `dotnet run --project tools/BusinessDesignSelfCheck -c Release -- --surcharge-only` (Windows).

Chạy `dotnet run --project tools/FreightQuoteSelfCheck` và `dotnet run --project tools/ContractSelfCheck` để kiểm tra.

### Giao diện kinh doanh, nhân sự và quản trị

Các màn Khách hàng, Hợp đồng, Phụ lục hợp đồng, Bảng giá, Phụ phí và Hóa đơn cùng các cửa sổ thêm/sửa/chi tiết nhận kiểu `ModernLook` theo `design.md`: nút bo góc, nút chính gradient theo theme và tiêu đề bảng sáng. Cửa sổ Người liên hệ và Chi tiết bảng giá cũng áp kiểu cho chân Đóng. Kiểm tra bằng `dotnet run --project tools/BusinessDesignSelfCheck -c Release` trên Windows với HTTP giả lập; tools chỉ có cục bộ.

Nhân viên, Bảng lương, Chấm công QR, Quản lý tài khoản và Hệ thống áp dụng `design.md`: nút theo bốn theme, bảng sáng, nhãn trạng thái và form có chân Lưu/Đóng cố định. Ô tiền lương/chi trả/cộng trừ dùng quy ước chung: gõ `500` thành `500.000 đ`, hoặc nhập đủ số đồng. Cấu hình KPI mới không điền sẵn K; nhập theo cấu hình được duyệt. Các quyền và API giữ nguyên.

Ở **Bảng lương**, chuột phải dòng → **Xem chi tiết** hoặc nhấp đúp để xem phiếu lương; đã bỏ nút Chi tiết ở chân màn hình. Mục này chỉ bật cho Admin/Kế toán khi nhân viên có phiếu trong kỳ. Menu vẫn giữ Mức lương/lịch sử và Cộng/trừ tháng cho nhân viên chưa có phiếu. Kiểm tra cục bộ: `dotnet run --project tools/ContractPricingSelfCheck -c Release --no-restore -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false -- --payroll-menu-only`.

Kiểm tra form và các trường hợp nhập thiếu với HTTP giả lập trên Windows: `dotnet run --project tools/PeopleDesignSelfCheck -c Release -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false`. Ảnh render nằm trong thư mục `bin/Release/net8.0-windows` của self-check. Kiểm tra hồi quy gợi ý địa chỉ: `dotnet run --project tools/AddressSuggestSelfCheck -c Release`.

### Phòng ban và chức danh

Màn Hệ thống chỉ hiển thị phòng ban. Chuột phải một phòng ban → **Xem chức danh**, **Sửa phòng ban** hoặc **Xóa phòng ban**. Trong cửa sổ chức danh, chuột phải dòng để **Sửa chức danh/Xóa chức danh**; thanh công cụ chỉ giữ Thêm và Làm mới. Form thêm phòng ban có nút **Hiện/Ẩn thêm chức danh**, nhập số lượng 0–100 để hiện các ô tên tương ứng; phòng ban và toàn bộ chức danh được lưu trong một lần. Ẩn phần nhập vẫn giữ nội dung và số lượng đã chọn; đặt số lượng về 0 nếu chỉ muốn tạo phòng ban. Tên chức danh không trùng trong cùng phòng ban. Form nhân viên lọc chức danh theo phòng ban, API kiểm tra lại cặp phòng ban/chức danh.

Cần áp migration **AddPositionDepartment** trước khi chạy API mới và cập nhật Desktop cùng đợt. Migration thêm khóa ngoại nullable, chỉ tự gán chức danh cũ khi tất cả nhân viên sử dụng đều thuộc cùng một phòng ban. Dữ liệu chức danh chưa rõ phòng ban vẫn được giữ; màn Hệ thống đã bỏ nút **Chức danh chưa phân phòng**. Không cho chuyển chức danh đang được nhân viên của phòng khác sử dụng, hoặc xóa phòng ban còn chức danh. GET giữ các trường cũ và bổ sung departmentId; POST /api/system/positions yêu cầu departmentId, POST /api/system/departments nhận thêm danh sách positions tùy chọn. Quyền quản trị Admin giữ nguyên.

Màn **Quản lý tài khoản** giữ nút Thêm tài khoản và Làm mới; chuột phải tài khoản để **Sửa / phân quyền** hoặc **Đặt lại mật khẩu**. Menu chọn đúng dòng vừa bấm, không mở khi đang tải/xử lý hoặc khi bấm vùng trống. Kiểm tra với HTTP giả lập trên Windows: `dotnet run --project tools/ManagementMenuSelfCheck -c Release -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false`.

Form thêm tài khoản ẩn checkbox hoạt động và mặc định tài khoản mới hoạt động; form sửa vẫn cho đổi trạng thái. Kiểm tra nghiệp vụ và giao diện trên Windows: dotnet run --project tools/SystemManagementSelfCheck -c Release -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false. Thư mục tools được giữ cục bộ theo .gitignore hiện có.

Form **Thêm nhân viên** ẩn trạng thái và ngày nghỉ việc; nhân viên mới mặc định **Đang làm**. Server tự sinh mã **NV-[STT]** (NV-001, NV-002…), bỏ qua mã client gửi và lấy ngày vào làm là ngày hiện tại theo giờ Việt Nam; form chỉ hiển thị hai ô này. STT lấy số lớn nhất của tiền tố NV- cộng một, không đổi mã cũ. Mã không được sửa trên form hoặc qua API. Ngày sinh bắt buộc khi tạo/sửa, từ năm 1900 và tuổi **từ đủ 18** (đúng sinh nhật 18 tuổi được chấp nhận). Nhân viên cũ thiếu ngày sinh cần bổ sung trước khi lưu sửa; API cũ bỏ qua cả ngày sinh/giới tính vẫn giữ ngày đã lưu nếu hợp lệ. Ngày nghỉ việc vẫn được xóa bằng Delete/Backspace. Giao dịch Serializable giữ nguyên việc lưu hồ sơ/lương/nhật ký cùng nhau và bảo vệ cấp mã đồng thời; khi tranh chấp hãy thử lưu lại. Không đổi dữ liệu lịch sử hoặc schema. Cập nhật API và Desktop cùng đợt. Kiểm tra: `dotnet run --project tools/EmployeeCreateSelfCheck -c Release -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false`; thêm `-- --edit-dates` để kiểm tra ô ngày trên form sửa.


### Hợp đồng, ưu đãi và bảng giá

Theo docs/HopDong.md và docs/BangGia.md: hợp đồng mới được server sinh mã HD-[CustomerId]-[năm Việt Nam]-[STT tối thiểu 3 số]; bảng giá mới và bảng sao chép có tên BG-[năm]-[STT tối thiểu 3 số], trạng thái theo ngày hiệu lực. CustomerId là mã khách hàng hiện có; số hợp đồng tăng theo khách hàng/năm, số bảng giá theo năm. Dùng quy tắc BG-[NĂM]-[STT] trong tài liệu, không thêm TC vì chưa có quy ước cho thành phần này. Tên/mã gửi từ client khi tạo được thay bằng giá trị server sinh; sửa dữ liệu cũ giữ cách xử lý hiện có. Giao dịch Serializable bảo vệ việc cấp số khi tạo đồng thời; nếu xảy ra tranh chấp, API trả 409 để người dùng thử lưu lại.

Admin chọn **Quản lý ưu đãi** ở màn Hợp đồng để lưu ngưỡng 0–100 (mặc định 20). Server kiểm tra ngưỡng khi tạo hoặc thay đổi ưu đãi; chỉ Admin được vượt ngưỡng và vẫn không vượt 100%. Đổi cấu hình không đổi ưu đãi đã lưu. Form hợp đồng lấy ngưỡng hiện tại khi mở, tự chọn bảng giá đang áp dụng; hợp đồng cũ vẫn giữ được bảng giá đã ngừng áp dụng. Chuột phải một dòng hợp đồng để mở Sửa/Chi tiết/Xóa. Chuột phải dòng bảng giá để xem chi tiết, tạo mới và sao chép, sửa hoặc **Hủy bảng giá**. Sửa/hủy chỉ bật cho bảng **Chưa áp dụng**. Form thêm/sửa/sao chép luôn hiện chi tiết giá; phải nhập cả giá/km và giá/tấn cho từng loại xe, có thể nhập 0 nếu không tính. Khi sao chép được chọn ngày hiệu lực, giá nguồn được tải để kiểm tra và chỉnh trước khi lưu.

Áp migration **20261105125758_AddMaxDiscountPercent** trước khi chạy API mới; migration chỉ tạo bảng cấu hình MaxDiscountPercent và dòng Id=1, Value=20, không sửa hợp đồng/bảng giá cũ. GET /api/contracts/max-discount-percent trả số nguyên; PUT cùng đường dẫn nhận { value: 20 }, chỉ Admin. Cập nhật API/Desktop cùng đợt. Self-check Windows cục bộ (tools bị ignore): dotnet run --project tools/ContractPricingSelfCheck -c Release -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false.

Cửa sổ **Xem chi tiết bảng giá** từ menu chuột phải hoặc nhấp đúp vào dòng hiển thị toàn bộ loại xe từ API cùng các dòng giá đã lưu trong DataGridView. Menu dùng `ModernMenu`, nhóm xem chi tiết / sao chép và sửa / hủy; thao tác bị khóa có tooltip giải thích. Tên bảng giá là nhãn, không còn ComboBox hiển thị. Loại xe chưa cấu hình ghi **Chưa có giá**. Nhập/sửa giá/km và giá/tấn trực tiếp trên từng dòng, rồi chọn **Lưu thay đổi**; không cần nút Thêm mới hay mở form riêng. Mỗi loại xe bắt buộc phải có cả hai giá (nhập 0 nếu không tính), không bỏ qua dòng còn thiếu giá. API kiểm tra toàn bộ các dòng và lưu cùng một lần, lỗi giữ dữ liệu nhập; đóng/làm mới khi chưa lưu có xác nhận bỏ thay đổi. Chỉ bảng giá chưa áp dụng cho sửa giá; ba trạng thái còn lại chỉ xem.

Lưu giá trực tiếp dùng PUT /api/price-lists/{priceListId}/tariff-rules với { rows: [{ id: null hoặc id dòng hiện có, vehicleType, pricePerKm, pricePerTon }] }. Giữ quyền Admin/Dispatcher/Accountant, server chỉ cho sửa bảng giá chưa đến ngày áp dụng và chưa hủy, dòng sai bảng/ID/loại xe trùng và đơn giá không hợp lệ; không sửa phụ phí cũ. Cập nhật API và Desktop cùng đợt, không cần migration mới cho việc nhập giá trực tiếp. Self-check cục bộ: dotnet run --project tools/ContractPricingSelfCheck -c Release --no-restore -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false -- --inline-pricing-only.

Khi **thêm bảng giá**, form có đủ 9 loại xe: Xe van 1 tấn; Xe tải 1,5 tấn; Xe tải 2,5 tấn; Xe tải 3 tấn; Xe tải 5 tấn; Xe tải 8 tấn; Xe tải 15 tấn; Đầu kéo container 20 feet; Đầu kéo container 40 feet. Đơn giá mới để trống để người dùng nhập rõ, không tự điền 0. POST tạo và PUT sửa phải có `tariffRules` đầy đủ; mỗi dòng phải có `pricePerKm` và `pricePerTon`. Sửa giữ ID và phụ phí lịch sử của dòng giá. Sao chép giữ cả loại xe riêng của nguồn; nguồn thiếu giá thì phải bổ sung trước khi lưu. Lỗi lưu giữ dữ liệu đã nhập, lỗi tải chi tiết khóa lưu.

Bảng giá có 4 trạng thái: **Đang áp dụng** (`Active`), **Ngưng áp dụng** (`Inactive`), **Chưa áp dụng** (`Pending`), **Đã hủy** (`Cancelled`). Tạo/sao chép bảng tương lai không thay trạng thái bảng hiện hành. Trạng thái được tính mỗi lần đọc theo ngày Việt Nam; tới ngày hiệu lực, bảng có ngày hiệu lực mới nhất được áp dụng, trùng ngày chọn Id lớn hơn. Không cần tác vụ nền. Hủy dùng `POST /api/price-lists/{id}/cancel`; `DELETE /api/price-lists/{id}` được giữ cho client cũ nhưng cũng chỉ hủy. Hủy chỉ dành cho bảng chưa áp dụng, giữ nguyên bảng và toàn bộ chi tiết; bảng đã hủy không tự áp dụng về sau. Hợp đồng/hóa đơn cũ giữ tham chiếu giá lịch sử.

Không đổi schema hoặc chuyển dữ liệu cũ: cột trạng thái số hiện có giữ Active=0, Inactive=1, thêm Pending=3 và Cancelled=4 (không dùng lại giá trị Expired=2 cũ). Bảng Active có ngày tương lai được hiển thị Chưa áp dụng; các bảng Active đã đến ngày chỉ có bảng mới nhất được chọn. Cập nhật API/Desktop và gói chatbot cùng đợt để dùng chung quy tắc. Self-check Windows cục bộ (tools bị ignore): build `tools/ContractPricingSelfCheck`, chạy `dotnet tools/ContractPricingSelfCheck/bin/Release/net8.0-windows/ContractPricingSelfCheck.dll --pricing-lifecycle-only`. Lượt 2026-11-12 đã build được nhưng Windows Application Control chặn chạy; chưa có kết quả kiểm tra runtime.

Phụ phí mới sinh tên trên server khi lưu, bỏ qua tên client gửi; ô Tên bảng trên form thêm chỉ đọc. Số tăng theo năm, dùng số lớn nhất đang có cộng một; sau 999 tiếp tục 1000. Giao dịch Serializable bảo vệ cấp số đồng thời, tranh chấp trả 409 để thử lại. Giữ tên bảng cũ và thao tác sửa nháp cũ. Endpoint POST /api/surcharge-lists/{id}/copy đã được gỡ theo yêu cầu bỏ sao chép. Không cần migration; cập nhật API/Desktop cùng đợt.

Để cho phép phụ phí cùng ngày, áp migration **20261105151437_AllowSameDaySurchargeLists** trước API mới. Migration thay unique index theo EffectiveDate bằng index không unique, không xóa hoặc sửa dữ liệu. Form thêm mặc định ngày hôm nay và không cho chọn ngày quá khứ. Giữ các bảng đã xác nhận để tra cứu lịch sử; trong cùng ngày, bản thêm sau thay bản trước. Rollback migration chỉ thực hiện được khi dữ liệu đáp ứng lại điều kiện duy nhất theo ngày.

Form **Thêm/Sửa phụ lục hợp đồng** thay chọn chi tiết bảng giá bằng **% ưu đãi của hợp đồng**. Chọn hợp đồng điền mức ưu đãi gốc để chỉnh lại; sửa phụ lục hiển thị mức đã lưu. Giá trị được lưu riêng trên phụ lục, không sửa hợp đồng gốc. Hợp đồng một lần giữ 0%; các mức khác từ 0–100%, tối đa 2 chữ số lẻ. Admin được vượt hạn mức cấu hình khi thêm/sửa, vẫn không vượt 100%. Các tài khoản khác bị chặn trước khi gửi và server kiểm tra lại hạn mức mới nhất khi lưu. Nếu mức đã lưu vượt hạn mức mới, form vẫn hiển thị nguyên giá trị; tài khoản không phải Admin cần chỉnh về mức hợp lệ trước khi lưu, không tự giảm dữ liệu. Danh sách phụ lục có cột Ưu đãi (%). Phần trăm vẫn chỉ là dữ liệu lưu, chưa tự trừ tiền hóa đơn.

Khi API khởi động, `DbSeeder` tự tạo dòng cấu hình `MaxDiscountPercent` Id=1, Value=20 nếu dòng này bị thiếu, kể cả khi đã có dữ liệu chuyến xe. Cấu hình đã tồn tại (bao gồm 0%) được giữ nguyên. Sau khi xóa dữ liệu bảng cấu hình, khởi động lại API để tạo lại mức mặc định 20%; cấu trúc bảng vẫn phải có từ migration.

Mã phụ lục mới tự sinh khi lưu theo **PL-[STT]-[Mã hợp đồng]**, ví dụ `PL-001-HD-2-2026-001`. STT tăng riêng theo hợp đồng từ 001, lấy số lớn nhất đang có cộng một và tiếp tục 1000 sau 999. Ô mã trên form thêm chỉ đọc, gợi ý đổi theo hợp đồng chọn; server bỏ qua mã client gửi khi tạo. Giao dịch Serializable bảo vệ cấp số đồng thời; nếu tranh chấp, API trả 409 để thử lại. Giữ nguyên mã phụ lục cũ và cách sửa hiện có. Không cần migration mới cho việc tự sinh mã.

Danh sách Phụ lục hiển thị **Trạng thái hợp đồng** theo ngày bắt đầu/kết thúc và **Trạng thái phụ lục** (Đang áp dụng / Ngừng áp dụng). Chỉ thêm hoặc chuyển phụ lục sang hợp đồng **Đang hiệu lực**; server kiểm tra lại khi lưu. Lưu phụ lục mới áp dụng ngay, chuyển phụ lục đang áp dụng trước đó của cùng hợp đồng sang Ngừng áp dụng trong một giao dịch. Sửa phụ lục cũ giữ trạng thái cũ; xóa bản đang áp dụng không tự khôi phục bản trước. Bỏ nhãn hạn mức ưu đãi ở cuối form thêm/sửa, giữ kiểm tra ưu đãi hiện có. **Sửa/Xóa** chuyển sang menu chuột phải theo `ModernMenu`.

Migration **20261105195819_AddContractAppendixStatus** thêm trạng thái và unique index có điều kiện, bảo đảm tối đa một phụ lục Đang áp dụng cho mỗi hợp đồng. Dữ liệu cũ giữ nguyên; phụ lục có CreatedAt mới nhất (Id lớn nhất khi trùng) được giữ Đang áp dụng, các bản còn lại chuyển Ngừng áp dụng. Cần áp migration sau `AddAppendixDiscountAndPriceList` trước khi chạy API mới; chưa tự áp vào database. Down xóa cột trạng thái nên mất thông tin áp dụng. Hóa đơn tính mới dùng phụ lục Đang áp dụng ngay khi lưu, kể cả Ngày có hiệu lực ghi tương lai; ngày vận chuyển vẫn dùng để kiểm tra hợp đồng và tính phụ phí. Hóa đơn đã lưu giữ nguyên các dòng tiền. Kiểm tra cục bộ: `dotnet run --project tools/ContractPricingSelfCheck -c Release -- --row-menus-only` (tools bị ignore).

Áp migration **20261105171929_AddAppendixDiscountAndPriceList** trước khi cập nhật API/Desktop cùng đợt. Migration thêm DiscountPercent và PriceListId nullable, cho phép TariffRuleId null, giữ nguyên dữ liệu/chi tiết giá lịch sử. API vẫn nhận TariffRuleId từ client cũ; client mới gửi PriceListId và DiscountPercent. Phụ lục mới không chọn một dòng giá; hóa đơn tra đơn giá theo loại xe trong bảng giá được chọn nếu chưa có dòng giá lịch sử phù hợp. Thiếu đơn giá hoặc đơn có nhiều loại xe mà chưa xác định được một dòng giá sẽ báo lỗi, không tự chọn giá bất kỳ. Rollback bị chặn nếu có phụ lục không có TariffRuleId; rollback thành công sẽ xóa hai cột mới nên cần lưu dữ liệu trước. Kiểm tra cục bộ: `dotnet run --project tools/ContractPricingSelfCheck -c Release -- --appendix-only` (tools bị ignore).

### Cập nhật thao tác và bộ lọc ngày — 07/11/2026

Menu nhân viên/khách hàng/hợp đồng/phụ lục ghi rõ đối tượng: **Sửa nhân viên**, **Sửa khách hàng**, **Sửa hợp đồng/Xóa hợp đồng**, **Sửa phụ lục/Xóa phụ lục**. Người liên hệ bỏ nút Sửa/Xóa, chuyển sang menu chuột phải theo dòng; giữ quyền Admin/Điều phối và xác nhận xóa.

Hợp đồng có tìm kiếm mã hợp đồng/tên khách/mã bảng giá, lọc loại và trạng thái, cùng khoảng **Từ/Đến** cho từng mốc ngày bắt đầu và ngày kết thúc. Bỏ cột Chi tiết giá khỏi danh sách. Phụ lục có tìm kiếm mã hợp đồng/phụ lục/bảng giá và khoảng **Từ/Đến** cho ngày lập/ngày hiệu lực. Mỗi ô ngày có checkbox: bỏ chọn là không lọc, biên ngày được tính cả hai đầu; các điều kiện kết hợp đồng thời. Khi lọc ngày kết thúc, hợp đồng không thời hạn không khớp. Khoảng Từ lớn hơn Đến không có kết quả; **Xóa lọc** khôi phục toàn bộ danh sách.

Lập phụ lục mới không chọn ngày lập/ngày hiệu lực trước hôm nay theo giờ Việt Nam; server kiểm tra lại khi lưu. Khi sửa phụ lục lịch sử, ngày cũ được giữ nếu không đổi, nhưng không được đổi sang một ngày quá khứ khác. Hạn thanh toán hóa đơn không chọn ngày quá khứ; server tiếp tục chặn như trước. Phụ phí cho phép từng tỷ lệ **0–100%**, tối đa 2 số lẻ, gồm cả hai tỷ lệ bằng 0; giữ nguyên quy tắc ngày và khóa sau lưu.

Kiểm tra tập trung: `dotnet run --project tools/EmployeeCreateSelfCheck -c Release -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false -- --requested-changes`. Kiểm tra cấp mã đồng thời trong schema PostgreSQL tạm riêng: cùng lệnh với `--requested-postgres` (cần kết nối DB local); schema được tự xóa khi kết thúc. Không áp migration vận hành.

Danh sách **Người liên hệ** không còn ComboBox/nhãn Khách hàng; mở từ khách hàng nào thì giữ đúng phạm vi đó khi làm mới. Form thêm/sửa người liên hệ tiếp tục nhận khách hàng tương ứng. Kiểm tra: `dotnet run --project tools/EmployeeCreateSelfCheck -c Release --no-restore -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false -- --contacts-only`.

Màn **Hóa đơn** đã bỏ bốn thẻ Hóa đơn tháng này, Tổng doanh thu, Chưa thanh toán/Quá hạn và Đã thanh toán cùng tiêu đề thống kê tháng. Bảng nằm ngay sau thanh công cụ/bộ lọc/trạng thái; giữ lập hóa đơn, xem chi tiết, đánh dấu thanh toán và các bộ lọc. Kiểm tra cục bộ: `dotnet run --project tools/InvoiceStatusUiSelfCheck -c Release --no-restore -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false`.

Màn **Phụ phí** và **Bảng giá** có ô tìm theo mã (`PP-…` / `BG-…`) và khoảng ngày hiệu lực **Từ/Đến** tùy chọn. Bỏ chọn checkbox để không lọc mốc đó; hai biên tính cả ngày và kết hợp với mã tìm kiếm. **Xóa lọc** khôi phục danh sách, **Làm mới** giữ bộ lọc. Dùng `FilterBar` để lọc dữ liệu đã tải, không đổi API/schema. Kiểm tra: `dotnet run --project tools/ContractPricingSelfCheck -c Release --no-restore -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false -- --list-filters-only`.

Thanh tìm kiếm/bộ lọc **Bảng giá** nằm trên nền chung bên ngoài khung trắng của bảng, đồng bộ với các màn danh sách khác. Cửa sổ chi tiết không hiển thị thanh lọc danh sách hoặc chừa hàng trống.

Màn **Nhân viên** có ô tìm theo mã nhân viên, khoảng ngày tuyển dụng **Từ/Đến** tùy chọn và các bộ lọc **Phòng ban**, **Chức danh**, **Trạng thái** (Đang làm/Nghỉ phép/Đã nghỉ việc). Thanh lọc nằm ngoài bảng, dùng nền chung; các điều kiện kết hợp đồng thời, tính cả hai ngày biên. **Xóa lọc** phục hồi danh sách; **Làm mới** giữ bộ lọc và nhân viên đang chọn nếu vẫn khớp. Phòng ban/chức danh lấy từ toàn bộ dữ liệu đã tải, `—` đại diện chưa phân công. Kiểm tra: `dotnet run --project tools/EmployeeCreateSelfCheck -c Release --no-restore -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false -- --employee-filters-only`.

Màn **Bảng lương** có tìm kiếm mã/tên nhân viên (không phân biệt hoa/thường, dấu), kết hợp trạng thái chi trả **Chưa trả / Trả một phần / Đã trả đủ / Chưa có phiếu**. Lọc cục bộ trên dữ liệu của tháng, giữ bộ lọc khi tải lại hoặc đổi tháng; tổng tiền và thao tác tính/chốt vẫn áp dụng cả kỳ. **Cấu hình KPI cho tài xế** bổ sung chú thích km/K/H và luôn hiển thị 9 loại xe chuẩn dùng chung với bảng giá, đồng thời giữ loại xe bổ sung/hệ số đã lưu. Các loại chưa cấu hình H cần nhập hệ số dương trước khi lưu; không tự gán hệ số hay sửa công thức tính lương. Kiểm tra: `dotnet run --project tools/ContractPricingSelfCheck -c Release --no-restore -p:UseSharedCompilation=false -p:BuildInParallel=false -p:NuGetAudit=false -- --payroll-filters-kpi-only`.

Danh sách **Nhân viên** mặc định sắp xếp mã nhân viên tăng dần khi mở, lọc hoặc làm mới; giữ nhân viên đang chọn nếu còn trong kết quả.

Danh sách nhân viên trong **Bảng lương** mặc định sắp theo **mã nhân viên tăng dần**, kể cả sau khi lọc, tải lại hoặc đổi tháng; giữ nguyên nhân viên đang chọn nếu còn hiển thị.


### Hóa đơn: hai trạng thái và chi tiết mở rộng

Danh sách, bộ lọc, API và Domain chỉ dùng **Chưa thanh toán** (`Unpaid=0`) / **Đã thanh toán** (`Paid=3`). Theo lựa chọn của người dùng, các hóa đơn cũ Nháp, Đã phát hành, Thanh toán một phần, Quá hạn và **Đã hủy** đều chuyển thành Chưa thanh toán; giữ nguyên tiền, dòng phí và giao dịch thu tiền. Có giao dịch một phần vẫn là Chưa thanh toán cho tới khi được đánh dấu đã thanh toán. Ngày quá hạn không tạo trạng thái thứ ba.

Áp migration **20261112000000_SimplifyInvoiceStatuses** trước khi chạy API mới, cập nhật API/Desktop cùng đợt. Đây là migration dữ liệu theo pattern migration gộp trạng thái hiện có: chỉ đổi `Invoices.Status` ngoài 0/3 thành 0; không đổi schema, không xóa hóa đơn, không tính lại tiền. Giữ số Paid=3 để bảo toàn dữ liệu đã thanh toán. Rollback không thể khôi phục từng trạng thái cũ đã gộp. Hóa đơn từng hủy nay được tính lại trong báo cáo doanh thu/công nợ; nếu một đơn đã có hóa đơn thay thế thì cả hai bản đều được giữ và cộng vào báo cáo theo yêu cầu chuyển tất cả. Đơn đã có bất kỳ hóa đơn nào không được lập thêm hóa đơn mới. Migration này chưa được áp tự động vào database trong lượt sửa mã.

**Xem chi tiết** (nhấp đúp hoặc menu chuột phải) hiển thị mã hóa đơn/đơn vận chuyển, trạng thái, ngày lập theo giờ Việt Nam, hạn thanh toán, khách hàng/mã số thuế/điện thoại/địa chỉ, hợp đồng và điểm nhận/giao. Tab **Chi tiết khoản phí** giữ các dòng đã chốt lúc lập hóa đơn; tab **Lịch sử thu tiền** hiển thị mã giao dịch, thời gian, phương thức và số tiền đã ghi nhận. Thông tin khách hàng/đơn lấy từ hồ sơ hiện tại, không giả định là bản chụp lịch sử. Khi thiếu dữ liệu, hiển thị thông báo rõ; không tự tính lại cước hoặc tạo giao dịch khi đánh dấu Paid.

Self-check cục bộ trong `tools/InvoiceFeeSelfCheck` và `tools/InvoiceStatusUiSelfCheck` đã cập nhật nghiệp vụ/trạng thái/DTO/UI/migration offline. Lượt 2026-11-12 build hệ thống và hai harness thành công; Windows Application Control chặn thực thi assembly (0x800711C7), nên chưa có kết quả runtime/ảnh UI/SQL offline tự sinh hoặc kiểm tra PostgreSQL thật. Các seed SQL cục bộ trong `tools/DevSeed` cũng dùng 0/3; chưa chạy lại seed.


Kiểm tra sửa ngày hiệu lực phụ phí (2026-11-12): `tools/SurchargeSelfCheck` bổ sung POST tạo/PUT nháp/POST công bố bảng tương lai, giữ bảng hiện hành, trường hợp chỉ có bảng tương lai, ngày hiệu lực vừa đến và trạng thái trùng tỷ lệ tính phí. Build harness và hệ thống thành công; chạy harness bị Windows Application Control chặn 0x800711C7, nên chưa xác minh runtime/UI/DB thật. Chạy lại bằng `dotnet tools/SurchargeSelfCheck/bin/Release/net8.0/SurchargeSelfCheck.dll` trên môi trường cho phép.
