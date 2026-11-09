namespace TruckingSystem.Desktop.DTO_TripManagement
{
    /// <summary>Khớp TripCargoLineDto bên API.</summary>
    public class TripCargoLineItem
    {
        public int CargoLineId { get; set; }
        public string CargoType { get; set; } = "";
        public decimal Qty { get; set; }
        public int PackagesApprox { get; set; }
        public decimal WeightKg { get; set; }
        public decimal VolumeM3 { get; set; }
        public bool IsFragile { get; set; }
        public bool IsHazmat { get; set; }
    }

    /// <summary>Khớp TripCargoChecklistDto bên API.</summary>
    public class TripCargoChecklistItem
    {
        public int TripId { get; set; }
        public string LegType { get; set; } = "";
        public string Status { get; set; } = "";
        public string OrderCode { get; set; } = "";
        public List<TripCargoLineItem> Lines { get; set; } = new();
        public int PhotoCount { get; set; }
    }

    /// <summary>Khớp TripPhotoDto bên API (TakenAt là UTC).</summary>
    public class TripPhotoItem
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public DateTime TakenAt { get; set; }
        public long SizeBytes { get; set; }
        public string ContentType { get; set; } = "";
    }
}
