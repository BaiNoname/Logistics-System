using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Sửa vị trí lưu trữ (StorageBin) đã có — nạp sẵn dữ liệu cũ.
    /// Backend tự chặn nếu sức chứa mới nhỏ hơn lượng hàng đang chứa, hoặc
    /// tọa độ mới đè lên vị trí khác / vượt ra ngoài khu vực.</summary>
    public partial class EditBinForm : BaseForm
    {
        private ApiServices? _api;
        private int _binId;

        public EditBinForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this, btnSave); // Esc = đóng, Enter = Lưu
            // Từ ngữ thống nhất: "kệ" (đặt bằng code, không sửa Designer).
            lblCode.Text = "Mã kệ * (VD: C-01)";
            lblWidth.Text = "Chiều rộng kệ (mét) *";
            lblHeight.Text = "Chiều cao kệ (mét) *";
            lblDistanceFromDoor.Text = "🚪 Khoảng cách tới cửa (mét) — hệ thống TỰ TÍNH từ vị trí cửa kho";
            lblNote.Text = "💡 Hệ thống tự kiểm tra: kệ không được đè lên kệ khác, không được vượt ra ngoài ranh giới khu vực.";
            btnSave.Text = "💾 Lưu kệ";
            Text = "Sửa kệ";
        }

        /// <param name="zoneId">Khu của kệ — có thì hiện sơ đồ xem trước (0 = không hiện).</param>
        public EditBinForm(ApiServices api, StorageBinItem bin, string zoneName, decimal zoneWidth, decimal zoneHeight, int zoneId = 0) : this()
        {
            _api = api;
            _binId = bin.Id;
            lblZoneInfo.Text = $"Khu vực \"{zoneName}\" — kích thước {zoneWidth}m x {zoneHeight}m (X: 0–{zoneWidth}, Y: 0–{zoneHeight})";

            txtCode.Text = bin.Code;
            // Nới giới hạn ô nhập nếu giá trị đang lưu vượt giới hạn mặc định (VD vị trí tạo qua
            // API) — trước đây gán thẳng làm form lỗi ngay khi mở.
            SetSafe(numCapacity, bin.CapacityM3);
            SetSafe(numMaxWeight, bin.MaxWeightKg);
            SetSafe(numPosX, bin.PosX);
            SetSafe(numPosY, bin.PosY);
            SetSafe(numWidth, bin.WidthMeters);
            SetSafe(numHeight, bin.HeightMeters);
            SetSafe(numDistanceFromDoor, bin.DistanceFromDoorMeters);
            PlacementSetup.Bin(this, inputs, numPosX, numPosY, numWidth, numHeight, lblPosX, lblPosY, lblWidth, lblHeight,
                btnSave, () => txtCode.Text.Trim(), api, zoneId, zoneName, zoneWidth, zoneHeight, bin.Id);
        }

        private static void SetSafe(NumericUpDown n, decimal value)
        {
            if (value > n.Maximum) n.Maximum = value;
            if (value < n.Minimum) n.Minimum = value;
            n.Value = value;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                FieldHint.Show(txtCode, lblStatus, "Vui lòng nhập mã kệ."); // tô đỏ + nhảy vào đúng ô
                return;
            }
            if (numCapacity.Value <= 0)
            {
                FieldHint.Show(numCapacity, lblStatus, "Sức chứa phải lớn hơn 0."); // tô đỏ + nhảy vào đúng ô
                return;
            }
            if (numWidth.Value <= 0 || numHeight.Value <= 0)
            {
                FieldHint.Show(numWidth, lblStatus, "Chiều rộng/cao kệ phải lớn hơn 0."); // tô đỏ + nhảy vào đúng ô
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                var result = await _api.Warehouses.UpdateBinAsync(
                    _binId, txtCode.Text.Trim(), numCapacity.Value, numMaxWeight.Value,
                    numPosX.Value, numPosY.Value, numWidth.Value, numHeight.Value, numDistanceFromDoor.Value);

                if (!result.Success)
                {
                    lblStatus.Text = "⚠️ " + result.Message;
                    return;
                }

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
