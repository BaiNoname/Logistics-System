using System.Globalization;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>Định dạng tiền và dịch trạng thái — dùng chung cho màn Tổng quan
    /// và form Chi tiết chi phí đơn hàng.</summary>
    internal static class ReportFormat
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        /// <summary>Dạng rút gọn cho thẻ số liệu: 12,5 tr · 1,25 tỷ · 850 k.</summary>
        public static string Money(decimal v)
        {
            var sign = v < 0 ? "-" : "";
            var a = Math.Abs(v);
            if (a >= 1_000_000_000m) return sign + (a / 1_000_000_000m).ToString("0.##", Vi) + " tỷ";
            if (a >= 1_000_000m) return sign + (a / 1_000_000m).ToString("0.#", Vi) + " tr";
            if (a >= 1_000m) return sign + (a / 1_000m).ToString("0.#", Vi) + " k";
            return sign + a.ToString("0", Vi) + " đ";
        }

        /// <summary>Dạng đầy đủ: 12.500.000 đ.</summary>
        public static string FullMoney(decimal v) => v.ToString("N0", Vi) + " đ";

        public static string Percent(decimal part, decimal total)
            => total <= 0 ? "0%" : (part / total * 100).ToString("0.#", Vi) + "%";

        public static string OrderStatus(string s) => s switch
        {
            "Draft" => "Nháp",
            "Confirmed" => "Đã xác nhận",
            "InProgress" => "Đang vận chuyển",
            "Done" => "Hoàn tất",
            "Cancelled" => "Đã hủy",
            _ => s,
        };

        public static string TripStatus(string s) => s switch
        {
            "Planned" => "Đã lên kế hoạch",
            "InProgress" => "Đang chạy",
            "PendingConfirmation" => "Chờ xác nhận",
            "Completed" => "Hoàn tất",
            "Cancelled" => "Đã hủy",
            _ => s,
        };
    }
}
