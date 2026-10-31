using System.Windows.Forms;
using TruckingSystem.Desktop.GUI_Shared;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>
    /// Quản lý vòng đời ứng dụng ĐỘC LẬP với bất kỳ Form cụ thể nào — khắc phục
    /// lỗi cũ: dùng Application.Run(new LoginForm()) gắn chết vòng đời cả app vào
    /// 1 instance LoginForm gốc, khiến việc đóng MainForm lúc Đăng xuất vô tình
    /// đóng luôn LoginForm gốc (đang ẩn) → thoát hẳn ứng dụng thay vì quay lại
    /// màn hình đăng nhập. Giờ đây ứng dụng CHỈ thoát khi gọi đúng ExitThread()
    /// (bấm nút ✕ ở màn Login), còn chuyển qua lại Login ↔ Main thoải mái.
    /// </summary>
    public class TruckingAppContext : ApplicationContext
    {
        public TruckingAppContext()
        {
            ShowLogin();
        }

        private void ShowLogin()
        {
            var login = new LoginForm();
            login.LoginSucceeded += (api, username) =>
            {
                login.Hide();
                ShowMain(api, username);
                // Đóng hẳn màn đăng nhập cũ (trước đây chỉ ẩn: mỗi lần đăng xuất -> đăng nhập lại lại tích thêm 1 màn ẩn).
                // Chạy sau khi nút Đăng nhập xử lý xong để không giải phóng form giữa chừng.
                login.BeginInvoke(new Action(login.Close));
            };
            login.ExitRequested += (_, _) => ExitThread();
            login.Show();
        }

        private void ShowMain(ApiServices api, string username)
        {
            var main = new MainForm(api, username);
            main.LogoutRequested += (_, _) =>
            {
                main.Close();
                ShowLogin();
            };
            main.FormClosed += (_, _) =>
            {
                // Người dùng bấm nút ✕ thật trên MainForm (không phải Đăng xuất) -> thoát hẳn app
                if (!main.IsLoggingOut) ExitThread();
            };
            main.Show();
        }
    }
}
