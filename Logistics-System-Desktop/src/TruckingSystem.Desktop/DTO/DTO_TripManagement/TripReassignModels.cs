namespace TruckingSystem.Desktop.DTO_TripManagement
{
    /// <summary>1 xe / tài xế có thể chọn khi đổi xe cho chuyến chưa chạy — khớp ReassignOptionDto bên API.</summary>
    public class ReassignOptionItem
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public bool Eligible { get; set; }
        /// <summary>Lý do không dùng được (Eligible = false) hoặc lưu ý (Eligible = true).</summary>
        public string? Reason { get; set; }
        /// <summary>Chỉ với tài xế: xe (Id) mà tài xế không lái được (hạng bằng) -> lý do. null = API cũ / lái được hết.</summary>
        public Dictionary<int, string>? CannotDrive { get; set; }

        public override string ToString()
            => Eligible
                ? (Reason is null ? "✅ " + Text : $"✅ {Text}  ⚠ {Reason}")
                : $"⛔ {Text} — {Reason}";
    }

    public class ReassignOptionsItem
    {
        public int TripId { get; set; }
        public List<int> AffectedTripIds { get; set; } = new();
        public int CurrentVehicleId { get; set; }
        public int CurrentDriverEmployeeId { get; set; }
        public List<ReassignOptionItem> Vehicles { get; set; } = new();
        public List<ReassignOptionItem> Drivers { get; set; } = new();
        /// <summary>!= null: chuyến này không đổi được (VD đã chạy).</summary>
        public string? Message { get; set; }
    }
}
