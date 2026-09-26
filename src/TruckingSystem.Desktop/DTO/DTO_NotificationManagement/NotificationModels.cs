namespace TruckingSystem.Desktop.DTO_NotificationManagement
{
    public class NotificationItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
