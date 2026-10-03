using TruckingSystem.Desktop.SERVICE_Auth;
using TruckingSystem.Desktop.SERVICE_AuditLogManagement;
using TruckingSystem.Desktop.SERVICE_CustomerManagement;
using TruckingSystem.Desktop.SERVICE_DriverManagement;
using TruckingSystem.Desktop.SERVICE_EmployeeManagement;
using TruckingSystem.Desktop.SERVICE_InvoiceManagement;
using TruckingSystem.Desktop.SERVICE_LocationManagement;
using TruckingSystem.Desktop.SERVICE_NotificationManagement;
using TruckingSystem.Desktop.SERVICE_RouteManagement;
using TruckingSystem.Desktop.SERVICE_TransportOrderManagement;
using TruckingSystem.Desktop.SERVICE_FuelManagement;
using TruckingSystem.Desktop.SERVICE_TripManagement;
using TruckingSystem.Desktop.SERVICE_VehicleManagement;
using TruckingSystem.Desktop.SERVICE_WarehouseManagement;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>
    /// "Cửa ngõ" duy nhất mà các GUI_* dùng để gọi API — gom tất cả SERVICE_*
    /// lại 1 chỗ, tất cả cùng chia sẻ 1 ApiConnection (giữ chung Token đăng nhập).
    /// Form/Control chỉ cần giữ 1 tham số kiểu ApiServices, gọi ra:
    ///   api.TransportOrders.GetTransportOrdersAsync()
    ///   api.Vehicles.CreateVehicleAsync(...)
    /// thay vì phải truyền/nhớ nhiều service riêng lẻ.
    /// </summary>
    public class ApiServices
    {
        public ApiConnection Connection { get; }

        public AuthApiService Auth { get; }
        public TransportOrderApiService TransportOrders { get; }
        public VehicleApiService Vehicles { get; }
        public WarehouseApiService Warehouses { get; }
        public CustomerApiService Customers { get; }
        public EmployeeApiService Employees { get; }
        public DriverApiService Drivers { get; }
        public InvoiceApiService Invoices { get; }
        public LocationApiService Locations { get; }
        public RouteApiService Routes { get; }
        public TripApiService Trips { get; }
        public FuelApiService Fuel { get; }
        public NotificationApiService Notifications { get; }
        public AuditLogApiService AuditLogs { get; }

        public ApiServices(string baseUrl)
        {
            Connection = new ApiConnection(baseUrl);

            Auth = new AuthApiService(Connection);
            TransportOrders = new TransportOrderApiService(Connection);
            Vehicles = new VehicleApiService(Connection);
            Warehouses = new WarehouseApiService(Connection);
            Customers = new CustomerApiService(Connection);
            Employees = new EmployeeApiService(Connection);
            Drivers = new DriverApiService(Connection);
            Invoices = new InvoiceApiService(Connection);
            Locations = new LocationApiService(Connection);
            Routes = new RouteApiService(Connection);
            Trips = new TripApiService(Connection);
            Fuel = new FuelApiService(Connection);
            Notifications = new NotificationApiService(Connection);
            AuditLogs = new AuditLogApiService(Connection);
        }
    }
}