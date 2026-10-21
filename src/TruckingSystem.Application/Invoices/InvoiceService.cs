using System.Data;
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

    public Task<List<InvoiceOrderOptionDto>> GetEligibleOrdersAsync()
        => _db.TransportOrders.AsNoTracking()
            .Where(o => o.Status == TransportOrderStatus.Done
                && !o.Invoices.Any(i => i.Status != InvoiceStatus.Cancelled))
            .OrderByDescending(o => o.Id)
            .Select(o => new InvoiceOrderOptionDto(o.Id, o.Code, o.Customer.Name, o.PickupPoint, o.DropoffPoint))
            .ToListAsync();

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
        invoice.DueDate = request.DueDate?.ToDateTime(TimeOnly.MinValue);
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return new InvoiceListItemDto(invoice.Id, order.Code, order.Customer.Name,
            invoice.TotalAmount, invoice.Status.ToString(), invoice.IssueDate);
    }
}
