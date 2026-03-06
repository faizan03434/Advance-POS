using StationeryStoreManagementSystem.DL;
using StationeryStoreManagementSystem.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFMediaKit.DirectShow.Controls;

namespace StationeryStoreManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {

        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
        public Settings()
        {
            InitializeComponent();
            ThemeCheckbox.IsChecked = GlobalSettings.CurrentTheme == GlobalSettings.Theme.Light;
            DisplayIdsCheckbox.IsChecked = GlobalSettings.DisplayIds == true;
            cameraField.ItemSource = MultimediaUtil.VideoInputNames.Cast<string>();
            printerField.ItemSource = PrinterSettings.InstalledPrinters.Cast<string>();
            cameraField.SelectedItem = ((IEnumerable<string>)cameraField.ItemSource).Where(x => x == GlobalSettings.CameraName).FirstOrDefault();
            printerField.SelectedItem = ((IEnumerable<string>)printerField.ItemSource).Where(x => x == GlobalSettings.PrinterName).FirstOrDefault();
        }

        private void ThemeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            GlobalSettings.CurrentTheme = GlobalSettings.Theme.Light;
            var handle = new WindowInteropHelper(Utils.CurrentMainWindow).Handle;
                DwmSetWindowAttribute(handle, 20, new[] { 0 }, 4);
        }

        private void ThemeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalSettings.CurrentTheme = GlobalSettings.Theme.Dark;
            var handle = new WindowInteropHelper(Utils.CurrentMainWindow).Handle;
            DwmSetWindowAttribute(handle, 20, new[] { 1 }, 4);
        }

        private void DisplayIdsCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            GlobalSettings.DisplayIds = true;
        }

        private void DisplayIdsCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalSettings.DisplayIds = false;
        }

        private void cameraField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxEntry)sender).ComboBox1.SelectedValue != null)
            {
                GlobalSettings.CameraName = (string?)((ComboBoxEntry)sender).ComboBox1.SelectedValue;
            }
        }

        private void printerField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxEntry)sender).ComboBox1.SelectedValue != null)
            {
                GlobalSettings.PrinterName = (string?)((ComboBoxEntry)sender).ComboBox1.SelectedValue;
            }
        }
    }
}
