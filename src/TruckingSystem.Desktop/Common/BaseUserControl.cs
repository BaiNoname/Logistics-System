using System.Drawing;
using System.Windows.Forms;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Tương tự BaseForm nhưng dành cho UserControl (các màn hình nội dung
    /// hiển thị bên trong MainForm) — cùng cách co giãn DPI, cùng kiểu chung khi mở.</summary>
    public class BaseUserControl : UserControl
    {
        public BaseUserControl()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96f, 96f);
            Font = Theme.FontNormal;
        }

        protected override void OnLoad(EventArgs e)
        {
            UiStyle.ApplyDefaults(this);
            if (ModernLook.IsMine(GetType())) ModernLook.Apply(this);
            base.OnLoad(e);
        }
    }
}
