namespace TruckingSystem.Desktop.DTO_LocationManagement
{
    public class LocationItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public double? Lat { get; set; }
        public double? Lng { get; set; }

        // Hiện tên trong ComboBox mà không cần set DisplayMember riêng
        public override string ToString() => Name;
    }

    /// <summary>1 gợi ý trả về khi gõ từ khóa tìm kiếm — ĐÃ CÓ SẴN tọa độ (xác
    /// nhận qua test thật: geometry.coordinates), không cần gọi thêm API nào nữa.</summary>
    public class PlaceSuggestionItem
    {
        public string RefId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public double Lat { get; set; }
        public double Lng { get; set; }

        // Hiện trong ListBox gợi ý — ưu tiên Address vì đầy đủ hơn Name
        public override string ToString() => string.IsNullOrWhiteSpace(Address) ? Name : $"{Name} — {Address}";
    }

    public class PlaceDetailItem
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
