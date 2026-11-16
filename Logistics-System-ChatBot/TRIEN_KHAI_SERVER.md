# Triển khai chatbot lên máy chủ

Logic và API của chatbot nằm trong `TruckingSystem.API` (hệ thống chính). Gói `Logistics-System-ChatBot` chỉ chứa giao diện. Vì vậy, giống như app mobile, máy chủ **không phải chạy thêm process nào và không phải sửa Cloudflare Tunnel**: chỉ cần chạy bản API chính mới.

```
App chatbot ──HTTPS──> trucking.logisticssystem.dpdns.org ──tunnel──> TruckingSystem.API (8080) ──> PostgreSQL
                       /api/public/freight-chat                       (có sẵn endpoint chatbot)
```

## Bước 1: Cập nhật API chính

Cập nhật như mọi lần sửa code API: chuột phải `Logistics-System-Desktop/installer/api_service_install.bat` → **Run as administrator**. Script sẽ dừng dịch vụ `TruckingSystemAPI`, xuất bản bản mới vào `C:\TruckingSystem\API` rồi chạy lại. Cấu hình hiện có được giữ nguyên.

## Bước 2: Bổ sung cấu hình AI

Thêm vào cấu hình của API chính trên máy chủ (`appsettings.json`, hoặc `C:\TruckingSystem\appsettings.Production.json` nếu đang chạy dạng dịch vụ):

```json
"FreightChat": {
  "Provider": "OpenAICompatible",
  "BaseUrl": "https://integrate.api.nvidia.com/v1/",
  "Model": "nvidia/nemotron-3.5-lightning-30b-a3b",
  "ApiKey": "<khóa Nvidia>"
}
```

- Thiếu `FreightChat:BaseUrl`/`Model`/`ApiKey` thì chatbot dùng mục `Nvidia` có sẵn. Mục `Nvidia` mẫu đang là model đọc danh thiếp, nên ghi rõ `FreightChat:Model` như trên.
- `Gemini:ApiKey` **luôn cần**, vì embedding bảng giá luôn dùng Gemini.
- `VietMap:RouteApiKey` cần để tính quãng đường khi báo giá. API chính đã có sẵn.
- Không commit khóa vào mã nguồn.

## Bước 3: Database

- Migration `20261102160000_AddFreightVectors` (bảng `FreightVectors`, extension `vector`) phải được áp dụng. API chính sẽ báo lỗi 503 kèm danh sách migration còn thiếu nếu chưa áp dụng.
- PostgreSQL trên máy chủ phải cài **pgvector**.
- Lần đầu chạy, worker sẽ index bảng giá vào `FreightVectors`. Log có trong console/dịch vụ.

## Bước 4: Cấu hình app chatbot

`chatbot.settings.json` cạnh `TruckingChatBot.exe`:

```json
{ "ServerUrl": "https://trucking.logisticssystem.dpdns.org" }
```

Sửa xong, mở lại app là có hiệu lực, không cần cài lại.

## Bước 5: Kiểm tra từ ngoài internet

```bash
curl -s -o /dev/null -w "%{http_code}\n" -X POST -H "Content-Type: application/json" -d "{\"message\":\"hi\"}" https://trucking.logisticssystem.dpdns.org/api/public/freight-chat
```

| Mã | Ý nghĩa | Cách xử lý |
|---|---|---|
| `200` | Thông suốt | Mở app chatbot và hỏi thử |
| `404` | Máy chủ còn chạy bản API cũ (chưa có endpoint chatbot) | Làm lại Bước 1 |
| `503` | API chạy nhưng AI/DB lỗi, hoặc còn migration chưa áp dụng | Xem log API, kiểm tra Bước 2 và 3 |
| `400` | Đã tới endpoint, body sai | Bình thường khi gửi `message` rỗng |
| `429` | Vượt 10 lượt/phút của một khách | Chờ 1 phút |

## Ghi chú

- Rate limit tính **theo từng khách**: request đến từ cloudflared (`localhost`) được lấy IP thật trong header `CF-Connecting-IP`.
- Endpoint chatbot ẩn danh, chỉ đọc bảng giá/phụ phí/kho và chỉ ghi vào bảng `FreightVectors`. Không đụng dữ liệu nghiệp vụ khác.
