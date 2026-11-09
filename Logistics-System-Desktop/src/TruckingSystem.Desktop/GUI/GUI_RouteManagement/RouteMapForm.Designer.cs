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

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = dòng thông tin (tự xuống dòng khi dài)
        /// / bản đồ WebView2 (chiếm phần còn lại). Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblInfo = new System.Windows.Forms.Label();
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            root.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblInfo, 0, 0);
            root.Controls.Add(webView, 0, 1);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Margin = new System.Windows.Forms.Padding(0);
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(0);
            root.RowCount = 2;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Size = new System.Drawing.Size(900, 700);
            root.TabIndex = 0;
            //
            // lblInfo
            //
            lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblInfo.AutoSize = true;
            lblInfo.BackColor = System.Drawing.Color.White;
            lblInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblInfo.Margin = new System.Windows.Forms.Padding(0);
            lblInfo.MinimumSize = new System.Drawing.Size(0, 36);
            lblInfo.Name = "lblInfo";
            lblInfo.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            lblInfo.TabIndex = 0;
            lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // webView
            //
            webView.DefaultBackgroundColor = System.Drawing.Color.White;
            webView.Dock = System.Windows.Forms.DockStyle.Fill;
            webView.Margin = new System.Windows.Forms.Padding(0);
            webView.Name = "webView";
            webView.TabIndex = 1;
            //
            // RouteMapForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(900, 700);
            Controls.Add(root);
            MinimumSize = new System.Drawing.Size(640, 480);
            Name = "RouteMapForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            root.ResumeLayout(false);
            root.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)webView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.TableLayoutPanel root;
    }
}
