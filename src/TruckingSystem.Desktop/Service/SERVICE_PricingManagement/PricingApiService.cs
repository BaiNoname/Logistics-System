using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PricingManagement;

namespace TruckingSystem.Desktop.SERVICE_PricingManagement;

public class PricingApiService(ApiConnection connection)
{
    public async Task<List<PriceListItem>> GetPriceListsAsync()
        => await connection.GetAsync<List<PriceListItem>>("/api/price-lists") ?? new();

    public Task<PriceListItem?> CreatePriceListAsync(string name, DateTime effectiveDate, string? status = null)
        => connection.PostAsync<PriceListItem>("/api/price-lists", new { name, effectiveDate, status });

    public Task<PriceListItem?> CopyPriceListAsync(int id, string name)
        => connection.PostAsync<PriceListItem>($"/api/price-lists/{id}/copy", new { name });

    public Task UpdatePriceListAsync(int id, string name, DateTime effectiveDate, string? status = null)
        => connection.PutAsync<object>($"/api/price-lists/{id}", new { name, effectiveDate, status });

    public Task DeletePriceListAsync(int id)
        => connection.DeleteAsync<object>($"/api/price-lists/{id}");

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

    public Task DeleteTariffRuleAsync(int priceListId, int id)
        => connection.DeleteAsync<object>($"/api/price-lists/{priceListId}/tariff-rules/{id}");

    public async Task<List<string>> GetVehicleTypesAsync()
        => await connection.GetAsync<List<string>>("/api/price-lists/vehicle-types") ?? new();
}
