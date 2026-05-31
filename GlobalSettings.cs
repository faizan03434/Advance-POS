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

        // AI Settings
        private static string? aiProvider = "OpenAI"; // OpenRouter uses OpenAI-compatible format
        public static string AIProvider
        {
            get => aiProvider ?? "OpenAI";
            set { aiProvider = value; SaveSettings(); }
        }

        private static string? aiUrl = "https://openrouter.ai/api/v1/chat/completions";
        public static string AIUrl
        {
            get => aiUrl ?? "https://openrouter.ai/api/v1/chat/completions";
            set { aiUrl = value; SaveSettings(); }
        }

        private static string? aiApiKey;
        public static string AIApiKey
        {
            get => aiApiKey ?? "";
            set { aiApiKey = value; SaveSettings(); }
        }

        private static string? aiModel = "nvidia/nemotron-3-super-120b-a12b:free";
        public static string AIModel
        {
            get => aiModel ?? "nvidia/nemotron-3-super-120b-a12b:free";
            set { aiModel = value; SaveSettings(); }
        }

        private static string? aiSystemPrompt;
        public static string AISystemPrompt
        {
            get => aiSystemPrompt ?? @"You are a Senior Business Growth Consultant and Retail Expert. 
You will be provided with a JSON object containing comprehensive store data.
Your task is to provide a master-level Business Intelligence briefing. 
1. Identify the 'Star' products and suggest reordering strategies.
2. Identify slow-moving 'Dead Stock' and suggest creative bundling/promotional strategies.
3. Highlight seasonal opportunities.
4. Note operational risks like discount anomalies.

Format your response as a professional executive summary with clear sections. Focus on maximizing profit.";
            set { aiSystemPrompt = value; SaveSettings(); }
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
                AIProvider = settings.SelectSingleNode("Settings/AIProvider")?.InnerText ?? "OpenAI";
                AIUrl = settings.SelectSingleNode("Settings/AIUrl")?.InnerText ?? "https://openrouter.ai/api/v1/chat/completions";
                AIApiKey = settings.SelectSingleNode("Settings/AIApiKey")?.InnerText ?? "";
                AIModel = settings.SelectSingleNode("Settings/AIModel")?.InnerText ?? "nvidia/nemotron-3-super-120b-a12b:free";
                AISystemPrompt = settings.SelectSingleNode("Settings/AISystemPrompt")?.InnerText ?? GlobalSettings.AISystemPrompt;
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
                Add("AIProvider", AIProvider);
                Add("AIUrl", AIUrl);
                Add("AIApiKey", AIApiKey);
                Add("AIModel", AIModel);
                Add("AISystemPrompt", AISystemPrompt);
                document.AppendChild(root);
                document.Save("Settings.xml");
            }
            catch { }
        }
    }
}
