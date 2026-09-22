namespace TruckingSystem.Application.Fuel;

public record FuelProviderDto(int Id, string Name, string? ContactPhone, string? Address);
public record CreateFuelProviderRequest(string Name, string? ContactPhone, string? Address);

public record FuelPriceHistoryDto(int Id, string FuelType, decimal Price, DateTime EffectiveDate, int? FuelProviderId, string? FuelProviderName);

/// <summary>Ghi nhận 1 mức giá xăng mới có hiệu lực — FuelProviderId để null
/// nếu chỉ ghi giá thị trường chung (không theo hãng cụ thể).</summary>
public record CreateFuelPriceRequest(string FuelType, decimal Price, DateTime EffectiveDate, int? FuelProviderId);

public record FuelActionResult(bool Success, string? Message);