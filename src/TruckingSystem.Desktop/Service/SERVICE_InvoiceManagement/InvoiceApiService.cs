using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_InvoiceManagement;

namespace TruckingSystem.Desktop.SERVICE_InvoiceManagement;

public class InvoiceApiService
{
    private readonly ApiConnection _conn;

    public InvoiceApiService(ApiConnection conn)
    {
        _conn = conn;
    }

    public Task<List<InvoiceListItem>> GetInvoicesAsync()
        => _conn.GetAsync<List<InvoiceListItem>>("/api/invoices")!;
}
