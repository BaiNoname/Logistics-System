using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_SurchargeManagement;

namespace TruckingSystem.Desktop.SERVICE_SurchargeManagement;

public sealed class SurchargeApiService(ApiConnection connection)
{
    private const string Path = "/api/surcharge-lists";

    public async Task<List<SurchargeListItem>> ListAsync() =>
        await connection.GetAsync<List<SurchargeListItem>>(Path) ?? [];

    public Task<SurchargeListItem?> SaveAsync(SurchargeListItem item, bool copy)
    {
        var effectiveDate = item.EffectiveDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        var input = new { item.Name, effectiveDate, item.FragilePercent, item.HazmatPercent, item.Revision };
        if (copy) return connection.PostAsync<SurchargeListItem>($"{Path}/{item.Id}/copy", new { item.Name, effectiveDate });
        return item.Id == 0 ? connection.PostAsync<SurchargeListItem>(Path, input)
            : connection.PutAsync<SurchargeListItem>($"{Path}/{item.Id}", input);
    }

    public Task<SurchargeListItem?> PublishAsync(SurchargeListItem item) =>
        connection.PostAsync<SurchargeListItem>($"{Path}/{item.Id}/publish", new { item.Revision });
}
