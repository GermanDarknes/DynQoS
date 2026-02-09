namespace DynQoS.UI
{
    internal class TrayIconContext : ApplicationContext
    {
        private readonly NotifyIcon TrayIcon;
        private ContextMenuStrip TrayContextMenu;

        public TrayIconContext(string TrayIconHoverText)
        {
            TrayIcon = new NotifyIcon
            {
                Text = TrayIconHoverText,
                Icon = Resources.IconData.TrayIcon
            };

            TrayContextMenu = new ContextMenuStrip();

            TrayIcon.Visible = true;
        }

        public void Close(object? sender, EventArgs? e)
        {
            TrayIcon.Visible = false;

            Application.Exit();
        }

        public ToolStripMenuItem AddElement(string MenuName, EventHandler? MenuFunction = null, bool MenuEnabled = true, ToolStripMenuItem? MenuItem = null, Image? MenuImage = null)
        {
            ToolStripMenuItem NewMenuItem = new ToolStripMenuItem(MenuName);
            NewMenuItem.Click += MenuFunction;
            NewMenuItem.Enabled = MenuEnabled;
            NewMenuItem.Image = MenuImage;

            if (MenuItem != null)
            {
                MenuItem.DropDownItems.Add(NewMenuItem);
            }
            else
            {
                TrayContextMenu.Items.Add(NewMenuItem);
            }

            return NewMenuItem;
        }

        public void AddStrip()
        {
            TrayContextMenu.Items.Add(new ToolStripSeparator());
        }

        public void ClearMenu()
        {
            TrayContextMenu = new ContextMenuStrip();
            TrayIcon.ContextMenuStrip = TrayContextMenu;
        }

        public void UpdateMenu()
        {
            TrayIcon.ContextMenuStrip = TrayContextMenu;
        }

        private void ShowMenu(object? Sender = null, EventArgs? E = null)
        {
            TrayContextMenu.Show(Cursor.Position);
        }
    }
}