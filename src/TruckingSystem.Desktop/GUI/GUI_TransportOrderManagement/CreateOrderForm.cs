using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;
using TruckingSystem.Desktop.DTO_LocationManagement;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement;

/// <summary>Logic tạo đơn vận chuyển — đơn giản hoá: chỉ 1 dòng hàng (CargoLine) mỗi đơn.
/// Giao diện nằm ở CreateOrderForm.Designer.cs.</summary>
public partial class CreateOrderForm : BaseForm
{
    private ApiServices? _api;
    private List<LocationItem> _locations = new();

    public CreateOrderForm()
    {
        InitializeComponent();

        dtCommitted.Enabled = false;
        chkHasCommittedDate.CheckedChanged += (_, _) => dtCommitted.Enabled = chkHasCommittedDate.Checked;

        cboPickupLocation.SelectedIndexChanged += (_, _) =>
        {
            if (cboPickupLocation.SelectedItem is LocationItem loc && loc.Id != 0) txtPickup.Text = loc.Address;
        };
        cboDropoffLocation.SelectedIndexChanged += (_, _) =>
        {
            if (cboDropoffLocation.SelectedItem is LocationItem loc && loc.Id != 0) txtDropoff.Text = loc.Address;
        };

        Load += async (_, _) => { await LoadCustomersAsync(); await LoadLocationsAsync(); };
    }

    public CreateOrderForm(ApiServices api) : this()
    {
        _api = api;
    }

    private async Task LoadCustomersAsync()
    {
        if (_api is null) return;
        try
        {
            var customers = await _api.Customers.GetCustomersAsync();
            cboCustomer.DataSource = customers;
            cboCustomer.DisplayMember = nameof(CustomerListItem.Name);
            cboCustomer.ValueMember = nameof(CustomerListItem.Id);
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Không tải được danh sách khách hàng: " + ex.Message;
        }
    }

    private async Task LoadLocationsAsync()
    {
        if (_api is null) return;
        try
        {
            _locations = await _api.Locations.GetLocationsAsync();
            var blank = new LocationItem { Id = 0, Name = "-- Không chọn, tự gõ --", Address = "" };

            var pickupSource = new List<LocationItem> { blank };
            pickupSource.AddRange(_locations);
            var dropoffSource = new List<LocationItem> { blank };
            dropoffSource.AddRange(_locations);

            cboPickupLocation.DataSource = pickupSource;
            cboDropoffLocation.DataSource = dropoffSource;
            cboPickupLocation.SelectedIndex = 0;
            cboDropoffLocation.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Không tải được danh sách địa điểm đã lưu: " + ex.Message;
        }
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        if (cboCustomer.SelectedValue is null)
        {
            lblStatus.Text = "Vui lòng chọn khách hàng.";
            return;
        }
        if (string.IsNullOrWhiteSpace(txtPickup.Text) || string.IsNullOrWhiteSpace(txtDropoff.Text))
        {
            lblStatus.Text = "Vui lòng nhập đủ điểm lấy/giao hàng.";
            return;
        }
        if (_api is null) return;

        btnSave.Enabled = false;
        try
        {
            var cargoLine = new CargoLineItem
            {
                CargoType = txtCargoType.Text,
                PackageCount = (int)numPackageCount.Value,
                WeightKg = numWeightKg.Value,
                VolumeM3 = numVolumeM3.Value,
                IsFragile = chkFragile.Checked,
                IsHazmat = chkHazmat.Checked,
                QtyTotal = numQtyTotal.Value,
            };

            await _api.TransportOrders.CreateTransportOrderAsync(
                (int)cboCustomer.SelectedValue,
                txtPickup.Text.Trim(),
                txtDropoff.Text.Trim(),
                chkHasCommittedDate.Checked ? dtCommitted.Value : null,
                new List<CargoLineItem> { cargoLine },
                pickupLocationId: (cboPickupLocation.SelectedItem as LocationItem)?.Id is int pid && pid != 0 ? pid : null,
                dropoffLocationId: (cboDropoffLocation.SelectedItem as LocationItem)?.Id is int did && did != 0 ? did : null
            );

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi khi lưu: " + ex.Message;
        }
        finally
        {
            btnSave.Enabled = true;
        }
    }
}
