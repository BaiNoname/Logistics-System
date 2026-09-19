namespace TruckingSystem.Desktop.GUI_Shared;

partial class DashboardControl
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        lblTitle = new Label();
        lblLoading = new Label();
        kpiPanel = new FlowLayoutPanel();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(0, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(400, 32);
        lblTitle.TabIndex = 2;
        lblTitle.Text = "Tổng quan hệ thống";
        // 
        // lblLoading
        // 
        lblLoading.AutoSize = true;
        lblLoading.Font = new Font("Segoe UI", 10F);
        lblLoading.ForeColor = Color.FromArgb(100, 116, 139);
        lblLoading.Location = new Point(4, 160);
        lblLoading.Name = "lblLoading";
        lblLoading.Size = new Size(259, 28);
        lblLoading.TabIndex = 0;
        lblLoading.Text = "Đang tải dữ liệu tổng quan...";
        // 
        // kpiPanel
        // 
        kpiPanel.Dock = DockStyle.Top;
        kpiPanel.Location = new Point(0, 0);
        kpiPanel.Name = "kpiPanel";
        kpiPanel.Size = new Size(1164, 140);
        kpiPanel.TabIndex = 1;
        kpiPanel.WrapContents = false;
        // 
        // DashboardControl
        // 
        AutoScaleDimensions = new SizeF(144F, 144F);
        BackColor = Color.FromArgb(241, 245, 249);
        Controls.Add(lblLoading);
        Controls.Add(kpiPanel);
        Controls.Add(lblTitle);
        Name = "DashboardControl";
        Size = new Size(1164, 483);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblLoading;
    private System.Windows.Forms.FlowLayoutPanel kpiPanel;
}
