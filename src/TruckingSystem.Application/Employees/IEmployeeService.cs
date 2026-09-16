namespace TruckingSystem.Application.Employees;

public interface IEmployeeService
{
    Task<List<EmployeeListItemDto>> GetAllAsync();
    Task<EmployeeListItemDto> CreateAsync(CreateEmployeeRequest request);
    Task<List<LookupItemDto>> GetDepartmentsAsync();
    Task<List<LookupItemDto>> GetPositionsAsync();
}
