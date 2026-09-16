using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TransportOrders;

public class TransportOrderService : ITransportOrderService
{
    private readonly IApplicationDbContext _db;

    public TransportOrderService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<TransportOrderListItemDto>> GetAllAsync()
    {
        return await _db.TransportOrders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.Id)
            .Select(o => new TransportOrderListItemDto(
                o.Id, o.Code, o.Customer.Name, o.Status.ToString(),
                o.ProgressPercent, o.PickupPoint, o.DropoffPoint, o.PublicTrackingToken))
            .ToListAsync();
    }

    public async Task<TransportOrderDetailDto?> GetByIdAsync(int id)
    {
        var order = await _db.TransportOrders
            .Include(o => o.Customer)
            .Include(o => o.CargoLines)
            .Include(o => o.PickupLocation)
            .Include(o => o.DropoffLocation)
            .Include(o => o.Trips).ThenInclude(t => t.Vehicle)
            .Include(o => o.Trips).ThenInclude(t => t.DriverEmployee)
            .Include(o => o.Trips).ThenInclude(t => t.TripLines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return null;

        return MapToDetail(order);
    }

    public async Task<TransportOrderDetailDto> CreateAsync(CreateTransportOrderRequest request)
    {
        var customer = await _db.Customers.FindAsync(request.CustomerId)
            ?? throw new InvalidOperationException("Khách hàng không tồn tại.");

        var code = $"TO-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var order = new TransportOrder
        {
            Code = code,
            CustomerId = request.CustomerId,
            PickupPoint = request.PickupPoint,
            DropoffPoint = request.DropoffPoint,
            CommittedDeliveryDate = request.CommittedDeliveryDate,
            Status = TransportOrderStatus.Draft,
            PickupLocationId = request.PickupLocationId,
            DropoffLocationId = request.DropoffLocationId,
            CargoLines = request.CargoLines.Select(c => new CargoLine
            {
                CargoType = c.CargoType,
                PackageCount = c.PackageCount,
                WeightKg = c.WeightKg,
                VolumeM3 = c.VolumeM3,
                IsFragile = c.IsFragile,
                IsHazmat = c.IsHazmat,
                QtyTotal = c.QtyTotal,
            }).ToList(),
        };

        _db.TransportOrders.Add(order);
        await _db.SaveChangesAsync();

        // Tải lại kèm Location để MapToDetail lấy được tọa độ Lat/Lng (nếu có chọn)
        if (request.PickupLocationId.HasValue)
            order.PickupLocation = await _db.Locations.FindAsync(request.PickupLocationId.Value);
        if (request.DropoffLocationId.HasValue)
            order.DropoffLocation = await _db.Locations.FindAsync(request.DropoffLocationId.Value);

        order.Customer = customer; // gán tay để MapToDetail không phải query lại
        order.Trips = new List<Trip>(); // đơn mới tạo chưa có chuyến nào
        return MapToDetail(order);
    }

    private static TransportOrderDetailDto MapToDetail(TransportOrder order) => new(
        order.Id, order.Code, order.Customer.Name, order.Status.ToString(), order.ProgressPercent,
        order.PickupPoint, order.DropoffPoint, order.CommittedDeliveryDate, order.PublicTrackingToken,
        order.CargoLines.Select(c => new CargoLineDto(
            c.CargoType, c.PackageCount, c.WeightKg, c.VolumeM3, c.IsFragile, c.IsHazmat, c.QtyTotal)).ToList(),
        order.Trips.Select(t => new TripSummaryDto(
            t.Id, t.Vehicle.PlateNumber, t.DriverEmployee.FullName, t.Status.ToString(),
            t.TripLines.Sum(tl => tl.QtyPlanned))).ToList(),
        order.PickupLocation?.Lat, order.PickupLocation?.Lng,
        order.DropoffLocation?.Lat, order.DropoffLocation?.Lng
    );
}
