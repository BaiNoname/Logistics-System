using System.Drawing.Drawing2D;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>HỢP ĐỒNG KHI TẠO / SỬA ĐƠN (mẫu hop_dong_don đã duyệt — chặn hẳn, không chỉ cảnh báo):
    ///  • Ô chọn hợp đồng: mỗi dòng ghi Loại (Dài hạn / Một lần), % ưu đãi, thời hạn, bảng giá. Dưới ô có 1 dòng tóm tắt
    ///    hợp đồng đang chọn (còn hiệu lực bao nhiêu ngày).
    ///  • Hợp đồng MỘT LẦN đã dùng cho đơn khác (chưa hủy): hiện mờ kèm lý do, không chọn được.
    ///  • Hạn giao SAU ngày kết thúc hợp đồng: báo đỏ dưới ô hợp đồng, nút 💾 Lưu đơn mờ — muốn hạn xa hơn thì gia hạn
    ///    hợp đồng trước. Máy chủ cũng kiểm tra (tạo / sửa / xác nhận đơn, đổi hạn giao, bắt đầu chuyến đầu tiên).</summary>
    public partial class CreateOrderForm
    {
        private readonly Label _contractInfo = new()
        {
            AutoSize = true, Dock = DockStyle.Fill, Visible = false, UseMnemonic = false,
            Font = new Font("Segoe UI", 9F), Padding = new Padding(6, 4, 6, 4),
        };
        private bool _contractBlocked;      // hạn giao vượt hạn hợp đồng
        private bool _saveDisabledByContract;
        private OrderContractOption? _lastContract;
        private static readonly Font ContractBold = new("Segoe UI", 9.5F, FontStyle.Bold);
        private static readonly Font ContractSmall = new("Segoe UI", 8.5F);

        /// <summary>Hợp đồng một lần đã gắn với đơn KHÁC (sửa chính đơn đó thì vẫn chọn được).</summary>
        private bool UsedElsewhere(OrderContractOption o) => o.UsedByOrderId is int used && used != _editOrderId;

        private void SetupContractRules()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            // Dòng tóm tắt ngay dưới ô Hợp đồng (cột bên phải).
            var row = fields.GetRow(cboContract) + 1;
            KpiStrip.InsertRow(fields, _contractInfo, row);
            fields.SetColumn(_contractInfo, 1);
            fields.SetColumnSpan(_contractInfo, 1);
            _contractInfo.Margin = new Padding(0, 0, S(8), S(4));

            // Danh sách xổ xuống 2 dòng / mục; ô đang chọn vẫn 1 dòng.
            cboContract.DrawMode = DrawMode.OwnerDrawVariable;
            cboContract.DropDownWidth = Math.Max(cboContract.DropDownWidth, S(460));
            cboContract.MeasureItem += (_, e) => e.ItemHeight = e.Index >= 0 && cboContract.Items[e.Index] is OrderContractOption { Id: > 0 } ? S(38) : S(24);
            cboContract.DrawItem += DrawContractItem;
            cboContract.SelectedIndexChanged += (_, _) => OnContractPicked();
            dtCommitted.ValueChanged += (_, _) => UpdateContractCheck();
        }

        private void OnContractPicked()
        {
            if (cboContract.SelectedItem is OrderContractOption o && UsedElsewhere(o))
            {
                // Không cho chọn: trả về hợp đồng trước đó, báo lý do ngay dưới ô.
                var back = _lastContract is not null && cboContract.Items.Contains(_lastContract) ? _lastContract : cboContract.Items[0];
                BeginInvoke(new Action(() =>
                {
                    cboContract.SelectedItem = back;
                    ShowContractText($"⛔ Hợp đồng {o.Code} là hợp đồng một lần, đã dùng cho đơn {o.UsedByOrderCode} — chọn hợp đồng khác hoặc khách lẻ.",
                        Theme.Danger, Color.FromArgb(254, 242, 242));
                }));
                return;
            }
            _lastContract = cboContract.SelectedItem as OrderContractOption;
            UpdateContractCheck();
        }

        /// <summary>Tóm tắt hợp đồng đang chọn + kiểm tra hạn giao không được sau ngày kết thúc hợp đồng.</summary>
        private void UpdateContractCheck()
        {
            var c = cboContract.SelectedItem as OrderContractOption;
            var blocked = false;
            if (c is null || c.Id == 0)
                _contractInfo.Visible = false;
            else if (c.Inactive)
                ShowContractText($"⚠️ Hợp đồng {c.Code} không còn hiệu lực — lúc Xác nhận đơn sẽ bị chặn. Chọn hợp đồng khác hoặc khách lẻ.",
                    Theme.Warning, Color.FromArgb(255, 251, 235));
            else if (c.EndDate is DateTime end && dtCommitted.Value.Date > end.Date)
            {
                blocked = true;
                ShowContractText($"⛔ Hợp đồng {c.Code} hết hạn {end:dd/MM/yyyy}, hạn giao đang chọn là {dtCommitted.Value:dd/MM/yyyy}. " +
                                 "Chọn hạn giao trước hoặc bằng ngày hết hạn, hoặc nhờ bộ phận kinh doanh gia hạn hợp đồng rồi tạo lại.",
                    Theme.Danger, Color.FromArgb(254, 242, 242));
            }
            else
            {
                var parts = new List<string>();
                if (c.TypeText.Length > 0) parts.Add(c.TypeText);
                if (c.DiscountPercent is > 0) parts.Add($"ưu đãi {c.DiscountPercent:0.##}%");
                parts.Add(c.EndDate is DateTime e
                    ? $"hiệu lực tới {e:dd/MM/yyyy} (còn {Math.Max(0, (e.Date - DateTime.Today).Days)} ngày)"
                    : "không thời hạn");
                if (!string.IsNullOrWhiteSpace(c.PriceListName)) parts.Add("bảng giá " + c.PriceListName);
                ShowContractText("📄 " + string.Join(" · ", parts), Color.FromArgb(3, 105, 161), Color.FromArgb(240, 249, 255));
            }

            _contractBlocked = blocked;
            if (blocked && btnSave.Enabled) { btnSave.Enabled = false; _saveDisabledByContract = true; }
            else if (!blocked && _saveDisabledByContract) { btnSave.Enabled = true; _saveDisabledByContract = false; }
            UpdateSummary();
        }

        private void ShowContractText(string text, Color fore, Color back)
        {
            _contractInfo.Text = text;
            _contractInfo.ForeColor = fore;
            _contractInfo.BackColor = back;
            _contractInfo.MaximumSize = new Size(Math.Max(LogicalToDeviceUnits(200), cboContract.Width), 0);
            _contractInfo.Visible = true;
        }

        /// <summary>Mục trong danh sách: dòng 1 = nhãn loại + ưu đãi + mã; dòng 2 = thời hạn · bảng giá (hoặc lý do không chọn được).</summary>
        private void DrawContractItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var item = cboContract.Items[e.Index];
            var g = e.Graphics;
            var editArea = (e.State & DrawItemState.ComboBoxEdit) != 0;
            var used = item is OrderContractOption uo && UsedElsewhere(uo);
            var selected = (e.State & DrawItemState.Selected) != 0 && !editArea && !used;
            using (var bg = new SolidBrush(selected ? Color.FromArgb(224, 242, 254) : used ? Color.FromArgb(248, 250, 252) : cboContract.BackColor))
                g.FillRectangle(bg, e.Bounds);

            if (item is not OrderContractOption o || o.Id == 0 || editArea)
            {
                TextRenderer.DrawText(g, item?.ToString() ?? "", cboContract.Font, Rectangle.Inflate(e.Bounds, -4, 0), Theme.TextPrimary,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            int S(int v) => LogicalToDeviceUnits(v);
            var x = e.Bounds.X + S(6);
            var y1 = e.Bounds.Y + S(3);
            void Chip(string text, Color fore, Color back)
            {
                var sz = TextRenderer.MeasureText(g, text, ContractSmall, Size.Empty, TextFormatFlags.NoPadding);
                var r = new Rectangle(x, y1 + S(1), sz.Width + S(10), sz.Height + S(2));
                using (var b = new SolidBrush(used ? Color.FromArgb(241, 245, 249) : back))
                using (var p = Anim.RoundRect(r, r.Height / 2f))
                    g.FillPath(b, p);
                TextRenderer.DrawText(g, text, ContractSmall, r, used ? Color.FromArgb(148, 163, 184) : fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                x = r.Right + S(4);
            }
            if (o.Inactive) Chip("Hết hiệu lực", Color.FromArgb(180, 83, 9), Color.FromArgb(254, 243, 199));
            else if (o.IsOneTime) Chip("Một lần", Color.FromArgb(91, 33, 182), Color.FromArgb(237, 233, 254));
            else if (o.Type == "LongTerm") Chip("Dài hạn", Color.FromArgb(30, 64, 175), Color.FromArgb(219, 234, 254));
            if (o.DiscountPercent is > 0) Chip($"−{o.DiscountPercent:0.##}%", Color.FromArgb(22, 101, 52), Color.FromArgb(220, 252, 231));
            TextRenderer.DrawText(g, o.Code, ContractBold, new Point(x, y1), used ? Color.FromArgb(148, 163, 184) : Theme.TextPrimary, TextFormatFlags.NoPrefix);

            var line2 = used
                ? $"⛔ Đã dùng cho đơn {o.UsedByOrderCode} — hợp đồng một lần chỉ cho 1 đơn"
                : $"Hiệu lực {o.StartDate:dd/MM/yyyy} – {(o.EndDate is DateTime end ? end.ToString("dd/MM/yyyy") : "không thời hạn")}" +
                  (string.IsNullOrWhiteSpace(o.PriceListName) ? "" : " · bảng giá " + o.PriceListName) +
                  (o.IsOneTime ? " · chưa dùng" : "");
            TextRenderer.DrawText(g, line2, ContractSmall,
                new Rectangle(e.Bounds.X + S(6), e.Bounds.Y + S(20), e.Bounds.Width - S(10), S(16)),
                used ? Color.FromArgb(180, 83, 9) : Theme.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        }
    }
}
