using System.Drawing;
using System.Windows.Forms;

namespace TruckingSystem.Desktop.Common;

/// <summary>
/// Lớp cha dùng chung cho MỌI Form trong ứng dụng — khắc phục lỗi "lệch chữ,
/// đè chồng control" khi chạy trên laptop có DPI cao (125%/150%/200% — rất phổ
/// biến trên laptop hiện đại). Toàn bộ giao diện được viết bằng tọa độ pixel
/// cố định (Top/Left/Width) giả định màn hình chuẩn 96 DPI (100%) — nếu không
/// khai báo AutoScaleMode, Windows sẽ chỉ "phóng to" cả cửa sổ như ảnh bị kéo
/// giãn (chữ mờ, control chồng lấn) thay vì tính lại đúng vị trí từng control.
///
/// AutoScaleMode.Dpi + khai báo đúng AutoScaleDimensions (96,96) yêu cầu
/// WinForms tự nhân lại toàn bộ Top/Left/Width/Height/Font theo đúng tỉ lệ
/// (DPI thật / 96) — giữ đúng bố cục tương đối, chữ luôn sắc nét ở mọi mức DPI.
/// </summary>
public class BaseForm : Form
{
    public BaseForm()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96f, 96f);
        Font = Theme.FontNormal;
    }
}
