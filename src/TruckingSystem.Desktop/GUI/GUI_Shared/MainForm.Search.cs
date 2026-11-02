using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.GUI_DriverManagement;
using TruckingSystem.Desktop.GUI_TransportOrderManagement;
using TruckingSystem.Desktop.GUI_TripManagement;
using TruckingSystem.Desktop.GUI_VehicleManagement;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>Ô TÌM NHANH TOÀN HỆ THỐNG trên thanh trên cùng (Ctrl+F): gõ mã đơn, tên khách, số chuyến, biển số, tên /
    /// mã tài xế -> hiện tối đa 12 kết quả; bấm (hoặc ↑ ↓ + Enter) để nhảy thẳng tới đúng dòng ở đúng màn.
    /// Dữ liệu tải 1 lần (đơn, chuyến, xe, tài xế — chỉ những gì tài khoản được xem) và giữ 1 phút; không tìm thì không gọi
    /// máy chủ. + Hết phiên đăng nhập: báo 1 lần rồi đưa về màn đăng nhập.</summary>
    public partial class MainForm
    {
        private readonly FlowLayoutPanel _topLeft = new()
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Anchor = AnchorStyles.Left,
            Margin = new Padding(0), Padding = new Padding(0),
        };
        private readonly TextBox _searchBox = new()
        {
            PlaceholderText = "🔎  Tìm đơn, chuyến, biển số, tài xế…   (Ctrl+F)",
            Font = new Font("Segoe UI", 10F), Width = 340, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 14, 0),
        };
        private readonly ListBox _searchList = new()
        {
            DrawMode = DrawMode.OwnerDrawFixed, BorderStyle = BorderStyle.FixedSingle, Visible = false,
            IntegralHeight = false, TabStop = false,
        };
        private readonly System.Windows.Forms.Timer _searchDelay = new() { Interval = 200 };
        private List<SearchHit> _searchData = new();
        private DateTime _searchLoadedAt = DateTime.MinValue;
        private bool _searchLoading;

        private sealed record SearchHit(string Icon, string Title, string Sub, string Haystack, string NavText, Func<Control> Build);

        private static readonly Font HitTitleFont = new("Segoe UI", 10F, FontStyle.Bold);
        private static readonly Font HitSubFont = new("Segoe UI", 8.5F);
        private static readonly Font HitIconFont = new("Segoe UI Emoji", 12F);

        private void SetupGlobalSearch()
        {
            // Chỉ Quản trị / Điều phối được xem đủ đơn, chuyến, xe, tài xế -> vai trò khác ẩn ô tìm.
            if (_api is null || !_api.Connection.IsInRole("Admin", "Dispatcher")) { _searchBox.Visible = false; return; }

            _searchBox.Width = LogicalToDeviceUnits(340);
            _searchList.ItemHeight = LogicalToDeviceUnits(44);
            _searchList.Width = LogicalToDeviceUnits(460);
            Controls.Add(_searchList);

            _searchBox.TextChanged += (_, _) => { _searchDelay.Stop(); _searchDelay.Start(); };
            _searchDelay.Tick += async (_, _) => { _searchDelay.Stop(); await RunSearchAsync(); };
            _searchBox.GotFocus += async (_, _) => await EnsureSearchDataAsync();
            _searchBox.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Down && _searchList.Visible && _searchList.Items.Count > 0)
                {
                    _searchList.SelectedIndex = Math.Min(_searchList.Items.Count - 1, _searchList.SelectedIndex + 1);
                    e.Handled = e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Up && _searchList.Visible && _searchList.Items.Count > 0)
                {
                    _searchList.SelectedIndex = Math.Max(0, _searchList.SelectedIndex - 1);
                    e.Handled = e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = e.SuppressKeyPress = true;
                    OpenHit(_searchList.SelectedItem as SearchHit ?? (_searchList.Items.Count > 0 ? _searchList.Items[0] as SearchHit : null));
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    e.Handled = e.SuppressKeyPress = true;
                    _searchBox.Clear();
                    HideSearchList();
                }
            };
            _searchBox.LostFocus += (_, _) => BeginInvoke(new Action(() => { if (!_searchList.Focused) HideSearchList(); }));
            _searchList.MouseClick += (_, e) =>
            {
                var i = _searchList.IndexFromPoint(e.Location);
                if (i >= 0) OpenHit(_searchList.Items[i] as SearchHit);
            };
            _searchList.MouseMove += (_, e) =>
            {
                var i = _searchList.IndexFromPoint(e.Location);
                if (i >= 0 && i != _searchList.SelectedIndex) _searchList.SelectedIndex = i;
            };
            _searchList.LostFocus += (_, _) => { if (!_searchBox.Focused) HideSearchList(); };
            _searchList.DrawItem += DrawHit;
            FormClosed += (_, _) => _searchDelay.Dispose();

            // Ctrl+F ở bất kỳ màn nào -> nhảy vào ô tìm.
            KeyPreview = true;
            KeyDown += (_, e) =>
            {
                if (e.Control && e.KeyCode == Keys.F && _searchBox.Visible)
                {
                    e.Handled = e.SuppressKeyPress = true;
                    _searchBox.Focus();
                    _searchBox.SelectAll();
                }
            };
        }

        private async Task EnsureSearchDataAsync()
        {
            if (_api is null || _searchLoading || DateTime.Now - _searchLoadedAt < TimeSpan.FromMinutes(1)) return;
            _searchLoading = true;
            try
            {
                var api = _api;
                var ordersTask = api.TransportOrders.GetTransportOrdersAsync();
                var tripsTask = api.Trips.GetTripsAsync();
                var vehiclesTask = api.Vehicles.GetVehiclesAsync();
                var driversTask = api.Drivers.GetDriversAsync();
                await Task.WhenAll(ordersTask, tripsTask, vehiclesTask, driversTask);

                var hits = new List<SearchHit>();
                foreach (var o in ordersTask.Result)
                    hits.Add(new SearchHit("📦", $"Đơn {o.Code} — {o.CustomerName}", $"Đơn vận chuyển · {o.PickupPoint} → {o.DropoffPoint}",
                        FilterBar.Normalize($"{o.Code} {o.CustomerName} {o.PickupPoint} {o.DropoffPoint}"), "Đơn vận chuyển",
                        () => { var c = new TransportOrdersControl(api); c.FocusOn(o.Code); return c; }));
                foreach (var t in tripsTask.Result)
                    hits.Add(new SearchHit("🛣️", $"Chuyến #{t.Id} — đơn {t.OrderCode}", $"Chuyến xe · xe {t.VehiclePlate} · {t.DriverName}",
                        FilterBar.Normalize($"#{t.Id} {t.Id} {t.OrderCode} {t.VehiclePlate} {t.DriverName}"), "Chuyến xe",
                        () => { var c = new TripsControl(api); c.FocusTrip(t.Id); return c; }));
                foreach (var v in vehiclesTask.Result)
                    hits.Add(new SearchHit("🚚", $"Xe {v.PlateNumber}", $"Đội xe · {v.VehicleType}" + (v.WarehouseName is null ? "" : $" · {v.WarehouseName}"),
                        FilterBar.Normalize($"{v.PlateNumber} {v.PlateNumber.Replace("-", "").Replace(".", "")} {v.VehicleType}"), "Đội xe",
                        () => { var c = new VehiclesControl(api); c.FocusOn(v.PlateNumber); return c; }));
                foreach (var d in driversTask.Result)
                    hits.Add(new SearchHit("🚕", $"Tài xế {d.FullName} ({d.Code})", "Tài xế" + (string.IsNullOrEmpty(d.Phone) ? "" : $" · {d.Phone}"),
                        FilterBar.Normalize($"{d.FullName} {d.Code} {d.Phone}"), "Tài xế",
                        () => { var c = new DriversControl(api); c.FocusOn(d.Code); return c; }));
                _searchData = hits;
                _searchLoadedAt = DateTime.Now;
            }
            catch { /* lỗi tải -> lần gõ sau thử lại; ô tìm không báo lỗi làm phiền */ }
            finally { _searchLoading = false; }
        }

        private async Task RunSearchAsync()
        {
            var q = FilterBar.Normalize(_searchBox.Text.Trim());
            if (q.Length < 2) { HideSearchList(); return; }
            await EnsureSearchDataAsync();
            if (IsDisposed || FilterBar.Normalize(_searchBox.Text.Trim()) != q) return; // đã gõ tiếp -> lượt sau xử lý

            var words = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var results = _searchData
                .Where(h => words.All(h.Haystack.Contains))
                .OrderByDescending(h => h.Haystack.StartsWith(q))   // khớp đầu (mã đơn, biển số) lên trước
                .Take(12)
                .ToList();

            _searchList.BeginUpdate();
            _searchList.Items.Clear();
            if (results.Count == 0)
                _searchList.Items.Add(new SearchHit("🔎", "Không tìm thấy kết quả", "Thử gõ mã đơn, số chuyến, biển số hoặc tên tài xế", "", "", () => new Control()));
            else
                foreach (var r in results) _searchList.Items.Add(r);
            _searchList.SelectedIndex = results.Count > 0 ? 0 : -1;
            _searchList.EndUpdate();

            var p = PointToClient(_searchBox.PointToScreen(new Point(0, _searchBox.Height + LogicalToDeviceUnits(4))));
            _searchList.Location = p;
            _searchList.Height = Math.Min(_searchList.Items.Count, 8) * _searchList.ItemHeight + 2;
            _searchList.Visible = true;
            _searchList.BringToFront();
        }

        private void HideSearchList()
        {
            if (!_searchList.IsDisposed) _searchList.Visible = false;
        }

        private void OpenHit(SearchHit? hit)
        {
            if (hit is null || hit.NavText.Length == 0) return;
            var btn = _navButtons.FirstOrDefault(b => b.Text.TrimEnd().EndsWith(hit.NavText));
            if (btn is null) return;
            HideSearchList();
            _searchBox.Clear();
            NavigateTo(btn, hit.Build);
        }

        private void DrawHit(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || _searchList.Items[e.Index] is not SearchHit h) return;
            var selected = (e.State & DrawItemState.Selected) != 0 && h.NavText.Length > 0;
            using (var bg = new SolidBrush(selected ? DayScene.Hex("#DBEAFE") : Color.White)) e.Graphics.FillRectangle(bg, e.Bounds);
            var pad = LogicalToDeviceUnits(10);
            var iconW = LogicalToDeviceUnits(30);
            TextRenderer.DrawText(e.Graphics, h.Icon, HitIconFont,
                new Rectangle(e.Bounds.X + pad, e.Bounds.Y, iconW, e.Bounds.Height), Theme.TextPrimary,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            var x = e.Bounds.X + pad + iconW + LogicalToDeviceUnits(4);
            var w = e.Bounds.Width - x - pad;
            var half = e.Bounds.Height / 2;
            TextRenderer.DrawText(e.Graphics, h.Title, HitTitleFont, new Rectangle(x, e.Bounds.Y + LogicalToDeviceUnits(3), w, half),
                Theme.TextPrimary, TextFormatFlags.Bottom | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(e.Graphics, h.Sub, HitSubFont, new Rectangle(x, e.Bounds.Y + half, w, half - LogicalToDeviceUnits(3)),
                Theme.TextSecondary, TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        // ================= Hết phiên đăng nhập =================

        private bool _sessionHandled;

        private void OnSessionExpired(object? sender, EventArgs e)
        {
            if (_sessionHandled || IsDisposed || !IsHandleCreated) return;
            _sessionHandled = true;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;
                MessageBox.Show(this, "Phiên đăng nhập đã hết hạn (để lâu không dùng hoặc tài khoản vừa được đổi quyền).\n\nVui lòng đăng nhập lại.",
                    "Hết phiên đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _api?.Connection.ClearToken();
                IsLoggingOut = true;
                LogoutRequested?.Invoke(this, EventArgs.Empty);
            }));
        }
    }
}
