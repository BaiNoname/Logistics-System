using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;

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
}
