using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;
using TruckingSystem.Desktop.SERVICE_PayrollManagement;

namespace TruckingSystem.Desktop.GUI_PayrollManagement;

/// <summary>Thêm cùng một khoản phụ cấp/thưởng cho nhiều tài xế trong một tháng.</summary>
public sealed class BulkDriverBonusForm : BaseForm
{
    private static readonly DateTime MinDate = new(2000, 1, 1);
    private static readonly DateTime MaxDate = new(2100, 12, 31);

    private sealed record KindItem(string Kind, string Label)
    {
        public override string ToString() => Label;
    }

    public BulkDriverBonusForm(PayrollApiService api, IReadOnlyList<(int Id, string Code, string FullName)> drivers, IReadOnlyList<string> excludedNames)
    {
        Text = $"Thưởng / phụ cấp cho {drivers.Count} tài xế"; Size = new Size(760, 640); StartPosition = FormStartPosition.CenterParent;

        // --- Thanh nhập ---
        var month = new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy", ShowUpDown = true, Width = 90,
            MinDate = MinDate, MaxDate = MaxDate, Value = DateTime.Today,
        };
        var kind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        kind.Items.Add(new KindItem("Allowance", "Phụ cấp"));
        kind.Items.Add(new KindItem("Bonus", "Thưởng"));
        kind.SelectedIndex = 0;
        var amount = new NumericUpDown
        {
            Width = 140, Minimum = 0, Maximum = 1_000_000_000, DecimalPlaces = 0, ThousandsSeparator = true, Increment = 10_000,
            TextAlign = HorizontalAlignment.Right,
        };
        var reason = new TextBox { Width = 300, PlaceholderText = "Lý do" };
        Label Caption(string t) => new() { Text = t, AutoSize = true, Margin = new Padding(0, 8, 6, 0) };

        var row1 = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        row1.Controls.AddRange(new Control[] { Caption("Tháng"), month, Caption("Loại"), kind, Caption("Số tiền (đ)"), amount });
        var row2 = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 6, 0, 0) };
        row2.Controls.AddRange(new Control[] { Caption("Lý do"), reason });
        var inputBox = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(8) };
        inputBox.Controls.AddRange(new Control[] { row1, row2 });
        if (excludedNames.Count > 0)
            inputBox.Controls.Add(new Label
            {
                AutoSize = true, MaximumSize = new Size(700, 0), Margin = new Padding(0, 8, 0, 0), ForeColor = Theme.Danger,
                Text = $"Đã loại {excludedNames.Count} nhân viên không phải tài xế: {string.Join(", ", excludedNames)}",
            });

        // --- Lưới tài xế ---
        var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false };
        grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Pick", HeaderText = "Chọn", Width = 60, TrueValue = true, FalseValue = false });
        foreach (var (name, title) in new[] { ("Code", "Mã"), ("FullName", "Họ tên") })
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = name, HeaderText = title, ReadOnly = true });
        Theme.StyleGrid(grid); GridLayout.FitColumns(grid);
        grid.Columns["Pick"].ReadOnly = false;
        foreach (var d in drivers)
        {
            var i = grid.Rows.Add(true, d.Code, d.FullName);
            grid.Rows[i].Tag = d.Id;
        }
        // Tick ngay khi bấm (không chờ rời ô)
        grid.CurrentCellDirtyStateChanged += (_, _) => { if (grid.IsCurrentCellDirty) grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };

        Controls.Add(grid); Controls.Add(inputBox);
        BackColor = Theme.CardBg;
        MinimumSize = new Size(620, 460);
        var save = Theme.MakePrimaryButton("Lưu", 120);
        FormStyle.SaveButton(save);
        ButtonStyles.Footer(this, null, FormStyle.NewCloseButton(this), save);
        DialogKeys.Enable(this, save);
        Anim.FadeIn(this);
        Load += (_, _) => Theme.UniformButtons(row1);

        var busy = false;
        async Task Run(Func<Task> work)
        {
            if (busy) return;
            busy = true; Enabled = false;
            try { await work(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Thưởng / phụ cấp", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { busy = false; Enabled = true; }
        }
        FormClosing += (_, e) => { if (busy) e.Cancel = true; };

        save.Click += async (_, _) =>
        {
            if (busy) return;
            grid.EndEdit();
            var picked = grid.Rows.Cast<DataGridViewRow>()
                .Where(r => r.Cells["Pick"].Value is true && r.Tag is int)
                .Select(r => (Id: (int)r.Tag!, Name: Convert.ToString(r.Cells["FullName"].Value) ?? ""))
                .ToList();
            if (picked.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn ít nhất một tài xế.", "Thưởng / phụ cấp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (kind.SelectedItem is not KindItem selectedKind)
            {
                MessageBox.Show(this, "Vui lòng chọn loại khoản.", "Thưởng / phụ cấp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var reasonText = reason.Text.Trim();
            if (reasonText.Length == 0)
            {
                MessageBox.Show(this, "Vui lòng nhập lý do.", "Thưởng / phụ cấp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (amount.Value <= 0)
            {
                MessageBox.Show(this, "Số tiền phải lớn hơn 0.", "Thưởng / phụ cấp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var year = month.Value.Year; var mon = month.Value.Month;
            var saved = false;
            await Run(async () =>
            {
                // Cảnh báo trùng: đã có khoản cùng lý do trong tháng
                var existing = await api.MonthlyAdjustmentsAsync(year, mon);
                var pickedIds = picked.Select(p => p.Id).ToHashSet();
                var duplicates = existing
                    .Where(a => pickedIds.Contains(a.EmployeeId) && string.Equals(a.Reason?.Trim(), reasonText, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.EmployeeName).Distinct().ToList();
                if (duplicates.Count > 0 && MessageBox.Show(this,
                        $"Các tài xế sau đã có khoản cùng lý do \"{reasonText}\" trong tháng {mon:00}/{year}:{Environment.NewLine}{string.Join(", ", duplicates)}{Environment.NewLine}{Environment.NewLine}Vẫn thêm khoản mới?",
                        "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                var result = await api.AddMonthlyAdjustmentsBulkAsync(
                    new(picked.Select(p => p.Id).ToList(), year, mon, selectedKind.Kind, amount.Value, reasonText));
                if (!result.Success) throw new InvalidOperationException(result.Message ?? "Không thể thực hiện.");
                if (!string.IsNullOrWhiteSpace(result.Message))
                    MessageBox.Show(this, result.Message, "Thưởng / phụ cấp", MessageBoxButtons.OK, MessageBoxIcon.Information);
                saved = true;
            });
            if (saved) DialogResult = DialogResult.OK; // sau khi busy đã được xóa, để FormClosing không hủy
        };
    }
}
