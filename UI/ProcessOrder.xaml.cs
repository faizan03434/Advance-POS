using Microsoft.Data.SqlClient.Server;
using Microsoft.IdentityModel.Tokens;
using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using StationeryStoreManagementSystem.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPFMediaKit.DirectShow.Controls;
using ZXing;
using ZXing.Windows.Compatibility;

namespace StationeryStoreManagementSystem.UI
{
    public partial class ProcessOrder : UserControl
    {
        DispatcherTimer cameraTimer = new DispatcherTimer();
        DispatcherTimer ipWebcamTimer = new DispatcherTimer();
        BarcodeReader codeReader = new BarcodeReader();
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        Order order;
        int invoiceNumber = -1;
        int cooldown = 0;

        public ProcessOrder()
        {
            InitializeComponent();

            order = new Order();
            ProductDataGrid.AutoGenerateColumns = false;
            ProductDataGrid.ItemsSource = order.Products;
            ProductDataGrid.CanUserAddRows = false;
            DataContext = order;

            // Physical USB/built-in camera (used in shop with real scanner machine)
            if (!GlobalSettings.UseIpWebcam && !GlobalSettings.CameraName.IsNullOrEmpty())
            {
                vce.VideoCaptureSource = GlobalSettings.CameraName;
                cameraTimer.IsEnabled = true;
                cameraTimer.Interval = TimeSpan.FromMilliseconds(300);
                cameraTimer.Tick += CameraTimer_Tick;
                cameraStatusText.Text = "Camera: " + GlobalSettings.CameraName;
            }
            // IP Webcam mode (mobile phone as barcode reader for testing)
            else if (GlobalSettings.UseIpWebcam && !GlobalSettings.IpWebcamUrl.IsNullOrEmpty())
            {
                vce.Visibility = Visibility.Collapsed;
                ipWebcamImage.Visibility = Visibility.Visible;
                ipWebcamTimer.Interval = TimeSpan.FromMilliseconds(400);
                ipWebcamTimer.Tick += IpWebcamTimer_Tick;
                ipWebcamTimer.Start();
                cameraStatusText.Text = "IP Cam: " + GlobalSettings.IpWebcamUrl;
            }
            else
            {
                cameraStatusText.Text = "No camera configured";
            }

            // Set focus to product ID field
            Loaded += (s, e) => productIdField.Focus();
        }

        // ===== KEYBOARD SHORTCUTS =====
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Delete && ProductDataGrid.SelectedIndex != -1)
            {
                order.RemoveProduct(ProductDataGrid.SelectedIndex);
                RefreshData();
                e.Handled = true;
            }
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                confirmButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        // Tab order: Product ID -> Qty -> Add -> Customer -> Received -> Confirm
        private void productIdField_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) quantityField.Focus();
        }

        private void quantityField_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddProductFromFields();
                customerNameField.Focus();
            }
        }

        private void customerNameField_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) receivedField.Focus();
        }

        private void receivedField_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) confirmButton_Click(sender, e);
        }

        // ===== CAMERA TICK (built-in/USB cam - for shop deployment) =====
        private void CameraTimer_Tick(object? sender, EventArgs e)
        {
            if (cooldown > 0) { cooldown--; return; }
            try
            {
                RenderTargetBitmap bmp = new RenderTargetBitmap((int)vce.ActualWidth, (int)vce.ActualHeight, 96, 96, PixelFormats.Default);
                vce.Measure(vce.RenderSize);
                vce.Arrange(new Rect(vce.RenderSize));
                bmp.Render(vce);
                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                using MemoryStream ms = new MemoryStream();
                encoder.Save(ms);
                Bitmap btiMap = new Bitmap(ms);
                var result = codeReader.Decode(btiMap);
                if (result != null)
                {
                    AddProductByBarcode(result.ToString());
                    cooldown = 40;
                }
            }
            catch { }
        }

        // ===== IP WEBCAM TICK (mobile phone for testing) =====
        private async void IpWebcamTimer_Tick(object? sender, EventArgs e)
        {
            if (cooldown > 0) { cooldown--; return; }
            try
            {
                string url = GlobalSettings.IpWebcamUrl!.TrimEnd('/') + "/shot.jpg";
                var bytes = await _httpClient.GetByteArrayAsync(url);

                // Show frame in UI
                using var ms = new MemoryStream(bytes);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                ipWebcamImage.Source = bitmapImage;

                // Decode barcode
                using var bmp = new Bitmap(new MemoryStream(bytes));
                var result = codeReader.Decode(bmp);
                if (result != null)
                {
                    AddProductByBarcode(result.ToString());
                    cooldown = 50;
                }
            }
            catch { }
        }

        private void AddProductByBarcode(string code)
        {
            order.AddProduct(code.Trim(), 1);
            RefreshData();
            // Flash product ID field to show scan
            Dispatcher.Invoke(() => {
                productIdField.Text = code.Trim();
                productIdField.SelectAll();
            });
        }

        // ===== MANUAL ENTRY (FIXED - was commented out) =====
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductFromFields();
        }

        private void AddProductFromFields()
        {
            string pid = productIdField.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(pid)) return;

            int qty = 1;
            if (!string.IsNullOrEmpty(quantityField.Text))
                int.TryParse(quantityField.Text, out qty);
            if (qty <= 0) qty = 1;

            order.AddProduct(pid, qty);
            productIdField.Text = string.Empty;
            quantityField.Text = "1";
            productIdField.Focus();
            RefreshData();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = ProductDataGrid.SelectedIndex;
            if (selectedIndex != -1)
            {
                order.RemoveProduct(selectedIndex);
                RefreshData();
            }
        }

        private void ProductDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            ProductDataGrid.ItemsSource = null;
            ProductDataGrid.ItemsSource = order.Products;
            totalLabel.TextData = order.GrandTotal.ToString("F2");
            savedLabel.TextData = order.SavedTotal.ToString("F2");
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (order.Products.Count == 0) { ShowMsg("No products in order."); return; }
            if (receivedField.Text.IsNullOrEmpty()) { ShowMsg("Enter cash received amount."); return; }

            if (!double.TryParse(receivedField.Text, out double received))
            { ShowMsg("Invalid cash amount."); return; }

            if (received < order.GrandTotal)
            { ShowMsg($"Insufficient payment. Total is Rs. {order.GrandTotal:F2}"); return; }

            returnField.Text = (received - order.GrandTotal).ToString("F2");
            order.CustomerName = customerNameField.Text.IsNullOrEmpty() ? null : customerNameField.Text.Trim();

            if (SaveOrder(ref invoiceNumber) && invoiceNumber != -1)
            {
                PrintBill();
                // Trigger stock alert check after sale
                _ = AlertService.CheckAfterSaleAsync();
                clearOrder();
            }
        }

        private void ShowMsg(string msg) =>
            System.Windows.MessageBox.Show(msg, "POS", MessageBoxButton.OK, MessageBoxImage.Information);

        public void clearOrder()
        {
            cameraTimer.Stop();
            ipWebcamTimer.Stop();
            order = new Order();
            invoiceNumber = -1;
            DataContext = order;
            customerNameField.Text = string.Empty;
            receivedField.Text = string.Empty;
            returnField.Text = string.Empty;
            RefreshData();
            cameraTimer.Start();
            if (GlobalSettings.UseIpWebcam) ipWebcamTimer.Start();
            productIdField.Focus();
        }

        public bool SaveOrder(ref int invoiceNumber)
        {
            if (receivedField.Text.IsNullOrEmpty()) return false;
            var objs = new List<(string, string, SqlDbType, object)>();
            SqlMetaData[] sqlMetas = new SqlMetaData[]
            {
                new SqlMetaData("ProductId",SqlDbType.Int),
                new SqlMetaData("SupplierId",SqlDbType.Int),
                new SqlMetaData("Price",SqlDbType.Money),
                new SqlMetaData("DiscountAmount",SqlDbType.Money),
                new SqlMetaData("TaxAmount",SqlDbType.Money),
                new SqlMetaData("Quantity",SqlDbType.Int),
            };
            var products = order.Products.Select(x =>
            {
                SqlDataRecord record = new SqlDataRecord(sqlMetas);
                record.SetInt32(0, x.Product.Id);
                record.SetInt32(1, x.Supplier.Id);
                record.SetSqlMoney(2, (System.Data.SqlTypes.SqlMoney)x.UnitPrice);
                record.SetSqlMoney(3, (System.Data.SqlTypes.SqlMoney)x.Discount);
                record.SetSqlMoney(4, (System.Data.SqlTypes.SqlMoney)x.Tax);
                record.SetInt32(5, x.Quantity);
                return record;
            });
            objs.Add(("OrderProducts", "udtt_OrderProducts", SqlDbType.Structured, products));
            objs.Add(("EmployeeId", null, SqlDbType.Int, Utils.CurrentEmployee.Id));
            objs.Add(("CustomerName", null, SqlDbType.NVarChar, (object?)order.CustomerName ?? DBNull.Value));
            invoiceNumber = (int)DataHandler.BulkDataExecuteSP("stpInsertOrder", objs);
            return true;
        }

        private void PrintBill()
        {
            var document = new PrintDocument();
            document.DefaultPageSettings.PaperSize = new PaperSize("Customer Size", 300, 600);
            if (!GlobalSettings.PrinterName.IsNullOrEmpty())
                document.DefaultPageSettings.PrinterSettings.PrinterName = GlobalSettings.PrinterName;
            document.PrintPage += BillContent;
            document.Print();
        }

        public void BillContent(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Courier New", 10);
            Font boldFont = new Font("Courier New", 12, System.Drawing.FontStyle.Bold);
            System.Drawing.Brush brush = new SolidBrush(System.Drawing.Color.Black);
            int x = 5, y = 10;

            g.DrawString("=== STATIONARY SHOP ===", boldFont, brush, x, y); y += 20;
            g.DrawString($"Invoice: #{invoiceNumber}", font, brush, x, y); y += 15;
            if (!string.IsNullOrEmpty(order.CustomerName))
            { g.DrawString($"Customer: {order.CustomerName}", font, brush, x, y); y += 15; }
            g.DrawString($"Cashier: {Utils.CurrentEmployee.Name}", font, brush, x, y); y += 15;
            g.DrawString($"Date: {DateTime.Now:dd-MMM-yyyy HH:mm}", font, brush, x, y); y += 15;
            g.DrawString(new string('-', 38), font, brush, x, y); y += 12;

            foreach (var item in order.Products)
            {
                string line = $"{item.Product.Name.PadRight(20).Substring(0, 20)} x{item.Quantity}";
                g.DrawString(line, font, brush, x, y); y += 13;
                g.DrawString($"  Rs.{item.TotalPrice:F2}", font, brush, x, y); y += 13;
            }
            g.DrawString(new string('-', 38), font, brush, x, y); y += 12;
            g.DrawString($"Total:    Rs.{order.GrandTotal:F2}", boldFont, brush, x, y); y += 18;
            g.DrawString($"Received: Rs.{receivedField.Text}", font, brush, x, y); y += 15;
            g.DrawString($"Change:   Rs.{returnField.Text}", font, brush, x, y); y += 15;
            g.DrawString(new string('-', 38), font, brush, x, y); y += 12;
            g.DrawString("Thank you for shopping!", font, brush, x, y);
        }
    }
}
