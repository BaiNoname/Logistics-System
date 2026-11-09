using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_NotificationManagement;
using TruckingSystem.Desktop.GUI_DriverManagement;
using TruckingSystem.Desktop.GUI_TransportOrderManagement;
using TruckingSystem.Desktop.GUI_TripManagement;
using TruckingSystem.Desktop.GUI_VehicleManagement;
using TruckingSystem.Desktop.GUI_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_NotificationManagement
{
    /// <summary>Nhấp đúp thông báo -> màn hình nào, lọc sẵn cái gì. MainForm chỉ việc tìm nút
    /// sidebar có đúng tên NavText (không có = tài khoản không được quyền vào màn đó) rồi mở.</summary>
    public static class NotificationNavigator
    {
        public static (string NavText, Func<Control> Build) Resolve(ApiServices api, NotificationTarget target) => target.Kind switch
        {
            NotificationTargetKind.Trip => ("Chuyến xe", () => // khớp tên mục menu trái (MainForm)
            {
                var c = new TripsControl(api);
                if (target.TripId is int id) c.FocusTrip(id);
                return c;
            }),
            NotificationTargetKind.Order => ("Đơn vận chuyển", () =>
            {
                var c = new TransportOrdersControl(api);
                c.FocusOn(target.Key);
                return c;
            }),
            NotificationTargetKind.Vehicle => ("Đội xe", () =>
            {
                var c = new VehiclesControl(api);
                c.FocusOn(target.Key);
                return c;
            }),
            NotificationTargetKind.Driver => ("Tài xế", () =>
            {
                var c = new DriversControl(api);
                c.FocusOn(target.Key);
                return c;
            }),
            NotificationTargetKind.TripIssue => ("Kho trung chuyển", () =>
            {
                var c = new WarehousesControl(api);
                c.OpenPendingReceiptsOnLoad(target.Key, tripIssues: true);
                return c;
            }),
            _ => ("Kho trung chuyển", () =>
            {
                var c = new WarehousesControl(api);
                c.OpenPendingReceiptsOnLoad(target.Key);
                return c;
            }),
        };
    }
}
