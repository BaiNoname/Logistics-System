namespace TruckingSystem.Application.Invoices;

public interface IInvoiceService
{
    Task<List<InvoiceListItemDto>> GetAllAsync();
}
