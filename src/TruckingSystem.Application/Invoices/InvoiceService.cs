using System.Data;
using TruckingSystem.Application.Pricing;
using TruckingSystem.Application.TransportOrders;
using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Invoices;

public class InvoiceService : IInvoiceService
{
    private readonly IApplicationDbContext _db;

    public InvoiceService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task MarkAsPaidAsync(int id)
    {
        var invoice = await _db.Invoices.SingleOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy hóa đơn.");
        invoice.MarkAsPaid();
        await _db.SaveChangesAsync();
    }

    public async Task<List<InvoiceListItemDto>> GetAllAsync()
    {
        return await _db.Invoices
            .Include(i => i.TransportOrder)
            .Include(i => i.Customer)
            .OrderByDescending(i => i.Id)
            .Select(i => new InvoiceListItemDto(
                i.Id, i.TransportOrder.Code, i.Customer.Name, i.TotalAmount, i.Status.ToString(), i.IssueDate))
            .ToListAsync();
    }

    public async Task<InvoiceDetailDto> GetDetailAsync(int id)
        => await _db.Invoices.AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InvoiceDetailDto(i.Id, i.TransportOrder.Code, i.Customer.Name, i.TotalAmount,
                i.Lines.OrderBy(l => l.Id)
                    .Select(l => new InvoiceQuoteLineDto(l.Description, l.Quantity, l.UnitPrice, l.Amount)).ToList()))
            .SingleOrDefaultAsync()
            ?? throw new KeyNotFoundException("Không tìm thấy hóa đơn.");

    public Task<List<InvoiceOrderOptionDto>> GetEligibleOrdersAsync()
        => _db.TransportOrders.AsNoTracking()
            .Where(o => o.Status == TransportOrderStatus.Done
                && !o.Invoices.Any(i => i.Status != InvoiceStatus.Cancelled))
            .OrderByDescending(o => o.Id)
            .Select(o => new InvoiceOrderOptionDto(o.Id, o.Code, o.Customer.Name, o.PickupPoint, o.DropoffPoint))
            .ToListAsync();

    public async Task<InvoiceQuoteDto> GetQuoteAsync(int orderId)
    {
        var order = await _db.TransportOrders.AsNoTracking()
            .Include(o => o.Contract).ThenInclude(c => c!.TariffRule)
            .Include(o => o.CargoLines)
            .SingleOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        if (order.Status != TransportOrderStatus.Done)
            throw new InvalidOperationException("Chỉ được lập hóa đơn cho đơn hàng đã hoàn thành.");
        if (await _db.Invoices.AnyAsync(i => i.TransportOrderId == orderId && i.Status != InvoiceStatus.Cancelled))
            throw new InvalidOperationException("Đơn hàng này đã có hóa đơn chưa hủy.");
        var contract = order.Contract
            ?? throw new InvalidOperationException("Đơn hàng chưa gắn hợp đồng, không thể tự tính hóa đơn.");
        if (contract.CustomerId != order.CustomerId)
            throw new InvalidOperationException("Hợp đồng không thuộc khách hàng của đơn.");

        var trips = await _db.Trips.AsNoTracking().Include(t => t.Vehicle)
            .Where(t => t.TransportOrderId == orderId && t.StartTime != null).ToListAsync();
        if (trips.Count == 0)
            throw new InvalidOperationException("Đơn hàng chưa có thời điểm bắt đầu vận chuyển.");
        var pricingDate = trips.Min(t => t.StartTime!.Value).AddHours(7).Date;
        if (Contract.GetStatus(contract.StartDate, contract.EndDate, pricingDate) != ContractStatus.Active)
            throw new InvalidOperationException("Hợp đồng không có hiệu lực tại ngày bắt đầu vận chuyển.");
        var appendix = await _db.ContractAppendices.AsNoTracking().Include(a => a.TariffRule)
            .Where(a => a.ContractId == contract.Id && a.EffectiveDate <= pricingDate)
            .OrderByDescending(a => a.EffectiveDate).ThenByDescending(a => a.IssueDate).ThenByDescending(a => a.Id)
            .FirstOrDefaultAsync();
        var tariff = appendix?.TariffRule ?? contract.TariffRule
            ?? throw new InvalidOperationException("Hợp đồng/phụ lục chưa có chi tiết giá.");
        var tripIds = trips.Select(t => t.Id).ToList();
        var points = (await _db.Waypoints.AsNoTracking().Where(w => tripIds.Contains(w.TripId)).ToListAsync())
            .ToLookup(w => w.TripId);
        decimal? distance = 0m;
        decimal? liters = 0m;
        foreach (var trip in trips)
        {
            var km = InvoicePricing.DistanceKm(points[trip.Id]);
            distance += km;
            var consumption = trip.FuelConsumptionPer100KmAtStart ?? trip.Vehicle.FuelConsumptionPer100Km;
            liters += consumption is > 0 ? km * consumption / 100m : null;
        }
        var source = $"HĐ {contract.Code}" + (appendix is null ? "" : $", PL {appendix.Code}")
            + $", ngày áp dụng {pricingDate:dd/MM/yyyy}";
        var policy = await SurchargeLookup.AtAsync(_db, pricingDate);
        var otherCosts = await _db.TripExpenses.Where(e => e.Trip.TransportOrderId == orderId)
            .SumAsync(e => (decimal?)e.Amount) ?? 0m;
        var storageFee = await StorageFeeRules.ComputeAsync(_db, orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        var lines = InvoicePricing.CalculateLines(order, tariff, distance, source, policy, otherCosts, storageFee.TotalFee);
        return new InvoiceQuoteDto(orderId, contract.Code, appendix?.Code, pricingDate, tariff.VehicleType,
            distance, order.CargoLines.Sum(c => c.WeightKg) / 1000m, liters, otherCosts,
            lines.Sum(l => l.Amount),
            lines.Select(l => new InvoiceQuoteLineDto(l.Description, l.Quantity, l.UnitPrice, l.Amount)).ToList(),
            !storageFee.IsFinal ? "Phí lưu kho của đơn vẫn đang tạm tính. Vui lòng tính lại hóa đơn sau khi phí lưu kho đã chốt."
                : distance is null ? "Chưa đủ GPS để tổng hợp quãng đường và nhiên liệu."
                : liters is null ? "Chưa đủ định mức xe để ước tính nhiên liệu." : null,
            StorageFeePending: !storageFee.IsFinal);
    }

    public async Task<InvoiceListItemDto> CreateAsync(CreateInvoiceRequest request)
    {
        if (request.DueDate < DateOnly.FromDateTime(VnClock.Today))
            throw new ArgumentException("Hạn thanh toán không được trước ngày lập hóa đơn.");

        // Serialize the eligibility check and insert so simultaneous requests cannot invoice an order twice.
        await using var transaction = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable)
            : null;
        var order = await _db.TransportOrders.Include(o => o.Customer)
            .SingleOrDefaultAsync(o => o.Id == request.TransportOrderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        if (await _db.Invoices.AnyAsync(i => i.TransportOrderId == order.Id && i.Status != InvoiceStatus.Cancelled))
            throw new InvalidOperationException("Đơn hàng này đã có hóa đơn chưa hủy.");

        var invoice = Invoice.Create(order, request.Amount);
        var quote = await GetQuoteAsync(order.Id);
        if (quote.StorageFeePending)
            throw new InvalidOperationException(quote.Note);
        if (request.Amount != quote.TotalAmount)
            throw new InvalidOperationException("Tổng tiền hóa đơn đã thay đổi. Vui lòng tính lại trước khi lưu.");
        invoice.TotalAmount = quote.TotalAmount;
        invoice.Lines = quote.Lines.Select(l => new InvoiceLine
        {
            Description = l.Description, Quantity = l.Quantity, UnitPrice = l.UnitPrice, Amount = l.Amount
        }).ToList();
        invoice.DueDate = request.DueDate?.ToDateTime(TimeOnly.MinValue);
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return new InvoiceListItemDto(invoice.Id, order.Code, order.Customer.Name,
            invoice.TotalAmount, invoice.Status.ToString(), invoice.IssueDate);
    }
}
