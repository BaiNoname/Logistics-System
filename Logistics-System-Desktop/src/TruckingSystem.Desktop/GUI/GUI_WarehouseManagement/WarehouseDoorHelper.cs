namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Dùng chung cho form Tạo kho và Sửa kho — quy đổi lựa chọn "cửa
    /// nằm ở tường nào" giữa giao diện (tiếng Việt) và giá trị gửi lên API
    /// ("Top"/"Bottom"/"Left"/"Right"), và kiểm tra nhanh trước khi gửi.
    /// Cửa CHỈ được khai báo bằng tường + khoảng cách dọc tường, nên luôn nằm
    /// đúng trên tường, không thể lọt vào giữa kho.</summary>
    internal static class WarehouseDoorHelper
    {
        private static readonly string?[] ApiValues = { null, "Top", "Bottom", "Left", "Right" };
        private static readonly string[] Labels = { "— Chưa khai báo —", "Tường trên", "Tường dưới", "Tường trái", "Tường phải" };

        public static void FillSideCombo(ComboBox cbo)
        {
            cbo.Items.Clear();
            cbo.Items.AddRange(Labels);
            cbo.SelectedIndex = 0;
        }

        public static string? ToApiValue(int index) => index > 0 && index < ApiValues.Length ? ApiValues[index] : null;

        public static int ToIndex(string? apiValue)
        {
            var i = Array.IndexOf(ApiValues, apiValue);
            return i < 0 ? 0 : i;
        }

        public static string WallName(string? apiValue) => apiValue switch
        {
            "Top" => "trên",
            "Bottom" => "dưới",
            "Left" => "trái",
            "Right" => "phải",
            _ => "",
        };

        /// <summary>Tường trên/dưới dài bằng CHIỀU DÀI kho, tường trái/phải dài
        /// bằng CHIỀU RỘNG kho (X theo chiều dài, Y theo chiều rộng).</summary>
        public static decimal WallLength(string apiValue, decimal length, decimal width)
            => apiValue is "Top" or "Bottom" ? length : width;

        /// <summary>Trả về thông báo lỗi, hoặc null nếu hợp lệ (kể cả khi chưa khai báo cửa).</summary>
        public static string? Validate(int sideIndex, decimal offset, decimal length, decimal width)
        {
            var side = ToApiValue(sideIndex);
            if (side is null) return null;
            if (length <= 0 || width <= 0)
                return "Cần nhập chiều dài và chiều rộng kho trước khi khai báo vị trí cửa.";
            var wallLength = WallLength(side, length, width);
            if (offset > wallLength)
                return $"Cửa phải nằm trên tường {WallName(side)} — khoảng cách từ góc tối đa {wallLength:0.##} m (đúng chiều dài bức tường này).";
            return null;
        }
    }
}