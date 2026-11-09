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
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
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
                FieldHint.Show(numAmount, lblStatus, "Số tiền phải lớn hơn 0."); // tô đỏ + nhảy vào đúng ô
                return;
            }
            if ((cboType.SelectedItem as ComboOption)?.Value == "Other" && string.IsNullOrWhiteSpace(txtNote.Text))
            {
                FieldHint.Show(cboType, lblStatus, "Chi phí loại \"Chi phí khác\" phải ghi chú là chi cho việc gì."); // tô đỏ + nhảy vào đúng ô
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
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 khoản chi phí trước.", ToastKind.Info);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;

            if (!Ask.Confirm(this, "Xóa khoản chi phí này?", "Khoản chi bị xóa hẳn, không hoàn tác được.", "🗑 Xóa khoản chi", danger: true)) return;

            try
            {
                var result = await _api.Trips.DeleteExpenseAsync(id);
                if (!result.Success)
                {
                    Ask.Fail(this, result.Message ?? "Không thể xóa.");
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
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
