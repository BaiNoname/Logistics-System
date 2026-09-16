using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_VehicleManagement;

public class CreateVehicleForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly Button _btnSave;
    private readonly TextBox _txtPlateNumber = new() { Left = 150, Top = 20, Width = 220 };
    private readonly TextBox _txtVehicleType = new() { Left = 150, Top = 55, Width = 220, Text = "Xe tải 5 tấn" };
    private readonly NumericUpDown _numCapacityQty = new() { Left = 150, Top = 90, Width = 120, Maximum = 1000000, DecimalPlaces = 2, Value = 5000 };
    private readonly NumericUpDown _numCapacityKg = new() { Left = 150, Top = 125, Width = 120, Maximum = 1000000, DecimalPlaces = 2, Value = 5000 };
    private readonly NumericUpDown _numCapacityM3 = new() { Left = 150, Top = 160, Width = 120, Maximum = 1000000, DecimalPlaces = 2, Value = 25 };
    private readonly Label _lblStatus = new() { Left = 150, Top = 240, Width = 250, ForeColor = Color.Red };

    public CreateVehicleForm(ApiServices api)
    {
        _api = api;
        Text = "Thêm xe mới";
        Width = 400; Height = 320;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        BackColor = Color.White;

        _btnSave = Theme.MakePrimaryButton("Lưu", 150, 38);
        _btnSave.Top = 200; _btnSave.Left = 150;

        Controls.Add(new Label { Left = 15, Top = 23, Width = 130, Text = "Biển số xe:" });
        Controls.Add(new Label { Left = 15, Top = 58, Width = 130, Text = "Loại xe:" });
        Controls.Add(new Label { Left = 15, Top = 93, Width = 130, Text = "Sức chứa (đơn vị):" });
        Controls.Add(new Label { Left = 15, Top = 128, Width = 130, Text = "Sức chứa (kg):" });
        Controls.Add(new Label { Left = 15, Top = 163, Width = 130, Text = "Sức chứa (m³):" });

        Controls.Add(_txtPlateNumber);
        Controls.Add(_txtVehicleType);
        Controls.Add(_numCapacityQty);
        Controls.Add(_numCapacityKg);
        Controls.Add(_numCapacityM3);
        Controls.Add(_btnSave);
        Controls.Add(_lblStatus);

        _btnSave.Click += BtnSave_Click;
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtPlateNumber.Text))
        {
            _lblStatus.Text = "Vui lòng nhập biển số xe.";
            return;
        }

        _btnSave.Enabled = false;
        try
        {
            await _api.Vehicles.CreateVehicleAsync(
                _txtPlateNumber.Text.Trim(), _txtVehicleType.Text.Trim(),
                _numCapacityQty.Value, _numCapacityKg.Value, _numCapacityM3.Value);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi khi lưu: " + ex.Message;
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }
}
