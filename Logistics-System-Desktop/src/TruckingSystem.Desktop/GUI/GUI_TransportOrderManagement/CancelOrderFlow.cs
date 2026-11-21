using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.GUI_Shared;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>HỦY ĐƠN (mẫu Round 171 đã chốt) — dùng chung cho màn Đơn và Chi tiết đơn:
    ///  A. Xem trước: hỏi máy chủ "hủy thì sẽ xảy ra gì" (chuyến nào bị hủy, có tạo chuyến Trả hàng không, hàng trả tại
    ///     kho, phí lưu kho, tài xế đang chạy cần gọi) rồi mới hỏi chắc chắn.
    ///  B. Chưa hủy được: hộp sửa nhanh nêu ĐỦ các việc còn thiếu, mỗi việc 1 nút làm ngay; làm xong bấm "Thử hủy lại".
    ///  C. Hủy xong mà còn chuyến Trả hàng: báo số chuyến, xe, tài xế + các bước tiếp, nút Lấy link tài xế / Mở chuyến.</summary>
    public static class CancelOrderFlow
    {
        /// <returns>true = đơn đã bị hủy (màn gọi tải lại dữ liệu).</returns>
        public static async Task<bool> RunAsync(Control owner, ApiServices api, int orderId, string orderCode)
        {
            CancelPreviewItem preview;
            try { preview = await api.TransportOrders.GetCancelPreviewAsync(orderId); }
            catch (Exception ex) { Ask.Fail(owner, "Không kiểm tra được đơn trước khi hủy: " + ex.Message); return false; }
            if (owner.IsDisposed) return false;
            var code = string.IsNullOrWhiteSpace(preview.OrderCode) ? orderCode : preview.OrderCode;

            if (!preview.CanCancel)
                return await ShowBlockedAsync(owner, api, orderId, code, preview.BlockMessage, preview.Fixes);

            if (!ConfirmPreview(owner, code, preview.Effects)) return false;

            OrderActionResultItem result;
            try { result = await api.TransportOrders.CancelOrderAsync(orderId); }
            catch (Exception ex) { Ask.Fail(owner, "Lỗi khi hủy đơn: " + ex.Message); return false; }
            if (owner.IsDisposed) return result.Success;
            // Giữa lúc xem trước và lúc bấm, người khác vừa đổi đơn (VD tài xế báo về kho) -> báo như B.
            if (!result.Success)
                return await ShowBlockedAsync(owner, api, orderId, code, result.Message, result.Fixes);

            ShowDone(owner, api, code, result);
            return true;
        }

        /// <summary>B — các việc phải làm trước khi hủy, mỗi việc 1 nút sửa nhanh; sửa xong hỏi "🔄 Thử hủy lại".</summary>
        private static async Task<bool> ShowBlockedAsync(Control owner, ApiServices api, int orderId, string code,
            string? message, List<FixHintItem>? fixes)
        {
            var cancelled = false;
            await QuickFix.ShowAsync(owner, api, $"Chưa hủy được đơn {code}", message, fixes,
                "🔄 Thử hủy lại", async () => cancelled = await RunAsync(owner, api, orderId, code));
            return cancelled;
        }

        // ================================================================ A. Xem trước

        private static bool ConfirmPreview(Control owner, string code, List<CancelEffectItem> effects)
        {
            var hasReturn = effects.Any(e => e.Kind == "Return");
            using var dlg = NewDialog(owner, "Hủy đơn vận chuyển", out var root, out var S);
            root.Controls.Add(Title($"Hủy đơn {code}?", Theme.Danger, S));
            root.Controls.Add(Caption("Hệ thống sẽ làm những việc sau:", S));
            foreach (var e in effects) root.Controls.Add(EffectRow(e, S));
            root.Controls.Add(new Label
            {
                Text = "⚠️ Hủy rồi không hoàn tác được.", AutoSize = true, ForeColor = Theme.Danger,
                Margin = new Padding(0, S(8), 0, S(4)), UseMnemonic = false,
            });

            var ok = new Button
            {
                Text = hasReturn ? "❌ Hủy đơn và tạo chuyến trả" : "❌ Hủy đơn",
                AutoSize = true, MinimumSize = new Size(S(140), S(36)), DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            };
            ButtonStyles.Primary(ok);
            ok.BackColor = Theme.Danger;
            ok.FlatAppearance.MouseOverBackColor = Color.FromArgb(185, 28, 28);
            var keep = new Button { Text = "Giữ đơn", AutoSize = true, MinimumSize = new Size(S(90), S(36)), DialogResult = DialogResult.Cancel };
            ButtonStyles.Secondary(keep);
            root.Controls.Add(Footer(S, ok, keep));
            dlg.CancelButton = keep; // Enter KHÔNG tự bấm hủy (tránh lỡ tay), Esc = Giữ đơn
            return Show(owner, dlg) == DialogResult.OK;
        }

        // ================================================================ C. Hủy xong

        private static void ShowDone(Control owner, ApiServices api, string code, OrderActionResultItem result)
        {
            var effects = result.Effects ?? new List<CancelEffectItem>();
            var returns = effects.Where(e => e.Kind == "Return" && e.TripId is int).ToList();
            var important = returns.Count > 0 || effects.Any(e => e.Kind is "OnRoad" or "ReturnAtWarehouse");
            if (!important)
            {
                // Không còn việc gì sau khi hủy: chỉ báo nhanh (máy chủ cũ không gửi Effects thì vẫn hiện lời nhắn cũ nếu có).
                if (effects.Count == 0 && !string.IsNullOrWhiteSpace(result.Message))
                    MessageBox.Show(owner.FindForm(), result.Message, "Đã hủy đơn — cần lưu ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    Toast.Show(owner, $"❌ Đã hủy đơn {code}.", ToastKind.Info);
                return;
            }

            using var dlg = NewDialog(owner, "Đã hủy đơn", out var root, out var S);
            root.Controls.Add(Title(returns.Count > 0
                ? $"❌ Đã hủy đơn {code} — còn {returns.Count} chuyến trả hàng"
                : $"❌ Đã hủy đơn {code} — cần lưu ý", Theme.TextPrimary, S));
            foreach (var e in effects) root.Controls.Add(EffectRow(e, S));

            Button? link = null, open = null;
            if (returns.Count > 0)
            {
                var first = returns[0].TripId!.Value;
                var ids = string.Join(", #", returns.Select(r => r.TripId));
                root.Controls.Add(Caption("Bước tiếp:", S));
                foreach (var (i, step) in new[]
                         {
                             $"Thủ kho XUẤT TRẢ hàng lên xe (màn Kho → 📤 Xuất kho theo chuyến) — chưa xuất thì tài xế chưa chạy được.",
                             $"Gửi 📱 link tài xế chuyến #{ids} — tài xế chạy từ kho về người gửi.",
                             "Tài xế trả xong báo về → ✅ Duyệt chuyến ở màn Chuyến xe → xe rảnh lại.",
                         }.Select((text, index) => (index + 1, text)))
                    root.Controls.Add(new Label
                    {
                        Text = $"{i}. {step}", AutoSize = true, MaximumSize = new Size(S(520), 0),
                        Margin = new Padding(S(4), 0, 0, S(4)), UseMnemonic = false,
                    });

                link = new Button { Text = "📱 Lấy link tài xế", AutoSize = true, MinimumSize = new Size(S(120), S(36)) };
                ButtonStyles.Secondary(link);
                link.Click += (_, _) =>
                {
                    var text = $"Server URL: {api.Connection.BaseUrl}\nTrip ID: {first}";
                    try { Clipboard.SetText(text); link.Text = $"✅ Đã sao chép link chuyến #{first}"; }
                    catch { MessageBox.Show(dlg, text, "Thông tin kết nối tài xế"); }
                };
                open = new Button { Text = $"🛣️ Mở chuyến trả hàng #{first}", AutoSize = true, MinimumSize = new Size(S(140), S(36)) };
                ButtonStyles.Primary(open);
                open.Click += (_, _) => { dlg.DialogResult = DialogResult.Yes; };
            }

            var close = new Button { Text = "Đóng", AutoSize = true, MinimumSize = new Size(S(90), S(36)), DialogResult = DialogResult.Cancel };
            ButtonStyles.Secondary(close);
            root.Controls.Add(open is null ? Footer(S, close) : Footer(S, open, link!, close));
            dlg.CancelButton = close;
            if (Show(owner, dlg) == DialogResult.Yes && returns.Count > 0 && !owner.IsDisposed)
                QuickFix.OpenTrip(owner, api, returns[0].TripId!.Value);
        }

        // ================================================================ khung hộp thoại dùng chung

        private static Form NewDialog(Control owner, string text, out TableLayoutPanel root, out Func<int, int> scale)
        {
            var dlg = new Form
            {
                Text = text,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false,
                StartPosition = owner.FindForm() is null ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent,
                AutoScaleMode = AutoScaleMode.Dpi, AutoScaleDimensions = new SizeF(96F, 96F),
                BackColor = Color.White, Font = new Font("Segoe UI", 10F),
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };
            scale = v => dlg.LogicalToDeviceUnits(v);
            var S = scale;
            root = new TableLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Dock = DockStyle.Fill,
                Padding = new Padding(S(20), S(16), S(20), S(12)), MinimumSize = new Size(S(440), 0),
            };
            dlg.Controls.Add(root);
            return dlg;
        }

        private static DialogResult Show(Control owner, Form dlg)
            => owner.FindForm() is Form parent ? dlg.ShowDialog(parent) : dlg.ShowDialog();

        private static Label Title(string text, Color color, Func<int, int> S) => new()
        {
            Text = text, AutoSize = true, MaximumSize = new Size(S(540), 0), Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = color, Margin = new Padding(0, 0, 0, S(10)), UseMnemonic = false,
        };

        private static Label Caption(string text, Func<int, int> S) => new()
        {
            Text = text, AutoSize = true, ForeColor = Theme.TextSecondary, Font = new Font("Segoe UI", 9F),
            Margin = new Padding(0, S(4), 0, S(4)), UseMnemonic = false,
        };

        /// <summary>1 dòng "sẽ / đã xảy ra gì" — chuyến Trả hàng nền xanh, tài xế đang chạy nền cam, trả tại kho nền xanh lá.</summary>
        private static Label EffectRow(CancelEffectItem e, Func<int, int> S)
        {
            var (back, fore) = e.Kind switch
            {
                "Return" => (Color.FromArgb(239, 246, 255), Color.FromArgb(30, 64, 175)),
                "OnRoad" => (Color.FromArgb(255, 251, 235), Color.FromArgb(146, 64, 14)),
                "ReturnAtWarehouse" => (Color.FromArgb(240, 253, 244), Color.FromArgb(22, 101, 52)),
                "Fee" or "Info" => (Color.White, Theme.TextSecondary),
                _ => (Color.White, Theme.TextPrimary),
            };
            var boxed = back != Color.White;
            return new Label
            {
                Text = e.Text, AutoSize = true, MaximumSize = new Size(S(540), 0), BackColor = back, ForeColor = fore,
                Font = e.Kind == "Return" ? new Font("Segoe UI", 10F, FontStyle.Bold) : new Font("Segoe UI", 10F),
                Padding = boxed ? new Padding(S(10), S(6), S(10), S(6)) : Padding.Empty,
                Margin = new Padding(0, 0, 0, S(6)), UseMnemonic = false,
            };
        }

        private static FlowLayoutPanel Footer(Func<int, int> S, params Button[] rightToLeft)
        {
            var foot = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0, S(10), 0, 0),
            };
            foreach (var b in rightToLeft)
            {
                b.Margin = new Padding(S(8), 0, 0, 0);
                foot.Controls.Add(b);
            }
            return foot;
        }
    }
}
