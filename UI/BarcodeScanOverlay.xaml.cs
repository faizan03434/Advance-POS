// UI/BarcodeScanOverlay.xaml.cs
// Supports BOTH physical camera (WPFMediaKit) AND IP Webcam (mobile phone).
// Uses ZXing TryHarder + multi-format hints for real product packaging barcodes.

using StationeryStoreManagementSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace StationeryStoreManagementSystem.UI
{
    public partial class BarcodeScanOverlay : Window
    {
        // ── Public results ──────────────────────────────────────────────
        public string ScannedValue { get; private set; }
        public Bitmap CapturedBarcodeImage { get; private set; }

        // ── ZXing reader — configured for real product barcodes ─────────
        // TryHarder = scans entire image, not just center
        // Multiple formats = EAN-13, EAN-8, UPC-A, CODE-128, CODE-39 etc.
        private readonly BarcodeReader _reader = new BarcodeReader
        {
            AutoRotate = true,
            TryInverted = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.EAN_8,
                    BarcodeFormat.UPC_A,
                    BarcodeFormat.UPC_E,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.CODE_39,
                    BarcodeFormat.ITF,
                    BarcodeFormat.QR_CODE,
                },
                // Pure barcode = false means it looks for barcode inside a larger image
                PureBarcode = false,
            }
        };

        private readonly DispatcherTimer _physicalTimer = new DispatcherTimer();
        private readonly DispatcherTimer _ipTimer = new DispatcherTimer();
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };
        private bool _cameraReady = false;
        private bool _scanning = false; // prevent overlapping decode calls

        public BarcodeScanOverlay()
        {
            InitializeComponent();
        }

        // ── Window loaded ───────────────────────────────────────────────
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // MODE 1: IP Webcam (mobile phone)
            if (GlobalSettings.UseIpWebcam &&
                !string.IsNullOrEmpty(GlobalSettings.IpWebcamUrl))
            {
                ipWebcamImage.Visibility = Visibility.Visible;
                vce.Visibility = Visibility.Collapsed;
                titleText.Text = "IP Webcam — point phone camera at barcode";
                statusText.Text = $"Connecting to {GlobalSettings.IpWebcamUrl} ...";

                _ipTimer.Interval = TimeSpan.FromMilliseconds(500);
                _ipTimer.Tick += IpTimer_Tick;
                _ipTimer.Start();
            }
            // MODE 2: Physical / USB camera
            else if (!string.IsNullOrEmpty(GlobalSettings.CameraName))
            {
                vce.Visibility = Visibility.Visible;
                ipWebcamImage.Visibility = Visibility.Collapsed;
                try
                {
                    vce.VideoCaptureSource = GlobalSettings.CameraName;
                    titleText.Text = "Camera active — point at barcode";
                    statusText.Text = "Camera starting...";

                    var warmup = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(900)
                    };
                    warmup.Tick += (s, _) =>
                    {
                        warmup.Stop();
                        _cameraReady = true;
                        statusText.Text = "Scanning — hold barcode steady.";
                        _physicalTimer.Interval = TimeSpan.FromMilliseconds(350);
                        _physicalTimer.Tick += PhysicalTimer_Tick;
                        _physicalTimer.Start();
                    };
                    warmup.Start();
                }
                catch (Exception ex)
                {
                    statusText.Text = $"Camera error: {ex.Message} — type manually.";
                }
            }
            // MODE 3: Nothing configured
            else
            {
                statusText.Text = "No camera configured in Settings — type barcode manually.";
            }

            manualEntry.Focus();
        }

        // ── Physical camera tick ────────────────────────────────────────
        private void PhysicalTimer_Tick(object sender, EventArgs e)
        {
            if (!_cameraReady || _scanning) return;
            _scanning = true;
            try
            {
                int w = (int)vce.ActualWidth;
                int h = (int)vce.ActualHeight;
                if (w <= 0 || h <= 0) { _scanning = false; return; }

                var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Default);
                vce.Measure(new System.Windows.Size(w, h));
                vce.Arrange(new Rect(0, 0, w, h));
                rtb.Render(vce);

                var enc = new JpegBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(rtb));
                using var ms = new MemoryStream();
                enc.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var frame = new Bitmap(ms);

                var result = TryDecode(frame);
                if (result != null)
                    FinishScan(result, frame);
            }
            catch { }
            finally { _scanning = false; }
        }

        // ── IP Webcam tick ──────────────────────────────────────────────
        private async void IpTimer_Tick(object sender, EventArgs e)
        {
            if (_scanning) return;
            _scanning = true;
            try
            {
                string url = GlobalSettings.IpWebcamUrl!.TrimEnd('/') + "/shot.jpg";
                byte[] bytes = await _http.GetByteArrayAsync(url);

                // Show live frame
                using var displayMs = new MemoryStream(bytes);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = displayMs;
                bi.EndInit();
                ipWebcamImage.Source = bi;
                statusText.Text = "Live — point phone at barcode...";

                // Decode
                using var decodeMs = new MemoryStream(bytes);
                var frame = new Bitmap(decodeMs);
                var result = TryDecode(frame);
                if (result != null)
                    FinishScan(result, frame);
            }
            catch
            {
                statusText.Text = "Waiting for IP Webcam... check URL in Settings.";
            }
            finally { _scanning = false; }
        }

        // ── Core decode — tries original + preprocessed versions ────────
        private string TryDecode(Bitmap original)
        {
            // Pass 1: decode original image as-is
            var r1 = _reader.Decode(original);
            if (r1 != null) return r1.Text;

            // Pass 2: convert to grayscale + boost contrast
            // This helps with shiny/reflective packaging like CupKake wrappers
            try
            {
                using var gray = ToGrayscaleHighContrast(original);
                var r2 = _reader.Decode(gray);
                if (r2 != null) return r2.Text;
            }
            catch { }

            // Pass 3: try a scaled-up version (helps with small barcodes)
            try
            {
                using var scaled = new Bitmap(original, original.Width * 2, original.Height * 2);
                var r3 = _reader.Decode(scaled);
                if (r3 != null) return r3.Text;
            }
            catch { }

            return null;
        }

        // ── Grayscale + contrast boost ──────────────────────────────────
        private static Bitmap ToGrayscaleHighContrast(Bitmap src)
        {
            var dest = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(dest);

            // ColorMatrix: grayscale + contrast
            float c = 1.6f; // contrast factor
            float t = -0.3f; // brightness offset
            var cm = new ColorMatrix(new float[][]
            {
                new float[] { c*0.299f, c*0.299f, c*0.299f, 0, 0 },
                new float[] { c*0.587f, c*0.587f, c*0.587f, 0, 0 },
                new float[] { c*0.114f, c*0.114f, c*0.114f, 0, 0 },
                new float[] { 0,        0,        0,        1, 0 },
                new float[] { t,        t,        t,        0, 1 },
            });
            var ia = new ImageAttributes();
            ia.SetColorMatrix(cm);
            g.DrawImage(src,
                new Rectangle(0, 0, src.Width, src.Height),
                0, 0, src.Width, src.Height,
                GraphicsUnit.Pixel, ia);
            return dest;
        }

        // ── Finish ──────────────────────────────────────────────────────
        private void FinishScan(string value, Bitmap frame)
        {
            _physicalTimer.Stop();
            _ipTimer.Stop();
            try { vce.VideoCaptureSource = null; } catch { }

            ScannedValue = value;
            CapturedBarcodeImage = frame;
            statusText.Text = $"✓  Scanned: {value}";
            DialogResult = true;
            Close();
        }

        // ── Manual entry ────────────────────────────────────────────────
        private void ManualEntry_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ConfirmManual();
        }

        private void ManualConfirmBtn_Click(object sender, RoutedEventArgs e)
            => ConfirmManual();

        private void ConfirmManual()
        {
            string val = manualEntry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(val)) return;
            _physicalTimer.Stop();
            _ipTimer.Stop();
            try { vce.VideoCaptureSource = null; } catch { }
            ScannedValue = val;
            CapturedBarcodeImage = null;
            DialogResult = true;
            Close();
        }

        // ── Cleanup ─────────────────────────────────────────────────────
        protected override void OnClosed(EventArgs e)
        {
            _physicalTimer.Stop();
            _ipTimer.Stop();
            try { vce.VideoCaptureSource = null; } catch { }
            base.OnClosed(e);
        }
    }
}