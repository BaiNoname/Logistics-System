namespace TruckingSystem.Application.Contracts;

public interface IContractService
{
    Task<List<ContractListItemDto>> GetAllAsync();
    Task<ContractListItemDto?> GetByIdAsync(int id);
    Task<ContractLookupDto> GetLookupsAsync();
    Task<ContractListItemDto> CreateAsync(SaveContractRequest request, bool isAdmin = false);
    Task<bool> UpdateAsync(int id, SaveContractRequest request, bool isAdmin = false);
    Task<int> GetMaxDiscountPercentAsync();
    Task<ContractDiscountLimits> GetDiscountLimitsAsync();
    Task SaveMaxDiscountPercentAsync(int value, decimal? lateDeliveryDiscountPercent = null);
    Task<bool> DeleteAsync(int id);
}
