namespace TruckingSystem.Application.Pricing;

public interface IPricingService
{
    Task<List<PriceListDto>> GetPriceListsAsync();
    Task<PriceListDto> CreatePriceListAsync(SavePriceListRequest request);
    Task<bool> UpdatePriceListAsync(int id, SavePriceListRequest request);
    Task<bool> DeletePriceListAsync(int id);
    Task<List<TariffRuleDto>> GetTariffRulesAsync(int priceListId);
    Task<TariffRuleDto> CreateTariffRuleAsync(int priceListId, SaveTariffRuleRequest request);
    Task<bool> UpdateTariffRuleAsync(int priceListId, int id, SaveTariffRuleRequest request);
    Task<bool> DeleteTariffRuleAsync(int priceListId, int id);
    Task<List<string>> GetVehicleTypesAsync();
}
