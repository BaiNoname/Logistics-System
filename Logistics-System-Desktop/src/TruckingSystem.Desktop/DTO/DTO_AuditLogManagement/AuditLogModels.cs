using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TruckingSystem.Desktop.DTO_AuditLogManagement
{
    public class AuditLogItem
    {
        public int Id { get; set; }
        public string Action { get; set; } = "";
        public string EntityName { get; set; } = "";
        public int? EntityId { get; set; }
        public string? Detail { get; set; }
        public DateTime Timestamp { get; set; }
        /// <summary>Người thao tác (null = hệ thống tự chạy).</summary>
        public string? UserName { get; set; }
        /// <summary>Vai trò lúc thao tác (mã role, nhiều role nối dấu phẩy); null = log cũ / hệ thống.</summary>
        public string? UserRole { get; set; }
    }

    /// <summary>Tên hiển thị TIẾNG VIỆT cho mã hành động / loại đối tượng lưu trong nhật ký.</summary>
    public static class AuditLabels
    {
        public const string SystemText = "(Hệ thống)";
        public const string UnknownRole = "Chưa rõ vai trò";

        /// <summary>Tên vai trò tiếng Việt — ô lọc "Vai trò" chỉ có vài lựa chọn cố định (thay cho lọc theo TỪNG người:
        /// công ty đông nhân viên thì danh sách người rất dài; muốn tìm 1 người cụ thể thì gõ tên vào ô tìm).</summary>
        public static string Role(string code) => code.Trim() switch
        {
            "Admin" => "Quản trị viên",
            "Dispatcher" => "Điều phối viên",
            "Accountant" => "Kế toán",
            "WarehouseStaff" => "Nhân viên kho",
            "Driver" => "Tài xế",
            "CustomerPortal" => "Khách hàng (cổng KH)",
            var c => c,
        };

        /// <summary>Các vai trò (tiếng Việt) của dòng log: hệ thống tự chạy -> "(Hệ thống)"; có người nhưng log cũ chưa lưu
        /// vai trò / đăng nhập sai -> "Chưa rõ vai trò".</summary>
        public static List<string> Roles(AuditLogItem l)
        {
            if (string.IsNullOrWhiteSpace(l.UserName)) return new() { SystemText };
            if (string.IsNullOrWhiteSpace(l.UserRole)) return new() { UnknownRole };
            return l.UserRole.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(Role).Distinct().ToList();
        }

        public static string Action(string code) => code switch
        {
            "Create" => "➕ Thêm",
            "Update" => "✏️ Sửa",
            "Delete" => "🗑️ Xóa",
            "StatusChange" => "🔄 Đổi trạng thái",
            "Login" => "🔑 Đăng nhập",
            "LoginFailed" => "⛔ Đăng nhập sai",
            "AllocateVehicles" => "🚚 Chia xe",
            "AllocateVehiclesLate" => "🚚 Chia xe (trễ hạn)",
            "ChangeDeliveryDeadline" => "📅 Đổi hạn giao",
            "RecordStorageFee" => "💰 Ghi nhận phí lưu kho",
            "ImportDriverAttendance" => "📥 Nhập chấm công tài xế",
            _ => "⚙️ " + Humanize(code),
        };

        public static string Entity(string code) => code switch
        {
            "TransportOrder" => "Đơn vận chuyển", "CargoLine" => "Dòng hàng", "Trip" => "Chuyến", "TripLine" => "Dòng hàng của chuyến",
            "Vehicle" => "Xe", "VehicleDocument" => "Giấy tờ xe", "Warehouse" => "Kho", "WarehouseZone" => "Khu vực kho",
            "StorageBin" => "Vị trí lưu trữ", "GoodsReceipt" => "Phiếu nhập kho", "GoodsIssue" => "Phiếu xuất kho",
            "Location" => "Địa điểm", "DriverProfile" => "Hồ sơ tài xế", "DriverSchedule" => "Lịch làm tài xế",
            "FuelLog" => "Đổ xăng", "FuelProvider" => "Hãng nhiên liệu", "FuelPriceHistory" => "Giá nhiên liệu",
            "TripExpense" => "Chi phí chuyến", "IncidentReport" => "Sự cố", "TripCargoPhoto" => "Ảnh hàng",
            "DeliveryDeadlineChange" => "Đổi hạn giao", "Customer" => "Khách hàng", "CustomerContact" => "Liên hệ khách hàng",
            "Contract" => "Hợp đồng", "PriceList" => "Bảng giá", "TariffRule" => "Quy tắc giá", "Invoice" => "Hóa đơn",
            "InvoiceLine" => "Dòng hóa đơn", "Payment" => "Thanh toán", "Employee" => "Nhân viên", "EmployeeDocument" => "Giấy tờ nhân viên",
            "Department" => "Phòng ban", "Position" => "Chức danh", "CustomerFeedback" => "Đánh giá của khách", "Account" => "Tài khoản",
            "MonthlyAttendance" => "Chấm công tháng", "PayrollPeriod" => "Kỳ lương", "PayrollSlip" => "Phiếu lương",
            "PayrollItem" => "Khoản lương", "SalaryPayment" => "Trả lương", "EmployeeSalary" => "Lương nhân viên",
            "ApplicationUser" => "Tài khoản", "ApplicationRole" => "Vai trò", "Permission" => "Quyền", "RolePermission" => "Phân quyền",
            "SalesOrder" => "Đơn bán", "ContractAppendix" => "Phụ lục hợp đồng",
            "AttendanceAdjustmentRequest" => "Yêu cầu sửa chấm công", "AttendanceQrCode" => "Mã QR chấm công", "AttendanceRecord" => "Lượt chấm công",
            "AttendanceScan" => "Lượt quét QR chấm công", "DriverKpiRate" => "Hệ số KPI tài xế", "MaxDiscountPercent" => "Ưu đãi tối đa",
            "PayrollKpiPolicy" => "Chính sách KPI lương", "PayrollMonthlyAdjustment" => "Điều chỉnh lương tháng", "SurchargeLine" => "Dòng phụ phí",
            "SurchargeList" => "Bảng phụ phí",
            _ => Humanize(code),
        };

        /// <summary>Mã tiếng Anh dạng PascalCase / camelCase chưa có trong bảng -> dịch TỪNG TỪ sang tiếng Việt
        /// (VD "LockPayrollPeriod" -> "Khóa kỳ lương", "standardDays" -> "Ngày công chuẩn") — mã do chức năng mới /
        /// của người khác ghi vẫn đọc được, không hiện nguyên mã cho người dùng.</summary>
        public static string Humanize(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "";
            if (Phrases.TryGetValue(code.Trim(), out var known)) return known;
            var words = Regex.Matches(code, "[A-Z]?[a-z]+|[A-Z]+(?![a-z])|\\d+").Select(m => m.Value).ToList();
            if (words.Count == 0) return code;
            // Tiếng Việt: danh từ trước, tính chất sau -> giữ động từ đầu, các từ còn lại đảo ngược ("Driver Attendance" -> "chấm công tài xế").
            var verb = Words.TryGetValue(words[0].ToLowerInvariant(), out var v0) && Verbs.Contains(words[0].ToLowerInvariant()) ? v0 : null;
            var rest = (verb is null ? words : words.Skip(1)).Select(w => Words.TryGetValue(w.ToLowerInvariant(), out var t) ? t : w).Reverse().ToList();
            var text = string.Join(" ", verb is null ? rest : rest.Prepend(verb));
            return text.Length == 0 ? code : char.ToUpper(text[0], CultureInfo.GetCultureInfo("vi-VN")) + text[1..];
        }

        public static string UserText(AuditLogItem l) => string.IsNullOrWhiteSpace(l.UserName) ? SystemText : l.UserName!;

        public static string EntityText(AuditLogItem l) => Entity(l.EntityName) + (l.EntityId.HasValue ? $" #{l.EntityId}" : "");

        /// <summary>Màu "viên thuốc" của cột Hành động (dùng chung màn Nhật ký và cửa sổ Lịch sử thay đổi).</summary>
        public static TruckingSystem.Desktop.Common.GridDecor.Tone PillTone(string action) => Tone(action) switch
        {
            "green" => TruckingSystem.Desktop.Common.GridDecor.Green,
            "red" => TruckingSystem.Desktop.Common.GridDecor.Red,
            "violet" => TruckingSystem.Desktop.Common.GridDecor.Violet,
            "slate" => TruckingSystem.Desktop.Common.GridDecor.Slate,
            "blue" => TruckingSystem.Desktop.Common.GridDecor.Blue,
            _ => TruckingSystem.Desktop.Common.GridDecor.Amber,
        };

        /// <summary>Nội dung ô "Chi tiết" của 1 dòng nhật ký: dòng đầu (thời gian, người + vai trò, hành động, đối tượng),
        /// rồi MỖI THAY ĐỔI 1 DÒNG ("Trạng thái: A → B"); nội dung JSON -> "Tên: giá trị".</summary>
        public static string DetailText(AuditLogItem log)
        {
            var lines = new List<string>
            {
                $"🕒 {log.Timestamp.ToLocalTime():dd/MM/yyyy HH:mm:ss}    👤 {UserText(log)}" +
                (string.IsNullOrWhiteSpace(log.UserName) ? "" : $" ({string.Join(", ", Roles(log))})") +
                $"    {Action(log.Action)}    📁 {EntityText(log)}",
                "",
            };
            var detail = log.Detail ?? "(không có nội dung)";
            var json = JsonLines(detail);
            if (json is not null)
            {
                lines.Add("Thông tin ghi nhận:");
                lines.AddRange(json.Select(p => "   • " + p));
            }
            else
            {
                var cut = detail.IndexOf(" — ", StringComparison.Ordinal);
                if (cut > 0)
                {
                    lines.Add(detail[..cut]);
                    lines.AddRange(detail[(cut + 3)..].Split(" · ").Select(p => "   • " + PrettyPart(p)));
                }
                else lines.Add(detail);
            }
            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>1 dòng tóm tắt nội dung (cột "Nội dung" khi xuất Excel): phần đầu + các thay đổi nối " · ".</summary>
        public static string DetailSummary(AuditLogItem log)
        {
            var json = JsonLines(log.Detail);
            if (json is not null) return string.Join(" · ", json);
            var d = log.Detail ?? "";
            var cut = d.IndexOf(" — ", StringComparison.Ordinal);
            return cut > 0 ? d[..cut] + ": " + string.Join(" · ", d[(cut + 3)..].Split(" · ").Select(PrettyPart)) : d;
        }

        /// <summary>Làm rõ 1 mảnh chi tiết cho người dùng: "máy ::1" (địa chỉ IP lúc đăng nhập) -> "Đăng nhập từ: chính máy
        /// chủ (localhost)" / "Đăng nhập từ máy có địa chỉ IP 192.168.1.20".</summary>
        public static string PrettyPart(string part)
        {
            var m = Regex.Match(part.Trim(), "^máy (.+)$");
            if (!m.Success) return part;
            var ip = m.Groups[1].Value.Trim();
            if (ip.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase)) ip = ip[7..];
            return ip is "::1" or "127.0.0.1"
                ? "Đăng nhập ngay trên máy chủ (localhost — máy đang chạy API)"
                : $"Đăng nhập từ máy có địa chỉ IP {ip}";
        }

        /// <summary>Nội dung nhật ký dễ đọc: nếu chức năng ghi dạng JSON ({"periodId":2,"WorkedDays":1,...}) thì đổi
        /// thành từng dòng "Tên tiếng Việt: giá trị" (ngày dd/MM/yyyy, số bỏ ",0" thừa); nội dung chữ thường giữ nguyên.</summary>
        public static List<string>? JsonLines(string? detail)
        {
            var text = detail?.Trim();
            if (string.IsNullOrEmpty(text) || text[0] != '{') return null;
            try
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
                return doc.RootElement.EnumerateObject()
                    .Select(p => $"{Field(p.Name)}: {Value(p.Name, p.Value)}")
                    .ToList();
            }
            catch (JsonException) { return null; }
        }

        private static string Field(string name)
            => name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && name.Length > 2
                ? Humanize(name[..^2]) + " #"
                : Humanize(name);

        private static string Value(string name, JsonElement v)
        {
            switch (v.ValueKind)
            {
                case JsonValueKind.Null or JsonValueKind.Undefined: return "(trống)";
                case JsonValueKind.True: return "Có";
                case JsonValueKind.False: return "Không";
                case JsonValueKind.Number:
                    // Năm / tháng / số đếm nhỏ in nguyên ("2026", không "2.026"); số tiền lớn mới có dấu chấm ngăn cách.
                    if (!v.TryGetDecimal(out var d)) return v.ToString();
                    return d.ToString(Math.Abs(d) >= 100_000 ? "#,##0.####" : "0.####", CultureInfo.GetCultureInfo("vi-VN"));
                case JsonValueKind.String:
                    var s = v.GetString() ?? "";
                    if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt) && s.Contains('-') && s.Contains('T'))
                        return dt.TimeOfDay == TimeSpan.Zero ? dt.ToString("dd/MM/yyyy") : dt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
                    return s;
                case JsonValueKind.Array:
                    return string.Join(", ", v.EnumerateArray().Select(e => Value(name, e)));
                default:
                    var lines = JsonLines(v.GetRawText());
                    return lines is null ? v.GetRawText() : "{ " + string.Join(" · ", lines) + " }";
            }
        }

        /// <summary>Cụm có nghĩa riêng (dịch nguyên cụm thay vì từng từ).</summary>
        private static readonly Dictionary<string, string> Phrases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Year"] = "Năm", ["Month"] = "Tháng", ["EffectiveFrom"] = "Áp dụng từ ngày", ["EffectiveTo"] = "Áp dụng đến ngày",
            ["MonthlySalary"] = "Lương tháng", ["Period"] = "Kỳ lương", ["Slip"] = "Phiếu lương", ["StandardDays"] = "Ngày công chuẩn", ["WorkedDays"] = "Ngày làm thực tế", ["DriverWorkDates"] = "Các ngày làm của tài xế",
            ["From"] = "Từ ngày", ["To"] = "Đến ngày", ["FromDate"] = "Từ ngày", ["ToDate"] = "Đến ngày",
            ["BaseSalary"] = "Lương cơ bản", ["NetPay"] = "Thực lãnh", ["GrossPay"] = "Tổng thu nhập",
            ["CommittedDeliveryDate"] = "Hạn giao", ["PlateNumber"] = "Biển số", ["UserName"] = "Tài khoản",
        };

        private static readonly HashSet<string> Verbs = new()
        {
            "import", "export", "create", "add", "update", "edit", "delete", "remove", "approve", "reject", "lock", "unlock",
            "set", "calculate", "recalculate", "pay", "confirm", "cancel", "send", "assign", "reassign", "change", "reset", "close", "open", "record",
        };

        private static readonly Dictionary<string, string> Words = new()
        {
            ["set"] = "thiết lập", ["effective"] = "hiệu lực", ["import"] = "nhập", ["export"] = "xuất", ["create"] = "tạo", ["add"] = "thêm", ["update"] = "cập nhật", ["edit"] = "sửa",
            ["delete"] = "xóa", ["remove"] = "gỡ", ["approve"] = "duyệt", ["reject"] = "từ chối", ["lock"] = "khóa", ["unlock"] = "mở khóa",
            ["calculate"] = "tính", ["recalculate"] = "tính lại", ["pay"] = "trả", ["confirm"] = "xác nhận", ["cancel"] = "hủy",
            ["send"] = "gửi", ["assign"] = "gán", ["reassign"] = "gán lại", ["change"] = "đổi", ["reset"] = "đặt lại", ["close"] = "đóng",
            ["open"] = "mở", ["record"] = "ghi nhận",
            ["driver"] = "tài xế", ["attendance"] = "chấm công", ["payroll"] = "lương", ["period"] = "kỳ", ["salary"] = "lương",
            ["slip"] = "phiếu", ["item"] = "khoản", ["payment"] = "thanh toán", ["invoice"] = "hóa đơn", ["contract"] = "hợp đồng",
            ["appendix"] = "phụ lục", ["customer"] = "khách hàng", ["employee"] = "nhân viên", ["account"] = "tài khoản",
            ["role"] = "vai trò", ["permission"] = "quyền", ["price"] = "giá", ["list"] = "bảng", ["tariff"] = "biểu giá", ["rule"] = "quy tắc",
            ["order"] = "đơn", ["trip"] = "chuyến", ["vehicle"] = "xe", ["warehouse"] = "kho", ["stock"] = "tồn kho", ["fuel"] = "nhiên liệu",
            ["expense"] = "chi phí", ["incident"] = "sự cố", ["document"] = "giấy tờ", ["location"] = "địa điểm", ["status"] = "trạng thái",
            ["date"] = "ngày", ["dates"] = "các ngày", ["days"] = "ngày", ["day"] = "ngày", ["work"] = "làm", ["worked"] = "đã làm",
            ["standard"] = "chuẩn", ["month"] = "tháng", ["monthly"] = "tháng", ["amount"] = "số tiền", ["total"] = "tổng", ["note"] = "ghi chú",
            ["reason"] = "lý do", ["name"] = "tên", ["code"] = "mã", ["type"] = "loại", ["count"] = "số", ["qty"] = "số lượng",
            ["bonus"] = "thưởng", ["deduction"] = "khấu trừ", ["allowance"] = "phụ cấp", ["overtime"] = "tăng ca", ["net"] = "thực", ["gross"] = "tổng",
            ["from"] = "từ", ["to"] = "đến", ["start"] = "bắt đầu", ["end"] = "kết thúc", ["deadline"] = "hạn", ["delivery"] = "giao hàng",
            ["pickup"] = "lấy hàng", ["feedback"] = "đánh giá", ["department"] = "phòng ban", ["position"] = "chức danh", ["user"] = "người dùng",
            ["password"] = "mật khẩu", ["login"] = "đăng nhập", ["storage"] = "lưu kho", ["fee"] = "phí",
        };

        /// <summary>Màu nhãn hành động: thêm xanh lá, sửa xanh dương, xóa / đăng nhập sai đỏ, đổi trạng thái tím...</summary>
        public static string Tone(string code) => code switch
        {
            "Create" => "green",
            "Delete" or "LoginFailed" => "red",
            "StatusChange" => "violet",
            "Login" => "slate",
            "Update" => "blue",
            _ => "amber",
        };
    }

    public class AuditLogFilterOptionsItem
    {
        public List<string> Actions { get; set; } = new();
        public List<string> Users { get; set; } = new();
        public List<string> Entities { get; set; } = new();
    }
}
