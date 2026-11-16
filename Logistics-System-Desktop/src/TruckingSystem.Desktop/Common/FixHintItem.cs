namespace TruckingSystem.Desktop.Common
{
    /// <summary>"Cách sửa nhanh" máy chủ gửi kèm câu báo lỗi (khớp FixHint phía server). Kind cho biết mở gì,
    /// Text là chữ trên nút, Note là dòng giải thích nhỏ dưới nút. Xem QuickFix.</summary>
    public class FixHintItem
    {
        public string Kind { get; set; } = "";
        public string Text { get; set; } = "";
        public int? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? WarehouseId { get; set; }
        public string? Note { get; set; }
    }
}
