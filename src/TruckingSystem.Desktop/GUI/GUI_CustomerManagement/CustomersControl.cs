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
            GridLayout.FitColumns(grid); // cột vừa nội dung, nhiều cột thì cuộn ngang thay vì cắt chữ
            Load += (_, _) => Theme.UniformButtons(toolbar); // nút đều nhau, xuống dòng vẫn thẳng cột
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
            filterBar.AddChoice("type", "Loại", new[] { "Doanh nghiệp", "Cá nhân" });
            filterBar.FilterChanged += (_, _) => BindGrid();
            Load += async (_, _) => await LoadAsync();
        }

        public CustomersControl(ApiServices api) : this()
        {
            _api = api;
            btnCreate.Visible = btnEdit.Visible = api.Connection.IsInRole("Admin", "Dispatcher");
            components ??= new System.ComponentModel.Container();
            var menu = new ContextMenuStrip(components);
            menu.Items.Add("Xem người liên hệ", null, (_, _) => ShowContacts());
            menu.Opening += (_, e) => e.Cancel = grid.CurrentRow is null;
            ModernMenu.Apply(menu);
            grid.ContextMenuStrip = menu;
            grid.CellMouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                    grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex >= 0 ? e.ColumnIndex : 1];
            };
        }

        private void ShowContacts()
        {
            if (_api is null || grid.CurrentRow is null) return;
            var id = (int)grid.CurrentRow.Cells["Id"].Value;
            var customer = _customers.First(c => c.Id == id);
            using var form = new BaseForm
            {
                Text = $"Người liên hệ — {customer.Name}", StartPosition = FormStartPosition.CenterParent,
                Size = new Size(1000, 600), MinimumSize = new Size(700, 450),
                ShowInTaskbar = false, MinimizeBox = false
            };
            var contacts = new CustomerContactsControl(_api, id) { Dock = DockStyle.Fill };
            form.Controls.Add(contacts);
            form.Shown += async (_, _) => await contacts.LoadAsync();
            ButtonStyles.Footer(form, null, FormStyle.NewCloseButton(form));
            Anim.FadeIn(form);
            form.ShowDialog(FindForm());
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

        private static string TypeText(CustomerListItem c) => c.CustomerType == "Company" ? "Doanh nghiệp" : "Cá nhân";

        /// <summary>Đổ lưới theo bộ lọc (tìm theo tên, SĐT, mã số thuế, địa chỉ; lọc loại khách).</summary>
        private void BindGrid()
        {
            var rows = _customers.Where(c => filterBar.Is("type", TypeText(c))
                                             && filterBar.MatchesSearch(c.Name, c.Phone, c.TaxCode, c.Address)).ToList();
            grid.DataSource = rows.Select(c => new
            {
                c.Id,
                Tên_khách_hàng = c.Name,
                Số_điện_thoại = c.Phone ?? "—",
                Loại = TypeText(c),
                Địa_chỉ = c.Address ?? "—",
            }).ToList();
            filterBar.SetCount(rows.Count, _customers.Count);
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.ForeColor = Theme.TextSecondary;
            lblStatus.Text = "Đang tải...";
            try
            {
                _customers = await _api.Customers.GetCustomersAsync();
                BindGrid();
                lblStatus.Text = $"Tổng {_customers.Count} khách hàng — chuột phải để xem người liên hệ";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }
    }
}
