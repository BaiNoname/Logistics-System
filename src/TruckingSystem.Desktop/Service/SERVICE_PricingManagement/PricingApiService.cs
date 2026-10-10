using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PricingManagement;

namespace TruckingSystem.Desktop.SERVICE_PricingManagement;

public class PricingApiService(ApiConnection connection)
{
    public async Task<List<PriceListItem>> GetPriceListsAsync()
        => await connection.GetAsync<List<PriceListItem>>("/api/price-lists") ?? new();

    public Task<PriceListItem?> CreatePriceListAsync(string name, DateTime effectiveDate)
        => connection.PostAsync<PriceListItem>("/api/price-lists", new { name, effectiveDate });

    public Task UpdatePriceListAsync(int id, string name, DateTime effectiveDate)
        => connection.PutAsync<object>($"/api/price-lists/{id}", new { name, effectiveDate });

    public Task DeletePriceListAsync(int id)
        => connection.DeleteAsync<object>($"/api/price-lists/{id}");

    public async Task<List<TariffRuleItem>> GetTariffRulesAsync(int priceListId)
        => await connection.GetAsync<List<TariffRuleItem>>($"/api/price-lists/{priceListId}/tariff-rules") ?? new();

    public Task CreateTariffRuleAsync(int priceListId, string vehicleType, decimal pricePerKm,
        decimal pricePerTon, decimal surchargeFragile, decimal surchargeHazmat)
        => connection.PostAsync<object>($"/api/price-lists/{priceListId}/tariff-rules",
            new { vehicleType, pricePerKm, pricePerTon, surchargeFragile, surchargeHazmat });

    public Task UpdateTariffRuleAsync(int priceListId, int id, string vehicleType, decimal pricePerKm,
        decimal pricePerTon, decimal surchargeFragile, decimal surchargeHazmat)
        => connection.PutAsync<object>($"/api/price-lists/{priceListId}/tariff-rules/{id}",
            new { vehicleType, pricePerKm, pricePerTon, surchargeFragile, surchargeHazmat });

    public Task DeleteTariffRuleAsync(int priceListId, int id)
        => connection.DeleteAsync<object>($"/api/price-lists/{priceListId}/tariff-rules/{id}");

    public async Task<List<string>> GetVehicleTypesAsync()
        => await connection.GetAsync<List<string>>("/api/price-lists/vehicle-types") ?? new();
}
