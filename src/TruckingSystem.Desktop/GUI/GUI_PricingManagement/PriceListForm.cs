using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PricingManagement;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Desktop.GUI.GUI_PricingManagement;

public class PriceListForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly PriceListItem? _item;
    private readonly bool _copy;
    private readonly TextBox _name = new() { AccessibleName = "Tên bảng giá" };
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", AccessibleName = "Ngày hiệu lực" };
    private readonly ComboBox _priceListStatus = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Trạng thái bảng giá" };
    private readonly Button _save = Theme.MakePrimaryButton("Lưu", 100);
    private readonly Label _status = new() { AutoSize = true, MaximumSize = new Size(440, 0), ForeColor = Theme.Danger };
    private readonly ErrorProvider _errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly FlowLayoutPanel _fields = new() { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(24) };
    private bool _saving;

    public int? SavedId { get; private set; }

    public PriceListForm(ApiServices api, PriceListItem? item = null, bool copy = false)
    {
        if (copy && item is null) throw new ArgumentException("Vui lòng chọn bảng giá cần sao chép.", nameof(item));
        _api = api;
        _item = item;
        _copy = copy;
        _errors.ContainerControl = this;
        Text = copy ? "Tạo mới và sao chép" : item is null ? "Thêm bảng giá" : "Sửa bảng giá";
        ClientSize = new Size(500, 400);
        BackColor = Theme.CardBg;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        _name.Text = copy ? "" : item?.Name ?? "";
        _date.Value = item?.EffectiveDate ?? DateTime.UtcNow.AddHours(7).Date;
        _priceListStatus.Items.AddRange(new object[] { "Đang áp dụng", "Ngừng áp dụng", "Hết hạn" });
        _priceListStatus.SelectedIndex = item?.Status switch { "Inactive" => 1, "Expired" => 2, _ => 0 };
        if (copy)
        {
            _save.Text = "Tạo mới và sao chép";
            _save.Width = 200;
            _fields.Controls.Add(new Label
            {
                Text = $"Sao chép từ: {item!.Name}\nGiữ nguyên ngày hiệu lực và toàn bộ chi tiết giá. Bảng mới đang áp dụng; bảng nguồn chuyển sang hết hạn khi lưu thành công.",
                AutoSize = true, MaximumSize = new Size(430, 0), Margin = new Padding(0, 0, 0, 16),
            });
        }
        _fields.Controls.Add(Theme.MakeLabeledInput(copy ? "Tên bảng giá mới *" : "Tên bảng giá *", _name, 430));
        if (!copy)
        {
            _fields.Controls.Add(Theme.MakeLabeledInput("Ngày hiệu lực *", _date, 430));
            _fields.Controls.Add(Theme.MakeLabeledInput("Trạng thái *", _priceListStatus, 430));
        }
        _fields.Controls.Add(_status);
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 68, Padding = new Padding(24, 10, 24, 10), FlowDirection = FlowDirection.RightToLeft };
        var cancel = Theme.MakeSecondaryButton("Hủy", 100);
        cancel.DialogResult = DialogResult.Cancel;
        footer.Controls.AddRange(new Control[] { _save, cancel });
        Controls.Add(_fields);
        Controls.Add(footer);
        AcceptButton = _save;
        CancelButton = cancel;
        _save.Click += async (_, _) => await SaveAsync();
        Shown += (_, _) => _name.Focus();
        FormClosing += (_, e) => e.Cancel = _saving;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _errors.Dispose();
        base.Dispose(disposing);
    }

    private async Task SaveAsync()
    {
        if (_saving) return;
        _errors.Clear();
        try { PriceList.Validate(_name.Text, _date.Value.Date); }
        catch (ArgumentException ex)
        {
            _errors.SetError(_name, ex.Message);
            _status.Text = ex.Message;
            _name.Focus();
            return;
        }
        _saving = true;
        _fields.Enabled = _save.Enabled = false;
        _status.Text = "Đang lưu...";
        try
        {
            var status = _priceListStatus.SelectedIndex switch { 1 => "Inactive", 2 => "Expired", _ => "Active" };
            if (_copy)
            {
                var saved = await _api.Pricing.CopyPriceListAsync(_item!.Id, _name.Text.Trim());
                SavedId = saved?.Id ?? throw new InvalidOperationException("Máy chủ chưa trả về bảng giá mới. Vui lòng làm mới danh sách để kiểm tra.");
            }
            else if (_item is null)
            {
                var saved = await _api.Pricing.CreatePriceListAsync(_name.Text.Trim(), _date.Value.Date, status);
                SavedId = saved?.Id;
            }
            else
            {
                await _api.Pricing.UpdatePriceListAsync(_item.Id, _name.Text.Trim(), _date.Value.Date, status);
                SavedId = _item.Id;
            }
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { _status.Text = "Lỗi: " + ex.Message; }
        finally { _saving = false; if (!IsDisposed) _fields.Enabled = _save.Enabled = true; }
    }
}
