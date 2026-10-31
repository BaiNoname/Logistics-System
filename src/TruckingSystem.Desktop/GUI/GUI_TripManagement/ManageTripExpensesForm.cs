using System.Data;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>Danh sách chi phí KHÁC ngoài xăng dầu (phí cầu đường/bến bãi/
    /// bốc xếp/phạt...) đã ghi nhận cho 1 chuyến — dùng để tính ĐỦ tổng chi
    /// phí thực tế của chuyến, không chỉ riêng xăng.</summary>
    public partial class ManageTripExpensesForm : BaseForm
    {
        private ApiServices? _api;
        private int _tripId;
        private List<TripExpenseItem> _expenses = new();

        public ManageTripExpensesForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this); // Esc = đóng
            Theme.StyleGrid(grid);
            cboType.Items.AddRange(new object[]
            {
                new ComboOption("Toll", "Phí cầu đường"),
                new ComboOption("Parking", "Phí bến bãi"),
                new ComboOption("Loading", "Phí bốc xếp"),
                new ComboOption("Fine", "Tiền phạt"),
                new ComboOption("Other", "Chi phí khác"),
            });
            cboType.SelectedIndex = 0;
        }

        public ManageTripExpensesForm(ApiServices api, int tripId, string tripLabel) : this()
        {
            _api = api;
            _tripId = tripId;
            Text = $"Chi phí khác — {tripLabel}";
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _expenses = await _api.Trips.GetExpensesAsync(_tripId);
                grid.DataSource = _expenses.Select(e => new
                {
                    e.Id,
                    Loại_chi_phí = TranslateType(e.Type),
                    Số_tiền = e.Amount.ToString("N0") + " đ",
                    Ghi_chú = e.Note ?? "—",
                }).ToList();
                grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };

                var total = _expenses.Sum(e => e.Amount);
                lblStatus.Text = $"{_expenses.Count} khoản — Tổng: {total:N0} đ";
                lblStatus.ForeColor = Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private async void BtnAddExpense_Click(object? sender, EventArgs e)
        {
            if (_api is null || !btnAddExpense.Enabled) return;
            if (numAmount.Value <= 0)
            {
                lblStatus.Text = "Số tiền phải lớn hơn 0.";
                lblStatus.ForeColor = Theme.Warning;
                return;
            }
            if ((cboType.SelectedItem as ComboOption)?.Value == "Other" && string.IsNullOrWhiteSpace(txtNote.Text))
            {
                lblStatus.Text = "Chi phí loại \"Chi phí khác\" phải ghi chú là chi cho việc gì.";
                lblStatus.ForeColor = Theme.Warning;
                return;
            }
            // Khóa nút trong lúc chờ server — bấm 2 lần trước đây tạo 2 khoản chi trùng.
            btnAddExpense.Enabled = false;
            try
            {
                await _api.Trips.CreateExpenseAsync(_tripId, (cboType.SelectedItem as ComboOption)?.Value ?? "Other", numAmount.Value,
                    string.IsNullOrWhiteSpace(txtNote.Text) ? null : txtNote.Text.Trim());
                numAmount.Value = 0;
                txtNote.Text = "";
                await LoadAsync();
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally
            {
                if (!IsDisposed) btnAddExpense.Enabled = true;
            }
        }

        private async void BtnDeleteExpense_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 khoản chi phí trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;

            var confirm = MessageBox.Show(FindForm(), "Xóa hẳn khoản chi phí này? Không thể hoàn tác.",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api.Trips.DeleteExpenseAsync(id);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string TranslateType(string s) => s switch
        {
            "Toll" => "🛣️ Phí cầu đường",
            "Parking" => "🅿️ Phí bến bãi",
            "Loading" => "📦 Phí bốc xếp",
            "Fine" => "🚔 Tiền phạt",
            _ => "❓ Khác",
        };
    }
}
