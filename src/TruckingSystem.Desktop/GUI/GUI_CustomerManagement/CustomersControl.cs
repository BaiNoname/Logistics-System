using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;

namespace TruckingSystem.Desktop.GUI_CustomerManagement
{
    public partial class CustomersControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<CustomerListItem> _customers = new();

        public CustomersControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
            Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
            Load += async (_, _) => await LoadAsync();
        }

        public CustomersControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateCustomerForm(_api);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 khách hàng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;
            var customer = _customers.First(c => c.Id == id);

            using var f = new EditCustomerForm(_api, customer);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _customers = await _api.Customers.GetCustomersAsync();
                grid.DataSource = _customers.Select(c => new
                {
                    c.Id,
                    Tên_khách_hàng = c.Name,
                    Số_điện_thoại = c.Phone ?? "—",
                    Loại = c.CustomerType == "Company" ? "Doanh nghiệp" : "Cá nhân",
                    Địa_chỉ = c.Address ?? "—",
                }).ToList();
                lblStatus.Text = $"Tổng {_customers.Count} khách hàng — chọn 1 dòng rồi bấm Sửa để chỉnh sửa";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }
    }
}
