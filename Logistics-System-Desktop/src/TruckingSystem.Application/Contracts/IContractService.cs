namespace TruckingSystem.Application.Contracts;

public interface IContractService
{
    Task<List<ContractListItemDto>> GetAllAsync();
    Task<ContractListItemDto?> GetByIdAsync(int id);
    Task<ContractLookupDto> GetLookupsAsync();
    Task<ContractListItemDto> CreateAsync(SaveContractRequest request, bool isAdmin = false);
    Task<bool> UpdateAsync(int id, SaveContractRequest request, bool isAdmin = false);
    Task<int> GetMaxDiscountPercentAsync();
    Task SaveMaxDiscountPercentAsync(int value);
    Task<bool> DeleteAsync(int id);
}
