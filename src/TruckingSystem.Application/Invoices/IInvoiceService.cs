namespace TruckingSystem.Application.Invoices;

public interface IInvoiceService
{
    Task<List<InvoiceListItemDto>> GetAllAsync();
    Task<List<InvoiceOrderOptionDto>> GetEligibleOrdersAsync();
    Task<InvoiceListItemDto> CreateAsync(CreateInvoiceRequest request);
}
