using Microsoft.Extensions.DependencyInjection;
using TruckingSystem.Application.Customers;
using TruckingSystem.Application.Contracts;
using TruckingSystem.Application.Pricing;
using TruckingSystem.Application.Drivers;
using TruckingSystem.Application.Employees;
using TruckingSystem.Application.Invoices;
using TruckingSystem.Application.Locations;
using TruckingSystem.Application.PublicTracking;
using TruckingSystem.Application.TripPhotos;
using TruckingSystem.Application.TransportOrders;
using TruckingSystem.Application.Fuel;
using TruckingSystem.Application.Trips;
using TruckingSystem.Application.Vehicles;
using TruckingSystem.Application.VehicleAllocation;
using TruckingSystem.Application.Warehouses;
using TruckingSystem.Application.Reports;
using TruckingSystem.Application.TripVideos;
using TruckingSystem.Application.SystemManagement;
using TruckingSystem.Application.Resources;

namespace TruckingSystem.Application;

/// <summary>
/// Đăng ký toàn bộ Service của Application layer vào DI container.
/// Program.cs (API) chỉ cần gọi 1 dòng builder.Services.AddApplication()
/// thay vì phải biết chi tiết bên trong Application có bao nhiêu service.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<Auth.AccountEmployeeService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<IFuelService, FuelService>();
        services.AddScoped<ITransportOrderService, TransportOrderService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IContractAppendixService, ContractAppendixService>();
        services.AddScoped<Surcharges.ISurchargeListService, Surcharges.SurchargeListService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<TruckingSystem.Application.Notifications.INotificationService, TruckingSystem.Application.Notifications.NotificationService>();
        services.AddScoped<TruckingSystem.Application.AuditLogs.IAuditLogService, TruckingSystem.Application.AuditLogs.AuditLogService>();
        services.AddScoped<IPublicTrackingService, PublicTrackingService>();
        services.AddScoped<IWarehouseSlottingService, WarehouseSlottingService>();
        services.AddScoped<IPendingReceiptService, PendingReceiptService>();
        services.AddScoped<ITripIssueService, TripIssueService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<Payroll.IPayrollService, Payroll.PayrollService>();
        services.AddScoped<Attendance.IAttendanceService, Attendance.AttendanceService>();
        services.AddScoped<Leave.ILeaveService, Leave.LeaveService>();
        services.AddScoped<ISystemManagementService, SystemManagementService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IVehicleAllocationService, VehicleAllocationService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ITripVideoService, TripVideoService>();
        services.AddScoped<ITripPhotoService, TripPhotoService>();
        services.AddScoped<IResourceOverviewService, ResourceOverviewService>();
        return services;
    }
}
