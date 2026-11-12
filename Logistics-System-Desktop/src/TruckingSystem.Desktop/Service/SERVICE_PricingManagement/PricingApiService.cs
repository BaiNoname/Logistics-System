using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PricingManagement;

namespace TruckingSystem.Desktop.SERVICE_PricingManagement;

public class PricingApiService(ApiConnection connection)
{
    public async Task<List<PriceListItem>> GetPriceListsAsync()
        => await connection.GetAsync<List<PriceListItem>>("/api/price-lists") ?? new();

    public Task<PriceListItem?> CreatePriceListAsync(string name, DateTime effectiveDate, string? status = null, List<SaveTariffRuleRow>? tariffRules = null)
        => connection.PostAsync<PriceListItem>("/api/price-lists", new { name, effectiveDate, status, tariffRules });

    public Task<PriceListItem?> CopyPriceListAsync(int id, string name, List<SaveTariffRuleRow>? tariffRules = null, DateTime? effectiveDate = null)
        => connection.PostAsync<PriceListItem>($"/api/price-lists/{id}/copy", new { name, tariffRules, effectiveDate });

    public Task UpdatePriceListAsync(int id, string name, DateTime effectiveDate, string? status = null, List<SaveTariffRuleRow>? tariffRules = null)
        => connection.PutAsync<object>($"/api/price-lists/{id}", new { name, effectiveDate, status, tariffRules });

    public Task CancelPriceListAsync(int id)
        => connection.PostAsync<object>($"/api/price-lists/{id}/cancel", new { });

    public async Task<List<TariffRuleItem>> GetTariffRulesAsync(int priceListId)
        => await connection.GetAsync<List<TariffRuleItem>>($"/api/price-lists/{priceListId}/tariff-rules") ?? new();

    public Task CreateTariffRuleAsync(int priceListId, string vehicleType, decimal pricePerKm,
        decimal pricePerTon)
        => connection.PostAsync<object>($"/api/price-lists/{priceListId}/tariff-rules",
            new { vehicleType, pricePerKm, pricePerTon });

    public Task UpdateTariffRuleAsync(int priceListId, int id, string vehicleType, decimal pricePerKm,
        decimal pricePerTon)
        => connection.PutAsync<object>($"/api/price-lists/{priceListId}/tariff-rules/{id}",
            new { vehicleType, pricePerKm, pricePerTon });

    public async Task<List<TariffRuleItem>> SaveTariffRulesAsync(int priceListId, List<SaveTariffRuleRow> rows)
        => await connection.PutAsync<List<TariffRuleItem>>($"/api/price-lists/{priceListId}/tariff-rules", new { rows })
            ?? throw new InvalidOperationException("Máy chủ chưa trả về dữ liệu giá. Vui lòng tải lại để kiểm tra.");

    public Task DeleteTariffRuleAsync(int priceListId, int id)
        => connection.DeleteAsync<object>($"/api/price-lists/{priceListId}/tariff-rules/{id}");

    public async Task<List<string>> GetVehicleTypesAsync()
        => await connection.GetAsync<List<string>>("/api/price-lists/vehicle-types") ?? new();
}
