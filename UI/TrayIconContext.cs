namespace DynQoS.UI
{
    internal class TrayIconContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private ContextMenuStrip _trayContextMenu;

        public TrayIconContext(string trayIconHoverText)
        {
            _trayIcon = new NotifyIcon
            {
                Text = trayIconHoverText,
                Icon = Resources.IconData.TrayIcon
            };

            _trayContextMenu = new ContextMenuStrip();

            _trayIcon.Visible = true;
        }

        public void Close(object? sender, EventArgs? e)
        {
            _trayIcon.Visible = false;

            Application.Exit();
        }

        public ToolStripMenuItem AddElement(string menuName, EventHandler? menuFunction = null, bool menuEnabled = true, ToolStripMenuItem? menuItem = null, Image? menuImage = null)
        {
            ToolStripMenuItem newMenuItem = new ToolStripMenuItem(menuName);
            newMenuItem.Click += menuFunction;
            newMenuItem.Enabled = menuEnabled;
            newMenuItem.Image = menuImage;

            if (menuItem != null)
            {
                menuItem.DropDownItems.Add(newMenuItem);
            }
            else
            {
                _trayContextMenu.Items.Add(newMenuItem);
            }

            return newMenuItem;
        }

        public void AddStrip()
        {
            _trayContextMenu.Items.Add(new ToolStripSeparator());
        }

        public void ClearMenu()
        {
            _trayContextMenu = new ContextMenuStrip();
            _trayIcon.ContextMenuStrip = _trayContextMenu;
        }

        public void UpdateMenu()
        {
            _trayIcon.ContextMenuStrip = _trayContextMenu;
        }

        private void ShowMenu(object? sender = null, EventArgs? e = null)
        {
            _trayContextMenu.Show(Cursor.Position);
        }
    }
}