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
        private bool _contractBlocked;      // chưa có / chưa chọn hợp đồng, hoặc hạn giao vượt hạn hợp đồng
        private string _contractBlockNote = ""; // ghi ở dòng tóm tắt dưới đáy
        private bool _noUsableContract;     // khách không có hợp đồng nào dùng được
        private readonly Button _contractGo = new()
        {
            Text = "📄 Mở màn Hợp đồng để tạo hợp đồng", AutoSize = true, Visible = false, Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Left, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false,
            BackColor = Color.FromArgb(217, 119, 6), ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        };
        private bool _saveDisabledByContract;
        private OrderContractOption? _lastContract;
        private static readonly Font ContractBold = new("Segoe UI", 9.5F, FontStyle.Bold);
        private static readonly Font ContractSmall = new("Segoe UI", 8.5F);

        /// <summary>Hợp đồng một lần đã gắn với đơn KHÁC (sửa chính đơn đó thì vẫn chọn được).</summary>
        private bool UsedElsewhere(OrderContractOption o) => o.UsedByOrderId is int used && used != _editOrderId;

        private void SetupContractRules()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            // Dòng tóm tắt ngay dưới ô Hợp đồng, CÙNG CỘT với ô: gói ô chọn + dòng tóm tắt vào 1 khung 2 tầng rồi đặt khung vào
            // đúng ô cũ của ô chọn (không chèn hàng mới vào bảng -> ô chọn không bị đẩy sang cột khác).
            var pos = fields.GetCellPosition(cboContract);
            var box = new TableLayoutPanel
            {
                ColumnCount = 1, RowCount = 3, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = Padding.Empty, TabIndex = cboContract.TabIndex,
            };
            box.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            box.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            box.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            box.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            fields.SuspendLayout();
            fields.Controls.Remove(cboContract);
            box.Controls.Add(cboContract, 0, 0);
            box.Controls.Add(_contractInfo, 0, 1);
            box.Controls.Add(_contractGo, 0, 2);
            _contractGo.FlatAppearance.BorderSize = 0;
            _contractGo.Padding = new Padding(S(8), S(2), S(8), S(2));
            _contractGo.Margin = new Padding(0, 0, 0, S(6));
            _contractGo.Click += (_, _) => GoToContracts();
            fields.Controls.Add(box, pos.Column, pos.Row);
            fields.ResumeLayout(true);
            _contractInfo.Margin = new Padding(0, 0, S(8), S(4));
            // Chữ dài thì xuống dòng trong bề ngang ô, không kéo giãn cả form.
            box.SizeChanged += (_, _) =>
                _contractInfo.MaximumSize = new Size(Math.Max(S(200), box.ClientSize.Width - _contractInfo.Margin.Horizontal), 0);
            // Nhãn "Hợp đồng:" bám mép trên, thẳng hàng với ô chọn (khi hiện dòng tóm tắt không bị trôi xuống giữa).
            lblContract.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblContract.Margin = new Padding(0, cboContract.Margin.Top + Math.Max(0, (cboContract.Height - lblContract.PreferredHeight) / 2), 12, 4);

            // Danh sách xổ xuống 2 dòng / mục; ô đang chọn vẫn 1 dòng.
            cboContract.DrawMode = DrawMode.OwnerDrawVariable;
            cboContract.DropDownWidth = Math.Max(cboContract.DropDownWidth, S(460));
            cboContract.MeasureItem += (_, e) => e.ItemHeight = e.Index >= 0 && cboContract.Items[e.Index] is OrderContractOption { Id: > 0 } ? S(38) : S(24);
            cboContract.DrawItem += DrawContractItem;
            // Danh sách cao đủ cho mọi mục (mỗi hợp đồng 2 dòng) — để mặc định Windows chỉ mở cao ~1 dòng, mục bị cắt.
            // Tắt IntegralHeight NGAY TỪ ĐẦU (đổi thuộc tính này làm ô tạo lại handle) và chỉ đặt chiều cao lúc nạp
            // danh sách — đặt trong sự kiện DropDown (lúc đang mở) làm văng AccessViolationException.
            cboContract.IntegralHeight = false;
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
                    ShowContractText($"⛔ Hợp đồng {o.Code} là hợp đồng một lần, đã dùng cho đơn {o.UsedByOrderCode} — chọn hợp đồng khác.",
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
            var note = "";
            if (c is null || c.Id == 0)
            {
                // Đơn BẮT BUỘC theo hợp đồng (có hợp đồng mới có bảng giá -> tổng tiền đúng, lập được hóa đơn).
                blocked = true;
                if (cboCustomer.SelectedValue is null || cboContract.Items.Count == 0) { _contractInfo.Visible = false; note = "chưa chọn hợp đồng"; }
                else if (_noUsableContract)
                {
                    note = "khách chưa có hợp đồng";
                    ShowContractText($"⚠️ Khách \"{cboCustomer.Text}\" chưa có hợp đồng còn hiệu lực. Đơn phải theo hợp đồng để có bảng giá " +
                                     "và lập được hóa đơn — cần tạo (hoặc gia hạn) hợp đồng cho khách trước.",
                        Color.FromArgb(180, 83, 9), Color.FromArgb(255, 251, 235));
                }
                else
                {
                    note = "chưa chọn hợp đồng";
                    ShowContractText("👉 Chọn 1 hợp đồng của khách — đơn bắt buộc theo hợp đồng.", Theme.Warning, Color.FromArgb(255, 251, 235));
                }
            }
            else if (c.Inactive)
                ShowContractText($"⚠️ Hợp đồng {c.Code} không còn hiệu lực — lúc Xác nhận đơn sẽ bị chặn. Chọn hợp đồng khác.",
                    Theme.Warning, Color.FromArgb(255, 251, 235));
            else if (c.EndDate is DateTime end && dtCommitted.Value.Date > end.Date)
            {
                blocked = true;
                note = "vượt hạn hợp đồng";
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
            _contractBlockNote = note;
            _contractGo.Visible = _contractInfo.Visible && _noUsableContract && (c is null || c.Id == 0);
            if (blocked && btnSave.Enabled) { btnSave.Enabled = false; _saveDisabledByContract = true; }
            else if (!blocked && _saveDisabledByContract) { btnSave.Enabled = true; _saveDisabledByContract = false; }
            UpdateSummary();
        }

        /// <summary>Nút trong khung cam: đóng form Tạo đơn và mở màn Hợp đồng (nghiệp vụ tạo hợp đồng là của màn Hợp đồng).</summary>
        private void GoToContracts()
        {
            var main = Application.OpenForms.OfType<TruckingSystem.Desktop.GUI_Shared.MainForm>().FirstOrDefault();
            if (main is null) return;
            if (MessageBox.Show(this, "Đóng form tạo đơn (thông tin đang nhập sẽ không được lưu) và mở màn Hợp đồng để tạo hợp đồng cho khách?",
                    "Mở màn Hợp đồng", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            DialogResult = DialogResult.Cancel;
            Close();
            main.BeginInvoke(new Action(() =>
            {
                if (!main.OpenNavByName("Hợp đồng"))
                    MessageBox.Show(main, "Tài khoản của bạn không có quyền vào màn Hợp đồng — nhờ bộ phận kinh doanh tạo hợp đồng cho khách.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        }

        /// <summary>Gọi sau khi nạp lại danh sách hợp đồng (KHÔNG gọi trong lúc danh sách đang mở).</summary>
        private void UpdateContractDropHeight()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            var total = 2;
            foreach (var it in cboContract.Items) total += it is OrderContractOption { Id: > 0 } ? S(38) : S(24);
            var h = Math.Max(S(40), Math.Min(S(380), total));
            if (cboContract.DropDownHeight != h) cboContract.DropDownHeight = h;
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
