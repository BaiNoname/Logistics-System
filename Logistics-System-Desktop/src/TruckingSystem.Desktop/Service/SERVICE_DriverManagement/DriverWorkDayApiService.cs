using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_DriverManagement;

namespace TruckingSystem.Desktop.SERVICE_DriverManagement
{
    /// <summary>Ngày làm việc của tài xế (bảng DriverSchedules) — đăng ký theo NGÀY,
    /// không chọn ca. Tách file riêng, không đụng EmployeeApiService/ApiServices —
    /// tạo bằng: new DriverWorkDayApiService(api.Connection).</summary>
    public class DriverWorkDayApiService
    {
        private readonly ApiConnection _conn;

        public DriverWorkDayApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public async Task<List<DriverWorkDayItem>> GetWorkDaysAsync(int employeeId)
            => await _conn.GetAsync<List<DriverWorkDayItem>>($"/api/drivers/{employeeId}/workdays") ?? new List<DriverWorkDayItem>();

        /// <summary>Gửi ngày dạng "yyyy-MM-dd" (chỉ phần ngày — không lệch múi giờ).</summary>
        public async Task<DriverWorkDayResultItem> RegisterAsync(int employeeId, DateTime fromDate, DateTime toDate)
            => await _conn.PostAsync<DriverWorkDayResultItem>($"/api/drivers/{employeeId}/workdays", new
            {
                fromDate = fromDate.ToString("yyyy-MM-dd"),
                toDate = toDate.ToString("yyyy-MM-dd"),
            })
               ?? new DriverWorkDayResultItem { Success = false, Message = "Không nhận được phản hồi từ server." };

        public async Task<DriverWorkDayResultItem> DeleteAsync(int workDayId)
            => await _conn.DeleteAsync<DriverWorkDayResultItem>($"/api/drivers/workdays/{workDayId}")
               ?? new DriverWorkDayResultItem { Success = false, Message = "Không nhận được phản hồi từ server." };
    }
}
