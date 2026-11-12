namespace LogisticsSystem.Employee.Models;

/// <summary>Định dạng hiển thị. Giờ từ API đã là giờ Việt Nam, không cần chuyển múi giờ.</summary>
public static class Display
{
    public static string Time(DateTime? value) =>
        value is null ? "--:--" : value.Value.ToString("HH:mm");

    public static string Date(DateTime value) => value.ToString("dd/MM/yyyy");
}
