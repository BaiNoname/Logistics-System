namespace TruckingSystem.Application.Invoices;

public interface IInvoiceService
{
    Task<List<InvoiceListItemDto>> GetAllAsync();
    Task<InvoiceDetailDto> GetDetailAsync(int id);
    Task<List<InvoiceOrderOptionDto>> GetEligibleOrdersAsync();
    Task<InvoiceQuoteDto> GetQuoteAsync(int orderId);
    Task<InvoiceListItemDto> CreateAsync(CreateInvoiceRequest request);
    Task MarkAsPaidAsync(int id);
}
