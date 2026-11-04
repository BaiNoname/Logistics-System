# Hướng dẫn phong cách giao diện — TruckingSystem.Desktop

Tài liệu này dành cho người và AI khi tạo hoặc sửa màn hình WinForms trong `src/TruckingSystem.Desktop`.
Mọi giá trị dưới đây lấy từ code hiện có (`Common/Theme.cs`, `Common/ButtonStyles.cs`, `Common/UiStyle.cs`…).
**Luôn dùng các hằng số và helper có sẵn, không tự đặt mã màu hay font mới.**

Bản thiết kế mẫu (form Tạo đơn vận chuyển): https://claude.ai/artifact/CvH22MRz26f3hbp1pDUybq

---

## 1. Nguyên tắc chung

1. **Một việc chính mỗi cửa sổ.** Chỉ có 1 nút Primary (màu xanh), đặt góc dưới bên phải.
2. **Không tọa độ cứng.** Bố cục bằng `TableLayoutPanel` / `FlowLayoutPanel`, `Dock`, `Anchor`, `AutoSize`.
   Khoảng cách tính qua `LogicalToDeviceUnits(...)` để đúng trên màn hình DPI cao.
3. **Báo lỗi ngay tại ô sai**, không chỉ một dòng chữ ở cuối form.
4. **Chữ đầy đủ, không viết tắt** ("Khối lượng (kg)" chứ không "KL"). Giải thích thêm bằng tooltip.
5. **Mặc định an toàn:** không điền sẵn số liệu giả (100 kg, 1 m³…) mà người dùng có thể lưu nhầm.
6. Kế thừa `BaseForm` (cửa sổ) hoặc `BaseUserControl` (màn trong khung chính) để tự nhận font và kiểu chung.

---

## 2. Màu sắc (`Theme`)

| Vai trò | Hằng số | Mã màu | Dùng cho |
|---|---|---|---|
| Chính | `Theme.Primary` | `#2563EB` | Nút chính, tiêu đề khối, mục menu đang chọn |
| Chính đậm | `Theme.PrimaryDark` | `#1D4ED8` | Hover nút chính |
| Chữ chính | `Theme.TextPrimary` | `#0F172A` | Nội dung, nhãn |
| Chữ phụ | `Theme.TextSecondary` | `#64748B` | Gợi ý, địa chỉ phụ, chú thích |
| Viền | `Theme.BorderColor` | `#E2E8F0` | Viền nút phụ, đường kẻ, lưới |
| Nền nội dung | `Theme.ContentBg` | `#F1F5F9` | Nền màn trong khung chính |
| Nền thẻ / form | `Theme.CardBg` | `#FFFFFF` | Thẻ, cửa sổ phụ |
| Dòng xen kẽ / chân form | `Theme.GridAltRow` | `#F8FAFC` | Dòng chẵn của bảng, nền footer, hover nút phụ |
| Tiêu đề bảng | `Theme.GridHeaderBg` | `#1E293B` | Nền tiêu đề cột (chữ trắng) |
| Thành công | `Theme.Success` | `#16A34A` | Trạng thái hoàn tất |
| Nguy hiểm / lỗi | `Theme.Danger` | `#DC2626` | Lỗi, nút xóa / hủy, dấu `*` bắt buộc |
| Cảnh báo | `Theme.Warning` | `#D97706` | Cảnh báo không chặn lưu |
| Sidebar | `Theme.SidebarBg` / `SidebarText` | `#1E293B` / `#CBD5E1` | Menu trái |

Màu phụ đang dùng cho **khung thông tin xanh nhạt** (tóm tắt, "Việc tiếp theo"):
nền `#EFF6FF` (239,246,255), chữ `#1E40AF` (30,64,175).

Quy tắc:
- **Đỏ = lỗi chặn lưu.** **Cam = cảnh báo** (vẫn lưu được). Đừng dùng chung một nhãn cho cả hai mà quên đổi màu lại.
- Không dùng `Color.Red`, `Color.Orange`… trực tiếp; luôn dùng `Theme.*`.

---

## 3. Chữ (`Theme.Font*`)

Font duy nhất: **Segoe UI**.

| Hằng số | Cỡ | Dùng cho |
|---|---|---|
| `Theme.FontTitle` | 18 đậm | Tiêu đề màn hình |
| `Theme.FontSectionTitle` | 13 đậm | Tiêu đề nhóm trên màn lớn |
| (tiêu đề khối trong form) | 11 đậm, `Theme.Primary` | "① Khách hàng và hợp đồng"… |
| `Theme.FontNormal` | 10 | Nội dung, nhãn, ô nhập (mặc định của `BaseForm`) |
| `Theme.FontBold` | 10 đậm | Nút, nhãn nhấn mạnh |
| `Theme.FontSmall` | 9 | Gợi ý, chú thích dưới ô |
| `Theme.FontKpiNumber` | 26 đậm | Số lớn trên thẻ KPI |

---

## 4. Nút (`ButtonStyles`)

| Kiểu | Hàm | Hình thức | Khi nào |
|---|---|---|---|
| Primary | `ButtonStyles.Primary(btn)` | Nền xanh đặc, chữ trắng | Việc chính — **chỉ 1 nút / cửa sổ** |
| Secondary | `ButtonStyles.Secondary(btn)` | Nền trắng, viền xám | Các nút khác: Đóng, Thêm dòng, Địa điểm mới… |
| DangerLink | `ButtonStyles.DangerLink(btn)` | Chữ đỏ, không nền | Xóa, Hủy đơn, Hoàn tác — đặt **góc dưới trái**, xa nút chính |

- Cao tối thiểu **34 px** (nút trong form) / **38 px** (nút chân form). Nút chính rộng tối thiểu 140 px.
- Chữ nút là **động từ + đối tượng**: "Lưu đơn", "Thêm dòng hàng", "Xóa dòng đã chọn".
- Biểu tượng: được thêm **1 emoji ở đầu** chữ nút (💾 Lưu, ➕ Thêm, 🗑 Xóa). Không đặt emoji trong nhãn ô nhập.
- Thanh công cụ nhiều nút: gọi `Theme.UniformButtons(bar)` trong `Load` để nút đều nhau.

### Chân cửa sổ (footer)

```
[DangerLink (nếu có)] .................................. [Đóng] [💾 Nút chính]
```

```csharp
var close = new Button { Text = "Đóng", AutoSize = true, DialogResult = DialogResult.Cancel };
ButtonStyles.Secondary(close);
ButtonStyles.Primary(btnSave);
ButtonStyles.Footer(this, danger: null, close, btnSave);
CancelButton = close;
```

Footer có nền `GridAltRow`, đường kẻ trên màu `BorderColor`, padding 14/8.

---

## 5. Bố cục cửa sổ nhập liệu

Khung chuẩn của một form tạo / sửa:

```
┌ Tiêu đề cửa sổ ─────────────────────────────────────────┐
│ ① Tên khối 1                                            │
│   Nhãn *            Nhãn                                │
│   [ ô nhập      ]   [ ô nhập      ]                     │
│ ───────────────────────────────────────────────────────  │
│ ② Tên khối 2                                            │
│   ...                                                   │
│ ③ Bảng chi tiết                [➕ Thêm] [🗑 Xóa]        │
│   ┌ bảng (tiêu đề nền tối) ──────────────────────────┐  │
│   └──────────────────────────────────────────────────┘  │
│   [ Dải tóm tắt xanh nhạt ]                              │
├─────────────────────────────────────────────────────────┤
│ * Trường bắt buộc                     [Đóng] [💾 Lưu]   │
└─────────────────────────────────────────────────────────┘
```

Quy tắc:
- **Chia khối đánh số** ① ② ③ khi form có trên ~6 trường. Tiêu đề khối: 11 đậm, `Theme.Primary`, margin trên 10 / dưới 6.
- **Padding cửa sổ:** 20 ngang, 16 dọc. **Khoảng cách giữa các ô:** 4 trên/dưới, nhãn cách ô 12.
- **Ô nhập rộng kín cột** (`Dock = Fill` hoặc `Anchor = Left | Right`).
- Hai trường ngắn, liên quan nhau (Khách hàng | Hợp đồng, Điểm lấy | Điểm giao) có thể xếp **2 cột** để form không phải cuộn.
- Phần nhập liệu nên `AutoSize`; phần bảng lấy chỗ còn lại (`Percent`) và giữ tối thiểu tiêu đề + 3 dòng.
- Luôn gọi ngay sau `InitializeComponent()`:
  ```csharp
  DialogFit.Enable(this, root); // vừa màn hình, bảng không bị ép
  DialogKeys.Enable(this);      // Esc = đóng (form ngắn: DialogKeys.Enable(this, btnSave) để Enter = Lưu)
  Anim.FadeIn(this);            // hiện dần
  ```
- `StartPosition = CenterParent`, `MinimumSize` hợp lý (vd 760×500).

### Nhãn và trường bắt buộc
- Có 2 kiểu đặt nhãn, mỗi form chỉ dùng **một** kiểu:
  - **Nhãn bên trái** (bảng 2 cột nhãn | ô): `FontNormal`, `TextPrimary`, có dấu hai chấm — `Tên trường *:`.
  - **Nhãn bên trên** (`Theme.MakeLabeledInput`): `FontSmall`, `TextSecondary`, không dấu hai chấm — `Tên trường *`.
- Dấu `*` luôn đứng ngay sau tên trường.
- Mọi trường bắt buộc đều có `*` (kể cả Khách hàng). Footer ghi chú "* Trường bắt buộc".
- Ô chỉ để hiển thị (địa chỉ tự điền…): **dòng chữ phụ** (`FontSmall`, `TextSecondary`) dưới ô chọn,
  không dùng TextBox chỉ-đọc. Nếu buộc dùng TextBox `ReadOnly` thì đặt `TabStop = false`.
- Ô người dùng phải chọn đứng **trước** thông tin phụ của nó.

### Thứ tự Tab
- `TabIndex` phải đi **theo thứ tự nhìn**: trái → phải, trên → dưới. Kiểm tra lại mỗi khi chèn / đổi chỗ trường.

---

## 6. Ô nhập

| Loại dữ liệu | Control | Ghi chú |
|---|---|---|
| Chọn từ danh sách | `ComboBox` `DropDownList` | Danh sách dài (khách hàng…) nên bật tìm kiếm / gợi ý |
| Số lượng, kg, m³ | `NumberBox` | Không dùng `NumericUpDown` (hiện "500,00" dễ đọc nhầm) |
| Tiền | `NumberBox` với `IsMoney = true` | Gõ tắt nghìn: 500 → 500.000 đ |
| Ngày | `DateTimePicker` | `UiStyle` tự đổi về `dd/MM/yyyy`. Đặt `MinDate` thay vì báo lỗi sau |
| Chọn 1 trong ít lựa chọn | `RadioButton` trong `FlowLayoutPanel` | Lựa chọn mặc định phải được chọn sẵn |
| Tìm địa điểm | `PlaceSearchBinder` | Địa điểm phải có tọa độ |

Định dạng số hiển thị: `#,##0` (số nguyên), `#,##0.##` (kg, số lượng), `#,##0.####` (m³). Ngày: `dd/MM/yyyy`.

---

## 7. Bảng (DataGridView)

- **Bảng chỉ xem:** `Theme.StyleGrid(grid)` — chỉ đọc, chọn cả dòng, không có dòng trống cuối.
- **Bảng nhập liệu:** chỉ `Theme.StyleGridLook(grid)` (`UiStyle` tự áp phần nhìn), và:
  - `AllowUserToAddRows = false` — thêm dòng bằng **nút "➕ Thêm dòng"**, không dùng dòng trống cuối bảng.
  - Bắt `DataError` để không hiện hộp thoại lỗi mặc định khi gõ sai kiểu:
    ```csharp
    grid.DataError += (_, e) => { e.ThrowException = false; FieldHint.Show(grid, lblStatus, "Ô này phải là số."); };
    ```
  - Ô checkbox chốt ngay khi tích (`CurrentCellDirtyStateChanged` → `CommitEdit`).
  - Kiểm tra từng dòng trước khi lưu (loại hàng không rỗng, số > 0).
- Tiêu đề cột nền `#1E293B`, chữ trắng đậm; dòng xen kẽ `#F8FAFC`; số căn phải.
- `GridLayout.FitColumns(grid)` để cột vừa nội dung; màn nhiều bảng xếp chồng dùng `GridLayout.GrowWithRows`.
- Cột cần giải thích: dùng `ToolTipText` trên tiêu đề cột.

---

## 8. Phản hồi cho người dùng

| Tình huống | Cách làm |
|---|---|
| Ô bắt buộc trống / sai khi bấm Lưu | `FieldHint.Show(field, lblStatus, "Vui lòng …"); return;` — tô đỏ, nhảy vào ô, ghi lý do |
| Cảnh báo không chặn (kho thiếu khu, hạn giao sát…) | Dòng chữ cam (`Theme.Warning`) **ngay dưới ô liên quan** |
| Gợi ý / ước tính | Dòng chữ phụ (`FontSmall`, `TextSecondary`) dưới ô |
| Lưu thành công | `Toast.Show(this, "✅ Đã lưu đơn #123");` — không dùng MessageBox "Thành công" |
| Lỗi hệ thống / câu hỏi xác nhận | Hộp thoại (`Ask` / `MessageBox`) |
| Việc chờ xử lý ở đầu màn | `PendingBanner` (nền vàng nhạt) |
| Bước tiếp theo trong màn chi tiết | `ButtonStyles.NextStep()` (khung xanh nhạt) |
| Tóm tắt dữ liệu đang nhập | Dải xanh nhạt `#EFF6FF` / chữ `#1E40AF`, padding 10/6, các mục ngăn bằng " · " |

Quy tắc cho `lblStatus`:
- Mặc định đỏ (`Theme.Danger`). Nếu có lúc đổi sang cam thì **luôn đặt lại màu** trước khi hiện lỗi mới.
- Xóa thông báo cũ khi người dùng đã sửa xong.
- Câu thông báo: nói **chuyện gì sai + cách sửa**. Ví dụ:
  "Vui lòng chọn Địa điểm giao hàng (chưa có thì bấm "＋ Địa điểm mới")."

Thứ tự kiểm tra khi Lưu phải **giống thứ tự trường trên màn hình**, để lỗi đầu tiên là ô gần trên nhất.

### Helper `FormStyle` (`Common/FormStyle.cs`)

| Hàm | Làm gì |
|---|---|
| `FormStyle.SaveButton(btn)` | Nút chính: `ButtonStyles.Primary` + thêm "💾 " ở đầu chữ |
| `FormStyle.CloseButton(form, btn)` / `NewCloseButton(form)` | Nút phụ "Đóng", gắn Esc (`CancelButton`) |
| `FormStyle.Status(lbl, text, StatusKind.X)` | Ghi dòng trạng thái và **đặt lại đúng màu**: `Error` đỏ, `Warning` cam, `Info` xám, `Success` xanh lá |
| `FormStyle.Section(lbl…)` | Tiêu đề khối 11 đậm, màu Primary |
| `FormStyle.FieldLabels(lbl…)` | Nhãn bên trái màu chữ chính |
| `FormStyle.NoTab(ctrl…)` | Ô chỉ để xem không nhận Tab |

Ô `TextBox` nhiều dòng trong form có `AcceptButton` phải đặt `AcceptsReturn = true`, nếu không Enter sẽ bấm Lưu.

---

## 9. Văn phong

- Tiếng Việt có dấu, câu ngắn, xưng hô trung tính.
- Viết hoa chữ đầu nhãn, không viết hoa toàn bộ (trừ nhấn mạnh rất hiếm).
- Dùng đúng thuật ngữ nghiệp vụ đã có: *đơn vận chuyển, chia xe, kho trung chuyển, hạn giao cam kết, dòng hàng*.
- Trạng thái đơn hiển thị tiếng Việt (Nháp, Chờ xác nhận…), không hiện mã (`Draft`).

---

## 10. Checklist trước khi xong một màn hình

- [ ] Kế thừa `BaseForm` / `BaseUserControl`; không mã màu / font tự đặt
- [ ] Đúng 1 nút Primary; footer `[nguy hiểm] … [Đóng] [Nút chính]`; `CancelButton` = Đóng
- [ ] Không tọa độ cứng; thử thu nhỏ cửa sổ tới `MinimumSize` và DPI 125–150%
- [ ] Mọi trường bắt buộc có `*`; nhãn cùng định dạng
- [ ] Tab đi đúng thứ tự nhìn; ô chỉ-đọc không nhận Tab
- [ ] Lỗi dùng `FieldHint`; cảnh báo màu cam đặt cạnh ô; thành công dùng `Toast`
- [ ] Bảng nhập liệu: không có dòng trống cuối, có `DataError`, kiểm tra từng dòng
- [ ] Không có số liệu mẫu điền sẵn
- [ ] Ngày `dd/MM/yyyy`, số theo `NumberBox` / định dạng ở mục 6
