using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Vehicles;

public class VehicleService : IVehicleService
{
    private readonly IApplicationDbContext _db;

    public VehicleService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<VehicleListItemDto>> GetAllAsync()
    {
        return await _db.Vehicles
            .OrderBy(v => v.PlateNumber)
            .Select(v => new VehicleListItemDto(v.Id, v.PlateNumber, v.VehicleType, v.CapacityQty, v.CapacityKg, v.CapacityM3, v.Status.ToString()))
            .ToListAsync();
    }

    public async Task<VehicleListItemDto> CreateAsync(CreateVehicleRequest request)
    {
        var vehicle = new Vehicle
        {
            PlateNumber = request.PlateNumber,
            VehicleType = request.VehicleType,
            CapacityQty = request.CapacityQty,
            CapacityKg = request.CapacityKg,
            CapacityM3 = request.CapacityM3,
            Status = VehicleStatus.Available,
        };

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        return new VehicleListItemDto(vehicle.Id, vehicle.PlateNumber, vehicle.VehicleType, vehicle.CapacityQty, vehicle.CapacityKg, vehicle.CapacityM3, vehicle.Status.ToString());
    }

    public async Task<VehicleActionResult> UpdateAsync(int id, UpdateVehicleRequest request)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle is null) return new VehicleActionResult(false, "Không tìm thấy xe.");

        if (!Enum.TryParse<VehicleStatus>(request.Status, out var status))
            return new VehicleActionResult(false, "Trạng thái không hợp lệ.");

        vehicle.PlateNumber = request.PlateNumber;
        vehicle.VehicleType = request.VehicleType;
        vehicle.CapacityQty = request.CapacityQty;
        vehicle.CapacityKg = request.CapacityKg;
        vehicle.CapacityM3 = request.CapacityM3;
        vehicle.Status = status;

        await _db.SaveChangesAsync();
        return new VehicleActionResult(true, null);
    }

    public async Task<List<VehicleDocumentDto>> GetDocumentsAsync(int vehicleId)
    {
        return await _db.VehicleDocuments
            .Where(d => d.VehicleId == vehicleId)
            .OrderByDescending(d => d.ExpiryDate)
            .Select(d => new VehicleDocumentDto(d.Id, d.DocType.ToString(), d.IssueDate, d.ExpiryDate, d.FileUrl))
            .ToListAsync();
    }

    public async Task<VehicleDocumentDto> CreateDocumentAsync(int vehicleId, CreateVehicleDocumentRequest request)
    {
        var docType = Enum.TryParse<VehicleDocumentType>(request.DocType, out var parsed) ? parsed : VehicleDocumentType.Other;

        var doc = new VehicleDocument
        {
            VehicleId = vehicleId,
            DocType = docType,
            IssueDate = request.IssueDate,
            ExpiryDate = request.ExpiryDate,
            FileUrl = request.FileUrl,
        };
        _db.VehicleDocuments.Add(doc);
        await _db.SaveChangesAsync();

        return new VehicleDocumentDto(doc.Id, doc.DocType.ToString(), doc.IssueDate, doc.ExpiryDate, doc.FileUrl);
    }

    public async Task<VehicleActionResult> DeleteDocumentAsync(int documentId)
    {
        var doc = await _db.VehicleDocuments.FindAsync(documentId);
        if (doc is null) return new VehicleActionResult(false, "Không tìm thấy giấy tờ.");

        _db.VehicleDocuments.Remove(doc);
        await _db.SaveChangesAsync();
        return new VehicleActionResult(true, null);
    }
}