using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;

namespace TruckingSystem.Desktop.SERVICE_ContractManagement;

public class ContractAppendixApiService(ApiConnection connection)
{
    public async Task<List<ContractAppendixItem>> GetAppendicesAsync()
        => await connection.GetAsync<List<ContractAppendixItem>>("/api/contract-appendices") ?? new();

    public async Task<ContractAppendixLookups> GetLookupsAsync()
        => await connection.GetAsync<ContractAppendixLookups>("/api/contract-appendices/lookups")
            ?? throw new InvalidOperationException("Không tải được hợp đồng và ưu đãi.");

    public Task<ContractAppendixItem?> CreateAppendixAsync(string code, int contractId, DateTime issueDate, DateTime effectiveDate, int? tariffRuleId, decimal? discountPercent = null, int? priceListId = null)
        => connection.PostAsync<ContractAppendixItem>("/api/contract-appendices", new { code, contractId, issueDate, effectiveDate, tariffRuleId, discountPercent, priceListId });

    public Task UpdateAppendixAsync(int id, string code, int contractId, DateTime issueDate, DateTime effectiveDate, int? tariffRuleId, decimal? discountPercent = null, int? priceListId = null)
        => connection.PutAsync<object>($"/api/contract-appendices/{id}", new { code, contractId, issueDate, effectiveDate, tariffRuleId, discountPercent, priceListId });

    public Task DeleteAppendixAsync(int id)
        => connection.DeleteAsync<object>($"/api/contract-appendices/{id}");
}
