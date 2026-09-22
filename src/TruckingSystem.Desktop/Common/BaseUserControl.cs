using System.Drawing;
using System.Windows.Forms;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Tương tự BaseForm nhưng dành cho UserControl (các màn hình nội dung
    /// hiển thị bên trong MainForm) — cùng lý do, cùng cách khắc phục DPI.</summary>
    public class BaseUserControl : UserControl
    {
        public BaseUserControl()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96f, 96f);
            Font = Theme.FontNormal;
        }
    }
}
