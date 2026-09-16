# TruckingSystem Backend — Hướng dẫn mở & chạy trên Visual Studio 2022

## 1. Mở project
1. Cài **Visual Studio 2022 Community** (nếu chưa có), khi cài nhớ tick workload **"ASP.NET and web development"**.
2. Mở file `TruckingSystem.sln` bằng Visual Studio (double-click hoặc File → Open → Project/Solution).
3. Visual Studio sẽ tự động **restore NuGet packages** (EF Core, Npgsql, Identity, JWT, Swagger...) — cần có mạng, chờ vài phút lần đầu.

## 2. Cài PostgreSQL (nếu máy chưa có)
- Tải tại https://www.postgresql.org/download/windows/ — nhớ đặt mật khẩu cho user `postgres` lúc cài, ghi nhớ lại.
- Sau khi cài, tạo database rỗng tên `trucking_system` (dùng pgAdmin đi kèm, hoặc psql: `CREATE DATABASE trucking_system;`).

## 3. Cấu hình connection string
Mở `src/TruckingSystem.API/appsettings.json`, sửa dòng:
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=trucking_system;Username=postgres;Password=YOUR_PASSWORD_HERE"
```
Thay `YOUR_PASSWORD_HERE` bằng mật khẩu Postgres bạn đã đặt.

Cũng nên đổi `Jwt:Key` thành 1 chuỗi bí mật riêng của bạn (ít nhất 32 ký tự bất kỳ).

## 4. Tạo migration đầu tiên & tạo bảng trong database
Mở **Package Manager Console** trong Visual Studio (menu Tools → NuGet Package Manager → Package Manager Console), chọn **Default project: TruckingSystem.Infrastructure**, chạy lần lượt:

```powershell
Add-Migration InitialCreate -Project TruckingSystem.Infrastructure -StartupProject TruckingSystem.API
Update-Database -Project TruckingSystem.Infrastructure -StartupProject TruckingSystem.API
```

Lệnh đầu tạo file migration (soi được schema sinh ra), lệnh sau thực sự tạo toàn bộ ~40 bảng trong PostgreSQL.

> Nếu báo lỗi thiếu tool, cài thêm gói dòng lệnh EF (chạy trong Terminal thường, không phải Package Manager Console):
> ```
> dotnet tool install --global dotnet-ef
> ```

## 5. Chạy thử API
- Trong Visual Studio, đặt **TruckingSystem.API** làm Startup Project (chuột phải → Set as Startup Project).
- Bấm F5 (hoặc nút Play màu xanh). Trình duyệt tự mở `https://localhost:xxxx/swagger` — đây là giao diện test API trực quan.

## 6. Tạo tài khoản đăng nhập đầu tiên để test
Vì `/api/auth/login` cần tài khoản tồn tại sẵn trong `AspNetUsers`, nhưng chưa có màn hình đăng ký (Register) — cách nhanh nhất để test ngay:

**Cách A — Thêm tạm 1 API Register** (khuyên dùng, xem gợi ý bên dưới) để tự tạo tài khoản qua Swagger.

**Cách B — Seed dữ liệu thủ công**: thêm đoạn code seed vào `Program.cs` (sau `var app = builder.Build();`) để tự tạo 1 tài khoản admin lúc khởi động lần đầu — nếu bạn muốn, nhắn lại để mình viết đoạn seed này.

## 7. Test với app driver (React Native)
1. Chạy API (bước 5), copy URL đang chạy (vd `https://localhost:7123` khi debug local, hoặc dùng **ngrok** để expose ra internet cho điện thoại thật gọi vào — giống cách bạn từng làm với Odoo).
2. Trong app driver, màn hình Login nhập đúng Server URL (URL ngrok/IP), tài khoản/mật khẩu đã tạo ở bước 6.
3. Sau khi login thành công, nhập Trip ID (phải là 1 Id có thật trong bảng `Trips` — cần insert tay 1-2 dòng test qua pgAdmin hoặc Swagger để có dữ liệu thử).
4. Bấm Start Tracking — theo dõi bảng `Waypoints` trong pgAdmin để xác nhận GPS ping có ghi vào DB không.

## Những endpoint đã có sẵn
| Method | Endpoint | Vai trò |
|---|---|---|
| POST | `/api/auth/login` | Đăng nhập, trả về JWT token |
| POST | `/api/trips/{id}/ping` | Ghi GPS ping (cần Bearer token) |
| POST | `/api/trips/{id}/confirm-arrival` | Xác nhận giao hàng xong, tự tính tiến độ đơn (cần Bearer token) |

## Việc còn thiếu (chưa làm ở bước này, sẽ bổ sung dần)
- API Register tài khoản + gán Role
- Controller cho `TransportOrders`, `Vehicles`, thuật toán chia xe (`VehicleAllocationService`)
- SignalR Hub cho Live Fleet Tracking
- Seed dữ liệu mẫu (Departments, Positions, Permissions mặc định...)
