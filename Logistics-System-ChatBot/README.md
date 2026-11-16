# LogisticsSystem.ChatBot — chatbot tra cước (gói riêng)

Toàn bộ tính năng chatbot tra cước công khai, tách khỏi hệ thống quản lý vận tải. Nằm trong repo `Logistics-System`, cạnh `Logistics-System-Desktop`.

| Project | Vai trò |
|---|---|
| `src/TruckingSystem.ChatBot.Domain` | Quy tắc tính cước báo giá theo chặng (`FreightQuotePricing`) |
| `src/TruckingSystem.ChatBot.Application` | Use case hội thoại `FreightChatService`, guard câu trả lời, tài liệu index, interface AI/vector store; `AddFreightChat()` |
| `src/TruckingSystem.ChatBot.Infrastructure` | Gemini / OpenAI-compatible, vector store pgvector (`FreightVectors`), worker index bảng giá, context bảo vệ bằng DataProtection; `AddFreightChatInfrastructure()` |
| `src/TruckingSystem.ChatBot.API` | Host `POST /api/public/freight-chat` (ẩn danh, 10 lần/phút/khách; sau Cloudflare Tunnel lấy IP thật từ `CF-Connecting-IP`), mặc định `https://localhost:8090` |
| `src/TruckingSystem.ChatBot.Desktop` | App khung chat `TruckingChatBot.exe`, đọc `ServerUrl` trong `chatbot.settings.json` cạnh exe: chỉ ghi domain (vd. `https://localhost:8090`) thì gọi `/api/public/freight-chat` trên đó; ghi URL có đường dẫn thì gọi đúng nguyên URL |

## Phụ thuộc vào hệ thống (Logistics-System-Desktop)

Gói này dùng lại `TruckingSystem.Domain`, `TruckingSystem.Application`, `TruckingSystem.Infrastructure` (DbContext, bảng giá/phụ phí, VietMap) và link các file `Common` của `TruckingSystem.Desktop` (ApiConnection, Theme, UiStyle, Anim). Đường dẫn lấy từ thuộc tính MSBuild `TruckingSystemSrc` trong `Directory.Build.props`, mặc định `..\Logistics-System-Desktop\src\` (hai thư mục đặt cạnh nhau trong repo; chuyển cả repo đi thì không phải sửa gì). Nếu đặt chỗ khác:

```bash
dotnet build LogisticsSystem.ChatBot.sln -p:TruckingSystemSrc=D:/duong-dan/Logistics-System/Logistics-System-Desktop/src/
```

Namespace của các lớp đã chuyển giữ nguyên (`TruckingSystem.Application.FreightChat`, `TruckingSystem.Infrastructure.ExternalServices`, `TruckingSystem.Domain.Entities`) để không đổi hợp đồng API.

## Cấu hình

- ChatBot API dùng chung DB PostgreSQL với API chính (đọc bảng giá/phụ phí, ghi `FreightVectors`, cần extension pgvector). Phải migrate DB bằng `TruckingSystem.API` trước; ChatBot API không chạy migration hay DbSeeder.
- Chép `src/TruckingSystem.ChatBot.API/appsettings.json.example` thành `appsettings.json` (không commit), điền `ConnectionStrings:DefaultConnection`, `VietMap`, `FreightChat`/`Gemini`/`Nvidia`. Tham khảo thêm `appsettings.FreightChat.example.json` và mục "Freight chatbot" trong README hệ thống. Không đưa key vào mã nguồn.

## Chạy

```bash
dotnet run --project src/TruckingSystem.ChatBot.API
```

```bash
dotnet run --project src/TruckingSystem.ChatBot.Desktop
```

## Đóng gói

```bash
dotnet publish src/TruckingSystem.ChatBot.Desktop -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/ChatBot
```

```bash
dotnet publish src/TruckingSystem.ChatBot.API -c Release -o publish/ChatBotApi
```

Sửa `ServerUrl` trong `publish/ChatBot/chatbot.settings.json` thành địa chỉ ChatBot API trên máy chủ thật (domain, hoặc URL endpoint đầy đủ); đặt `appsettings.json` cạnh `publish/ChatBotApi`. Thư mục publish có thể mang theo `appsettings.json` cục bộ chứa key — không phát tán.
