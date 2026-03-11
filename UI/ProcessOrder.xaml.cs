using Microsoft.Data.SqlClient.Server;
using Microsoft.IdentityModel.Tokens;
using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        BarcodeReader codeReader = new BarcodeReader();
        Order order;
        int invoiceNumber = -1;
        int cooldown = 0;

        public ProcessOrder()
        {
            InitializeComponent();

            // Camera Logic (Original as it is)
            if (!GlobalSettings.CameraName.IsNullOrEmpty())
            {
                vce.VideoCaptureSource = GlobalSettings.CameraName;
                cameraTimer.IsEnabled = true;
                cameraTimer.Interval = TimeSpan.FromMilliseconds(500);
                cameraTimer.Tick += CameraTimer_Tick;
            }

            order = new Order();

            
            ProductDataGrid.AutoGenerateColumns = false;
            ProductDataGrid.ItemsSource = order.Products;
            ProductDataGrid.CanUserAddRows = false;

            DataContext = order;
        }

        private void CameraTimer_Tick(object? sender, EventArgs e)
        {
            if (cooldown > 0)
            {
                cooldown--;
                return;
            }
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)vce.ActualWidth, (int)vce.ActualHeight, 96, 96, PixelFormats.Default);
            vce.Measure(vce.RenderSize);
            vce.Arrange(new Rect(vce.RenderSize));
            bmp.Render(vce);
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                Bitmap btiMap = new Bitmap(ms);
                var result = codeReader.Decode(btiMap);
                if (result != null)
                {
                    order.AddProduct(result.ToString(), 1);
                    cooldown = 60;
                    refreshData();
                }
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = ProductDataGrid.SelectedIndex;
            // Safety check added
            if (selectedIndex != -1)
            {
                order.RemoveProduct(selectedIndex);
                refreshData();
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (productIdField.Text.IsNullOrEmpty() || quantityField.Text.IsNullOrEmpty())
                return;
            order.AddProduct(productIdField.Text, int.Parse(quantityField.Text));
            refreshData();
        }

        private void ProductDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            refreshData();
        }

        private void refreshData()
        {
            ProductDataGrid.ItemsSource = null;
            ProductDataGrid.ItemsSource = order.Products;

            // Re-using your exact property names
            totalLabel.TextData = order.GrandTotal.ToString();
            savedLabel.TextData = order.SavedTotal.ToString();
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (receivedField.Text.IsNullOrEmpty())
                return;

            // Logic original: Payment validation
            if (double.Parse(receivedField.Text) < double.Parse(totalLabel.TextData))
            {
                MessageBox.Show("Insufficient payment amount.", "Payment Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            returnField.Text = (double.Parse(receivedField.Text) - double.Parse(totalLabel.TextData)).ToString();
            order.CustomerName = customerNameField.Text.IsNullOrEmpty() ? null : customerNameField.Text.Trim();

            if (SaveOrder(ref invoiceNumber) == true && invoiceNumber != -1)
            {
                var document = new PrintDocument();
                document.DefaultPageSettings.PaperSize = new PaperSize("Customer Size", 50, 100);
                if (!GlobalSettings.PrinterName.IsNullOrEmpty())
                    document.DefaultPageSettings.PrinterSettings.PrinterName = GlobalSettings.PrinterName;
                document.PrintPage += new PrintPageEventHandler(BillContent);
                document.Print();
                clearOrder();
            }
        }

        public void clearOrder()
        {
            order = new Order();
            invoiceNumber = -1;
            DataContext = order;
            customerNameField.Text = string.Empty;
            refreshData();
        }

        public bool SaveOrder(ref int invoiceNumber)
        {
            if (receivedField.Text.IsNullOrEmpty())
                return false;

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
            objs.Add(("CustomerName", null, SqlDbType.NVarChar, order.CustomerName));

            invoiceNumber = (int)DataHandler.BulkDataExecuteSP("stpInsertOrder", objs);
            return true;
        }

        public void BillContent(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Font font = new Font("Courier New", 10);
            System.Drawing.Brush brush = new SolidBrush(System.Drawing.Color.Black);

            int startX = 0;
            int startY = 0;
            int Offset = 10;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("=========================================================");
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine($"Invoice Number: {invoiceNumber}");
            if (!string.IsNullOrEmpty(order.CustomerName))
                builder.AppendLine($"Customer Name: {order.CustomerName}");
            builder.AppendLine($"Processed By: {Utils.CurrentEmployee.Name}");
            builder.AppendLine($"Dated: {DateTime.Now}");
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine();
            builder.Append("Code".PadRight(12));
            builder.Append("Product".PadRight(12 + 10));
            builder.Append("Q.ty".PadRight(6));
            builder.Append("GST".PadRight(6));
            builder.Append("Total".PadRight(12));
            builder.AppendLine();

            foreach (var item in order.Products)
            {
                builder.Append(item.Code.PadRight(12));
                builder.Append(item.Product.Name.PadRight(12 + 10));
                builder.Append(item.Quantity.ToString().PadRight(6));
                builder.Append(item.Product.Category.GST.ToString().PadRight(6));
                builder.Append($"{item.TotalPrice} Rs".PadRight(12));
                builder.AppendLine();
            }
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine($"Grand Total: {totalLabel.TextData} Rs");
            builder.AppendLine($"Received: {receivedField.Text} Rs");
            builder.AppendLine($"Total Payable: {totalLabel.TextData} Rs");
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine("Thank you for Shopping here!".PadRight(10));

            builder.AppendLine("=========================================================");
            graphics.DrawString("Stationary Shop".PadLeft(25), new Font("Courier New", 18), brush, new PointF(startX, startY + Offset + 10));
            graphics.DrawString(builder.ToString(), font, brush, new PointF(startX, startY + Offset));
        }
    }
}