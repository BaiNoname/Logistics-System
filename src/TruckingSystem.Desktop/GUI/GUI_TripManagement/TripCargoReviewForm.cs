using System.Diagnostics;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;
using TruckingSystem.Desktop.SERVICE_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>Xem KIỂM HÀNG + ẢNH HÀNG tài xế gửi khi xác nhận chặng (lấy hàng / giao /
    /// trả). Chế độ DUYỆT (chuyến Chờ xác nhận): Vận hành xem ảnh, đối chiếu tên hàng +
    /// số lượng rồi bấm Duyệt hoặc Từ chối — form trả về DialogResult.Yes (duyệt) /
    /// DialogResult.No (từ chối) / Cancel (đóng). Chế độ XEM: chỉ có nút Đóng.
    /// Giao diện: TripCargoReviewForm.Designer.cs.</summary>
    public partial class TripCargoReviewForm : BaseForm
    {
        private readonly TripPhotoApiService? _service;
        private readonly int _tripId;
        private readonly bool _reviewMode;


        private List<TripPhotoItem> _photos = new();
        private readonly Dictionary<int, byte[]> _photoCache = new();
        private int _index;

        /// <summary>Dùng cho trình thiết kế (Designer) của Visual Studio.</summary>
        public TripCargoReviewForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this); // Esc = đóng
            Theme.StyleGrid(gridLines);
        }

        public TripCargoReviewForm(ApiServices api, int tripId, string tripLabel, bool reviewMode) : this()
        {
            _service = new TripPhotoApiService(api.Connection);
            _tripId = tripId;
            _reviewMode = reviewMode;
            btnApprove.Visible = btnReject.Visible = reviewMode; // chế độ XEM: chỉ nút Đóng
            lblTitle.Text = (reviewMode ? "🔎 Kiểm tra trước khi duyệt — " : "📷 Ảnh hàng — ") + tripLabel;
            Text = reviewMode ? $"Duyệt chuyến #{tripId}" : $"Ảnh hàng — chuyến #{tripId}";
            Load += async (_, _) => await LoadAsync();
        }

        private async void BtnPrev_Click(object? sender, EventArgs e) => await ShowPhotoAsync(_index - 1);
        private async void BtnNext_Click(object? sender, EventArgs e) => await ShowPhotoAsync(_index + 1);
        private void BtnOpen_Click(object? sender, EventArgs e) => OpenCurrentPhoto();
        private void PicPhoto_DoubleClick(object? sender, EventArgs e) => OpenCurrentPhoto();

        private void BtnApprove_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void BtnReject_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async Task LoadAsync()
        {
            if (_service is null) return;
            try
            {
                var checklist = await _service.GetChecklistAsync(_tripId);
                if (IsDisposed) return;
                gridLines.Rows.Clear();
                if (checklist is not null)
                {
                    foreach (var l in checklist.Lines)
                    {
                        var flags = (l.IsFragile ? " · dễ vỡ" : "") + (l.IsHazmat ? " · nguy hiểm" : "");
                        gridLines.Rows.Add(l.CargoType + flags, l.Qty.ToString("0.##"), l.PackagesApprox, l.WeightKg.ToString("N0"));
                    }
                }
                gridLines.ClearSelection();

                _photos = await _service.GetPhotosAsync(_tripId);
                if (IsDisposed) return;
                var legText = checklist?.LegType switch { "Pickup" => "lấy hàng", "Return" => "trả hàng", _ => "giao hàng" };
                lblInfo.Text = _photos.Count == 0
                    ? $"⚠️ Chuyến này CHƯA có ảnh hàng (dữ liệu cũ, hoặc điều phối viên bấm Hoàn tất trên máy tính)."
                    : $"Tài xế đã tick xác nhận ĐỦ {checklist?.Lines.Count ?? 0} dòng hàng ({legText}) kèm {_photos.Count} ảnh. " +
                      "Đối chiếu ảnh với tên hàng + số lượng bên trái — hàng sai/thiếu thì Từ chối và liên hệ tài xế.";
                lblInfo.ForeColor = _photos.Count == 0 ? Theme.Warning : Theme.TextSecondary;
                await ShowPhotoAsync(0);
            }
            catch (Exception ex)
            {
                lblInfo.Text = "Lỗi tải dữ liệu: " + ex.Message;
                lblInfo.ForeColor = Theme.Danger;
            }
        }

        private async Task ShowPhotoAsync(int index)
        {
            btnPrev.Enabled = btnNext.Enabled = btnOpen.Enabled = false;
            if (_photos.Count == 0)
            {
                picPhoto.Image = null;
                lblPhoto.Text = "Không có ảnh";
                return;
            }
            _index = Math.Clamp(index, 0, _photos.Count - 1);
            var photo = _photos[_index];
            lblPhoto.Text = $"Ảnh {_index + 1}/{_photos.Count} · {photo.TakenAt.ToLocalTime():dd/MM HH:mm}";
            try
            {
                if (!_photoCache.TryGetValue(photo.Id, out var bytes))
                {
                    if (_service is null) return;
                    bytes = await _service.DownloadAsync(photo.Id);
                    if (IsDisposed) return; // form đã đóng trong lúc tải
                    _photoCache[photo.Id] = bytes;
                }
                using var ms = new MemoryStream(bytes);
                using var img = Image.FromStream(ms);
                var old = picPhoto.Image;
                picPhoto.Image = new Bitmap(img); // copy -> không phụ thuộc stream đã đóng
                old?.Dispose();
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                var old = picPhoto.Image;
                picPhoto.Image = null;
                old?.Dispose();
                lblPhoto.Text = "Không mở được ảnh: " + ex.Message;
            }
            btnPrev.Enabled = _index > 0;
            btnNext.Enabled = _index < _photos.Count - 1;
            btnOpen.Enabled = true;
        }

        /// <summary>Mở ảnh bằng trình xem ảnh của Windows (xem kích thước thật).</summary>
        private void OpenCurrentPhoto()
        {
            if (_photos.Count == 0) return;
            var photo = _photos[_index];
            if (!_photoCache.TryGetValue(photo.Id, out var bytes)) return;
            try
            {
                var ext = photo.ContentType switch { "image/png" => ".png", "image/webp" => ".webp", _ => ".jpg" };
                var path = Path.Combine(Path.GetTempPath(), $"anh-hang-chuyen{_tripId}-{photo.Id}{ext}");
                File.WriteAllBytes(path, bytes);
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Không mở được ảnh: " + ex.Message);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            picPhoto.Image?.Dispose(); // giải phóng ảnh đang hiện
            picPhoto.Image = null;
            base.OnFormClosed(e);
        }
    }
}
