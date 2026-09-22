namespace TruckingSystem.Desktop.GUI_RouteManagement
{
    partial class RouteMapForm
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
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.lblInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.SuspendLayout();
            //
            // webView
            //
            this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            //
            // lblInfo
            //
            this.lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInfo.Height = 36;
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblInfo.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblInfo.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.lblInfo.BackColor = TruckingSystem.Desktop.Common.Theme.CardBg;
            //
            // RouteMapForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(900, 700);
            this.Controls.Add(this.webView);
            this.Controls.Add(this.lblInfo);
            this.Name = "RouteMapForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private System.Windows.Forms.Label lblInfo;
    }
}
