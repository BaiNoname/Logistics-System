namespace TruckingSystem.Desktop.DTO_NotificationManagement
{
    public class NotificationItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>"Chuyến chờ duyệt" / "Giấy tờ xe" / "Lịch tài xế" / "Khác".
        /// Server cũ chưa trả trường này -> Desktop tự suy ra từ tiêu đề.</summary>
        public string? Category { get; set; }

        public string CategoryText => !string.IsNullOrWhiteSpace(Category) ? Category! : GuessCategory(Title);

        /// <summary>Giờ Việt Nam để hiển thị (server lưu UTC).</summary>
        public DateTime CreatedAtLocal => CreatedAt.Kind switch
        {
            DateTimeKind.Local => CreatedAt,
            _ => DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc).ToLocalTime(),
        };

        public static string GuessCategory(string title)
        {
            if (title.StartsWith("🟢")) return "Chuyến chờ duyệt";
            if (title.StartsWith("🔴") || title.StartsWith("🟡")) return "Giấy tờ xe";
            if (title.StartsWith("📅")) return "Lịch tài xế";
            return "Khác";
        }
    }
}
