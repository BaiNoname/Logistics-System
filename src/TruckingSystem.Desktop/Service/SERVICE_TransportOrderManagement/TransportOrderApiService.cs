using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.SERVICE_TransportOrderManagement;

public class TransportOrderApiService
{
    private readonly ApiConnection _conn;

    public TransportOrderApiService(ApiConnection conn)
    {
        _conn = conn;
    }

    public Task<List<TransportOrderListItem>> GetTransportOrdersAsync()
        => _conn.GetAsync<List<TransportOrderListItem>>("/api/transport-orders")!;

    public Task CreateTransportOrderAsync(int customerId, string pickup, string dropoff, DateTime? committedDate,
        List<CargoLineItem> cargoLines, int? pickupLocationId = null, int? dropoffLocationId = null)
        => _conn.PostAsync<object>("/api/transport-orders", new
        {
            customerId, pickupPoint = pickup, dropoffPoint = dropoff,
            committedDeliveryDate = committedDate, cargoLines,
            pickupLocationId, dropoffLocationId,
        });

    public Task<TransportOrderDetailItem> GetOrderDetailAsync(int id)
        => _conn.GetAsync<TransportOrderDetailItem>($"/api/transport-orders/{id}")!;

    public Task<AllocateVehiclesResult> AllocateVehiclesAsync(int transportOrderId)
        => _conn.PostAsync<AllocateVehiclesResult>($"/api/transport-orders/{transportOrderId}/allocate-vehicles", new { })!;
}
