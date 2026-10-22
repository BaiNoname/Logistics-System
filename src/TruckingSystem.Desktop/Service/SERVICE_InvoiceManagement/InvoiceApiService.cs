using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_InvoiceManagement;

namespace TruckingSystem.Desktop.SERVICE_InvoiceManagement
{
    public class InvoiceApiService
    {
        private readonly ApiConnection _conn;

        public InvoiceApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public Task<List<InvoiceListItem>> GetInvoicesAsync()
            => _conn.GetAsync<List<InvoiceListItem>>("/api/invoices")!;

        public Task<List<InvoiceOrderOption>> GetEligibleOrdersAsync()
            => _conn.GetAsync<List<InvoiceOrderOption>>("/api/invoices/eligible-orders")!;

        public Task<InvoiceListItem?> CreateAsync(int transportOrderId, decimal amount, DateTime? dueDate)
            => _conn.PostAsync<InvoiceListItem>("/api/invoices", new
            {
                transportOrderId,
                amount,
                dueDate = dueDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
            });
    }
}
