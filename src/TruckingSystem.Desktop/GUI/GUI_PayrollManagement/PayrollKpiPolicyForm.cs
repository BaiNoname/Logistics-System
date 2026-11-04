using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;

namespace TruckingSystem.Desktop.GUI_PayrollManagement;

public sealed class PayrollKpiPolicyForm : BaseForm
{
    public PayrollKpiPolicyForm(List<KpiPolicyDto> history, List<string> types, Func<SaveKpiPolicyRequest, Task<PayrollResult>> save)
    {
        Text = "KPI — K chung, điểm chuẩn chung và H theo loại xe"; Size = new Size(920, 580); StartPosition = FormStartPosition.CenterParent;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var hint = new Label { AutoSize = true, Dock = DockStyle.Fill, Text = "Lương tài xế = LCB × (tổng điểm / điểm chuẩn), không giới hạn 100%.\nĐiểm chuyến = (0,5 + km/K) × H. Không chia LCB theo số ngày làm.\nCấu hình có hiệu lực từ đầu tháng. Lưu cùng ngày hiệu lực sẽ cập nhật cấu hình; phiếu đã chốt giữ nguyên." };
        var fields = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 12, 0, 12) };
        var historyBox = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, DataSource = history,
            DisplayMember = "EffectiveFrom", FormatString = "dd/MM/yyyy" };
        var date = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short,
            Value = new DateTime(DateTime.UtcNow.AddHours(7).Year, DateTime.UtcNow.AddHours(7).Month, 1) };
        var km = new NumericUpDown { Minimum = 0.01m, Maximum = 1_000_000, DecimalPlaces = 2, Value = 200, Width = 100 };
        var target = new NumericUpDown { Maximum = 1_000_000, DecimalPlaces = 2, Width = 100 };
        fields.Controls.AddRange(new Control[] { new Label { Text = "Lịch sử", AutoSize = true }, historyBox,
            new Label { Text = "Hiệu lực", AutoSize = true }, date, new Label { Text = "K", AutoSize = true }, km,
            new Label { Text = "Điểm chuẩn", AutoSize = true }, target });
        var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false };
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "VehicleType", DataPropertyName = "VehicleType", HeaderText = "Loại xe", ReadOnly = true });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Factor", DataPropertyName = "Factor", HeaderText = "Hệ số H" });
        Theme.StyleGridLook(grid); GridLayout.FitColumns(grid);
        void ShowPolicy(KpiPolicyDto? policy)
        {
            if (policy is not null) { date.Value = policy.EffectiveFrom; km.Value = policy.KmPerPoint; target.Value = policy.TargetPoints; }
            grid.DataSource = types.Concat(policy?.Factors.Select(f => f.VehicleType) ?? Enumerable.Empty<string>()).Distinct().Order()
                .Select(type => new KpiFactorDto { VehicleType = type, Factor = policy?.Factors.SingleOrDefault(f => f.VehicleType == type)?.Factor ?? 0 }).ToList();
        }
        historyBox.SelectedIndexChanged += (_, _) => ShowPolicy(historyBox.SelectedItem as KpiPolicyDto);
        ShowPolicy(history.FirstOrDefault());
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
        var close = Theme.MakeSecondaryButton("Đóng"); close.DialogResult = DialogResult.Cancel;
        var submit = Theme.MakePrimaryButton("Lưu cấu hình"); buttons.Controls.AddRange(new Control[] { close, submit });
        layout.Controls.Add(hint, 0, 0); layout.Controls.Add(fields, 0, 1); layout.Controls.Add(grid, 0, 2); layout.Controls.Add(buttons, 0, 3); Controls.Add(layout);
        CancelButton = close;
        var saving = false; var invalid = false;
        grid.DataError += (_, e) => { invalid = true; e.ThrowException = false; MessageBox.Show(this, "Hệ số H phải là số hợp lệ."); };
        FormClosing += (_, e) => { if (saving) e.Cancel = true; };
        submit.Click += async (_, _) =>
        {
            invalid = false; if (!grid.EndEdit() || invalid) return;
            var factors = (List<KpiFactorDto>)grid.DataSource;
            BindingContext?[factors]?.EndCurrentEdit();
            saving = true; Enabled = false;
            try { var result = await save(new(date.Value.Date, km.Value, target.Value, factors));
                if (!result.Success) throw new InvalidOperationException(result.Message);
                saving = false; DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Cấu hình KPI", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { saving = false; Enabled = true; }
        };
    }
}
