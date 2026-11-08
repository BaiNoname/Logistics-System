using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Desktop.GUI_PayrollManagement;

public sealed class PayrollKpiPolicyForm : BaseForm
{
    public PayrollKpiPolicyForm(List<KpiPolicyDto> history, List<string> types, Func<SaveKpiPolicyRequest, Task<PayrollResult>> save)
    {
        Text = "Cấu hình KPI cho tài xế"; Size = new Size(920, 650); StartPosition = FormStartPosition.CenterParent;
        BackColor = Theme.CardBg;
        MinimumSize = new Size(640, 440);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var hint = new Label { AutoSize = true, Dock = DockStyle.Fill, Text = "Lương tài xế = LCB × (tổng điểm / điểm chuẩn), không giới hạn 100%.\nĐiểm chuyến = (0,5 + km/K) × H. Không chia LCB theo số ngày làm.\nkm: Số km chạy được trong tháng.\nK: Số km để được 1 điểm.\nH: Số điểm cộng theo loại xe.\nCấu hình có hiệu lực từ đầu tháng. Lưu cùng ngày hiệu lực sẽ cập nhật cấu hình; phiếu đã chốt giữ nguyên." };
        layout.SizeChanged += (_, _) => hint.MaximumSize = new Size(Math.Max(1, layout.ClientSize.Width - layout.Padding.Horizontal - hint.Margin.Horizontal), 0);
        var fields = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 12, 0, 12) };
        var historyBox = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, DataSource = history,
            DisplayMember = "EffectiveFrom", FormatString = "dd/MM/yyyy" };
        var date = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short,
            Value = new DateTime(DateTime.UtcNow.AddHours(7).Year, DateTime.UtcNow.AddHours(7).Month, 1) };
        var km = new NumberBox { Maximum = 1_000_000, DecimalPlaces = 2, Width = 100 };
        var target = new NumberBox { Maximum = 1_000_000, DecimalPlaces = 2, Width = 100 };
        void Field(string text, Control input)
        {
            var group = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Margin = new Padding(3, 0, 12, 6) };
            group.Controls.Add(new Label { Text = text, AutoSize = true, ForeColor = Theme.TextPrimary });
            input.Dock = DockStyle.Top;
            input.AccessibleName = text.TrimEnd(':');
            group.Controls.Add(input);
            fields.Controls.Add(group);
        }
        Field("Lịch sử:", historyBox);
        Field("Hiệu lực *:", date);
        Field("Km / điểm (K) *:", km);
        Field("Điểm chuẩn *:", target);
        var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false };
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "VehicleType", DataPropertyName = "VehicleType", HeaderText = "Loại xe", ReadOnly = true });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Factor", DataPropertyName = "Factor", HeaderText = "Hệ số H" });
        Theme.StyleGridLook(grid); GridLayout.FitColumns(grid);
        void ShowPolicy(KpiPolicyDto? policy)
        {
            if (policy is not null) { date.Value = policy.EffectiveFrom; km.Value = policy.KmPerPoint; target.Value = policy.TargetPoints; }
            grid.DataSource = TariffRule.DefaultVehicleTypes.Concat(types).Concat(policy?.Factors.Select(f => f.VehicleType) ?? Enumerable.Empty<string>()).Distinct()
                .Select(type => new KpiFactorDto { VehicleType = type, Factor = policy?.Factors.SingleOrDefault(f => f.VehicleType == type)?.Factor ?? 0 }).ToList();
        }
        historyBox.SelectedIndexChanged += (_, _) => ShowPolicy(historyBox.SelectedItem as KpiPolicyDto);
        ShowPolicy(history.FirstOrDefault());
        var close = FormStyle.NewCloseButton(this);
        var submit = Theme.MakePrimaryButton("Lưu cấu hình", 160);
        var status = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.Danger };
        layout.Controls.Add(hint, 0, 0); layout.Controls.Add(fields, 0, 1); layout.Controls.Add(grid, 0, 2); layout.Controls.Add(status, 0, 3); Controls.Add(layout);
        FormStyle.EditorFooter(this, close, submit);
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
        var saving = false; var invalid = false;
        grid.DataError += (_, e) => { invalid = true; e.ThrowException = false; FieldHint.Show(grid, status, "Hệ số H phải là số hợp lệ."); };
        FormClosing += (_, e) => { if (saving) e.Cancel = true; };
        submit.Click += async (_, _) =>
        {
            invalid = false; if (!grid.EndEdit() || invalid) return;
            if (km.Value <= 0) { FieldHint.Show(km, status, "Vui lòng nhập số km / điểm lớn hơn 0."); return; }
            if (target.Value <= 0) { FieldHint.Show(target, status, "Vui lòng nhập điểm chuẩn lớn hơn 0."); return; }
            var factors = (List<KpiFactorDto>)grid.DataSource;
            BindingContext?[factors]?.EndCurrentEdit();
            for (var i = 0; i < factors.Count; i++)
            {
                var factor = factors[i].Factor;
                if (factor > 0 && factor <= 1000 && decimal.Round(factor, 2) == factor) continue;
                grid.CurrentCell = grid.Rows[i].Cells["Factor"];
                FieldHint.Show(grid, status, "Hệ số H phải lớn hơn 0, tối đa 1.000 và có tối đa 2 số lẻ.");
                return;
            }
            saving = true; Enabled = false;
            try { var result = await save(new(date.Value.Date, km.Value, target.Value, factors));
                if (!result.Success) throw new InvalidOperationException(result.Message);
                saving = false; DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Cấu hình KPI", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { saving = false; Enabled = true; }
        };
    }
}
