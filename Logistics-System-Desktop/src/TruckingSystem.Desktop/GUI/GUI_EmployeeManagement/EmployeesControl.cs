using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_EmployeeManagement;
using TruckingSystem.Desktop.GUI_LeaveManagement;

namespace TruckingSystem.Desktop.GUI_EmployeeManagement
{
    public partial class EmployeesControl : BaseUserControl
    {
        private ApiServices? _api;
        private bool _loading;
        private List<EmployeeListItem> _employees = new();
        private readonly FilterBar _filters = new() { Dock = DockStyle.Fill, SearchPlaceholder = "Tìm mã nhân viên..." };

        public EmployeesControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            bar.Controls.AddRange(new Control[] { btnCreate, btnRefresh });
            btnCreate.Text = "➕ Thêm nhân viên";
            foreach (var button in bar.Controls.OfType<Button>())
            {
                button.AutoSize = true;
                button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                button.MinimumSize = new Size(150, 38);
            }
            ButtonStyles.Secondary(btnRefresh);
            lblTitle.AutoSize = true;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.Padding = new Padding(0, 6, 0, 6);
            grid.Dock = DockStyle.Fill;
            _filters.AddDate("hireFrom", "Tuyển dụng từ");
            _filters.AddDate("hireTo", "Tuyển dụng đến");
            _filters.AddChoice("department", "Phòng ban", Array.Empty<string>());
            _filters.AddChoice("position", "Chức danh", Array.Empty<string>());
            _filters.AddChoice("status", "Trạng thái", new[] { "Đang làm", "Nghỉ phép", "Đã nghỉ việc" });
            _filters.FilterChanged += (_, _) => BindEmployees();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(16) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(lblTitle, 0, 0);
            layout.Controls.Add(bar, 0, 1);
            layout.Controls.Add(_filters, 0, 2);
            layout.Controls.Add(lblStatus, 0, 3);
            layout.Controls.Add(grid, 0, 4);
            Controls.Add(layout);
            GridDecor.Pills(grid, "Trạng_thái", (text, _) => text switch
            {
                "Đang làm" => GridDecor.Green, "Nghỉ phép" => GridDecor.Amber, _ => GridDecor.Slate,
            });
            Load += (_, _) => Theme.UniformButtons(bar);
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
            grid.MultiSelect = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            var menu = new ContextMenuStrip();
            var editItem = menu.Items.Add("✏️ Sửa nhân viên", null, BtnEdit_Click);
            var leaveItem = menu.Items.Add("📅 Ngày nghỉ…", null, BtnLeave_Click);
            var bonusItem = menu.Items.Add("💰 Thưởng/phụ cấp…", null, BtnBulkBonus_Click);
            menu.Opening += (_, e) =>
            {
                var selected = SelectedEmployees();
                var single = selected.Count == 1;
                var driverCount = selected.Count(x => x.IsDriver);
                var admin = _api is not null && _api.Connection.IsInRole("Admin");
                // Không đọc lại ToolStripItem.Visible ở đây: getter trả false khi menu chưa hiện → menu luôn bị hủy.
                var showSingle = single && admin;
                var showBonus = admin && driverCount > 0;
                editItem.Visible = leaveItem.Visible = showSingle;
                bonusItem.Visible = showBonus;
                bonusItem.Text = $"💰 Thưởng/phụ cấp cho {driverCount} tài xế đã chọn…";
                e.Cancel = _api is null || _loading || !(showSingle || showBonus);
            };
            ModernMenu.Apply(menu);
            grid.ContextMenuStrip = menu;
            Disposed += (_, _) => menu.Dispose();
            grid.CellMouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
                if (grid.Rows[e.RowIndex].Selected) return; // giữ nguyên vùng chọn
                grid.ClearSelection();
                grid.Rows[e.RowIndex].Selected = true;
                grid.CurrentCell = grid.Rows[e.RowIndex].Cells["Mã_nhân_viên"];
            };
            Load += async (_, _) => await LoadAsync();
        }

        public EmployeesControl(ApiServices api) : this()
        {
            _api = api;
            btnCreate.Visible = api.Connection.IsInRole("Admin");
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateEmployeeForm(_api);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        /// <summary>Các nhân viên đang được chọn trên lưới (theo thứ tự hiển thị).</summary>
        private List<EmployeeListItem> SelectedEmployees()
        {
            var ids = grid.SelectedRows.Cast<DataGridViewRow>()
                .OrderBy(r => r.Index)
                .Select(r => r.Cells["Id"].Value)
                .OfType<int>()
                .ToList();
            return ids.Select(id => _employees.FirstOrDefault(x => x.Id == id)).OfType<EmployeeListItem>().ToList();
        }

        private void BtnLeave_Click(object? sender, EventArgs e)
        {
            if (_api is null || _loading) return;
            var selected = SelectedEmployees();
            if (selected.Count != 1)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn đúng 1 nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var employee = selected[0];
            using var f = new LeaveForm(_api.Leaves, employee.Id, employee.Code, employee.FullName);
            f.ShowDialog(FindForm());
        }

        private void BtnBulkBonus_Click(object? sender, EventArgs e)
        {
            if (_api is null || _loading || !_api.Connection.IsInRole("Admin")) return;
            var selected = SelectedEmployees();
            var drivers = selected.Where(x => x.IsDriver).Select(x => (x.Id, x.Code, x.FullName)).ToList();
            if (drivers.Count == 0) return;
            var excluded = selected.Where(x => !x.IsDriver).Select(x => x.FullName).ToList();
            using var f = new GUI_PayrollManagement.BulkDriverBonusForm(_api.Payroll, drivers, excluded);
            f.ShowDialog(FindForm());
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (_api is null || _loading || !_api.Connection.IsInRole("Admin")) return;
            var selected = SelectedEmployees();
            if (selected.Count != 1)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn đúng 1 nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var employee = selected[0];
            using var f = new CreateEmployeeForm(_api, employee);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private void BtnSchedule_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 nhân viên trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var employeeId = (int)grid.CurrentRow.Cells["Id"].Value;
            var employee = _employees.First(e => e.Id == employeeId);

            using var f = new ManageDriverScheduleForm(_api, employeeId, employee.FullName);
            f.ShowDialog(FindForm());
        }

        private async void BtnMakeDriver_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 nhân viên trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var employeeId = (int)grid.CurrentRow.Cells["Id"].Value;
            var employee = _employees.First(e => e.Id == employeeId);

            // Form tự phát hiện nhân viên ĐÃ có hồ sơ hay chưa và tự chuyển
            // Tạo mới / Sửa cho đúng — không cần chặn ở đây nữa.
            using var f = new CreateDriverProfileForm(_api, employeeId, employee.FullName);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private static string StatusText(string status) => status switch
        {
            "Active" => "Đang làm", "OnLeave" => "Nghỉ phép", "Terminated" => "Đã nghỉ việc", _ => status,
        };

        private void BindEmployees(int? selectedId = null)
        {
            selectedId ??= grid.CurrentRow?.Cells["Id"].Value as int?;
            var from = _filters.Date("hireFrom");
            var to = _filters.Date("hireTo");
            var employees = _employees.Where(e => _filters.MatchesSearch(e.Code)
                && (!from.HasValue || e.HireDate.Date >= from.Value)
                && (!to.HasValue || e.HireDate.Date <= to.Value)
                && _filters.Is("department", e.DepartmentName ?? "—")
                && _filters.Is("position", e.PositionName ?? "—")
                && _filters.Is("status", StatusText(e.Status))).OrderBy(e => e.Code).ToList();
            grid.DataSource = employees.Select(e => new
            {
                e.Id,
                Mã_nhân_viên = e.Code,
                Họ_tên = e.FullName,
                Ngày_sinh = e.DateOfBirth?.ToString("dd/MM/yyyy") ?? "—",
                Giới_tính = e.Gender switch { "Male" => "Nam", "Female" => "Nữ", "Other" => "Khác", _ => "Chưa cập nhật" },
                Căn_Cước = e.IdentityCardNo ?? "—",
                Điện_thoại = e.Phone ?? "—",
                Địa_chỉ = e.Address ?? "—",
                Ngày_tuyển_dụng = e.HireDate.ToString("dd/MM/yyyy"),
                Phòng_ban = e.DepartmentName ?? "—",
                Chức_danh = e.PositionName ?? "—",
                Trạng_thái = StatusText(e.Status),
            }).ToList();
            foreach (DataGridViewRow row in grid.Rows)
                if ((int)row.Cells["Id"].Value == selectedId) grid.CurrentCell = row.Cells["Mã_nhân_viên"];
            _filters.SetCount(employees.Count, _employees.Count);
            FormStyle.Status(lblStatus, $"Hiển thị {employees.Count}/{_employees.Count} nhân viên", StatusKind.Info);
        }

        private async Task LoadAsync()
        {
            if (_api is null || _loading) return;
            var selectedId = grid.CurrentRow?.Cells["Id"].Value as int?;
            _loading = true;
            _filters.Enabled = false;
            FormStyle.Status(lblStatus, "Đang tải...", StatusKind.Info);
            try
            {
                _employees = await _api.Employees.GetEmployeesAsync();
                if (IsDisposed) return;
                _filters.SetChoiceOptions("department", _employees.Select(e => e.DepartmentName ?? "—"));
                _filters.SetChoiceOptions("position", _employees.Select(e => e.PositionName ?? "—"));
                BindEmployees(selectedId);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally { _loading = false; if (!IsDisposed) _filters.Enabled = true; }
        }
    }
}
