using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_InvoiceManagement;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement;

public partial class CreateInvoiceForm : BaseForm
{
    private readonly ApiServices? _api;
    private bool _saving;

    public CreateInvoiceForm()
    {
        InitializeComponent();
        cboOrder.SelectedIndexChanged += (_, _) =>
        {
            lblRoute.Text = cboOrder.SelectedItem is InvoiceOrderOption order
                ? $"Lấy hàng: {order.PickupPoint}\nGiao hàng: {order.DropoffPoint}" : "";
        };
        Shown += async (_, _) => await LoadOrdersAsync();
        FormClosing += (_, e) => e.Cancel = _saving;
    }

    public CreateInvoiceForm(ApiServices api) : this() => _api = api;

    private async Task LoadOrdersAsync()
    {
        if (_api is null) return;
        btnSave.Enabled = false;
        lblStatus.ForeColor = Theme.TextSecondary;
        lblStatus.Text = "Đang tải đơn hàng...";
        try
        {
            cboOrder.DataSource = await _api.Invoices.GetEligibleOrdersAsync();
            btnSave.Enabled = cboOrder.Items.Count > 0;
            lblStatus.Text = btnSave.Enabled
                ? "Nhập cước phí đã chốt với khách hàng. Hóa đơn sẽ được lưu ở trạng thái Nháp."
                : "Không có đơn đã hoàn thành đang chờ lập hóa đơn.";
        }
        catch (Exception ex)
        {
            lblStatus.ForeColor = Theme.Danger;
            lblStatus.Text = "Lỗi: " + ex.Message;
        }
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_api is null || _saving) return;
        lblStatus.ForeColor = Theme.Danger;
        if (cboOrder.SelectedItem is not InvoiceOrderOption order)
        {
            lblStatus.Text = "Vui lòng chọn đơn hàng.";
            cboOrder.Focus();
            return;
        }
        if (numAmount.Value <= 0)
        {
            lblStatus.Text = "Cước phí phải lớn hơn 0.";
            numAmount.Focus();
            return;
        }
        if (dtDueDate.Checked && dtDueDate.Value.Date < DateTime.UtcNow.AddHours(7).Date)
        {
            lblStatus.Text = "Hạn thanh toán không được trước ngày lập hóa đơn.";
            dtDueDate.Focus();
            return;
        }

        _saving = true;
        btnSave.Enabled = btnCancel.Enabled = fields.Enabled = false;
        lblStatus.ForeColor = Theme.TextSecondary;
        lblStatus.Text = "Đang lưu hóa đơn...";
        try
        {
            var invoice = await _api.Invoices.CreateAsync(order.Id, numAmount.Value,
                dtDueDate.Checked ? dtDueDate.Value.Date : null);
            if (invoice is null || invoice.Id <= 0)
                throw new InvalidOperationException("Chưa nhận được xác nhận lưu hóa đơn. Hãy làm mới danh sách để kiểm tra.");
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblStatus.ForeColor = Theme.Danger;
            lblStatus.Text = "Lỗi: " + ex.Message;
        }
        finally
        {
            _saving = false;
            btnSave.Enabled = btnCancel.Enabled = fields.Enabled = true;
        }
    }
}
