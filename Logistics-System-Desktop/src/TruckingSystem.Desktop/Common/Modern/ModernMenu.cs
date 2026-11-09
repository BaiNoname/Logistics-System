namespace TruckingSystem.Desktop.Common
{
    /// <summary>Giao diện menu thả xuống / menu chuột phải theo phong cách phần mềm doanh nghiệp: nền trắng, viền
    /// xám nhạt, dòng cao thoáng, rê chuột tô xanh nhạt, mục không dùng được chữ xám — thay cho menu mặc định của
    /// WinForms (cột biểu tượng xám bên trái, dòng sát nhau, nhìn cũ kỹ).
    /// Dùng: ModernMenu.Apply(contextMenu) sau khi đã thêm xong các mục.</summary>
    public static class ModernMenu
    {
        private static readonly Color Hover = Color.FromArgb(239, 246, 255);      // xanh rất nhạt
        private static readonly Color HoverBorder = Color.FromArgb(191, 219, 254);
        private static readonly Color Border = Color.FromArgb(226, 232, 240);
        private static readonly Color DisabledText = Color.FromArgb(148, 163, 184);

        public static void Apply(ToolStripDropDown menu, int minWidth = 0)
        {
            menu.Renderer = new Renderer();
            menu.Font = new Font("Segoe UI", 10F);
            menu.Padding = new Padding(4, 6, 4, 6);
            menu.BackColor = Color.White;
            if (menu is ToolStripDropDownMenu dd)
            {
                dd.ShowImageMargin = false;
                dd.ShowCheckMargin = false;
            }
            if (minWidth > 0) menu.MinimumSize = new Size(minWidth, 0);
            menu.ShowItemToolTips = true;
            foreach (ToolStripItem item in menu.Items) Style(item);
        }

        private static void Style(ToolStripItem item)
        {
            switch (item)
            {
                case ToolStripSeparator sep:
                    sep.Margin = new Padding(0, 4, 0, 4);
                    break;
                case ToolStripMenuItem mi:
                    mi.Padding = new Padding(6, 7, 18, 7);
                    if (mi.DropDownItems.Count > 0) Apply(mi.DropDown);
                    break;
            }
        }

        private sealed class Renderer : ToolStripProfessionalRenderer
        {
            public Renderer() : base(new Colors()) { RoundedEdges = false; }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if (e.Item is ToolStripMenuItem)
                    e.TextColor = e.Item.Enabled ? Theme.TextPrimary : DisabledText;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = e.Item?.Enabled == false ? DisabledText : Theme.TextSecondary;
                base.OnRenderArrow(e);
            }
        }

        private sealed class Colors : ProfessionalColorTable
        {
            public Colors() { UseSystemColors = false; }
            public override Color ToolStripDropDownBackground => Color.White;
            public override Color MenuBorder => Border;
            public override Color MenuItemBorder => HoverBorder;
            public override Color MenuItemSelected => Hover;
            public override Color MenuItemSelectedGradientBegin => Hover;
            public override Color MenuItemSelectedGradientEnd => Hover;
            public override Color MenuItemPressedGradientBegin => Hover;
            public override Color MenuItemPressedGradientMiddle => Hover;
            public override Color MenuItemPressedGradientEnd => Hover;
            public override Color ImageMarginGradientBegin => Color.White;
            public override Color ImageMarginGradientMiddle => Color.White;
            public override Color ImageMarginGradientEnd => Color.White;
            public override Color SeparatorDark => Border;
            public override Color SeparatorLight => Color.White;
        }
    }
}
