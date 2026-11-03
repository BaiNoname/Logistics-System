using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ResourceManagement;
using TruckingSystem.Desktop.SERVICE_ResourceManagement;

namespace TruckingSystem.Desktop.GUI_ResourceManagement
{
    /// <summary>🧭 NGUỒN LỰC THEO KHO — 1 màn để nhân viên điều phối nhìn nhanh: kho này hôm nay
    /// có những XE nào, TÀI XẾ nào chạy được, cái nào đang vướng gì (giấy tờ thiếu / sắp hết hạn,
    /// chưa đăng ký ngày làm, đang bận chuyến...). Server tính theo ĐÚNG quy tắc Chia xe / Bắt đầu
    /// chuyến (chỉ đọc). Bấm 1 xe -> bảng Tài xế đánh dấu ai đủ hạng bằng lái xe đó (và ngược lại).
    /// Nút thao tác nhanh mở đúng các form có sẵn (Giấy tờ xe, Nhiên liệu, Hồ sơ tài xế, Ngày làm).
    /// Giao diện tĩnh ở ResourcesControl.Designer.cs.</summary>
    public partial class ResourcesControl : BaseUserControl
    {
        private const int ExpiringDays = 30;
        private ApiServices? _api;
        private ResourceApiService? _service;
        private ResourceOverview _data = new();
        private bool _loadingWarehouses;
        private int _loadId;

        public ResourcesControl()
        {
            InitializeComponent();
            ScreenKit.Help(this, "resources", "Xem nhanh xe và tài xế của từng kho: ai / xe nào chạy được hôm nay, vì sao không.",
                new[] { "Chọn kho", "Xem cột \"Chạy được hôm nay\" — rê chuột để xem lý do", "Nhấp đúp 1 dòng để xem đủ thông tin và sửa" },
                "Ctrl+F tìm nhanh");
            Theme.StyleGrid(gridVehicles);
            Theme.StyleGrid(gridDrivers);
            gridVehicles.DataBindingComplete += (_, _) =>
            {
                if (gridVehicles.Columns["Id"] is { } c) c.Visible = false;
                // Cột chữ đổi theo dòng chọn ở bảng kia ("✖ Không đủ hạng bằng") -> chừa sẵn chỗ.
                if (gridVehicles.Columns["Hợp_tài_xế_đang_chọn"] is { } pick) pick.MinimumWidth = MatchColumnWidth(gridVehicles);
                UpdateButtons();
            };
            gridDrivers.DataBindingComplete += (_, _) =>
            {
                if (gridDrivers.Columns["EmployeeId"] is { } c) c.Visible = false;
                if (gridDrivers.Columns["Hợp_xe_đang_chọn"] is { } pick) pick.MinimumWidth = MatchColumnWidth(gridDrivers);
                UpdateButtons();
            };
            // Cột vừa nội dung (không cắt chữ). Mỗi bảng CAO CỐ ĐỊNH ~10 dòng, có thanh cuộn DỌC + NGANG riêng — trước
            // đây bảng "cao theo số dòng" để cả trang cuộn, nhưng nhiều xe / tài xế thì dòng cuối bị khuất, không cuộn
            // xem hết được. Bảng chỉ giữ cột chính; nhấp đúp 1 dòng xem ĐẦY ĐỦ thông tin (ResourceDetailForm).
            GridLayout.FitColumns(gridVehicles);
            GridLayout.FitColumns(gridDrivers);
            foreach (var g in new[] { gridVehicles, gridDrivers })
            {
                // Bảng LẤP ĐẦY ô của khung (Dock = Fill) — rộng đúng bằng phần nhìn thấy, cột dư ra thì CHÍNH BẢNG hiện
                // thanh cuộn ngang; cao cố định ~10 dòng (Min = Max) và có thanh cuộn dọc riêng.
                // (Trước đây bảng neo Left|Right giữ bề ngang thiết kế 1100 -> màn hẹp hơn thì cột cuối bị khung CHE
                // mất, bảng tưởng vẫn đủ chỗ nên không hiện thanh cuộn ngang.)
                var h = LogicalToDeviceUnits(380);
                g.ScrollBars = ScrollBars.Both;
                // Bề ngang "mặc định" nhỏ: khung AutoSize bên ngoài lấy bề ngang của bảng làm kích thước ưa thích —
                // để 1100 (Designer) thì cả trang bị đẩy rộng ra. Dock = Fill sẽ giãn bảng ra đúng bề ngang thật.
                g.Size = new Size(LogicalToDeviceUnits(300), h);
                g.Dock = DockStyle.Fill;
                g.MinimumSize = new Size(0, h);
                g.MaximumSize = new Size(0, h);
                g.Height = h;
            }
            gridVehicles.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) ShowVehicleDetail(); };
            gridDrivers.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) ShowDriverDetail(); };
            Load += (_, _) => Theme.UniformButtons(vehicleBar, driverBar);

            // Bộ lọc cho 2 bảng (lọc trên dữ liệu đã tải, không gọi lại API).
            vehicleFilter.AddChoice("state", "Tình trạng", new[] { StateReady, StateBlocked, StateExpiring });
            vehicleFilter.AddChoice("status", "Trạng thái", new[] { "Rảnh", "Đang chạy", "Bảo trì", "Ngưng hoạt động" });
            vehicleFilter.AddChoice("license", "Cần bằng", Array.Empty<string>());
            vehicleFilter.FilterChanged += (_, _) => BindVehicles();
            FilterMemory.Attach(vehicleFilter, "res-vehicles"); // nhớ bộ lọc khi chuyển màn rồi quay lại
            driverFilter.AddChoice("state", "Tình trạng", new[] { StateReady, StateBlocked, StateExpiring });
            driverFilter.AddChoice("workday", "Ngày làm", new[] { WorkToday, WorkNotToday });
            driverFilter.AddChoice("license", "Hạng bằng", Array.Empty<string>());
            driverFilter.FilterChanged += (_, _) => BindDrivers();
            FilterMemory.Attach(driverFilter, "res-drivers"); // nhớ bộ lọc khi chuyển màn rồi quay lại
            SetupRowMenus();
            // Rê chuột vào ô "Chạy được hôm nay" -> lý do đầy đủ (ô chỉ ghi lý do ngắn).
            gridVehicles.CellToolTipTextNeeded += (_, e) =>
            {
                if (e.RowIndex < 0 || gridVehicles.Columns[e.ColumnIndex].Name != "Chạy_được_hôm_nay") return;
                if (gridVehicles.Rows[e.RowIndex].Cells["Id"].Value is int id && _data.Vehicles.FirstOrDefault(x => x.Id == id) is { CanRunToday: false } v)
                    e.ToolTipText = v.CurrentTrip is not null ? "Đang chạy " + v.CurrentTrip : v.BlockReason ?? "";
            };
            gridDrivers.CellToolTipTextNeeded += (_, e) =>
            {
                if (e.RowIndex < 0 || gridDrivers.Columns[e.ColumnIndex].Name != "Chạy_được_hôm_nay") return;
                if (gridDrivers.Rows[e.RowIndex].Cells["EmployeeId"].Value is int id && _data.Drivers.FirstOrDefault(x => x.EmployeeId == id) is { CanRunToday: false } d)
                    e.ToolTipText = d.CurrentTrip is not null ? "Đang chạy " + d.CurrentTrip : d.BlockReason ?? "";
            };
            // Ẩn / khóa sẵn các nút theo "chưa chọn dòng" TRƯỚC khi màn hiện — không còn cảnh hiện cả loạt nút rồi tải xong mới ẩn đi.
            UpdateButtons();
            Load += async (_, _) => await LoadAsync();
        }

        /// <summary>Màn này để NHÌN NHANH hôm nay kho có xe / tài xế nào chạy được -> bỏ 2 thanh nút (trùng màn Đội xe,
        /// Tài xế), thao tác chuyển vào CHUỘT PHẢI trên từng dòng: xe = Giấy tờ / Nhiên liệu; tài xế = Hồ sơ / Ngày làm.</summary>
        private void SetupRowMenus()
        {
            vehicleBar.Visible = false;
            driverBar.Visible = false;

            // Sửa giấy tờ / nhiên liệu (xe), hồ sơ / ngày làm (tài xế): làm ngay trong cửa sổ "Xem đầy đủ" — chuột phải
            // chỉ còn 2 mục (ghép xe – tài xế, xem đầy đủ / sửa), không lặp lại các mục sửa riêng.
            var vMenu = new ContextMenuStrip();
            var vDetail = new ToolStripMenuItem("🔍  Xem đầy đủ / sửa giấy tờ, nhiên liệu") { ShortcutKeyDisplayString = "Nhấp đúp" };
            vDetail.Click += (_, _) => ShowVehicleDetail();
            var vPair = new ToolStripMenuItem("🔗  Tài xế đang đi cùng / tài xế lái được xe này") { Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            vPair.Click += (_, _) => ShowVehiclePairing();
            vMenu.Items.AddRange(new ToolStripItem[] { vPair, vDetail });
            ModernMenu.Apply(vMenu);
            vMenu.Opening += (_, e) => { if (SelectedVehicle() is null) e.Cancel = true; };
            AttachMenu(gridVehicles, vMenu);

            var dMenu = new ContextMenuStrip();
            var dDetail = new ToolStripMenuItem("🔍  Xem đầy đủ / sửa hồ sơ, ngày làm") { ShortcutKeyDisplayString = "Nhấp đúp" };
            dDetail.Click += (_, _) => ShowDriverDetail();
            var dPair = new ToolStripMenuItem("🔗  Xe đang dùng / xe lái được") { Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            dPair.Click += (_, _) => ShowDriverPairing();
            dMenu.Items.AddRange(new ToolStripItem[] { dPair, dDetail });
            ModernMenu.Apply(dMenu);
            dMenu.Opening += (_, e) => { if (SelectedDriver() is null) e.Cancel = true; };
            AttachMenu(gridDrivers, dMenu);
        }

        // ================= Ghép xe – tài xế (chuột phải → 🔗) =================

        private void ShowVehiclePairing()
        {
            if (SelectedVehicle() is not VehicleResource v) return;
            var list = _data.Drivers.Where(d => d.DrivableVehicleIds.Contains(v.Id))
                .OrderByDescending(d => d.CanRunToday).ThenBy(d => d.CurrentTrip is not null).ThenBy(d => d.FullName)
                .Select(d => new
                {
                    Mã_NV = d.Code,
                    Họ_tên = d.FullName,
                    Hạng_bằng = d.LicenseClass ?? "—",
                    Ngày_làm_hôm_nay = d.WorksToday ? "✅ Có" : "❌ Chưa ĐK",
                    Tình_trạng = d.CanRunToday ? "✅ Chạy được" : d.CurrentTrip is not null ? "🚚 Đang bận chuyến" : "⛔ " + d.BlockReason,
                    Đang_ở_chuyến = d.CurrentTrip is null ? "—" : (d.CurrentVehiclePlate is null ? "" : $"xe {d.CurrentVehiclePlate} · ") + d.CurrentTrip,
                }).ToList();
            using var f = new PairingForm($"🔗 Xe {v.PlateNumber} ({v.CapacityKg / 1000m:0.##} tấn · cần bằng {v.RequiredLicense})",
                DriverOnTrip(v), "Tài xế đủ hạng bằng lái xe này", list, list.Count);
            f.ShowDialog(FindForm());
        }

        private void ShowDriverPairing()
        {
            if (SelectedDriver() is not DriverResource d) return;
            var list = _data.Vehicles.Where(v => d.DrivableVehicleIds.Contains(v.Id))
                .OrderByDescending(v => v.CanRunToday).ThenBy(v => v.CurrentTrip is not null).ThenBy(v => v.PlateNumber)
                .Select(v => new
                {
                    Biển_số = v.PlateNumber,
                    Loại_xe = $"{v.VehicleType} ({v.CapacityKg / 1000m:0.##} tấn)",
                    Cần_bằng = v.RequiredLicense,
                    Giấy_tờ = DocsSummary(v),
                    Tình_trạng = v.CanRunToday ? "✅ Chạy được" : v.CurrentTrip is not null ? "🚚 Đang bận chuyến" : "⛔ " + v.BlockReason,
                }).ToList();
            var onTrip = d.CurrentTrip is null ? "—" : (d.CurrentVehiclePlate is null ? "" : $"xe {d.CurrentVehiclePlate} · ") + d.CurrentTrip;
            using var f = new PairingForm($"🔗 Tài xế {d.FullName} ({d.Code}) · bằng {d.LicenseClass ?? "—"}",
                onTrip, "Xe tài xế này lái được (đủ hạng bằng)", list, list.Count);
            f.ShowDialog(FindForm());
        }

        private static void AttachMenu(DataGridView grid, ContextMenuStrip menu)
        {
            grid.ContextMenuStrip = menu;
            grid.CellMouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
                var col = Math.Max(0, e.ColumnIndex);
                if (grid.Rows[e.RowIndex].Cells[col].Visible) grid.CurrentCell = grid.Rows[e.RowIndex].Cells[col];
                else grid.Rows[e.RowIndex].Selected = true;
            };
        }

        public ResourcesControl(ApiServices api) : this()
        {
            _api = api;
            _service = new ResourceApiService(api.Connection);
        }

        private static int MatchColumnWidth(DataGridView grid)
            => TextRenderer.MeasureText("✖ Không đủ hạng bằng", Theme.FontBold).Width + grid.Font.Height * 2;

        /// <summary>Bấm vào 1 bảng cao hơn khung nhìn: WinForms mặc định cuộn trang về ĐẦU bảng (nhảy
        /// màn hình). Giữ nguyên vị trí cuộn hiện tại.</summary>
        protected override Point ScrollToControl(Control activeControl) => DisplayRectangle.Location;

        // ================= Tải dữ liệu =================

        private int? SelectedWarehouseId => cboWarehouse.SelectedItem is ResourceWarehouse w && w.Id != 0 ? w.Id : null;

        private async Task LoadAsync()
        {
            if (_service is null) return;
            var loadId = ++_loadId;
            lblStatus.Text = "Đang tải...";
            lblStatus.ForeColor = Theme.TextSecondary;
            try
            {
                var data = await _service.GetOverviewAsync(SelectedWarehouseId);
                if (IsDisposed || loadId != _loadId) return; // đã đổi kho tiếp -> bỏ kết quả cũ
                _data = data;
                BindWarehouses();
                BindSummary();
                vehicleFilter.SetChoiceOptions("license", _data.Vehicles.Select(v => v.RequiredLicense));
                driverFilter.SetChoiceOptions("license", _data.Drivers.Select(d => d.LicenseClass ?? "—"));
                BindVehicles();
                BindDrivers();
                lblStatus.Text = $"Cập nhật lúc {DateTime.Now:HH:mm} — \"Chạy được hôm nay\" tính đúng như Chia xe: xe Rảnh + đủ 3 giấy tờ còn hạn; " +
                                 "tài xế có hồ sơ, bằng lái / khám sức khỏe còn hạn, đã đăng ký ngày làm hôm nay, không bận chuyến khác.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải dữ liệu: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private void BindWarehouses()
        {
            var keep = SelectedWarehouseId;
            _loadingWarehouses = true;
            try
            {
                var items = new List<ResourceWarehouse> { new() { Id = 0, Name = "Tất cả kho" } };
                items.AddRange(_data.Warehouses);
                cboWarehouse.DataSource = items;
                cboWarehouse.DisplayMember = nameof(ResourceWarehouse.Name);
                var idx = keep is int k ? items.FindIndex(w => w.Id == k) : 0;
                cboWarehouse.SelectedIndex = idx >= 0 ? idx : 0;
            }
            finally { _loadingWarehouses = false; }
        }

        private void BindSummary()
        {
            var s = _data.Summary;
            lblSumVehicles.Text = $"🚚 Xe chạy được hôm nay\n{s.VehiclesReady} / {s.VehiclesTotal}";
            lblSumVehicles.ForeColor = s.VehiclesReady == 0 && s.VehiclesTotal > 0 ? Theme.Danger : Theme.TextPrimary;
            lblSumDrivers.Text = $"👷 Tài xế chạy được hôm nay\n{s.DriversReady} / {s.DriversTotal}";
            lblSumDrivers.ForeColor = s.DriversReady == 0 && s.DriversTotal > 0 ? Theme.Danger : Theme.TextPrimary;
            lblSumDocs.Text = $"📄 Giấy tờ: ⛔ {s.DocsExpiredOrMissing} thiếu / hết hạn\n🟡 {s.DocsExpiringSoon} sắp hết hạn ({ExpiringDays} ngày)";
            lblSumDocs.ForeColor = s.DocsExpiredOrMissing > 0 ? Theme.Danger : s.DocsExpiringSoon > 0 ? Theme.Warning : Theme.Success;
            lblSumTrips.Text = $"🛣️ Chuyến hôm nay\n{s.TripsStartingToday} sẽ xuất phát · {s.TripsRunning} đang chạy";
        }

        // ================= Hiển thị =================

        /// <summary>Ô giấy tờ: "✅ 12/05/2027" / "🟡 20/10/2026 (còn 6 ngày)" / "⛔ Hết hạn 01/10/2026" / "⛔ Chưa nhập".</summary>
        private static string DocText(DateTime? expiry, bool required = true)
        {
            if (expiry is not DateTime d) return required ? "⛔ Chưa nhập" : "— chưa nhập";
            var days = (d.Date - DateTime.Today).Days;
            if (days < 0) return $"⛔ Hết hạn {d:dd/MM/yyyy}";
            if (days <= ExpiringDays) return $"🟡 {d:dd/MM/yyyy} (còn {days} ngày)";
            return $"✅ {d:dd/MM/yyyy}";
        }

        private static string VehicleStatusText(string s) => s switch
        {
            "Available" => "Rảnh",
            "OnTrip" => "Đang chạy",
            "UnderMaintenance" => "Bảo trì",
            "OutOfService" => "Ngưng hoạt động",
            _ => s,
        };

        private static string FuelText(VehicleResource v)
            => v.FuelCurrentLiters is not decimal cur ? "— chưa khai"
             : v.FuelTankCapacityLiters is decimal cap && cap > 0 ? $"{(cur / cap <= 0.15m ? "⚠️ " : "")}~{cur:0} / {cap:0} L" : $"~{cur:0} L";

        private const string StateReady = "✅ Chạy được hôm nay";
        private const string StateBlocked = "⛔ Không chạy được";
        private const string StateExpiring = "🟡 Giấy tờ sắp hết hạn";
        private const string WorkToday = "✅ Có làm hôm nay";
        private const string WorkNotToday = "❌ Chưa ĐK hôm nay";

        /// <summary>Ô "Tình trạng": "sắp hết hạn" có thể trùng với "chạy được" nên xét riêng.</summary>
        private static bool MatchState(string? state, bool canRun, int expiringSoon) => state switch
        {
            StateReady => canRun,
            StateBlocked => !canRun,
            StateExpiring => expiringSoon > 0,
            _ => true,
        };

        private void BindVehicles()
        {
            var keep = SelectedVehicle()?.Id;
            var rows = _data.Vehicles.Where(v =>
                    MatchState(vehicleFilter.Choice("state"), v.CanRunToday, v.ExpiringSoonCount) &&
                    vehicleFilter.Is("status", VehicleStatusText(v.Status)) &&
                    vehicleFilter.Is("license", v.RequiredLicense) &&
                    vehicleFilter.MatchesSearch(v.PlateNumber, v.VehicleType, v.WarehouseName, v.CurrentTrip, v.BlockReason))
                .ToList();
            vehicleFilter.SetCount(rows.Count, _data.Vehicles.Count);
            // CHỈ cột chính (cùng kiểu màn Chuyến); giấy tờ từng loại, nhiên liệu, chuyến hiện tại, lý do đầy đủ: nhấp đúp.
            gridVehicles.DataSource = rows.Select(v => new
            {
                v.Id,
                Biển_số = v.PlateNumber,
                Tải_trọng = $"{v.CapacityKg / 1000m:0.##} tấn",
                Trạng_thái = VehicleStatusText(v.Status),
                Giấy_tờ = DocsSummary(v),
                Chạy_được_hôm_nay = RunText(v.CanRunToday, v.CurrentTrip, v.BlockReason),
                // "Đang đi cùng tài xế nào" + "tài xế nào lái được": chuột phải → 🔗 (PairingForm) cho bảng gọn.
            }).ToList();
            grpVehicles.Text = $"🚚 Xe ({rows.Count}/{_data.Vehicles.Count}) — nhấp đúp: xem đầy đủ / sửa giấy tờ, nhiên liệu · chuột phải: tài xế đi cùng / lái được";
            Reselect(gridVehicles, "Id", keep, "Biển_số");
        }

        private void BindDrivers()
        {
            var keep = SelectedDriver()?.EmployeeId;
            var rows = _data.Drivers.Where(d =>
                    MatchState(driverFilter.Choice("state"), d.CanRunToday, d.ExpiringSoonCount) &&
                    driverFilter.Is("workday", d.WorksToday ? WorkToday : WorkNotToday) &&
                    driverFilter.Is("license", d.LicenseClass ?? "—") &&
                    driverFilter.MatchesSearch(d.Code, d.FullName, d.Phone, d.WarehouseName, d.CurrentTrip, d.CurrentVehiclePlate, d.BlockReason))
                .ToList();
            driverFilter.SetCount(rows.Count, _data.Drivers.Count);
            gridDrivers.DataSource = rows.Select(d => new
            {
                d.EmployeeId,
                Mã_NV = d.Code,
                Họ_tên = d.FullName,
                Hạng_bằng = d.LicenseClass ?? "—",
                Ngày_làm_hôm_nay = d.WorksToday ? "✅ Có" : "❌ Chưa ĐK",
                Chạy_được_hôm_nay = RunText(d.CanRunToday, d.CurrentTrip, d.BlockReason),
            }).ToList();
            grpDrivers.Text = $"👷 Tài xế ({rows.Count}/{_data.Drivers.Count}) — nhấp đúp: xem đầy đủ / sửa hồ sơ, ngày làm · chuột phải: xe đang dùng / lái được";
            Reselect(gridDrivers, "EmployeeId", keep, "Mã_NV");
        }

        /// <summary>"Chạy được hôm nay": đang có chuyến chưa xong thì ghi BẬN (không phải lỗi giấy tờ / ngày làm) —
        /// trước đây xe / tài xế đang chạy chuyến cũng hiện "⛔ Không" như thể có vấn đề.</summary>
        private static string RunText(bool canRun, string? currentTrip, string? reason)
            => canRun ? "✅ Được" : currentTrip is not null ? "🚚 Đang bận chuyến" : "⛔ " + ShortReason(reason);

        /// <summary>Lý do NGẮN ngay trong ô "Chạy được hôm nay" (VD "Hết hạn đăng kiểm") — trước đây chỉ ghi "⛔ Không",
        /// phải mở chi tiết mới biết vì sao. Rê chuột vào ô xem lý do đầy đủ.</summary>
        private static string ShortReason(string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return "Không";
            var first = reason.Split(new[] { ";", " — ", " - ", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim().TrimEnd('.');
            if (first.Length > 0) first = char.ToUpper(first[0]) + first[1..];
            return first.Length > 40 ? first[..38] + "…" : first;
        }

        /// <summary>Tài xế đang đi cùng xe này (chuyến chưa xong) — ghép theo biển số xe của tài xế.</summary>
        private string DriverOnTrip(VehicleResource v)
        {
            var d = _data.Drivers.FirstOrDefault(x => x.CurrentVehiclePlate == v.PlateNumber);
            return d is not null ? $"{d.FullName} ({d.Code}) · {d.CurrentTrip}" : v.CurrentTrip is null ? "—" : v.CurrentTrip;
        }

        /// <summary>Gộp 3 loại giấy tờ xe thành 1 ô: "✅ Đủ" / "🟡 1 sắp hết hạn" / "⛔ Thiếu 2 loại".</summary>
        private static string DocsSummary(VehicleResource v)
        {
            var docs = new[] { v.RegistrationExpiry, v.InsuranceExpiry, v.PermitExpiry };
            var bad = docs.Count(d => d is null || d.Value.Date < DateTime.Today);
            if (bad > 0) return $"⛔ Thiếu / hết hạn {bad} loại";
            var soon = docs.Count(d => (d!.Value.Date - DateTime.Today).Days <= ExpiringDays);
            return soon > 0 ? $"🟡 {soon} loại sắp hết hạn" : "✅ Đủ";
        }

        // ================= Chi tiết (nhấp đúp) =================

        private void ShowVehicleDetail()
        {
            if (_api is null || SelectedVehicle() is not VehicleResource sel) return;
            var id = sel.Id;
            VehicleResource? V() => _data.Vehicles.FirstOrDefault(x => x.Id == id); // lấy lại sau mỗi lần sửa (dữ liệu mới)
            using var f = new ResourceDetailForm($"🚚 Xe {sel.PlateNumber}", () => V() is not VehicleResource v
                ? new[] { ("Không còn thấy xe này", "— có thể đã đổi kho / bị xóa") }
                : new (string, string)[]
                {
                    ("# Hôm nay", ""),
                    ("Chạy được", v.CanRunToday ? "✅ Được" : v.CurrentTrip is not null ? "🚚 Đang bận chuyến" : "⛔ Không — " + v.BlockReason),
                    ("Trên chuyến", DriverOnTrip(v)),
                    ("Tài xế lái được", $"{_data.Drivers.Count(d => d.DrivableVehicleIds.Contains(v.Id))} người đủ hạng bằng · " +
                                         $"{_data.Drivers.Count(d => d.DrivableVehicleIds.Contains(v.Id) && d.CanRunToday)} người rảnh hôm nay"),
                    ("# Thông tin xe", ""),
                    ("Xe", $"{v.VehicleType} · {v.CapacityKg / 1000m:0.##} tấn · cần bằng {v.RequiredLicense}"),
                    ("Kho / trạng thái", $"{v.WarehouseName ?? "(mọi kho)"} · {VehicleStatusText(v.Status)}"),
                    ("Nhiên liệu", FuelText(v)),
                    ("# Giấy tờ", ""),
                    ("Đăng kiểm", DocText(v.RegistrationExpiry)),
                    ("Bảo hiểm", DocText(v.InsuranceExpiry)),
                    ("Phù hiệu xe", DocText(v.PermitExpiry)),
                },
                () => V() is not VehicleResource v ? Array.Empty<(string, bool, Func<IWin32Window, Task>)>()
                    : new (string, bool, Func<IWin32Window, Task>)[]
                    {
                        ("📄 Sửa giấy tờ xe", true, owner => EditVehicleDocsAsync(v, owner)),
                        ("⛽ Cập nhật nhiên liệu", true, owner => EditVehicleFuelAsync(v, owner)),
                    });
            f.ShowDialog(FindForm());
        }

        private void ShowDriverDetail()
        {
            if (_api is null || SelectedDriver() is not DriverResource sel) return;
            var id = sel.EmployeeId;
            DriverResource? D() => _data.Drivers.FirstOrDefault(x => x.EmployeeId == id);
            using var f = new ResourceDetailForm($"👷 Tài xế {sel.FullName} ({sel.Code})", () => D() is not DriverResource d
                ? new[] { ("Không còn thấy tài xế này", "— có thể đã đổi kho") }
                : new (string, string)[]
                {
                    ("# Hôm nay", ""),
                    ("Chạy được", d.CanRunToday ? "✅ Được" : d.CurrentTrip is not null ? "🚚 Đang bận chuyến" : "⛔ Không — " + d.BlockReason),
                    ("Ngày làm", (d.WorksToday ? "✅ Có làm hôm nay" : "❌ Hôm nay chưa đăng ký") + $" · 7 ngày tới: {d.WorkDaysNext7} ngày"),
                    ("Trên chuyến", d.CurrentTrip is null ? "—" : (d.CurrentVehiclePlate is null ? "" : $"xe {d.CurrentVehiclePlate} · ") + d.CurrentTrip),
                    ("Xe lái được", $"{d.DrivableVehicleIds.Count} xe trong danh sách"),
                    ("# Thông tin", ""),
                    ("Liên hệ", d.Phone ?? "—"),
                    ("Kho", !d.HasProfile ? "—" : d.WarehouseName ?? "(mọi kho)"),
                    ("# Giấy tờ", ""),
                    ("Hồ sơ / hạng bằng", d.HasProfile ? $"✅ Đã có · bằng {d.LicenseClass ?? "—"}" : "⛔ Chưa có hồ sơ"),
                    ("Bằng lái", d.HasProfile ? DocText(d.LicenseExpiry) : "—"),
                    ("Khám sức khỏe", d.HasProfile ? DocText(d.HealthCertExpiry, required: false) : "—"),
                },
                () => D() is not DriverResource d ? Array.Empty<(string, bool, Func<IWin32Window, Task>)>()
                    : new (string, bool, Func<IWin32Window, Task>)[]
                    {
                        (d.HasProfile ? "✏️ Sửa hồ sơ" : "📝 Tạo hồ sơ", true, owner => EditDriverProfileAsync(d, owner)),
                        ("📅 Ngày làm việc", d.HasProfile, owner => EditDriverWorkDaysAsync(d, owner)),
                    });
            f.ShowDialog(FindForm());
        }

        private static void Reselect(DataGridView grid, string idColumn, int? id, string visibleColumn)
        {
            grid.ClearSelection();
            if (id is not int want) { grid.CurrentCell = null; return; }
            foreach (DataGridViewRow r in grid.Rows)
                if (r.Cells[idColumn].Value is int v && v == want) { grid.CurrentCell = r.Cells[visibleColumn]; r.Selected = true; return; }
            grid.CurrentCell = null;
        }

        private VehicleResource? SelectedVehicle()
            => gridVehicles.CurrentRow?.Cells["Id"].Value is int id && gridVehicles.CurrentRow.Selected
                ? _data.Vehicles.FirstOrDefault(v => v.Id == id) : null;

        private DriverResource? SelectedDriver()
            => gridDrivers.CurrentRow?.Cells["EmployeeId"].Value is int id && gridDrivers.CurrentRow.Selected
                ? _data.Drivers.FirstOrDefault(d => d.EmployeeId == id) : null;

        private void UpdateButtons()
        {
            btnVehicleDocs.Enabled = btnVehicleFuel.Enabled = SelectedVehicle() is not null;
            var d = SelectedDriver();
            btnDriverProfile.Enabled = d is not null;
            btnDriverWorkDays.Enabled = d is { HasProfile: true };
            btnDriverProfile.Text = d is { HasProfile: false } ? "📝 Tạo hồ sơ tài xế" : "📝 Hồ sơ tài xế";
        }

        // ================= Ghép xe – tài xế (theo hạng bằng) =================

        private void GridVehicles_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtons();
            gridDrivers.Invalidate(); // cột "Xe đang chọn" + tô màu dòng tài xế lái được
            var v = SelectedVehicle();
            if (v is not null)
            {
                var can = _data.Drivers.Count(d => d.DrivableVehicleIds.Contains(v.Id));
                var ready = _data.Drivers.Count(d => d.DrivableVehicleIds.Contains(v.Id) && d.CanRunToday);
                lblHint.Text = $"Xe {v.PlateNumber} cần bằng {v.RequiredLicense}: {can} tài xế đủ hạng bằng, trong đó {ready} người chạy được hôm nay — chuột phải → 🔗 để xem danh sách.";
            }
        }

        private void GridDrivers_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtons();
            gridVehicles.Invalidate();
            var d = SelectedDriver();
            if (d is not null)
                lblHint.Text = d.LicenseClass is null
                    ? $"{d.FullName} chưa có hồ sơ / hạng bằng — chưa lái được xe nào."
                    : $"{d.FullName} (bằng {d.LicenseClass}) lái được {d.DrivableVehicleIds.Count} xe trong danh sách — chuột phải → 🔗 để xem danh sách.";
        }

        private void GridVehicles_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle is null) return;
            var name = gridVehicles.Columns[e.ColumnIndex].Name;
            if (name == "Hợp_tài_xế_đang_chọn")
            {
                var d = SelectedDriver();
                var vid = gridVehicles.Rows[e.RowIndex].Cells["Id"].Value as int?;
                if (d is null || vid is null) { e.Value = "—"; e.CellStyle.ForeColor = Theme.TextSecondary; }
                else if (d.DrivableVehicleIds.Contains(vid.Value)) { e.Value = "✔ Lái được"; e.CellStyle.ForeColor = Theme.Success; e.CellStyle.Font = Theme.FontBold; }
                else { e.Value = "✖ Không đủ hạng bằng"; e.CellStyle.ForeColor = Theme.TextSecondary; }
                e.FormattingApplied = true;
                return;
            }
            ColorStatusCell(name, e);
        }

        private void GridDrivers_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle is null) return;
            var name = gridDrivers.Columns[e.ColumnIndex].Name;
            if (name == "Hợp_xe_đang_chọn")
            {
                var v = SelectedVehicle();
                var did = gridDrivers.Rows[e.RowIndex].Cells["EmployeeId"].Value as int?;
                var driver = did is null ? null : _data.Drivers.FirstOrDefault(x => x.EmployeeId == did);
                if (v is null || driver is null) { e.Value = "—"; e.CellStyle.ForeColor = Theme.TextSecondary; }
                else if (driver.DrivableVehicleIds.Contains(v.Id)) { e.Value = "✔ Lái được"; e.CellStyle.ForeColor = Theme.Success; e.CellStyle.Font = Theme.FontBold; }
                else { e.Value = "✖ Không đủ hạng bằng"; e.CellStyle.ForeColor = Theme.TextSecondary; }
                e.FormattingApplied = true;
                return;
            }
            ColorStatusCell(name, e);
        }

        /// <summary>Tô màu ô theo ký hiệu đầu chữ: ✅ xanh, 🟡 cam, ⛔ / ❌ đỏ.</summary>
        private static void ColorStatusCell(string column, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is not string text || e.CellStyle is null) return;
            if (text.StartsWith("✅")) e.CellStyle.ForeColor = Theme.Success;
            else if (text.StartsWith("🟡") || text.StartsWith("⚠️")) e.CellStyle.ForeColor = Theme.Warning;
            else if (text.StartsWith("⛔") || text.StartsWith("❌")) e.CellStyle.ForeColor = Theme.Danger;
            if (column is "Chạy_được_hôm_nay") e.CellStyle.Font = Theme.FontBold;
        }

        // ================= Thao tác =================

        private void CboWarehouse_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loadingWarehouses) return;
            _ = LoadAsync();
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnVehicleDocs_Click(object? sender, EventArgs e)
        {
            if (SelectedVehicle() is VehicleResource v) await EditVehicleDocsAsync(v, FindForm());
        }

        private async void BtnVehicleFuel_Click(object? sender, EventArgs e)
        {
            if (SelectedVehicle() is VehicleResource v) await EditVehicleFuelAsync(v, FindForm());
        }

        private async void BtnDriverProfile_Click(object? sender, EventArgs e)
        {
            if (SelectedDriver() is DriverResource d) await EditDriverProfileAsync(d, FindForm());
        }

        private async void BtnDriverWorkDays_Click(object? sender, EventArgs e)
        {
            if (SelectedDriver() is DriverResource d) await EditDriverWorkDaysAsync(d, FindForm());
        }

        // Mỗi việc sửa: mở form trên cửa sổ đang mở (owner) -> đóng xong thì tải lại dữ liệu (Task chỉ xong khi đã tải lại,
        // để cửa sổ chi tiết cập nhật đúng số liệu mới).
        private async Task EditVehicleDocsAsync(VehicleResource v, IWin32Window? owner)
        {
            if (_api is null) return;
            using (var f = new TruckingSystem.Desktop.GUI_VehicleManagement.ManageVehicleDocumentsForm(_api, v.Id, v.PlateNumber))
                f.ShowDialog(owner);
            await LoadAsync();
        }

        private async Task EditVehicleFuelAsync(VehicleResource v, IWin32Window? owner)
        {
            if (_api is null) return;
            bool ok;
            using (var f = new TruckingSystem.Desktop.GUI_VehicleManagement.VehicleFuelLevelForm(_api, v.Id, v.PlateNumber))
                ok = f.ShowDialog(owner) == DialogResult.OK;
            if (ok) await LoadAsync();
        }

        private async Task EditDriverProfileAsync(DriverResource d, IWin32Window? owner)
        {
            if (_api is null) return;
            bool ok;
            using (var f = new TruckingSystem.Desktop.GUI_EmployeeManagement.CreateDriverProfileForm(_api, d.EmployeeId, d.FullName))
                ok = f.ShowDialog(owner) == DialogResult.OK;
            if (ok) await LoadAsync();
        }

        private async Task EditDriverWorkDaysAsync(DriverResource d, IWin32Window? owner)
        {
            if (_api is null) return;
            bool changed;
            using (var f = new TruckingSystem.Desktop.GUI_DriverManagement.DriverWorkDaysForm(_api, d.EmployeeId, d.FullName))
            {
                f.ShowDialog(owner);
                changed = f.Changed;
            }
            if (changed) await LoadAsync();
        }
    }
}
