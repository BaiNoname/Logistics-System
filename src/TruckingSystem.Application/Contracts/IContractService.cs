namespace TruckingSystem.Application.Contracts;

public interface IContractService
{
    Task<List<ContractListItemDto>> GetAllAsync();
    Task<ContractLookupDto> GetLookupsAsync();
    Task<ContractListItemDto> CreateAsync(SaveContractRequest request);
    Task<bool> UpdateAsync(int id, SaveContractRequest request);
}
