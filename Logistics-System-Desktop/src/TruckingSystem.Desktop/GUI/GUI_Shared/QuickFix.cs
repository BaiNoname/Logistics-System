using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.GUI_DriverManagement;
using TruckingSystem.Desktop.GUI_TripManagement;
using TruckingSystem.Desktop.GUI_VehicleManagement;
using TruckingSystem.Desktop.GUI_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>SỬA NHANH NGAY TẠI CHỖ BÁO LỖI (mẫu đã chốt): thao tác bị chặn thì thay dòng báo cam tự tắt bằng hộp nhỏ
    /// ghi lý do + các nút dẫn THẲNG tới chỗ sửa (đã điền sẵn đúng tài xế / xe / ngày / kho — máy chủ gửi kèm "cách sửa",
    /// xem FixHintItem). Sửa xong hỏi làm lại thao tác vừa bị chặn ("▶️ Bắt đầu chuyến #12 ngay", "🚚 Chia xe lại ngay").
    /// Không có cách sửa nào thì báo như cũ (Ask.Fail). Màn của bộ phận khác (Hợp đồng) chỉ MỞ tới, không sửa gì bên đó.</summary>
    public static class QuickFix
    {
        /// <summary>Kind chỉ màn gọi xử lý được (VD đổi ô Ngày xuất phát ở Chi tiết đơn) — truyền qua tham số onLocal.</summary>
        public const string ChangeStartDate = "ChangeStartDate";

        /// <param name="title">Câu ngắn, VD "Chưa bắt đầu được chuyến #12".</param>
        /// <param name="retryText">Chữ nút làm lại, VD "▶️ Bắt đầu chuyến #12 ngay" (null = không hỏi làm lại).</param>
        /// <param name="onLocal">Xử lý các cách sửa ngay trên màn gọi (Kind = ChangeStartDate); null = ẩn các cách đó.</param>
        public static async Task ShowAsync(Control owner, ApiServices? api, string title, string? message, List<FixHintItem>? fixes,
            string? retryText = null, Func<Task>? retry = null, Action<FixHintItem>? onLocal = null)
        {
            var usable = (fixes ?? new()).Where(f => f.Kind != ChangeStartDate || onLocal is not null).ToList();
            if (api is null || usable.Count == 0)
            {
                Ask.Fail(owner, message);
                return;
            }
            var pick = Pick(owner, title, message, usable);
            if (pick is null) return;
            if (pick.Kind == ChangeStartDate) { onLocal?.Invoke(pick); return; }

            var changed = await RunAsync(owner, api, pick);
            if (!changed || retry is null || retryText is null || owner.IsDisposed) return;
            if (Ask.Confirm(owner, "Đã sửa xong — làm lại luôn?",
                    $"Đã mở \"{Plain(pick.Text)}\". Nếu đã sửa xong, bấm nút bên dưới để làm lại thao tác vừa bị chặn.", retryText))
                await retry();
        }

        /// <summary>Lối tắt VIỆC TIẾP THEO sau khi thao tác thành công (VD chuyến lấy hàng xong -> "📦 Nhập kho ngay").</summary>
        public static async Task OfferAsync(Control owner, ApiServices? api, string title, string message, List<FixHintItem>? fixes)
        {
            if (api is null || fixes is not { Count: > 0 } || owner.IsDisposed) return;
            var next = fixes[0];
            if (Ask.Confirm(owner, title, message, next.Text, cancelText: "Để sau"))
                await RunAsync(owner, api, next);
        }

        private static string Plain(string text) => text.Trim();

        /// <summary>Hộp chọn cách sửa: lý do (khung đỏ nhạt) + mỗi cách sửa 1 nút lớn (chữ chính + dòng giải thích),
        /// cách đầu tiên là nút chính. null = bấm Để sau.</summary>
        private static FixHintItem? Pick(Control owner, string title, string? message, List<FixHintItem> fixes)
        {
            using var dlg = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterParent,
                AutoScaleMode = AutoScaleMode.Dpi, AutoScaleDimensions = new SizeF(96F, 96F),
                BackColor = Color.White, Font = new Font("Segoe UI", 10F),
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };
            int S(int v) => dlg.LogicalToDeviceUnits(v);
            var width = S(500);
            var root = new TableLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Dock = DockStyle.Fill,
                Padding = new Padding(S(20), S(16), S(20), S(12)), MinimumSize = new Size(S(420), 0),
            };
            root.Controls.Add(new Label
            {
                Text = "⛔ " + title, AutoSize = true, MaximumSize = new Size(width, 0), Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Theme.TextPrimary, Margin = new Padding(0, 0, 0, S(8)), UseMnemonic = false,
            });
            if (!string.IsNullOrWhiteSpace(message))
                root.Controls.Add(new Label
                {
                    Text = message.Trim().TrimStart('⚠', '️', ' '), AutoSize = true, MaximumSize = new Size(width, 0),
                    BackColor = Color.FromArgb(254, 242, 242), ForeColor = Color.FromArgb(127, 29, 29),
                    Padding = new Padding(S(10), S(8), S(10), S(8)), Margin = new Padding(0, 0, 0, S(10)), UseMnemonic = false,
                });
            root.Controls.Add(new Label
            {
                Text = fixes.Count == 1 ? "Cách sửa nhanh:" : "Chọn 1 cách sửa:", AutoSize = true, ForeColor = Theme.TextSecondary,
                Font = new Font("Segoe UI", 9F), Margin = new Padding(0, 0, 0, S(4)),
            });

            FixHintItem? chosen = null;
            for (var i = 0; i < fixes.Count; i++)
            {
                var fix = fixes[i];
                var b = new Button
                {
                    Text = fix.Text + (string.IsNullOrWhiteSpace(fix.Note) ? "" : "\n" + fix.Note),
                    AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    MinimumSize = new Size(width, S(44)), MaximumSize = new Size(width, 0),
                    TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(S(8), S(4), S(8), S(4)),
                    Margin = new Padding(0, 0, 0, S(6)), UseMnemonic = false,
                };
                if (i == 0) ButtonStyles.Primary(b); else ButtonStyles.Secondary(b);
                b.TextAlign = ContentAlignment.MiddleLeft;
                b.Click += (_, _) => { chosen = fix; dlg.DialogResult = DialogResult.OK; };
                root.Controls.Add(b);
            }

            var cancel = new Button { Text = "Để sau", AutoSize = true, MinimumSize = new Size(S(90), S(36)), DialogResult = DialogResult.Cancel };
            ButtonStyles.Secondary(cancel);
            var foot = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0, S(6), 0, 0),
            };
            foot.Controls.Add(cancel);
            root.Controls.Add(foot);
            dlg.Controls.Add(root);
            dlg.CancelButton = cancel;
            var parent = owner.FindForm();
            return (parent is null ? dlg.ShowDialog() : dlg.ShowDialog(parent)) == DialogResult.OK ? chosen : null;
        }

        /// <summary>Mở đúng chỗ sửa. true = đã mở 1 cửa sổ sửa (có thể đã sửa -> hỏi làm lại); false = chỉ chuyển màn / không mở được.</summary>
        public static async Task<bool> RunAsync(Control owner, ApiServices api, FixHintItem fix)
        {
            var parent = owner.FindForm();
            try
            {
                switch (fix.Kind)
                {
                    case "DriverWorkDays" when fix.Id is int driverId:
                        using (var f = new DriverWorkDaysForm(api, driverId, fix.Name ?? ""))
                        {
                            if (fix.From is DateTime from)
                                f.Preset(from, fix.To ?? from, "📌 Điền sẵn các ngày còn thiếu — bấm Đăng ký là xong.");
                            f.ShowDialog(parent);
                            return f.Changed;
                        }
                    case "DriverProfile" when fix.Id is int employeeId:
                        using (var f = new TruckingSystem.Desktop.GUI_EmployeeManagement.CreateDriverProfileForm(api, employeeId, fix.Name ?? ""))
                            return f.ShowDialog(parent) == DialogResult.OK;
                    case "VehicleDocs" when fix.Id is int vehicleId:
                        using (var f = new ManageVehicleDocumentsForm(api, vehicleId, fix.Name ?? ""))
                            f.ShowDialog(parent);
                        return true;
                    case "Reassign" when fix.Id is int tripId:
                        using (var f = new ReassignTripForm(api, tripId, fix.Name ?? $"#{tripId}"))
                            return f.ShowDialog(parent) == DialogResult.OK;
                    case "PendingReceipt" when fix.WarehouseId is int wid:
                        using (var f = new PendingReceiptsForm(api, wid, fix.Name ?? ""))
                            f.ShowDialog(parent);
                        return true;
                    case "TripIssue" when fix.WarehouseId is int wid:
                        using (var f = new TripIssuesForm(api, wid, fix.Name ?? ""))
                            f.ShowDialog(parent);
                        return true;
                    case "ManualReceive" when fix.WarehouseId is int wid:
                        using (var f = new ManualStockForm(api, wid, ManualStockMode.Receive))
                            f.ShowDialog(parent);
                        return true;
                    case "WarehouseLayout" or "EditWarehouse" when fix.WarehouseId is int wid:
                    {
                        var warehouse = (await api.Warehouses.GetWarehousesAsync()).FirstOrDefault(w => w.Id == wid);
                        if (warehouse is null) { Ask.Fail(owner, "Không tìm thấy kho — có thể đã bị xóa."); return false; }
                        if (fix.Kind == "WarehouseLayout")
                        {
                            using var layout = new WarehouseLayoutForm(api, warehouse);
                            layout.ShowDialog(parent);
                            return true;
                        }
                        using var edit = new EditWarehouseForm(api, warehouse);
                        return edit.ShowDialog(parent) == DialogResult.OK;
                    }
                    case "Vehicle":
                        return Navigate(owner, "Đội xe", () => { var c = new VehiclesControl(api); c.FocusOn(fix.Name ?? ""); return c; });
                    case "VehiclesBlocked":
                        return Navigate(owner, "Đội xe", () => { var c = new VehiclesControl(api); c.ShowDocsBlocked(); return c; });
                    case "Driver":
                        return Navigate(owner, "Tài xế", () => { var c = new DriversControl(api); c.FocusOn(fix.Name ?? ""); return c; });
                    case "Contracts":
                        return Navigate(owner, "Hợp đồng", null);
                }
            }
            catch (Exception ex)
            {
                Ask.Fail(owner, "Không mở được chỗ sửa: " + ex.Message);
                return false;
            }
            Ask.Fail(owner, "Chưa hỗ trợ mở nhanh chỗ sửa này.");
            return false;
        }

        /// <summary>Chuyển sang màn khác (đóng hộp thoại đang mở để thấy màn đó). Luôn trả false: không hỏi làm lại.</summary>
        private static bool Navigate(Control owner, string navName, Func<Control>? build)
        {
            var main = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
            var ok = main is not null && (build is null ? main.OpenNavByName(navName) : main.OpenNavWith(navName, build));
            if (!ok)
            {
                Ask.Fail(owner, $"Tài khoản của bạn không có quyền vào màn \"{navName}\".");
                return false;
            }
            if (owner.FindForm() is Form f && f != main && f.Modal) f.Close();
            return false;
        }
    }
}
