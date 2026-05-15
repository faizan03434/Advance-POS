using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;

namespace StationeryStoreManagementSystem
{
    public static class GlobalSettings
    {
        public enum Theme { Dark, Light }

        private static Theme currentTheme = Theme.Dark;
        public static Theme CurrentTheme
        {
            get { return currentTheme; }
            set { currentTheme = value; SaveSettings(); ApplyTheme(); }
        }

        private static bool displayIds = false;
        public static bool DisplayIds
        {
            get { return displayIds; }
            set { displayIds = value; SaveSettings(); }
        }

        private static string? cameraName;
        public static string? CameraName
        {
            get { return cameraName; }
            set { cameraName = value; SaveSettings(); }
        }

        private static string? printerName;
        public static string? PrinterName
        {
            get { return printerName; }
            set { printerName = value; SaveSettings(); }
        }

        // Mobile/IP Webcam settings
        private static string? ipWebcamUrl;
        public static string? IpWebcamUrl
        {
            get { return ipWebcamUrl; }
            set { ipWebcamUrl = value; SaveSettings(); }
        }

        private static bool useIpWebcam = false;
        public static bool UseIpWebcam
        {
            get { return useIpWebcam; }
            set { useIpWebcam = value; SaveSettings(); }
        }

        // Admin email for alerts
        private static string? adminEmail;
        public static string? AdminEmail
        {
            get { return adminEmail; }
            set { adminEmail = value; SaveSettings(); }
        }

        // Low stock threshold override (optional global)
        private static int lowStockAlertDays = 30;
        public static int ExpiryAlertDays
        {
            get { return lowStockAlertDays; }
            set { lowStockAlertDays = value; SaveSettings(); }
        }

        public static void ApplyTheme()
        {
            if (Utils.CurrentMainWindow != null)
            {
                Collection<ResourceDictionary> dictionary = Utils.CurrentMainWindow.Resources.MergedDictionaries;
                dictionary.Clear();
                string path = CurrentTheme == Theme.Dark ? "/UI/Themes/DarkTheme.xaml" : "/UI/Themes/LightTheme.xaml";
                dictionary.Add(new ResourceDictionary() { Source = new Uri(path, UriKind.RelativeOrAbsolute) });
            }
        }

        public static void LoadSettings()
        {
            try
            {
                var settings = new XmlDocument();
                settings.Load("Settings.xml");
                CurrentTheme = (Theme)Enum.Parse(typeof(Theme), settings.SelectSingleNode("Settings/Theme")?.InnerText ?? "Dark");
                DisplayIds = bool.Parse(settings.SelectSingleNode("Settings/DisplayIds")?.InnerText ?? "false");
                CameraName = settings.SelectSingleNode("Settings/Camera")?.InnerText;
                PrinterName = settings.SelectSingleNode("Settings/Printer")?.InnerText;
                IpWebcamUrl = settings.SelectSingleNode("Settings/IpWebcamUrl")?.InnerText;
                UseIpWebcam = bool.Parse(settings.SelectSingleNode("Settings/UseIpWebcam")?.InnerText ?? "false");
                AdminEmail = settings.SelectSingleNode("Settings/AdminEmail")?.InnerText;
                ExpiryAlertDays = int.Parse(settings.SelectSingleNode("Settings/ExpiryAlertDays")?.InnerText ?? "30");
            }
            catch { }
        }

        public static void SaveSettings()
        {
            try
            {
                var document = new XmlDocument();
                XmlElement root = document.CreateElement("Settings");
                void Add(string name, string? value) { var el = document.CreateElement(name); el.InnerText = value ?? ""; root.AppendChild(el); }
                Add("Theme", CurrentTheme.ToString());
                Add("DisplayIds", DisplayIds.ToString());
                Add("Camera", CameraName);
                Add("Printer", PrinterName);
                Add("IpWebcamUrl", IpWebcamUrl);
                Add("UseIpWebcam", UseIpWebcam.ToString());
                Add("AdminEmail", AdminEmail);
                Add("ExpiryAlertDays", ExpiryAlertDays.ToString());
                document.AppendChild(root);
                document.Save("Settings.xml");
            }
            catch { }
        }
    }
}
