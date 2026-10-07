using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.SystemManagement;

public class SystemManagementService : ISystemManagementService
{
    private readonly IApplicationDbContext _db;

    public SystemManagementService(IApplicationDbContext db) => _db = db;

    public Task<List<SystemItemDto>> GetDepartmentsAsync() => _db.Departments
        .OrderBy(d => d.Name)
        .Select(d => new SystemItemDto(d.Id, d.Name, d.Employees.Count))
        .ToListAsync();

    public Task<List<SystemItemDto>> GetPositionsAsync() => _db.Positions
        .OrderBy(p => p.Name)
        .Select(p => new SystemItemDto(p.Id, p.Name, p.Employees.Count))
        .ToListAsync();

    public async Task<SystemActionResult> CreateDepartmentAsync(NameRequest request)
    {
        var name = request.Name?.Trim();
        if (!ValidName(name)) return new(Error: SystemActionError.InvalidName, Message: "Tên phòng ban phải có từ 1 đến 100 ký tự.");
        if (await _db.Departments.AnyAsync(d => d.Name.ToLower() == name!.ToLower()))
            return new(Error: SystemActionError.Duplicate, Message: "Phòng ban đã tồn tại.");
        var item = new Department { Name = name! };
        _db.Departments.Add(item);
        await _db.SaveChangesAsync();
        return new(new SystemItemDto(item.Id, item.Name, 0));
    }

    public async Task<SystemActionResult> CreatePositionAsync(NameRequest request)
    {
        var name = request.Name?.Trim();
        if (!ValidName(name)) return new(Error: SystemActionError.InvalidName, Message: "Tên chức danh phải có từ 1 đến 100 ký tự.");
        if (await _db.Positions.AnyAsync(p => p.Name.ToLower() == name!.ToLower()))
            return new(Error: SystemActionError.Duplicate, Message: "Chức danh đã tồn tại.");
        var item = new Position { Name = name! };
        _db.Positions.Add(item);
        await _db.SaveChangesAsync();
        return new(new SystemItemDto(item.Id, item.Name, 0));
    }

    public async Task<SystemActionResult> UpdateDepartmentAsync(int id, NameRequest request)
    {
        var name = request.Name?.Trim();
        if (!ValidName(name)) return new(Error: SystemActionError.InvalidName, Message: "Tên phòng ban phải có từ 1 đến 100 ký tự.");
        var item = await _db.Departments.FindAsync(id);
        if (item is null) return new(Error: SystemActionError.NotFound);
        if (await _db.Departments.AnyAsync(d => d.Id != id && d.Name.ToLower() == name!.ToLower()))
            return new(Error: SystemActionError.Duplicate, Message: "Phòng ban đã tồn tại.");
        item.Name = name!;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new(new SystemItemDto(item.Id, item.Name, await _db.Employees.CountAsync(e => e.DepartmentId == id)));
    }

    public async Task<SystemActionResult> UpdatePositionAsync(int id, NameRequest request)
    {
        var name = request.Name?.Trim();
        if (!ValidName(name)) return new(Error: SystemActionError.InvalidName, Message: "Tên chức danh phải có từ 1 đến 100 ký tự.");
        var item = await _db.Positions.FindAsync(id);
        if (item is null) return new(Error: SystemActionError.NotFound);
        if (await _db.Positions.AnyAsync(p => p.Id != id && p.Name.ToLower() == name!.ToLower()))
            return new(Error: SystemActionError.Duplicate, Message: "Chức danh đã tồn tại.");
        item.Name = name!;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new(new SystemItemDto(item.Id, item.Name, await _db.Employees.CountAsync(e => e.PositionId == id)));
    }

    public async Task<SystemActionResult> DeleteDepartmentAsync(int id)
    {
        var item = await _db.Departments.FindAsync(id);
        if (item is null) return new(Error: SystemActionError.NotFound);
        if (await _db.Employees.AnyAsync(e => e.DepartmentId == id))
            return new(Error: SystemActionError.InUse, Message: "Phòng ban đang được nhân viên sử dụng.");
        _db.Departments.Remove(item);
        await _db.SaveChangesAsync();
        return new();
    }

    public async Task<SystemActionResult> DeletePositionAsync(int id)
    {
        var item = await _db.Positions.FindAsync(id);
        if (item is null) return new(Error: SystemActionError.NotFound);
        if (await _db.Employees.AnyAsync(e => e.PositionId == id))
            return new(Error: SystemActionError.InUse, Message: "Chức danh đang được nhân viên sử dụng.");
        _db.Positions.Remove(item);
        await _db.SaveChangesAsync();
        return new();
    }

    private static bool ValidName(string? name) => !string.IsNullOrWhiteSpace(name) && name.Length <= 100;
}
