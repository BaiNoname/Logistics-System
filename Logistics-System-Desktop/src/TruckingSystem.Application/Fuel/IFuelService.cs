namespace TruckingSystem.Application.Fuel;

public interface IFuelService
{
    Task<List<FuelProviderDto>> GetProvidersAsync();
    Task<FuelProviderDto> CreateProviderAsync(CreateFuelProviderRequest request);
    Task<FuelActionResult> DeleteProviderAsync(int id);

    /// <summary>providerId = null lấy TẤT CẢ lịch sử giá (mọi hãng + giá chung), sắp mới nhất trước.</summary>
    Task<List<FuelPriceHistoryDto>> GetPriceHistoryAsync(int? providerId);
    Task<FuelPriceHistoryDto> CreatePriceAsync(CreateFuelPriceRequest request);

    /// <summary>Loại nhiên liệu cho dropdown. pricedOnly = true: chỉ các loại ĐÃ CÓ
    /// giá (dùng cho form Xe); false: danh mục chuẩn + các loại đang có trong DB
    /// (dùng cho form thêm giá).</summary>
    Task<List<string>> GetFuelTypesAsync(bool pricedOnly);

    /// <summary>Xóa 1 mức giá đã ghi — cần thiết để có thể xóa hãng xăng sau
    /// này (DeleteProviderAsync chặn xóa nếu hãng CÒN lịch sử giá, nhưng
    /// trước đây KHÔNG có cách nào xóa từng mức giá để dọn đường — ngõ cụt
    /// thật sự, hãng nào đã có giá thì vĩnh viễn không xóa được).</summary>
    Task<FuelActionResult> DeletePriceAsync(int priceId);
}