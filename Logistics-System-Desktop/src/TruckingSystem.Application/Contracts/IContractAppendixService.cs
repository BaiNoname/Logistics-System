namespace TruckingSystem.Application.Contracts;

public interface IContractAppendixService
{
    Task<List<ContractAppendixDto>> GetAllAsync();
    Task<ContractAppendixLookupsDto> GetLookupsAsync();
    Task<ContractAppendixDto> CreateAsync(SaveContractAppendixRequest request, bool isAdmin = false);
    Task<bool> UpdateAsync(int id, SaveContractAppendixRequest request, bool isAdmin = false);
    Task<bool> DeleteAsync(int id);
}
