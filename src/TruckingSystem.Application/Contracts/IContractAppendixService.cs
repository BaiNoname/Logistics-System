namespace TruckingSystem.Application.Contracts;

public interface IContractAppendixService
{
    Task<List<ContractAppendixDto>> GetAllAsync();
    Task<ContractAppendixLookupsDto> GetLookupsAsync();
    Task<ContractAppendixDto> CreateAsync(SaveContractAppendixRequest request);
    Task<bool> UpdateAsync(int id, SaveContractAppendixRequest request);
    Task<bool> DeleteAsync(int id);
}
