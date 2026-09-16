using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;
using TruckingSystem.Desktop.DTO_LocationManagement;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement;

/// <summary>Form tạo đơn vận chuyển mới — đơn giản hoá: chỉ 1 dòng hàng (CargoLine) mỗi đơn.
/// Hỗ trợ chọn nhanh địa điểm đã lưu (Location) để tự điền text, hoặc gõ tay như trước.</summary>
public class CreateOrderForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly Button _btnSave;
    private readonly ComboBox _cboCustomer = new() { Left = 150, Top = 20, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };

    private readonly TextBox _txtPickup = new() { Left = 150, Top = 55, Width = 300 };
    private readonly ComboBox _cboPickupLocation = new() { Left = 150, Top = 88, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };

    private readonly TextBox _txtDropoff = new() { Left = 150, Top = 123, Width = 300 };
    private readonly ComboBox _cboDropoffLocation = new() { Left = 150, Top = 156, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };

    private readonly DateTimePicker _dtCommitted = new() { Left = 150, Top = 195, Width = 200 };
    private readonly CheckBox _chkHasCommittedDate = new() { Left = 360, Top = 198, Text = "Có hẹn giao" };

    private readonly TextBox _txtCargoType = new() { Left = 150, Top = 240, Width = 300, Text = "Hàng khô đóng kiện" };
    private readonly NumericUpDown _numPackageCount = new() { Left = 150, Top = 275, Width = 120, Maximum = 100000 };
    private readonly NumericUpDown _numWeightKg = new() { Left = 150, Top = 310, Width = 120, Maximum = 1000000, DecimalPlaces = 2 };
    private readonly NumericUpDown _numVolumeM3 = new() { Left = 150, Top = 345, Width = 120, Maximum = 100000, DecimalPlaces = 2 };
    private readonly NumericUpDown _numQtyTotal = new() { Left = 150, Top = 380, Width = 120, Maximum = 1000000, DecimalPlaces = 2 };
    private readonly CheckBox _chkFragile = new() { Left = 300, Top = 277, Text = "Dễ vỡ" };
    private readonly CheckBox _chkHazmat = new() { Left = 300, Top = 310, Text = "Hàng nguy hiểm" };

    private readonly Label _lblStatus = new() { Left = 150, Top = 460, Width = 320, ForeColor = Color.Red };

    private List<LocationItem> _locations = new();

    public CreateOrderForm(ApiServices api)
    {
        _api = api;
        Text = "Tạo đơn vận chuyển mới";
        Width = 500; Height = 560;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        BackColor = Color.White;

        _btnSave = Theme.MakePrimaryButton("Lưu đơn", 150, 38);
        _btnSave.Top = 420; _btnSave.Left = 150;

        Controls.Add(new Label { Left = 15, Top = 23, Width = 130, Text = "Khách hàng:" });
        Controls.Add(new Label { Left = 15, Top = 58, Width = 130, Text = "Điểm lấy hàng:" });
        Controls.Add(new Label { Left = 15, Top = 91, Width = 130, Text = "🔽 chọn nhanh:" });
        Controls.Add(new Label { Left = 15, Top = 126, Width = 130, Text = "Điểm giao hàng:" });
        Controls.Add(new Label { Left = 15, Top = 159, Width = 130, Text = "🔽 chọn nhanh:" });
        Controls.Add(new Label { Left = 15, Top = 198, Width = 130, Text = "Ngày hẹn giao:" });
        Controls.Add(new Label { Left = 15, Top = 243, Width = 130, Text = "Loại hàng:" });
        Controls.Add(new Label { Left = 15, Top = 278, Width = 130, Text = "Số kiện:" });
        Controls.Add(new Label { Left = 15, Top = 313, Width = 130, Text = "Trọng lượng (kg):" });
        Controls.Add(new Label { Left = 15, Top = 348, Width = 130, Text = "Thể tích (m³):" });
        Controls.Add(new Label { Left = 15, Top = 383, Width = 130, Text = "Số lượng (để chia xe):" });

        _dtCommitted.Enabled = false;
        _chkHasCommittedDate.CheckedChanged += (_, _) => _dtCommitted.Enabled = _chkHasCommittedDate.Checked;

        _cboPickupLocation.SelectedIndexChanged += (_, _) =>
        {
            if (_cboPickupLocation.SelectedItem is LocationItem loc && loc.Id != 0) _txtPickup.Text = loc.Address;
        };
        _cboDropoffLocation.SelectedIndexChanged += (_, _) =>
        {
            if (_cboDropoffLocation.SelectedItem is LocationItem loc && loc.Id != 0) _txtDropoff.Text = loc.Address;
        };

        Controls.Add(_cboCustomer);
        Controls.Add(_txtPickup);
        Controls.Add(_cboPickupLocation);
        Controls.Add(_txtDropoff);
        Controls.Add(_cboDropoffLocation);
        Controls.Add(_dtCommitted);
        Controls.Add(_chkHasCommittedDate);
        Controls.Add(_txtCargoType);
        Controls.Add(_numPackageCount);
        Controls.Add(_numWeightKg);
        Controls.Add(_numVolumeM3);
        Controls.Add(_numQtyTotal);
        Controls.Add(_chkFragile);
        Controls.Add(_chkHazmat);
        Controls.Add(_btnSave);
        Controls.Add(_lblStatus);

        _btnSave.Click += BtnSave_Click;
        Load += async (_, _) => { await LoadCustomersAsync(); await LoadLocationsAsync(); };
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            var customers = await _api.Customers.GetCustomersAsync();
            _cboCustomer.DataSource = customers;
            _cboCustomer.DisplayMember = nameof(CustomerListItem.Name);
            _cboCustomer.ValueMember = nameof(CustomerListItem.Id);
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Không tải được danh sách khách hàng: " + ex.Message;
        }
    }

    private async Task LoadLocationsAsync()
    {
        try
        {
            _locations = await _api.Locations.GetLocationsAsync();
            var blank = new LocationItem { Id = 0, Name = "-- Không chọn, tự gõ --", Address = "" };

            var pickupSource = new List<LocationItem> { blank };
            pickupSource.AddRange(_locations);
            var dropoffSource = new List<LocationItem> { blank };
            dropoffSource.AddRange(_locations);

            _cboPickupLocation.DataSource = pickupSource;
            _cboDropoffLocation.DataSource = dropoffSource;
            _cboPickupLocation.SelectedIndex = 0;
            _cboDropoffLocation.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Không tải được danh sách địa điểm đã lưu: " + ex.Message;
        }
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_cboCustomer.SelectedValue is null)
        {
            _lblStatus.Text = "Vui lòng chọn khách hàng.";
            return;
        }
        if (string.IsNullOrWhiteSpace(_txtPickup.Text) || string.IsNullOrWhiteSpace(_txtDropoff.Text))
        {
            _lblStatus.Text = "Vui lòng nhập đủ điểm lấy/giao hàng.";
            return;
        }

        _btnSave.Enabled = false;
        try
        {
            var cargoLine = new CargoLineItem
            {
                CargoType = _txtCargoType.Text,
                PackageCount = (int)_numPackageCount.Value,
                WeightKg = _numWeightKg.Value,
                VolumeM3 = _numVolumeM3.Value,
                IsFragile = _chkFragile.Checked,
                IsHazmat = _chkHazmat.Checked,
                QtyTotal = _numQtyTotal.Value,
            };

            await _api.TransportOrders.CreateTransportOrderAsync(
                (int)_cboCustomer.SelectedValue,
                _txtPickup.Text.Trim(),
                _txtDropoff.Text.Trim(),
                _chkHasCommittedDate.Checked ? _dtCommitted.Value : null,
                new List<CargoLineItem> { cargoLine },
                pickupLocationId: (_cboPickupLocation.SelectedItem as LocationItem)?.Id is int pid && pid != 0 ? pid : null,
                dropoffLocationId: (_cboDropoffLocation.SelectedItem as LocationItem)?.Id is int did && did != 0 ? did : null
            );

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
