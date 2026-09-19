namespace TruckingSystem.Desktop.GUI_Shared;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.sidebar = new System.Windows.Forms.Panel();
        this.content = new System.Windows.Forms.Panel();
        this.topBar = new System.Windows.Forms.Panel();
        this.SuspendLayout();
        //
        // sidebar
        //
        this.sidebar.Dock = System.Windows.Forms.DockStyle.Left;
        this.sidebar.Width = 260;
        this.sidebar.BackColor = TruckingSystem.Desktop.Common.Theme.SidebarBg;
        //
        // content
        //
        this.content.Dock = System.Windows.Forms.DockStyle.Fill;
        this.content.Padding = new System.Windows.Forms.Padding(24);
        //
        // topBar
        //
        this.topBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.topBar.Height = 60;
        this.topBar.BackColor = System.Drawing.Color.White;
        this.topBar.Padding = new System.Windows.Forms.Padding(24, 0, 24, 0);
        //
        // MainForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = TruckingSystem.Desktop.Common.Theme.ContentBg;
        this.ClientSize = new System.Drawing.Size(1280, 800);
        this.MinimumSize = new System.Drawing.Size(1100, 700);
        this.Controls.Add(this.content);
        this.Controls.Add(this.topBar);
        this.Controls.Add(this.sidebar);
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Trucking System";
        this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Panel sidebar;
    private System.Windows.Forms.Panel content;
    private System.Windows.Forms.Panel topBar;
}
