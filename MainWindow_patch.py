with open("MainWindow.xaml.cs", "r", encoding="utf-8-sig") as f:
    content = f.read()

# 1. Add Services using
if "using StationeryStoreManagementSystem.Services;" not in content:
    content = content.replace(
        "using StationeryStoreManagementSystem.UI;",
        "using StationeryStoreManagementSystem.UI;\nusing StationeryStoreManagementSystem.Services;"
    )

# 2. Start AlertService after login in SetButtons, and add hotkeys
old_setbuttons = '''        private void SetButtons(object sender, EventArgs e)
        {
            sideBar.Visibility = Visibility.Visible;
            sideBar.Children.Clear(); // Clearing children to prevent duplicates on re-login'''

new_setbuttons = '''        // Global keyboard shortcuts
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Utils.CurrentEmployee == null) return;
            bool isAdmin = Utils.CurrentEmployee is Admin;
            switch (e.Key)
            {
                case Key.F1: DashboardButton_Click(this, new RoutedEventArgs()); e.Handled = true; break;
                case Key.F2: ProcessOrderButton_Click(this, new RoutedEventArgs()); e.Handled = true; break;
                case Key.F3: if (isAdmin) ManageProductButton_Click(this, new RoutedEventArgs()); e.Handled = true; break;
                case Key.F4: if (isAdmin) ManageSuppliersButton_Click(this, new RoutedEventArgs()); e.Handled = true; break;
                case Key.F5: if (isAdmin) ManageEmployeesButton_Click(this, new RoutedEventArgs()); e.Handled = true; break;
                case Key.F6: if (isAdmin) ManageNotificationsButton_Click(this, new RoutedEventArgs()); e.Handled = true; break;
                case Key.F7: SettingsButton_Click(this, new RoutedEventArgs()); e.Handled = true; break;
                case Key.Escape: DashboardButton_Click(this, new RoutedEventArgs()); e.Handled = true; break;
            }
        }

        private void SetButtons(object sender, EventArgs e)
        {
            AlertService.Start(); // Start AI alert monitoring after login
            sideBar.Visibility = Visibility.Visible;
            sideBar.Children.Clear(); // Clearing children to prevent duplicates on re-login'''

content = content.replace(old_setbuttons, new_setbuttons)

with open("MainWindow.xaml.cs", "w", encoding="utf-8") as f:
    f.write(content)

print("MainWindow patched OK")
