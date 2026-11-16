# Triển khai ChatBot API lên máy chủ

Mục tiêu: app chatbot (`TruckingChatBot.exe`) ở máy bất kỳ gọi qua internet tới máy chủ và **dùng chung database** với hệ thống quản lý vận tải.

## Vì sao phải chạy thêm một process

- App chatbot không đọc database trực tiếp. App gửi câu hỏi tới **TruckingSystem.ChatBot.API**, rồi ChatBot API đọc bảng giá/phụ phí trong database và gọi AI.
- Database chỉ mở trong máy chủ, nên ChatBot API phải chạy **trên chính máy chủ**, song song với API chính (`TruckingSystem.API`, cổng 8080).
- Domain `https://trucking.logisticssystem.dpdns.org` hiện chỉ dẫn về API chính. API chính không có endpoint chatbot, nên gọi vào đó sẽ nhận **404**.

```
App chatbot ──HTTPS──> Cloudflare ──tunnel──> cloudflared (máy chủ)
                                               ├─ /api/...                 -> TruckingSystem.API          (8080)
                                               └─ /api/public/freight-chat -> TruckingSystem.ChatBot.API  (8091)
                                                                                   │
                                                                     PostgreSQL trucking_system (dùng chung)
```

## Điều kiện có sẵn trên máy chủ

- API chính đang chạy và **đã migrate database**. ChatBot API không tự migrate: bảng `FreightVectors` và extension `vector` đến từ migration `AddFreightVectors` của API chính.
- PostgreSQL có cài **pgvector**. Nếu API chính đã migrate thành công tới `AddFreightVectors` thì đã có.
- .NET 8 Runtime (ASP.NET Core). Đã có nếu API chính chạy được.
- `cloudflared` đang chạy tunnel cho domain.

## Bước 1: Xuất bản ChatBot API

Chạy từ thư mục repo `Logistics-System` (hai thư mục `Logistics-System-ChatBot` và `Logistics-System-Desktop` phải nằm cạnh nhau):

```bash
dotnet publish Logistics-System-ChatBot/src/TruckingSystem.ChatBot.API -c Release -r win-x64 --self-contained false -o C:/TruckingSystem/ChatBotApi
```

## Bước 2: Tạo file cấu hình

Copy `Logistics-System-ChatBot/src/TruckingSystem.ChatBot.API/appsettings.json.example` thành `C:\TruckingSystem\ChatBotApi\appsettings.json`, rồi điền:

| Mục | Giá trị |
|---|---|
| `ConnectionStrings:DefaultConnection` | Giống hệt chuỗi kết nối của API chính (`Host=localhost;Port=5432;Database=trucking_system;...`). Đây là chỗ làm cho hai bên dùng chung database. |
| `FreightChat:Provider` | `OpenAICompatible` (Nvidia) hoặc `Gemini` |
| `Nvidia:ApiKey` | Khóa Nvidia, khi Provider là `OpenAICompatible` |
| `Gemini:ApiKey` | Khóa Gemini. **Luôn cần**, vì embedding bảng giá luôn dùng Gemini. |
| `VietMap:RouteApiKey`, `VietMap:MapApiKey` | Khóa VietMap, dùng để tính quãng đường khi báo giá |

> `appsettings.json` chứa khóa và mật khẩu. File này đã nằm trong `.gitignore`: không commit, không gửi đi.

## Bước 3: Chạy thử bằng tay

```bash
C:/TruckingSystem/ChatBotApi/TruckingSystem.ChatBot.API.exe --urls http://localhost:8091 --contentRoot C:/TruckingSystem/ChatBotApi
```

- Chỉ nghe **http nội bộ**, vì Cloudflare đã lo HTTPS. Không cần mở firewall: tunnel kết nối qua `localhost`.
- Không thêm cổng https. Nếu nghe cả https, `UseHttpsRedirection` sẽ trả 307 sang cổng https và request đi qua tunnel bị hỏng.
- Lần đầu chạy, worker sẽ index bảng giá vào `FreightVectors` (gọi Gemini embedding). Cửa sổ console có log.

Kiểm tra ngay trên máy chủ (mở cửa sổ khác):

```bash
curl -s -o /dev/null -w "%{http_code}\n" -X POST -H "Content-Type: application/json" -d "{\"message\":\"hi\"}" http://localhost:8091/api/public/freight-chat
```

Kết quả `200` là ChatBot API chạy đúng.

## Bước 4: Cho ChatBot API tự chạy khi bật máy

API chính chạy dưới dạng dịch vụ Windows, còn ChatBot API **chưa có hỗ trợ dịch vụ Windows**. Cách không cần sửa code là dùng Task Scheduler. Mở cmd **Run as administrator**:

```bash
schtasks /Create /TN "TruckingChatBotAPI" /SC ONSTART /RU SYSTEM /RL HIGHEST /F /TR "\"C:\TruckingSystem\ChatBotApi\TruckingSystem.ChatBot.API.exe\" --urls http://localhost:8091 --contentRoot C:\TruckingSystem\ChatBotApi"
```

```bash
schtasks /Run /TN "TruckingChatBotAPI"
```

- Bắt buộc có `--contentRoot`, vì tác vụ chạy từ `System32`. Thiếu tham số này, ChatBot API không tìm thấy `appsettings.json`.
- Dừng: `schtasks /End /TN "TruckingChatBotAPI"`. Gỡ: `schtasks /Delete /TN "TruckingChatBotAPI" /F`.

## Bước 5: Thêm route trên Cloudflare Tunnel

Vào Cloudflare **Zero Trust → Networks → Tunnels → (tunnel hiện tại) → Public Hostname → Add a public hostname**. Chọn **một** trong hai cách:

**Cách 1: Subdomain riêng (đơn giản nhất)**

| Trường | Giá trị |
|---|---|
| Subdomain | `chatbot` |
| Domain | `logisticssystem.dpdns.org` |
| Service | `HTTP` → `localhost:8091` |

App chatbot sẽ dùng: `https://chatbot.logisticssystem.dpdns.org`

**Cách 2: Giữ domain hiện tại, tách theo path**

| Trường | Giá trị |
|---|---|
| Subdomain / Domain | `trucking` / `logisticssystem.dpdns.org` |
| Path | `^/api/public/freight-chat` |
| Service | `HTTP` → `localhost:8091` |

Kéo rule này **lên trên** rule đang trỏ về `localhost:8080`, vì Cloudflare khớp từ trên xuống. App chatbot giữ nguyên `https://trucking.logisticssystem.dpdns.org`.

> Nếu tunnel được quản lý bằng file `config.yml` thay vì dashboard, thêm mục ingress tương ứng (`hostname` + `path` + `service: http://localhost:8091`) lên trước mục 8080, rồi khởi động lại `cloudflared`.

## Bước 6: Cấu hình app chatbot

Sửa `chatbot.settings.json` cạnh `TruckingChatBot.exe`, trong thư mục cài đặt hoặc trong `src/TruckingSystem.ChatBot.Desktop` trước khi build Setup:

```json
{ "ServerUrl": "https://chatbot.logisticssystem.dpdns.org" }
```

- Chỉ ghi domain: app tự gọi `{domain}/api/public/freight-chat`.
- Ghi URL có đường dẫn: app gọi đúng nguyên URL đó.
- Sửa xong, mở lại app là có hiệu lực, không cần cài lại.

## Bước 7: Kiểm tra từ ngoài internet

Chạy từ một máy khác:

```bash
curl -s -o /dev/null -w "%{http_code}\n" -X POST -H "Content-Type: application/json" -d "{\"message\":\"hi\"}" https://chatbot.logisticssystem.dpdns.org/api/public/freight-chat
```

| Mã | Ý nghĩa | Cách xử lý |
|---|---|---|
| `200` | Thông suốt | Mở app chatbot và hỏi thử |
| `404` | Tunnel chưa trỏ tới ChatBot API (vẫn vào API chính 8080) | Kiểm tra Bước 5 và thứ tự rule |
| `502` / `530` | Cloudflare không gọi được `localhost:8091` | ChatBot API chưa chạy (Bước 3/4), hoặc sai cổng |
| `503` | Đã tới ChatBot API nhưng AI hoặc DB lỗi | Xem log ChatBot API, kiểm tra khóa AI và chuỗi kết nối (Bước 2) |
| `400` | Đã tới ChatBot API, body sai | Bình thường khi gửi `message` rỗng |
| `429` | Vượt 10 lượt/phút của một khách | Chờ 1 phút |

## Cập nhật phiên bản mới

1. `schtasks /End /TN "TruckingChatBotAPI"`
2. Chạy lại lệnh `dotnet publish` ở Bước 1. **Cẩn thận:** nếu trong `src/TruckingSystem.ChatBot.API` của máy đang publish có sẵn một file `appsettings.json` (ví dụ máy dev), file đó sẽ được copy đè lên cấu hình ở `C:\TruckingSystem\ChatBotApi`. Nên sao lưu `appsettings.json` của máy chủ trước khi publish, rồi chép lại sau.
3. `schtasks /Run /TN "TruckingChatBotAPI"`

Khi hợp đồng response thay đổi, phải cập nhật ChatBot API và app chatbot **cùng đợt**.

## Ghi chú

- Rate limit 10 lượt/phút tính **theo từng khách**: khi request đến từ cloudflared (`localhost`), ChatBot API lấy IP thật trong header `CF-Connecting-IP`.
- ChatBot API không cần khóa JWT của API chính, và không ghi gì vào dữ liệu nghiệp vụ. Nó chỉ đọc bảng giá/phụ phí/kho, và chỉ ghi vào bảng `FreightVectors`.
