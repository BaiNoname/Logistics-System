using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;

namespace TruckingSystem.Desktop.SERVICE_ContractManagement;

public class ContractApiService(ApiConnection connection)
{
    public Task<List<ContractListItem>> GetContractsAsync()
        => connection.GetAsync<List<ContractListItem>>("/api/contracts")!;

    public Task<ContractLookups> GetLookupsAsync()
        => connection.GetAsync<ContractLookups>("/api/contracts/lookups")!;

    public Task<ContractListItem?> GetContractAsync(int id)
        => connection.GetAsync<ContractListItem>($"/api/contracts/{id}");

    public Task DeleteContractAsync(int id)
        => connection.DeleteAsync<object>($"/api/contracts/{id}");

    public Task CreateContractAsync(int customerId, string code, DateTime startDate, DateTime? endDate, string? terms, int? priceListId)
        => connection.PostAsync<object>("/api/contracts", new { customerId, code, startDate, endDate, terms, priceListId });

    public async Task UpdateContractAsync(int id, int customerId, string code, DateTime startDate, DateTime? endDate, string? terms, int? priceListId)
    {
        await connection.PutAsync<object>($"/api/contracts/{id}", new { customerId, code, startDate, endDate, terms, priceListId });
    }
}
