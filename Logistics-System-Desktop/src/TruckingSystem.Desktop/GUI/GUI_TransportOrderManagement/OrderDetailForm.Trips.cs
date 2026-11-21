using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.GUI_RouteManagement;
using TruckingSystem.Desktop.GUI_Shared;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>VÙNG "CHUYẾN ĐÃ GÁN" của Chi tiết đơn (mẫu đã chốt):
    ///  • 2 nút cạnh tiêu đề bảng: "🗺️ Tuyến dự kiến cả đơn" (nút Xem lộ trình cũ, đổi tên cho rõ) và
    ///    "🛰️ Xem đường đi chuyến" (chuyến đang chọn: đã xong = xem lại GPS thật + video, đang chạy = theo dõi trực tiếp).
    ///  • Nhấp đúp 1 chuyến = mở bản đồ chuyến đó ngay tại đây (giống màn Chuyến xe); chuột phải thêm "Mở ở màn Chuyến xe"
    ///    (đổi xe, nhập chi phí, duyệt chuyến). Dòng gợi ý dưới tiêu đề.
    ///  • Bảng chuyến: thêm Ngày chạy + Km GPS, gộp Xe · Tài xế, nới cột Điểm đi / Điểm đến (rê chuột xem đủ tên);
    ///    bảng hàng thấp vừa số dòng -> không còn khoảng trống giữa 2 bảng.
    /// Dời / thêm control bằng code, không sửa Designer.</summary>
    public partial class OrderDetailForm
    {
        private readonly Button _btnTripMap = new() { AutoSize = true, Enabled = false, Text = "🛰️ Xem đường đi chuyến" };
        private readonly ContextMenuStrip _tripMenu = new();

        private void SetupTripsArea()
        {
            int S(int v) => LogicalToDeviceUnits(v);

            // ① Tiêu đề + dòng gợi ý (trái) .......... [🗺️ Tuyến dự kiến cả đơn] [🛰️ Xem đường đi chuyến] (phải)
            var row = root.GetRow(lblTripsTitle);
            root.Controls.Remove(lblTripsTitle);
            actionBar.Controls.Remove(btnViewRoute);
            btnViewRoute.Text = "🗺️ Tuyến dự kiến cả đơn";
            btnViewRoute.AutoSize = true;
            ButtonStyles.Secondary(btnViewRoute);
            ButtonStyles.Primary(_btnTripMap);
            // 2 nút CAO BẰNG NHAU, chữ không bao giờ bị cắt: cùng font, cùng lề trong, tự giãn theo chữ (AutoSize).
            foreach (var b in new[] { btnViewRoute, _btnTripMap })
            {
                b.Font = new Font("Segoe UI", 9.5F);
                b.AutoSize = true;
                b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                b.MinimumSize = new Size(0, S(36));
                b.MaximumSize = new Size(0, S(36));
                b.Padding = new Padding(S(10), 0, S(10), 0);
                b.Margin = new Padding(S(8), 0, 0, 0);
                b.Anchor = AnchorStyles.Right;
            }
            var tip = new ToolTip();
            tip.SetToolTip(btnViewRoute, "Đường dự kiến của cả đơn: điểm lấy hàng → kho → điểm giao (chưa phải đường xe chạy thật).");
            tip.SetToolTip(_btnTripMap, "Chuyến đang chọn: đã xong = xem lại đường GPS thật + video; đang chạy = theo dõi trực tiếp.");

            // Hàng 1: [Tiêu đề] ....... [🗺️ Tuyến dự kiến cả đơn] [🛰️ Xem đường đi chuyến]
            // Hàng 2: dòng gợi ý trải hết bề ngang, KHÔNG giãn theo chữ (màn hẹp thì tự thêm "…", rê chuột xem đủ)
            // -> chữ dài không còn đẩy / đè lên 2 nút như trước.
            lblTripsTitle.Margin = Padding.Empty;
            lblTripsTitle.Anchor = AnchorStyles.Left;
            const string hintText = "💡 Nhấp đúp 1 chuyến để xem đường đi · chuột phải: Mở ở màn Chuyến xe (đổi xe, nhập chi phí, duyệt)";
            var hint = new Label
            {
                Text = hintText, AutoSize = false, AutoEllipsis = true, Dock = DockStyle.Fill,
                ForeColor = Theme.TextSecondary, Font = new Font("Segoe UI", 8.5F),
                Margin = new Padding(0, S(2), 0, 0), UseMnemonic = false,
            };
            hint.Height = hint.Font.Height + S(4);
            tip.SetToolTip(hint, hintText);
            var head = new TableLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 4, RowCount = 2,
                Dock = DockStyle.Fill, Margin = new Padding(0, S(6), 0, S(4)),
            };
            head.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            head.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            head.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            head.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            head.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            head.RowStyles.Add(new RowStyle(SizeType.Absolute, hint.Height + S(2)));
            head.Controls.Add(lblTripsTitle, 0, 0);
            head.Controls.Add(btnViewRoute, 2, 0);
            head.Controls.Add(_btnTripMap, 3, 0);
            head.Controls.Add(hint, 0, 1);
            head.SetColumnSpan(hint, 4);
            root.Controls.Add(head, 0, row);

            // ② Bảng chuyến: chọn dòng -> nút ghi rõ chuyến nào; nhấp đúp = bản đồ; chuột phải = menu.
            gridTrips.ShowCellToolTips = true;
            gridTrips.SelectionChanged += (_, _) => UpdateTripMapButton();
            gridTrips.DataBindingComplete += (_, _) =>
            {
                if (gridTrips.Columns["Id"] is { } id) id.Visible = false;
                // Điểm đi / Điểm đến không bị bóp quá hẹp (tên địa điểm dài) — phần quá dài rê chuột xem đủ.
                if (gridTrips.Columns["Điểm_đi"] is { } from) from.MinimumWidth = S(140);
                if (gridTrips.Columns["Điểm_đến"] is { } to) to.MinimumWidth = S(140);
                UpdateTripMapButton();
            };
            // Cột rộng VỪA ĐỦ chữ + tiêu đề 1 dòng (không còn "Chuy", "Trạng thá…" bị cắt); thiếu chỗ thì cuộn ngang
            // thay vì bóp chữ — đúng mọi cỡ màn hình / mức phóng to. Bảng hàng cũng vậy ("Giường ngủ g…").
            GridLayout.FitColumns(gridTrips);
            GridLayout.FitColumns(gridCargo);
            gridTrips.CellToolTipTextNeeded += (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                var name = gridTrips.Columns[e.ColumnIndex].Name;
                if (name is "Điểm_đi" or "Điểm_đến" or "Xe_và_tài_xế")
                    e.ToolTipText = gridTrips.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string ?? "";
                else if (name == "Km_GPS" && SelectedTripAt(e.RowIndex) is { GpsKm: null })
                    e.ToolTipText = "Chuyến chưa chạy / chưa có điểm GPS từ app tài xế.";
            };
            gridTrips.CellDoubleClick += async (_, e) =>
            {
                if (e.RowIndex >= 0) await OpenTripMapAsync();
            };
            gridTrips.CellMouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
                var col = Math.Max(0, e.ColumnIndex);
                if (gridTrips.Rows[e.RowIndex].Cells[col].Visible) gridTrips.CurrentCell = gridTrips.Rows[e.RowIndex].Cells[col];
            };
            var mapItem = new ToolStripMenuItem("🛰️ Xem đường đi chuyến", null, async (_, _) => await OpenTripMapAsync());
            var openItem = new ToolStripMenuItem("🛣️ Mở ở màn Chuyến xe (đổi xe, nhập chi phí, duyệt)", null, (_, _) =>
            {
                if (_api is not null && SelectedTrip() is { } t) QuickFix.OpenTrip(this, _api, t.Id);
            });
            _tripMenu.Items.AddRange(new ToolStripItem[] { mapItem, openItem });
            _tripMenu.Opening += (_, e) =>
            {
                var t = SelectedTrip();
                if (t is null) { e.Cancel = true; return; }
                mapItem.Text = $"🛰️ Xem đường đi chuyến #{t.Id}";
                openItem.Text = $"🛣️ Mở chuyến #{t.Id} ở màn Chuyến xe (đổi xe, nhập chi phí, duyệt)";
            };
            gridTrips.ContextMenuStrip = _tripMenu;
            _btnTripMap.Click += async (_, _) => await OpenTripMapAsync();
        }

        /// <summary>Đổ bảng chuyến (gọi trong LoadAsync).</summary>
        private void BindTrips(TransportOrderDetailItem order)
        {
            // Cột Chặng: cùng 1 lô hàng đi qua chặng Lấy hàng rồi Giao hàng (và Trả hàng khi
            // hủy) — không có cột này thì cộng "số lượng chở" ra gấp đôi/gấp ba.
            gridTrips.DataSource = order.Trips.Select(t => new
            {
                t.Id,
                Chuyến = $"#{t.Id}",
                Chặng = LegText(t.LegType),
                Ngày_chạy = t.RunDate is DateTime d ? d.ToString("dd/MM") : "—",
                Điểm_đi = t.FromName ?? "—",
                Điểm_đến = t.ToName ?? "—",
                Xe_và_tài_xế = $"{t.VehiclePlate} · {t.DriverName}",
                Trạng_thái = ReportFormat.TripStatus(t.Status),
                Số_lượng = t.QtyPlanned,
                Km_GPS = t.GpsKm is decimal km ? $"{km:0.#}" : "—",
            }).ToList();
            FitCargoHeight();
        }

        /// <summary>Bảng hàng thấp vừa số dòng (tối đa 5 dòng, nhiều hơn thì cuộn) — phần còn lại dành cho bảng chuyến.</summary>
        private void FitCargoHeight()
        {
            var rows = Math.Clamp(gridCargo.Rows.Count, 1, 5);
            var height = gridCargo.ColumnHeadersHeight + gridCargo.RowTemplate.Height * rows + LogicalToDeviceUnits(4);
            var r = root.GetRow(gridCargo);
            if (r >= 0 && r < root.RowStyles.Count)
                root.RowStyles[r] = new RowStyle(SizeType.Absolute, height + gridCargo.Margin.Vertical);
            var tr = root.GetRow(gridTrips);
            if (tr >= 0 && tr < root.RowStyles.Count)
                root.RowStyles[tr] = new RowStyle(SizeType.Percent, 100F);
        }

        private TripSummaryItem? SelectedTripAt(int row)
            => row >= 0 && row < gridTrips.Rows.Count && gridTrips.Rows[row].Cells["Id"].Value is int id
                ? _currentOrder?.Trips.FirstOrDefault(t => t.Id == id) : null;

        private TripSummaryItem? SelectedTrip() => gridTrips.CurrentRow is { } r ? SelectedTripAt(r.Index) : null;

        /// <summary>Nút ghi rõ việc theo chuyến đang chọn: đang chạy = theo dõi, đã xong = xem lại.</summary>
        private void UpdateTripMapButton()
        {
            var t = SelectedTrip();
            _btnTripMap.Enabled = t is not null;
            _btnTripMap.Text = t?.Status switch
            {
                null => "🛰️ Xem đường đi chuyến",
                "InProgress" => $"🛰️ Theo dõi chuyến #{t.Id}",
                "Completed" or "PendingConfirmation" => $"🛰️ Xem lại đường đi chuyến #{t.Id}",
                _ => $"🛰️ Xem đường đi chuyến #{t.Id}",
            };
        }

        /// <summary>Bản đồ của 1 chuyến — cùng cách màn Chuyến xe (RouteMapForm có tripId: GPS thật, video, theo dõi trực tiếp).</summary>
        private async Task OpenTripMapAsync()
        {
            if (_api is null || _currentOrder is null) return;
            var t = SelectedTrip();
            if (t is null) { Toast.Show(this, "👆 Chọn 1 chuyến trong bảng trước.", ToastKind.Info); return; }
            try
            {
                var route = await _api.Trips.GetTripRouteAsync(t.Id);
                if (IsDisposed) return;
                if (route.Stops.Count(s => s.Lat.HasValue && s.Lng.HasValue) < 2)
                {
                    Ask.Fail(this, $"Chuyến #{t.Id} thiếu tọa độ (điểm khách chưa chọn từ gợi ý VietMap, hoặc kho chưa có tọa độ) — không vẽ được bản đồ.");
                    return;
                }
                using var f = new RouteMapForm(_api, $"Chuyến #{t.Id} — chặng {LegText(t.LegType)} — Đơn {_currentOrder.Code}",
                    route.Stops, tripId: t.Id, tripStatus: t.Status);
                f.ShowDialog(this);
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi tải bản đồ chuyến: " + ex.Message);
            }
        }
    }
}
