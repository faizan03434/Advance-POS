// UI/BarcodeScanOverlay.xaml.cs
// Uses WPFMediaKit (same as ProcessOrder) — no new NuGet packages needed.

using StationeryStoreManagementSystem;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ZXing;
using ZXing.Windows.Compatibility;

namespace StationeryStoreManagementSystem.UI
{
    public partial class BarcodeScanOverlay : Window
    {
        public string ScannedValue { get; private set; }

        private readonly BarcodeReader _reader = new BarcodeReader();
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public BarcodeScanOverlay()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(GlobalSettings.CameraName))
            {
                vce.VideoCaptureSource = GlobalSettings.CameraName;
                _timer.Interval = TimeSpan.FromMilliseconds(300);
                _timer.Tick += Timer_Tick;
                _timer.Start();
                statusText.Text = "Camera active — scanning...";
            }
            else
            {
                statusText.Text = "No camera configured. Type the barcode manually below.";
            }
        }

        // Exact same frame-grab pattern as ProcessOrder.CameraTimer_Tick
        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                RenderTargetBitmap bmp = new RenderTargetBitmap(
                    (int)vce.ActualWidth, (int)vce.ActualHeight, 96, 96, PixelFormats.Default);
                vce.Measure(vce.RenderSize);
                vce.Arrange(new Rect(vce.RenderSize));
                bmp.Render(vce);

                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                using MemoryStream ms = new MemoryStream();
                encoder.Save(ms);
                Bitmap btiMap = new Bitmap(ms);

                var result = _reader.Decode(btiMap);
                if (result != null)
                {
                    _timer.Stop();
                    vce.VideoCaptureSource = null;
                    ScannedValue = result.Text;
                    DialogResult = true;
                    Close();
                }
            }
            catch { }
        }

        private void ManualConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            string val = manualEntry.Text?.Trim();
            if (string.IsNullOrEmpty(val)) return;
            ScannedValue = val;
            DialogResult = true;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            try { vce.VideoCaptureSource = null; } catch { }
            base.OnClosed(e);
        }
    }
}
