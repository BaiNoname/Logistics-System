using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_InvoiceManagement;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement;

public partial class CreateInvoiceForm : BaseForm
{
    private readonly ApiServices? _api;
    private bool _saving;
    private bool _loadingOrders;
    private int _quoteVersion;
    private InvoiceQuote? _quote;

    public CreateInvoiceForm()
    {
        InitializeComponent();
        Theme.StyleGrid(gridQuote);
        cboOrder.SelectedIndexChanged += async (_, _) =>
        {
            lblRoute.Text = cboOrder.SelectedItem is InvoiceOrderOption order
                ? $"Lấy hàng: {order.PickupPoint}\nGiao hàng: {order.DropoffPoint}" : "";
            if (!_loadingOrders) await LoadQuoteAsync();
        };
        btnRecalculate.Click += async (_, _) =>
        {
            if (cboOrder.Items.Count == 0) await LoadOrdersAsync();
            else await LoadQuoteAsync();
        };
        Shown += async (_, _) => await LoadOrdersAsync();
        FormClosing += (_, e) => e.Cancel = _saving;
    }

    public CreateInvoiceForm(ApiServices api) : this() => _api = api;

    private async Task LoadOrdersAsync()
    {
        if (_api is null) return;
        btnSave.Enabled = false;
        btnRecalculate.Enabled = false;
        lblStatus.ForeColor = Theme.TextSecondary;
        lblStatus.Text = "Đang tải đơn hàng...";
        try
        {
            _loadingOrders = true;
            var orders = await _api.Invoices.GetEligibleOrdersAsync();
            if (IsDisposed) return;
            cboOrder.DataSource = orders;
            _loadingOrders = false;
            await LoadQuoteAsync();
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            lblStatus.ForeColor = Theme.Danger;
            lblStatus.Text = "Lỗi: " + ex.Message;
        }
        finally
        {
            _loadingOrders = false;
            if (!IsDisposed) btnRecalculate.Enabled = true;
        }
    }

    private async Task LoadQuoteAsync()
    {
        if (_api is null || _saving || IsDisposed) return;
        var version = ++_quoteVersion;
        _quote = null;
        btnSave.Enabled = false;
        numAmount.Value = 0;
        gridQuote.DataSource = null;
        lblPricing.Text = lblOperatingCosts.Text = "";
        if (cboOrder.SelectedItem is not InvoiceOrderOption order)
        {
            FormStyle.Status(lblStatus, "Không có đơn đã hoàn thành đang chờ lập hóa đơn.", StatusKind.Warning);
            return;
        }
        lblStatus.ForeColor = Theme.TextSecondary;
        lblStatus.Text = "Đang tính tiền theo hợp đồng/phụ lục...";
        try
        {
            var quote = await _api.Invoices.GetQuoteAsync(order.Id)
                ?? throw new InvalidOperationException("Chưa nhận được bảng tính hóa đơn.");
            if (IsDisposed || version != _quoteVersion) return;
            if (quote.TransportOrderId != order.Id || quote.TotalAmount <= 0 || quote.TotalAmount > numAmount.Maximum)
                throw new InvalidOperationException("Bảng tính hóa đơn không hợp lệ. Vui lòng tính lại.");
            _quote = quote;
            lblPricing.Text = $"Hợp đồng: {quote.ContractCode}"
                + (quote.AppendixCode is null ? "" : $" · Phụ lục: {quote.AppendixCode}")
                + $"\nNgày bắt đầu vận chuyển: {quote.PricingDate:dd/MM/yyyy} · Loại xe theo giá: {quote.VehicleType}"
                + $"\nQuãng đường GPS: {quote.DistanceKm?.ToString("N3") ?? "Chưa đủ dữ liệu"} km · Khối lượng: {quote.WeightTons:N5} tấn";
            gridQuote.DataSource = quote.Lines;
            lblOperatingCosts.Text = $"Đối chiếu vận hành — không cộng vào hóa đơn:\n"
                + $"Nhiên liệu tiêu hao ước tính: {quote.EstimatedFuelLiters?.ToString("N2") ?? "Chưa đủ dữ liệu"} lít"
                + $" · Chi phí khác: {quote.OtherCosts:N2} đ";
            numAmount.Value = quote.TotalAmount;
            btnSave.Enabled = true;
            lblStatus.Text = quote.Note ?? "Tiền được tính tự động. Hóa đơn lưu ở trạng thái Nháp.";
        }
        catch (Exception ex)
        {
            if (IsDisposed || version != _quoteVersion) return;
            _quote = null;
            lblStatus.ForeColor = Theme.Danger;
            lblStatus.Text = "Lỗi: " + ex.Message + " Chọn Tính lại sau khi kiểm tra dữ liệu.";
        }
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_api is null || _saving) return;
        if (cboOrder.SelectedItem is not InvoiceOrderOption order)
        {
            FieldHint.Show(cboOrder, lblStatus, "Vui lòng chọn đơn hàng.");
            return;
        }
        if (_quote is null || _quote.TransportOrderId != order.Id)
        {
            FormStyle.Status(lblStatus, "Vui lòng bấm Tính lại trước khi lưu hóa đơn.");
            btnRecalculate.Focus();
            return;
        }
        if (dtDueDate.Checked && dtDueDate.Value.Date < DateTime.UtcNow.AddHours(7).Date)
        {
            FieldHint.Show(dtDueDate, lblStatus, "Hạn thanh toán không được trước ngày lập hóa đơn.");
            return;
        }

        _saving = true;
        btnSave.Enabled = btnCancel.Enabled = btnRecalculate.Enabled = fields.Enabled = false;
        lblStatus.ForeColor = Theme.TextSecondary;
        lblStatus.Text = "Đang lưu hóa đơn...";
        try
        {
            var invoice = await _api.Invoices.CreateAsync(order.Id, _quote.TotalAmount,
                dtDueDate.Checked ? dtDueDate.Value.Date : null);
            if (invoice is null || invoice.Id <= 0)
                throw new InvalidOperationException("Chưa nhận được xác nhận lưu hóa đơn. Hãy làm mới danh sách để kiểm tra.");
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _quote = null;
            lblStatus.ForeColor = Theme.Danger;
            lblStatus.Text = "Lỗi: " + ex.Message + " Chọn Tính lại trước khi lưu tiếp.";
        }
        finally
        {
            _saving = false;
            if (!IsDisposed)
            {
                btnSave.Enabled = _quote is not null;
                btnCancel.Enabled = btnRecalculate.Enabled = fields.Enabled = true;
            }
        }
    }
}
