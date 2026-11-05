using System.Drawing.Printing;
using QRCoder;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_AttendanceManagement;

namespace TruckingSystem.Desktop.GUI_AttendanceManagement;

/// <summary>Quản lý chấm công QR: phát mã QR theo ngày cho từng kho, cấu hình bán kính GPS,
/// duyệt yêu cầu điều chỉnh từ Mobile, xem bảng công và nhật ký quét gốc.
/// Dữ liệu ở đây chưa phải công tính lương — bảng công lương vẫn nhập/xác nhận ở "Công &amp; lương".</summary>
public sealed class AttendanceControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly bool _isAdmin;

    // Tab QR
    private readonly ComboBox _site = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
    private readonly DateTimePicker _qrDate = new() { Format = DateTimePickerFormat.Short, Width = 130 };
    private readonly NumberBox _radius = new() { Minimum = 20, Maximum = 5000, Increment = 10, Width = 90 };
    private readonly NumberBox _accuracy = new() { Minimum = 5, Maximum = 1000, Increment = 5, Width = 90 };
    private readonly PictureBox _qrImage = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
    private readonly Label _qrInfo = new() { Dock = DockStyle.Bottom, Height = 56, TextAlign = ContentAlignment.MiddleCenter, Font = Theme.FontBold };
    private AttendanceQrDto? _currentQr;

    // Tab điều chỉnh
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly DataGridView _adjustGrid = MakeGrid();
    private List<AdjustmentRequestDto> _adjustments = new();

    // Tab bảng công / nhật ký
    private readonly DateTimePicker _from = new() { Format = DateTimePickerFormat.Short, Width = 130 };
    private readonly DateTimePicker _to = new() { Format = DateTimePickerFormat.Short, Width = 130 };
    private readonly DataGridView _recordGrid = MakeGrid();
    private readonly DateTimePicker _scanDate = new() { Format = DateTimePickerFormat.Short, Width = 130 };
    private readonly DataGridView _scanGrid = MakeGrid();

    public AttendanceControl(ApiServices api)
    {
        _api = api;
        _isAdmin = api.Connection.IsInRole("Admin");
        GridDecor.Pills(_adjustGrid, "Trạng_thái", (text, _) => text switch
        {
            "Đã duyệt" => GridDecor.Green, "Từ chối" => GridDecor.Red, _ => GridDecor.Amber,
        });
        GridDecor.Pills(_scanGrid, "Kết_quả", (text, _) => text == "Hợp lệ" ? GridDecor.Green : GridDecor.Red);
        Dock = DockStyle.Fill;
        BackColor = Theme.ContentBg;
        Padding = new Padding(16);

        var title = new Label { Text = "Chấm công QR", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 44 };
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = Theme.FontNormal };
        tabs.TabPages.Add(BuildQrTab());
        tabs.TabPages.Add(BuildAdjustmentTab());
        tabs.TabPages.Add(BuildRecordTab());
        tabs.TabPages.Add(BuildScanTab());
        Controls.Add(tabs);
        Controls.Add(title);

        _qrDate.Value = _scanDate.Value = _to.Value = DateTime.Today;
        _from.Value = DateTime.Today.AddDays(1 - DateTime.Today.Day);
        Load += async (_, _) =>
        {
            await LoadSitesAsync();
            await LoadAdjustmentsAsync();
        };
        tabs.SelectedIndexChanged += async (_, _) =>
        {
            if (tabs.SelectedIndex == 2 && _recordGrid.DataSource is null) await LoadRecordsAsync();
            if (tabs.SelectedIndex == 3 && _scanGrid.DataSource is null) await LoadScansAsync();
        };
    }

    // ================= Tab 1: mã QR =================

    private TabPage BuildQrTab()
    {
        var page = new TabPage("Mã QR theo ngày") { BackColor = Color.White, Padding = new Padding(12) };
        var show = Theme.MakePrimaryButton("Hiện mã QR");
        var save = Theme.MakeSecondaryButton("Lưu ảnh PNG");
        var print = Theme.MakeSecondaryButton("In mã QR");
        var bar = Bar(Caption("Kho"), _site, Caption("Ngày"), _qrDate, show, save, print);

        var saveSite = Theme.MakeSecondaryButton("Lưu cấu hình", 150);
        saveSite.Enabled = _isAdmin;
        _radius.Enabled = _accuracy.Enabled = _isAdmin;
        var config = Bar(Caption("Bán kính cho phép (m)"), _radius, Caption("Sai số GPS tối đa (m)"), _accuracy, saveSite,
            new Label { AutoSize = true, ForeColor = Theme.TextSecondary, Margin = new Padding(8, 10, 0, 0),
                Text = _isAdmin ? "Áp dụng cho mọi lượt quét tại kho này." : "Chỉ Admin được sửa cấu hình." });

        _qrInfo.Text = "Chọn kho và ngày rồi bấm \"Hiện mã QR\".";
        page.Controls.Add(_qrImage);
        page.Controls.Add(_qrInfo);
        page.Controls.Add(config);
        page.Controls.Add(bar);

        _site.SelectedIndexChanged += (_, _) =>
        {
            if (_site.SelectedItem is not AttendanceSiteDto s) return;
            _radius.Value = Math.Clamp(s.RadiusMeters, (int)_radius.Minimum, (int)_radius.Maximum);
            _accuracy.Value = Math.Clamp(s.MaxAccuracyMeters, (int)_accuracy.Minimum, (int)_accuracy.Maximum);
            ClearQr();
        };
        _qrDate.ValueChanged += (_, _) => ClearQr();
        show.Click += async (_, _) => await ShowQrAsync();
        save.Click += (_, _) => SaveQr();
        print.Click += (_, _) => PrintQr();
        saveSite.Click += async (_, _) => await SaveSiteAsync();
        return page;
    }

    private async Task LoadSitesAsync()
    {
        try
        {
            var sites = await _api.Attendance.SitesAsync();
            _site.Items.Clear();
            foreach (var s in sites) _site.Items.Add(s);
            if (_site.Items.Count > 0) _site.SelectedIndex = 0;
            else _qrInfo.Text = "Chưa có kho nào — tạo kho (có tọa độ) ở mục Kho trung chuyển trước.";
        }
        catch (Exception ex) { Warn(ex.Message); }
    }

    private async Task ShowQrAsync()
    {
        if (_site.SelectedItem is not AttendanceSiteDto site) { Warn("Chọn kho."); return; }
        try
        {
            var qr = await _api.Attendance.QrAsync(site.WarehouseId, _qrDate.Value.Date);
            using var data = new QRCodeGenerator().CreateQrCode(qr.QrContent!, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data).GetGraphic(20);
            _qrImage.Image?.Dispose();
            using var ms = new MemoryStream(png);
            _qrImage.Image = new Bitmap(Image.FromStream(ms));
            _currentQr = qr;
            _qrInfo.Text = $"{qr.WarehouseName} — ngày {qr.WorkDate:dd/MM/yyyy}. Dán tại địa điểm chấm công; mã chỉ hợp lệ trong ngày này.";
        }
        catch (Exception ex) { ClearQr(); Warn(ex.Message); }
    }

    private void ClearQr()
    {
        _currentQr = null;
        _qrImage.Image?.Dispose();
        _qrImage.Image = null;
        _qrInfo.Text = "Chọn kho và ngày rồi bấm \"Hiện mã QR\".";
    }

    private void SaveQr()
    {
        if (_currentQr is null || _qrImage.Image is null) { Warn("Hãy hiện mã QR trước."); return; }
        using var dialog = new SaveFileDialog
        {
            Filter = "Ảnh PNG|*.png",
            FileName = $"QR-ChamCong-{Safe(_currentQr.WarehouseName ?? "")}-{_currentQr.WorkDate:yyyyMMdd}.png",
        };
        if (dialog.ShowDialog(this) == DialogResult.OK) _qrImage.Image.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
    }

    private void PrintQr()
    {
        if (_currentQr is null || _qrImage.Image is null) { Warn("Hãy hiện mã QR trước."); return; }
        var qr = _currentQr;
        var image = _qrImage.Image;
        using var doc = new PrintDocument { DocumentName = $"QR chấm công {qr.WarehouseName} {qr.WorkDate:dd/MM/yyyy}" };
        doc.PrintPage += (_, e) =>
        {
            var g = e.Graphics!;
            var area = e.MarginBounds;
            using var big = new Font("Segoe UI", 22, FontStyle.Bold);
            using var normal = new Font("Segoe UI", 13);
            var center = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString("MÃ QR CHẤM CÔNG", big, Brushes.Black, new RectangleF(area.Left, area.Top, area.Width, 50), center);
            g.DrawString($"{qr.WarehouseName} — ngày {qr.WorkDate:dd/MM/yyyy}", normal, Brushes.Black,
                new RectangleF(area.Left, area.Top + 50, area.Width, 30), center);
            var size = Math.Min(area.Width, area.Height - 160);
            g.DrawImage(image, area.Left + (area.Width - size) / 2, area.Top + 100, size, size);
            g.DrawString("Mở ứng dụng TS Chấm công → Quét QR. Mã chỉ hợp lệ trong ngày in trên giấy.", normal, Brushes.Black,
                new RectangleF(area.Left, area.Top + 110 + size, area.Width, 40), center);
        };
        using var dialog = new PrintDialog { Document = doc, UseEXDialog = true };
        if (dialog.ShowDialog(this) == DialogResult.OK) doc.Print();
    }

    private async Task SaveSiteAsync()
    {
        if (_site.SelectedItem is not AttendanceSiteDto site) { Warn("Chọn kho."); return; }
        try
        {
            var result = await _api.Attendance.UpdateSiteAsync(site.WarehouseId,
                new UpdateAttendanceSiteRequest((int)_radius.Value, (int)_accuracy.Value));
            if (!result.Success) { Warn(result.Message ?? "Không lưu được."); return; }
            var index = _site.SelectedIndex;
            _site.Items[index] = site with { RadiusMeters = (int)_radius.Value, MaxAccuracyMeters = (int)_accuracy.Value };
            _site.SelectedIndex = index;
            Toast.Show(this, "✅ " + (result.Message ?? "Đã lưu cấu hình chấm công."));
        }
        catch (Exception ex) { Warn(ex.Message); }
    }

    // ================= Tab 2: yêu cầu điều chỉnh =================

    private TabPage BuildAdjustmentTab()
    {
        var page = new TabPage("Yêu cầu điều chỉnh") { BackColor = Color.White, Padding = new Padding(12) };
        _status.Items.AddRange(new object[] { new ComboOption("Pending", "Chờ duyệt"), new ComboOption("Approved", "Đã duyệt"),
            new ComboOption("Rejected", "Từ chối"), new ComboOption("", "Tất cả") });
        _status.SelectedIndex = 0;
        var refresh = Theme.MakeSecondaryButton("Tải lại");
        var approve = Theme.MakePrimaryButton("Duyệt điều chỉnh", 170);
        var reject = Theme.MakeSecondaryButton("Từ chối điều chỉnh", 170);
        page.Controls.Add(_adjustGrid);
        page.Controls.Add(Bar(Caption("Trạng thái"), _status, refresh, approve, reject,
            new Label { AutoSize = true, ForeColor = Theme.TextSecondary, Margin = new Padding(8, 10, 0, 0),
                Text = "Duyệt sẽ ghi giờ điều chỉnh, giữ nguyên nhật ký quét gốc." }));

        _status.SelectedIndexChanged += async (_, _) => await LoadAdjustmentsAsync();
        refresh.Click += async (_, _) => await LoadAdjustmentsAsync();
        approve.Click += async (_, _) => await ReviewAsync(true);
        reject.Click += async (_, _) => await ReviewAsync(false);
        return page;
    }

    private async Task LoadAdjustmentsAsync()
    {
        try
        {
            _adjustments = await _api.Attendance.AdjustmentsAsync((_status.SelectedItem as ComboOption)?.Value);
            _adjustGrid.DataSource = _adjustments.Select(a => new
            {
                Mã_nhân_viên = a.EmployeeCode, Họ_tên = a.EmployeeName, Ngày = a.WorkDate.ToString("dd/MM/yyyy"),
                Hiện_tại = $"{Time(a.CurrentCheckInAt)} → {Time(a.CurrentCheckOutAt)}",
                Đề_xuất = $"{Time(a.RequestedCheckInAt, "giữ")} → {Time(a.RequestedCheckOutAt, "giữ")}",
                Lý_do = a.ReasonText, Giải_thích = a.Note, Trạng_thái = a.StatusText,
                Gửi_lúc = a.CreatedAt.ToString("dd/MM HH:mm"),
                Người_xử_lý = a.ReviewedBy ?? "", Ghi_chú_xử_lý = a.ReviewNote ?? "",
            }).ToList();
        }
        catch (Exception ex) { Warn(ex.Message); }
    }

    private async Task ReviewAsync(bool approve)
    {
        var index = _adjustGrid.CurrentRow?.Index ?? -1;
        if (index < 0 || index >= _adjustments.Count) { Warn("Chọn một yêu cầu."); return; }
        var item = _adjustments[index];
        if (item.Status != "Pending") { Warn("Yêu cầu này đã được xử lý."); return; }

        var question = $"{(approve ? "Duyệt" : "Từ chối")} yêu cầu của {item.EmployeeName} ngày {item.WorkDate:dd/MM/yyyy}?\n" +
                       $"Đề xuất: vào {Time(item.RequestedCheckInAt, "giữ nguyên")} · ra {Time(item.RequestedCheckOutAt, "giữ nguyên")}";
        var note = AskNote(approve ? "Duyệt điều chỉnh" : "Từ chối điều chỉnh", question, required: !approve);
        if (note is null) return;
        try
        {
            var result = await _api.Attendance.ReviewAsync(item.Id, new ReviewAdjustmentRequest(approve, note));
            if (!result.Success) { Warn(result.Message ?? "Không xử lý được."); return; }
            await LoadAdjustmentsAsync();
            _recordGrid.DataSource = null; // bảng công đổi → tải lại khi mở tab
        }
        catch (Exception ex) { Warn(ex.Message); }
    }

    /// <summary>Hộp nhập ghi chú; null = hủy.</summary>
    private string? AskNote(string caption, string question, bool required)
    {
        using var form = new BaseForm { Text = caption, ClientSize = new Size(580, 360), MinimumSize = new Size(500, 320),
            StartPosition = FormStartPosition.CenterParent, BackColor = Theme.CardBg, MaximizeBox = false, MinimizeBox = false };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(20, 16, 20, 16) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var questionLabel = new Label { Text = question, AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.TextPrimary };
        var label = new Label { Text = required ? "Lý do từ chối *:" : "Ghi chú (không bắt buộc):", AutoSize = true, Margin = new Padding(3, 12, 3, 6) };
        var box = new TextBox { Multiline = true, Dock = DockStyle.Fill, MaxLength = 500, ScrollBars = ScrollBars.Vertical, AccessibleName = label.Text };
        var status = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.Danger };
        layout.Controls.Add(questionLabel, 0, 0); layout.Controls.Add(label, 0, 1); layout.Controls.Add(box, 0, 2); layout.Controls.Add(status, 0, 3);
        form.Controls.Add(layout);
        var ok = Theme.MakePrimaryButton("Xác nhận điều chỉnh", 190);
        var cancel = FormStyle.NewCloseButton(form);
        FormStyle.SaveButton(ok);
        ButtonStyles.Footer(form, required ? new Label { Text = "* Trường bắt buộc", AutoSize = true, Font = Theme.FontSmall, ForeColor = Theme.TextSecondary } : null, cancel, ok);
        ModernLook.Apply(form);
        DialogKeys.Enable(form);
        Anim.FadeIn(form);
        ok.Click += (_, _) =>
        {
            if (required && string.IsNullOrWhiteSpace(box.Text)) { FieldHint.Show(box, status, "Vui lòng nhập lý do từ chối."); return; }
            form.DialogResult = DialogResult.OK;
        };
        return form.ShowDialog(this) == DialogResult.OK ? box.Text.Trim() : null;
    }

    // ================= Tab 3: bảng công =================

    private TabPage BuildRecordTab()
    {
        var page = new TabPage("Bảng chấm công") { BackColor = Color.White, Padding = new Padding(12) };
        var view = Theme.MakePrimaryButton("Xem dữ liệu", 150);
        page.Controls.Add(_recordGrid);
        page.Controls.Add(Bar(Caption("Từ"), _from, Caption("Đến"), _to, view,
            new Label { AutoSize = true, ForeColor = Theme.TextSecondary, Margin = new Padding(8, 10, 0, 0),
                Text = "Tối đa 93 ngày. Dữ liệu tham khảo — công tính lương vẫn xác nhận ở \"Công & lương\"." }));
        view.Click += async (_, _) => await LoadRecordsAsync();
        return page;
    }

    private async Task LoadRecordsAsync()
    {
        if (_to.Value.Date < _from.Value.Date || (_to.Value.Date - _from.Value.Date).TotalDays > 93)
        {
            Warn("Khoảng ngày không hợp lệ (tối đa 93 ngày).");
            return;
        }
        try
        {
            var records = await _api.Attendance.RecordsAsync(_from.Value.Date, _to.Value.Date);
            _recordGrid.DataSource = records.Select(r => new
            {
                Ngày = r.WorkDate.ToString("dd/MM/yyyy"), Mã_NV = r.EmployeeCode, Họ_tên = r.EmployeeName,
                Giờ_vào = Time(r.CheckInAt), Nơi_vào = r.CheckInSite ?? "", Giờ_ra = Time(r.CheckOutAt), Nơi_ra = r.CheckOutSite ?? "",
                Điều_chỉnh = r.AdjustedCheckInAt is null && r.AdjustedCheckOutAt is null ? ""
                    : $"vào {Time(r.AdjustedCheckInAt, "-")} · ra {Time(r.AdjustedCheckOutAt, "-")}",
                Tình_trạng = r.StatusText,
            }).ToList();
        }
        catch (Exception ex) { Warn(ex.Message); }
    }

    // ================= Tab 4: nhật ký quét =================

    private TabPage BuildScanTab()
    {
        var page = new TabPage("Nhật ký quét") { BackColor = Color.White, Padding = new Padding(12) };
        var view = Theme.MakePrimaryButton("Xem nhật ký", 150);
        page.Controls.Add(_scanGrid);
        page.Controls.Add(Bar(Caption("Ngày"), _scanDate, view,
            new Label { AutoSize = true, ForeColor = Theme.TextSecondary, Margin = new Padding(8, 10, 0, 0),
                Text = "Mọi lượt quét gốc, kể cả bị từ chối (không sửa/xóa)." }));
        view.Click += async (_, _) => await LoadScansAsync();
        return page;
    }

    private async Task LoadScansAsync()
    {
        try
        {
            var scans = await _api.Attendance.ScansAsync(_scanDate.Value.Date);
            _scanGrid.DataSource = scans.Select(s => new
            {
                Giờ = s.ScannedAt.ToString("HH:mm:ss"), Mã_NV = s.EmployeeCode, Họ_tên = s.EmployeeName, Kho = s.SiteName ?? "",
                Thao_tác = s.Action, Kết_quả = s.Result,
                Cách_kho_m = s.DistanceMeters?.ToString("0") ?? "", Sai_số_GPS_m = s.AccuracyMeters?.ToString("0") ?? "",
                Lý_do_từ_chối = s.RejectReason ?? "",
            }).ToList();
        }
        catch (Exception ex) { Warn(ex.Message); }
    }

    // ================= Helpers =================

    private static DataGridView MakeGrid()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };
        Theme.StyleGrid(grid);
        return grid;
    }

    private FlowLayoutPanel Bar(params Control[] controls)
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(0, 0, 0, 8) };
        foreach (var c in controls) c.Margin = c is Label ? new Padding(8, 10, 4, 0) : new Padding(4);
        bar.Controls.AddRange(controls);
        Load += (_, _) => Theme.UniformButtons(bar);
        return bar;
    }

    private static Label Caption(string text) => new() { Text = text, AutoSize = true, ForeColor = Theme.TextPrimary };

    private static string Time(DateTime? value, string empty = "--:--") => value?.ToString("HH:mm") ?? empty;


    private static string Safe(string name) => string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) || c == ' ' ? '_' : c));

    private void Warn(string message) => MessageBox.Show(this, message, "Chấm công QR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
