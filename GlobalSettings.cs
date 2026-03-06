using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace StationeryStoreManagementSystem
{
    public static class GlobalSettings
    {
        public enum Theme
        {
            Dark,
            Light
        }
        private static Theme currentTheme = Theme.Dark;
        public static Theme CurrentTheme
        {
            get 
            { 
                return currentTheme; 
            }
            set
            { 
                currentTheme = value;
                SaveSettings();
                ApplyTheme();
            }
        }
        private static bool displayIds = false;

        public static bool DisplayIds
        {
            get 
            { 
                return displayIds; 
            }
            set 
            { 
                displayIds = value;
                SaveSettings();
            }
        }
        private static string? cameraName;

        public static string? CameraName
        {
            get { return cameraName; }
            set { 
                cameraName = value;
                SaveSettings();
            }
        }

        private static string? printerName;

        public static string? PrinterName
        {
            get { return printerName; }
            set { 
                printerName = value;
                SaveSettings();
            }
        }


        public static void ApplyTheme()
        {
            if (Utils.CurrentMainWindow != null)
            {
                Collection<ResourceDictionary> dictionary = Utils.CurrentMainWindow.Resources.MergedDictionaries;
                dictionary.Clear();
                string path;
                if (CurrentTheme == Theme.Dark)
                    path = "/UI/Themes/DarkTheme.xaml";
                else
                    path = "/UI/Themes/LightTheme.xaml";
                dictionary.Add(new ResourceDictionary() { Source = new Uri(path, UriKind.RelativeOrAbsolute) });
            }
        }
        public static void LoadSettings()
        {
            try
            {
                var settings = new XmlDocument();
                settings.Load("Settings.xml");
                var theme = settings.SelectSingleNode("Settings/Theme");
                var displayIds = settings.SelectSingleNode("Settings/DisplayIds");
                var cameraName = settings.SelectSingleNode("Settings/Camera");
                var printerName = settings.SelectSingleNode("Settings/Printer");
                CurrentTheme = (Theme)Enum.Parse(typeof(Theme), theme.InnerText);
                DisplayIds = bool.Parse(displayIds.InnerText);
                CameraName = cameraName?.InnerText;
                PrinterName = printerName?.InnerText;
            }
            catch (Exception)
            {

            }
        }
        public static void SaveSettings()
        {
            var document = new XmlDocument();
            XmlElement settings = document.CreateElement("Settings");
            var theme = document.CreateElement("Theme");
            var displayIds = document.CreateElement("DisplayIds");
            var cameraName = document.CreateElement("Camera");
            var printerName = document.CreateElement("Printer");
            theme.InnerText = CurrentTheme.ToString();
            displayIds.InnerText = DisplayIds.ToString();
            cameraName.InnerText = CameraName;
            printerName.InnerText = PrinterName;
            settings.AppendChild(theme);
            settings.AppendChild(displayIds);
            settings.AppendChild(cameraName);
            settings.AppendChild(printerName);
            document.AppendChild(settings);
            document.Save("Settings.xml");
        }
    }
}
