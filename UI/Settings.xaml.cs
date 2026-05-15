using StationeryStoreManagementSystem.DL;
using StationeryStoreManagementSystem.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using WPFMediaKit.DirectShow.Controls;

namespace StationeryStoreManagementSystem.UI
{
    public partial class Settings : UserControl
    {
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        private bool _loading = true;

        public Settings()
        {
            InitializeComponent();
            _loading = true;

            ThemeCheckbox.IsChecked = GlobalSettings.CurrentTheme == GlobalSettings.Theme.Light;
            DisplayIdsCheckbox.IsChecked = GlobalSettings.DisplayIds;

            cameraField.ItemSource = MultimediaUtil.VideoInputNames.Cast<string>();
            printerField.ItemSource = PrinterSettings.InstalledPrinters.Cast<string>();
            cameraField.SelectedItem = ((IEnumerable<string>)cameraField.ItemSource).FirstOrDefault(x => x == GlobalSettings.CameraName);
            printerField.SelectedItem = ((IEnumerable<string>)printerField.ItemSource).FirstOrDefault(x => x == GlobalSettings.PrinterName);

            rbIpCam.IsChecked = GlobalSettings.UseIpWebcam;
            rbPhysicalCam.IsChecked = !GlobalSettings.UseIpWebcam;
            ipWebcamField.Text = GlobalSettings.IpWebcamUrl ?? "";
            adminEmailField.Text = GlobalSettings.AdminEmail ?? "";
            expiryDaysField.Text = GlobalSettings.ExpiryAlertDays.ToString();

            _loading = false;
        }

        private void ThemeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            GlobalSettings.CurrentTheme = GlobalSettings.Theme.Light;
            try { var h = new WindowInteropHelper(Utils.CurrentMainWindow).Handle; DwmSetWindowAttribute(h, 20, new[] { 0 }, 4); } catch { }
        }

        private void ThemeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalSettings.CurrentTheme = GlobalSettings.Theme.Dark;
            try { var h = new WindowInteropHelper(Utils.CurrentMainWindow).Handle; DwmSetWindowAttribute(h, 20, new[] { 1 }, 4); } catch { }
        }

        private void DisplayIdsCheckbox_Checked(object sender, RoutedEventArgs e) => GlobalSettings.DisplayIds = true;
        private void DisplayIdsCheckbox_Unchecked(object sender, RoutedEventArgs e) => GlobalSettings.DisplayIds = false;

        private void cameraField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loading && ((ComboBoxEntry)sender).ComboBox1.SelectedValue != null)
                GlobalSettings.CameraName = (string?)((ComboBoxEntry)sender).ComboBox1.SelectedValue;
        }

        private void printerField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loading && ((ComboBoxEntry)sender).ComboBox1.SelectedValue != null)
                GlobalSettings.PrinterName = (string?)((ComboBoxEntry)sender).ComboBox1.SelectedValue;
        }

        private void rbPhysicalCam_Checked(object sender, RoutedEventArgs e)
        {
            if (!_loading) GlobalSettings.UseIpWebcam = false;
        }

        private void rbIpCam_Checked(object sender, RoutedEventArgs e)
        {
            if (!_loading) GlobalSettings.UseIpWebcam = true;
        }

        private void ipWebcamField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_loading) GlobalSettings.IpWebcamUrl = ipWebcamField.Text.Trim();
        }

        private void adminEmailField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_loading) GlobalSettings.AdminEmail = adminEmailField.Text.Trim();
        }

        private void expiryDaysField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_loading && int.TryParse(expiryDaysField.Text, out int days) && days > 0)
                GlobalSettings.ExpiryAlertDays = days;
        }
    }
}
