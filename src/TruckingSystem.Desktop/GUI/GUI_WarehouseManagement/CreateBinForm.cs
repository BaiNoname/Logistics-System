using TruckingSystem.Desktop.Common;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Tạo vị trí lưu trữ (StorageBin) mới trong 1 khu vực — khai báo
    /// tọa độ + kích thước THẬT (mét), tương tự cách Khu vực định vị trong
    /// Kho. Backend tự kiểm tra không đè lên vị trí khác và không vượt ra
    /// ngoài ranh giới khu vực.</summary>
    public partial class CreateBinForm : BaseForm
    {
        private ApiServices? _api;
        private int _zoneId;

        public CreateBinForm()
        {
            InitializeComponent();
        }

        public CreateBinForm(ApiServices api, int zoneId, string zoneName, decimal zoneWidth, decimal zoneHeight) : this()
        {
            _api = api;
            _zoneId = zoneId;
            lblZoneInfo.Text = $"Khu vực \"{zoneName}\" — kích thước {zoneWidth}m x {zoneHeight}m (X: 0–{zoneWidth}, Y: 0–{zoneHeight})";
            lblCode.Text = "Mã vị trí * (tự sinh — có thể sửa)";
            Load += async (_, _) => await SuggestCodeAsync(zoneName);
        }

        /// <summary>Tự sinh mã vị trí kế tiếp theo khu vực: "Khu A" -> A-01, A-02...
        /// (lấy số lớn nhất đang có + 1). Server vẫn kiểm tra trùng mã trong kho.</summary>
        private async Task SuggestCodeAsync(string zoneName)
        {
            if (_api is null) return;
            var prefix = ZonePrefix(zoneName);
            try
            {
                var bins = await _api.Warehouses.GetBinsAsync(_zoneId) ?? new();
                var pattern = new System.Text.RegularExpressions.Regex($"^{System.Text.RegularExpressions.Regex.Escape(prefix)}-(\\d+)$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var maxNo = bins
                    .Select(b => pattern.Match(b.Code.Trim()))
                    .Where(m => m.Success)
                    .Select(m => int.Parse(m.Groups[1].Value))
                    .DefaultIfEmpty(0)
                    .Max();
                txtCode.Text = $"{prefix}-{maxNo + 1:00}";
            }
            catch
            {
                txtCode.Text = $"{prefix}-01";
            }
        }

        /// <summary>"Khu A" -> "A"; "Khu dễ vỡ" -> "KDV"; "Kệ B2" -> "B2".</summary>
        private static string ZonePrefix(string zoneName)
        {
            var plain = RemoveDiacritics(zoneName ?? "").ToUpperInvariant();
            var words = plain.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => new string(w.Where(char.IsLetterOrDigit).ToArray()))
                .Where(w => w.Length > 0)
                .ToList();
            if (words.Count == 0) return "V";
            var last = words[^1];
            if (words.Count > 1 && (last.Length == 1 || (last.Length <= 3 && last.Any(char.IsDigit)))) return last;
            if (words.Count == 1) return last.Length <= 3 ? last : last[..1];
            return string.Concat(words.Select(w => w[0]));
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Replace('đ', 'd').Replace('Đ', 'D').Normalize(System.Text.NormalizationForm.FormD);
            var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                                              != System.Globalization.UnicodeCategory.NonSpacingMark);
            return new string(chars.ToArray()).Normalize(System.Text.NormalizationForm.FormC);
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                lblStatus.Text = "Vui lòng nhập mã vị trí.";
                return;
            }
            if (numCapacity.Value <= 0)
            {
                lblStatus.Text = "Sức chứa phải lớn hơn 0.";
                return;
            }
            if (numWidth.Value <= 0 || numHeight.Value <= 0)
            {
                lblStatus.Text = "Chiều rộng/cao vị trí phải lớn hơn 0.";
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                await _api.Warehouses.CreateBinAsync(
                    _zoneId, txtCode.Text.Trim(), numCapacity.Value, numMaxWeight.Value,
                    numPosX.Value, numPosY.Value, numWidth.Value, numHeight.Value, numDistanceFromDoor.Value);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}
