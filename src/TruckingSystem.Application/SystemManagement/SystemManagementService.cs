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
        .Select(d => new SystemItemDto(d.Id, d.Name, d.Employees.Count, null))
        .ToListAsync();

    public Task<List<SystemItemDto>> GetPositionsAsync() => _db.Positions
        .OrderBy(p => p.Name)
        .Select(p => new SystemItemDto(p.Id, p.Name, p.Employees.Count, p.DepartmentId))
        .ToListAsync();

    public async Task<SystemActionResult> CreateDepartmentAsync(NameRequest request)
    {
        var name = request.Name?.Trim();
        if (!ValidName(name)) return new(Error: SystemActionError.InvalidName, Message: "Tên phòng ban phải có từ 1 đến 100 ký tự.");
        if (await _db.Departments.AnyAsync(d => d.Name.ToLower() == name!.ToLower()))
            return new(Error: SystemActionError.Duplicate, Message: "Phòng ban đã tồn tại.");
        var names = request.Positions ?? new List<string>();
        if (names.Count > 100 || names.Any(n => !ValidName(n?.Trim())))
            return new(Error: SystemActionError.InvalidName, Message: "Nhập tối đa 100 chức danh, mỗi tên từ 1 đến 100 ký tự.");
        var trimmed = names.Select(n => n.Trim()).ToList();
        if (trimmed.Distinct(StringComparer.OrdinalIgnoreCase).Count() != trimmed.Count)
            return new(Error: SystemActionError.Duplicate, Message: "Tên chức danh không được trùng nhau.");
        var item = new Department { Name = name! };
        foreach (var positionName in trimmed)
            _db.Positions.Add(new Position { Name = positionName, Department = item });
        _db.Departments.Add(item);
        await _db.SaveChangesAsync();
        return new(new SystemItemDto(item.Id, item.Name, 0));
    }

    public async Task<SystemActionResult> CreatePositionAsync(NameRequest request)
    {
        var name = request.Name?.Trim();
        if (!ValidName(name)) return new(Error: SystemActionError.InvalidName, Message: "Tên chức danh phải có từ 1 đến 100 ký tự.");
        if (!request.DepartmentId.HasValue || !await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId))
            return new(Error: SystemActionError.InvalidName, Message: "Vui lòng chọn phòng ban tồn tại.");
        if (await _db.Positions.AnyAsync(p => p.DepartmentId == request.DepartmentId && p.Name.ToLower() == name!.ToLower()))
            return new(Error: SystemActionError.Duplicate, Message: "Chức danh đã tồn tại.");
        var item = new Position { Name = name!, DepartmentId = request.DepartmentId };
        _db.Positions.Add(item);
        await _db.SaveChangesAsync();
        return new(new SystemItemDto(item.Id, item.Name, 0, item.DepartmentId));
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
        if (await _db.Positions.AnyAsync(p => p.Id != id && p.DepartmentId == (request.DepartmentId ?? item.DepartmentId) && p.Name.ToLower() == name!.ToLower()))
            return new(Error: SystemActionError.Duplicate, Message: "Chức danh đã tồn tại.");
        if (request.DepartmentId.HasValue && request.DepartmentId != item.DepartmentId)
        {
            if (!await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId))
                return new(Error: SystemActionError.InvalidName, Message: "Phòng ban không tồn tại.");
            if (await _db.Employees.AnyAsync(e => e.PositionId == id && e.DepartmentId != request.DepartmentId))
                return new(Error: SystemActionError.InUse, Message: "Chức danh đang được nhân viên phòng ban khác sử dụng.");
            item.DepartmentId = request.DepartmentId;
        }
        item.Name = name!;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new(new SystemItemDto(item.Id, item.Name, await _db.Employees.CountAsync(e => e.PositionId == id), item.DepartmentId));
    }

    public async Task<SystemActionResult> DeleteDepartmentAsync(int id)
    {
        var item = await _db.Departments.FindAsync(id);
        if (item is null) return new(Error: SystemActionError.NotFound);
        if (await _db.Employees.AnyAsync(e => e.DepartmentId == id))
            return new(Error: SystemActionError.InUse, Message: "Phòng ban đang được nhân viên sử dụng.");
        if (await _db.Positions.AnyAsync(p => p.DepartmentId == id))
            return new(Error: SystemActionError.InUse, Message: "Phòng ban còn chức danh. Vui lòng chuyển hoặc xóa chức danh trước.");
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
