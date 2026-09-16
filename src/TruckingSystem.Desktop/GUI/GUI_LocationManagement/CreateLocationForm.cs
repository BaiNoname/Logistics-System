using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_LocationManagement;

public class CreateLocationForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly TextBox _txtName = new();
    private readonly TextBox _txtAddress = new();
    private readonly NumericUpDown _numLat = new() { Minimum = -90, Maximum = 90, DecimalPlaces = 6, Increment = 0.0001m };
    private readonly NumericUpDown _numLng = new() { Minimum = -180, Maximum = 180, DecimalPlaces = 6, Increment = 0.0001m };
    private readonly Label _lblStatus = new() { Font = Theme.FontSmall, ForeColor = Theme.Danger, Top = 300, Left = 24, AutoSize = true };

    public CreateLocationForm(ApiServices api)
    {
        _api = api;
        Text = "Thêm địa điểm mới";
        Width = 460; Height = 400;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        BackColor = Color.White;

        var p1 = Theme.MakeLabeledInput("Tên địa điểm *", _txtName, 400); p1.Top = 20; p1.Left = 24;
        var p2 = Theme.MakeLabeledInput("Địa chỉ đầy đủ *", _txtAddress, 400); p2.Top = 90; p2.Left = 24;
        var p3 = Theme.MakeLabeledInput("Vĩ độ (Lat) — tuỳ chọn", _numLat, 190); p3.Top = 160; p3.Left = 24;
        var p4 = Theme.MakeLabeledInput("Kinh độ (Lng) — tuỳ chọn", _numLng, 190); p4.Top = 160; p4.Left = 234;

        var note = new Label
        {
            Text = "Ghi chú: chưa tích hợp tự động tra tọa độ (VietMap Geocoding) — nhập tay nếu biết, hoặc để trống.",
            Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = 220, Left = 24, Width = 400, Height = 40,
        };

        var btnSave = Theme.MakePrimaryButton("Lưu địa điểm", 180); btnSave.Top = 310; btnSave.Left = 244;
        btnSave.Click += Save;

        Controls.Add(p1); Controls.Add(p2); Controls.Add(p3); Controls.Add(p4);
        Controls.Add(note);
        Controls.Add(btnSave); Controls.Add(_lblStatus);
    }

    private async void Save(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text) || string.IsNullOrWhiteSpace(_txtAddress.Text))
        {
            _lblStatus.Text = "Vui lòng nhập đủ tên và địa chỉ.";
            return;
        }
        try
        {
            double? lat = _numLat.Value == 0 ? null : (double)_numLat.Value;
            double? lng = _numLng.Value == 0 ? null : (double)_numLng.Value;
            await _api.Locations.CreateLocationAsync(_txtName.Text.Trim(), _txtAddress.Text.Trim(), lat, lng);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi: " + ex.Message;
        }
    }
}
