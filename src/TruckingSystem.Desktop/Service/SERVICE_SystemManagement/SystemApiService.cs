using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.SERVICE_SystemManagement;

public class SystemApiService
{
    private readonly ApiConnection _conn;

    public SystemApiService(ApiConnection conn) => _conn = conn;

    public async Task<List<SystemItem>> GetAsync(string kind) =>
        await _conn.GetAsync<List<SystemItem>>($"/api/system/{kind}") ?? new();

    public Task<SystemItem?> CreateAsync(string kind, string name, int? departmentId = null, List<string>? positions = null) =>
        _conn.PostAsync<SystemItem>($"/api/system/{kind}", new { name, departmentId, positions });

    public Task<SystemItem?> UpdateAsync(string kind, int id, string name, int? departmentId = null) =>
        _conn.PutAsync<SystemItem>($"/api/system/{kind}/{id}", new { name, departmentId });

    public Task<object?> DeleteAsync(string kind, int id) =>
        _conn.DeleteAsync<object>($"/api/system/{kind}/{id}");
}

public class SystemItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int EmployeeCount { get; set; }
    public int? DepartmentId { get; set; }
}
