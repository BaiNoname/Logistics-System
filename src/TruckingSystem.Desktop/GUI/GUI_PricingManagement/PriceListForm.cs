using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PricingManagement;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Desktop.GUI.GUI_PricingManagement;

public partial class PriceListForm : BaseForm
{
    private readonly ApiServices _api = null!;
    private readonly PriceListItem? _item;
    private readonly bool _copy;
    private bool _saving;

    public int? SavedId { get; private set; }

    public PriceListForm()
    {
        InitializeComponent();
        _save.Text = "Lưu bảng giá";
        Controls.Remove(_footer);
        FormStyle.EditorFooter(this, _cancel, _save);
        _footer.Dispose();
        foreach (Control block in _fields.Controls)
        {
            block.Dock = DockStyle.Top;
            block.TabIndex = _fields.Controls.GetChildIndex(block);
            foreach (Control input in block.Controls)
                input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FormStyle.FieldLabels(block.Controls.OfType<Label>().ToArray());
        }
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
    }

    public PriceListForm(ApiServices api, PriceListItem? item = null, bool copy = false) : this()
    {
        if (copy && item is null) throw new ArgumentException("Vui lòng chọn bảng giá cần sao chép.", nameof(item));
        _api = api;
        _item = item;
        _copy = copy;
        Text = copy ? "Tạo mới và sao chép" : item is null ? "Thêm bảng giá" : "Sửa bảng giá";
        _name.Text = copy ? "" : item?.Name ?? "";
        var today = DateTime.UtcNow.AddHours(7).Date;
        _date.MinDate = today;
        _date.Value = !copy && item is not null && item.EffectiveDate.Date >= today ? item.EffectiveDate.Date : today;
        if (!copy && item is not null && item.EffectiveDate.Date < today)
            FormStyle.Status(_status, "Ngày hiệu lực cũ đã qua. Khi lưu, bảng giá sẽ dùng ngày hiệu lực mới từ hôm nay trở đi.", StatusKind.Warning);
        _priceListStatus.SelectedIndex = item?.Status is "Inactive" or "Expired" ? 1 : 0;
        if (copy)
        {
            _save.Text = "Tạo mới và sao chép";
            _save.Width = 200;
            _copyInfo.Text = $"Sao chép từ: {item!.Name}\nSao chép toàn bộ chi tiết giá. Bảng mới có hiệu lực từ hôm nay; bảng nguồn chuyển sang ngưng áp dụng khi lưu thành công.";
            _copyInfo.Visible = true;
            _nameLabel.Text = "Tên bảng giá mới *:";
            _datePanel.Visible = false;
            _priceListStatusPanel.Visible = false;
        }
        if (!copy && item is not null && item.Status != "Active")
        {
            _fields.Enabled = _save.Enabled = false;
            FormStyle.Status(_status, "Không thể sửa bảng giá đã ngừng áp dụng. Có thể sao chép để tạo bảng giá mới.", StatusKind.Warning);
        }
        // Chân cửa sổ chuẩn: [Đóng] [💾 Lưu / Tạo mới và sao chép].
        FormStyle.SaveButton(_save);
        FormStyle.CloseButton(this, _cancel);
        _save.Click += async (_, _) => await SaveAsync();
        Shown += (_, _) => _name.Focus();
        FormClosing += (_, e) => e.Cancel = _saving;
    }

    private async Task SaveAsync()
    {
        if (_saving || !_copy && _item is not null && _item.Status != "Active") return;
        _errors.Clear();
        var today = DateTime.UtcNow.AddHours(7).Date;
        var effectiveDate = _copy ? today : _date.Value.Date;
        try { PriceList.Validate(_name.Text, effectiveDate, today); }
        catch (ArgumentException ex)
        {
            Control input = string.IsNullOrWhiteSpace(_name.Text) ? _name : _date;
            FieldHint.Show(input, _status, ex.Message);
            return;
        }
        _saving = true;
        _fields.Enabled = _save.Enabled = false;
        FormStyle.Status(_status, "Đang lưu...", StatusKind.Info);
        try
        {
            var status = _priceListStatus.SelectedIndex == 1 ? "Inactive" : "Active";
            if (_copy)
            {
                var saved = await _api.Pricing.CopyPriceListAsync(_item!.Id, _name.Text.Trim());
                SavedId = saved?.Id ?? throw new InvalidOperationException("Máy chủ chưa trả về bảng giá mới. Vui lòng làm mới danh sách để kiểm tra.");
            }
            else if (_item is null)
            {
                var saved = await _api.Pricing.CreatePriceListAsync(_name.Text.Trim(), effectiveDate, status);
                SavedId = saved?.Id;
            }
            else
            {
                await _api.Pricing.UpdatePriceListAsync(_item.Id, _name.Text.Trim(), effectiveDate, status);
                SavedId = _item.Id;
            }
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { FormStyle.Status(_status, "Lỗi: " + ex.Message); }
        finally { _saving = false; if (!IsDisposed) _fields.Enabled = _save.Enabled = true; }
    }
}
