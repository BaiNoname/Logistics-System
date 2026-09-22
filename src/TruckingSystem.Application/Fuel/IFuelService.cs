namespace TruckingSystem.Application.Fuel;

public interface IFuelService
{
    Task<List<FuelProviderDto>> GetProvidersAsync();
    Task<FuelProviderDto> CreateProviderAsync(CreateFuelProviderRequest request);
    Task<FuelActionResult> DeleteProviderAsync(int id);

    /// <summary>providerId = null lấy TẤT CẢ lịch sử giá (mọi hãng + giá chung), sắp mới nhất trước.</summary>
    Task<List<FuelPriceHistoryDto>> GetPriceHistoryAsync(int? providerId);
    Task<FuelPriceHistoryDto> CreatePriceAsync(CreateFuelPriceRequest request);
}