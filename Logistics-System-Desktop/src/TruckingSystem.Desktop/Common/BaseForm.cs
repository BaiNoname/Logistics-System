using System.Drawing;
using System.Windows.Forms;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>
    /// Lớp cha dùng chung cho MỌI Form. Co giãn theo DPI (AutoScaleMode.Dpi, chuẩn 96 DPI =
    /// màn hình 100%): laptop để 125% / 150% thì toàn bộ vị trí, kích thước, font được nhân
    /// đúng tỉ lệ -> giao diện giống nhau trên laptop 14 inch và màn PC lớn.
    /// Khi form mở lần đầu, tự áp kiểu chung (UiStyle.ApplyDefaults): mọi bảng giãn kín bề
    /// ngang + tiêu đề cột không còn dấu "_", mọi ô chọn ngày hiện dd/MM/yyyy.
    /// </summary>
    public class BaseForm : Form
    {
        public BaseForm()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96f, 96f);
            Font = Theme.FontNormal;
        }

        protected override void OnLoad(EventArgs e)
        {
            UiStyle.ApplyDefaults(this);
            if (ModernLook.IsMine(GetType())) ModernLook.Apply(this);
            FitToScreen();
            base.OnLoad(e);
            // Hộp thoại khác (của Minh): chỉ bo góc nút cho đồng bộ — gọi sau Load để nút đã tô kiểu xong.
            if (ModernLook.RoundButtonsOnly(GetType())) ModernLook.ApplyButtons(this);
        }

        /// <summary>Form mở ra LỚN HƠN vùng làm việc của màn hình (laptop 14 inch để 150%: chỉ
        /// còn khoảng 910 x 510 điểm) -> thu lại vừa màn hình và canh giữa, không để nút / ô nhập
        /// rơi ra ngoài hay nằm dưới thanh Taskbar. Bố cục tự co giãn nên nội dung vẫn dàn đủ.</summary>
        private void FitToScreen()
        {
            if (WindowState != FormWindowState.Normal) return;
            var area = Screen.FromControl((Control?)Owner ?? this).WorkingArea;
            var w = Math.Min(Width, area.Width);
            var h = Math.Min(Height, area.Height);
            if (w == Width && h == Height) return;
            if (MinimumSize.Width > w || MinimumSize.Height > h)
                MinimumSize = new Size(Math.Min(MinimumSize.Width, w), Math.Min(MinimumSize.Height, h));
            Size = new Size(w, h);
            Location = new Point(area.Left + (area.Width - w) / 2, area.Top + (area.Height - h) / 2);
        }
    }
}
