namespace TruckingSystem.Desktop.DTO_FuelManagement
{
    public class FuelProviderItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public override string ToString() => Name;
    }

    public class FuelPriceHistoryItem
    {
        public int Id { get; set; }
        public string FuelType { get; set; } = "";
        public decimal Price { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int? FuelProviderId { get; set; }
        public string? FuelProviderName { get; set; }
    }

    public class FuelActionResultItem
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
