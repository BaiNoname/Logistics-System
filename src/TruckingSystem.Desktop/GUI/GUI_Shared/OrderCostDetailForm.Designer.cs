namespace TruckingSystem.Desktop.GUI_Shared
{
    partial class OrderCostDetailForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        // Nội dung (thẻ tổng hợp + bảng khoản chi) dựng bằng code trong
        // OrderCostDetailForm.cs (BuildLayout) vì phụ thuộc dữ liệu từng đơn.
        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(1000, 620);
            BackColor = Color.FromArgb(241, 245, 249);
            MinimumSize = new Size(820, 480);
            StartPosition = FormStartPosition.CenterParent;
            Name = "OrderCostDetailForm";
            Text = "Chi phí đơn hàng";
            ResumeLayout(false);
        }

        #endregion
    }
}
