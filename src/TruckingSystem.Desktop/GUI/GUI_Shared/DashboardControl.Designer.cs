namespace TruckingSystem.Desktop.GUI_Shared
{
    partial class DashboardControl
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        // Bố cục màn Tổng quan được dựng bằng code trong DashboardControl.cs
        // (BuildLayout) vì phần lớn nội dung là động: thẻ số liệu, biểu đồ vẽ
        // tay, bảng lãi/lỗ — giống cách MainForm dựng sidebar từ danh sách.
        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(144F, 144F);
            BackColor = Color.FromArgb(241, 245, 249);
            Name = "DashboardControl";
            Size = new Size(1164, 700);
            ResumeLayout(false);
        }

        #endregion
    }
}
