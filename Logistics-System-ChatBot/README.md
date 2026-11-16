# LogisticsSystem.ChatBot — giao diện chatbot tra cước (gói riêng)

Gói này chỉ chứa **phần giao diện** của chatbot tra cước công khai: app khung chat `TruckingChatBot.exe` và bộ cài. Toàn bộ logic và API (use case `FreightChat`, AI/vector store, quy tắc tính cước báo giá, endpoint `POST /api/public/freight-chat`) nằm trong hệ thống chính `Logistics-System-Desktop`, chạy chung với `TruckingSystem.API`. Vì vậy app chatbot chỉ cần trỏ tới domain của API chính là dùng chung database, máy chủ không phải chạy thêm gì.

| Project | Vai trò |
|---|---|
| `src/TruckingSystem.ChatBot.Desktop` | App khung chat `TruckingChatBot.exe`. Đọc `ServerUrl` trong `chatbot.settings.json` cạnh exe: chỉ ghi domain (vd. `https://trucking.logisticssystem.dpdns.org`) thì gọi `/api/public/freight-chat` trên đó; ghi URL có đường dẫn thì gọi đúng nguyên URL. Thiếu file/sai URL thì dùng `https://localhost:8080`. |
| `TruckingSystem.ChatBot.Setup` | Bộ cài (Visual Studio Installer Project), đóng gói cả `chatbot.settings.json` |

## Phần nằm ở hệ thống chính (Logistics-System-Desktop)

| Tầng | File |
|---|---|
| Domain | `TruckingSystem.Domain/Entities/FreightQuotePricing.cs` |
| Application | `TruckingSystem.Application/FreightChat/*` (đăng ký trong `DependencyInjection.cs`) |
| Infrastructure | `TruckingSystem.Infrastructure/ExternalServices/Freight*.cs` (AI, vector store pgvector, worker index bảng giá, context DataProtection; đăng ký trong `AddInfrastructure`) |
| API | `Controllers/FreightChatController.cs`, `RateLimiting/FreightChatRateLimit.cs` (10 lượt/phút/khách; sau Cloudflare Tunnel lấy IP thật từ `CF-Connecting-IP`) |

Cấu hình AI (`FreightChat`, `Gemini`, `Nvidia`) đặt trong `appsettings.json` của `TruckingSystem.API`, xem mục "Freight chatbot" trong README hệ thống.

## Phụ thuộc

App chatbot link các file `Common` của `TruckingSystem.Desktop` (ApiConnection, Theme, UiStyle, Anim). Đường dẫn lấy từ thuộc tính MSBuild `TruckingSystemSrc` trong `Directory.Build.props`, mặc định `..\Logistics-System-Desktop\src\`. Nếu đặt chỗ khác:

```bash
dotnet build LogisticsSystem.ChatBot.sln -p:TruckingSystemSrc=D:/duong-dan/Logistics-System/Logistics-System-Desktop/src/
```

## Chạy

Chạy `TruckingSystem.API` (solution `Logistics-System-Desktop/TruckingSystem.sln`), rồi:

```bash
dotnet run --project src/TruckingSystem.ChatBot.Desktop
```

## Đóng gói

```bash
dotnet publish src/TruckingSystem.ChatBot.Desktop -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/ChatBot
```

Sửa `ServerUrl` trong `publish/ChatBot/chatbot.settings.json` thành domain của API chính trên máy chủ thật. Triển khai máy chủ: xem `TRIEN_KHAI_SERVER.md`.
