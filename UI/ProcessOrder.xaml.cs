using Microsoft.Data.SqlClient.Server;
using Microsoft.IdentityModel.Tokens;
using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPFMediaKit;
using WPFMediaKit.DirectShow.Controls;
using ZXing;
using ZXing.Windows.Compatibility;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace StationeryStoreManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for ProcessOrder.xaml
    /// </summary>
    public partial class ProcessOrder : UserControl
    {
        DispatcherTimer cameraTimer = new DispatcherTimer();
        BarcodeReader codeReader = new BarcodeReader();
        Order order;
        Customer customer;
        int invoiceNumber = -1;
        int cooldown = 0;
        public ProcessOrder()
        {
            InitializeComponent();
            if (!GlobalSettings.CameraName.IsNullOrEmpty())
            {
                vce.VideoCaptureSource = GlobalSettings.CameraName;
                cameraTimer.IsEnabled = true;
                cameraTimer.Interval = new TimeSpan(200);
                cameraTimer.Tick += CameraTimer_Tick;
            }

            order = new Order();
            List<(string, string, bool)> bindings = new List<(string, string, bool)>
            {
                    ("Product Code","Code",true),
                    ("Product Name","Product.Name",true),
                    ("Company Name","Product.Company.Name",true),
                    ("Category", "Product.Category.Name", true),
                    ("Price", "UnitPrice", true),
                    ("Discount", "Discount", true),
                    ("Total", "TotalPrice", true),
                    ("Q.ty", "Quantity", false),
            };
            for (int i = bindings.Count - 1; i >= 0; i--)
            {
                DataGridTextColumn column = new DataGridTextColumn();
                column.Header = bindings[i].Item1;
                column.Binding = new Binding(bindings[i].Item2) { UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                column.IsReadOnly = bindings[i].Item3;
                ProductDataGrid.Columns.Insert(0, column);
            }
            ProductDataGrid.AutoGenerateColumns = false;
            ProductDataGrid.ItemsSource = order.Products;
            ProductDataGrid.CanUserAddRows = false;

            DataContext = order;



            List<(string, string)> bindings2 = new List<(string, string)>
            {
                ("CNIC","CNIC"),
                ("Name","Name"),
                ("Contact","Contact"),
                ("Gender","Gender")
            };
            dataGridView.SetBindings(bindings2);
            dataGridView.SearchAttributes = new List<string>() { "Name" };
            dataGridView.IsSelect = true;
            dataGridView.Refresh(CustomerDL.GetCustomersView());
            dataGridView.SelectButtonClicked += DataGridView_SelectButtonClicked;




            gender_cb.ItemSource = DataHandler.LookupData("Gender");
            gender_cb.DisplayPathName = "Value";
            city_cb.ItemSource = DataHandler.LookupData("CityPakistan");
            city_cb.DisplayPathName = "Value";

            ConfirmButton.Content = "Add";
            titleBlock.Title = "Add Customer";
        }
        private void DataGridView_SelectButtonClicked(DataGrid dataGrid, int selectedIndex)
        {
            object id = ((DataView)(dataGrid.ItemsSource))[selectedIndex].Row.ItemArray[0];
            order.Customer = CustomerDL.GetCustomer((int)id);
            customerLabel.TextData = order.Customer.Name;
            getCustomerGrid.Visibility = Visibility.Collapsed;
            gridMain.Visibility = Visibility.Visible;
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
            order.RemoveProduct(selectedIndex);
            refreshData();
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
            totalLabel.TextData = order.GrandTotal.ToString();
            savedLabel.TextData = order.SavedTotal.ToString();
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (receivedField.Text.IsNullOrEmpty())
                return;
            if ((double.Parse(receivedField.Text) - double.Parse(totalLabel.TextData) < 0))
            {
                if (order.Customer == null)
                    return;
                else
                    returnField.Text = 0.ToString();
            }
            else
                returnField.Text = (double.Parse(receivedField.Text) - double.Parse(totalLabel.TextData)).ToString();
            if (SaveOrder(ref invoiceNumber) == true && invoiceNumber!=-1)
            {
                var document = new PrintDocument();
                document.DefaultPageSettings.PaperSize = new PaperSize("Customer Size", 50, 100);
                if(!GlobalSettings.PrinterName.IsNullOrEmpty())
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
            objs.Add(("CustomerId", null, SqlDbType.Int,order.Customer?.Id));
            if (order.Customer != null)
            {
                if (deductExtraCheckBox.IsChecked == true)
                {
                    double remainingAmount = double.Parse(receivedField.Text) - double.Parse(totalLabel.TextData);
                    if (order.Customer.PaymentDues <= remainingAmount)
                        objs.Add(("PaymentDues", null, SqlDbType.Money, order.Customer.PaymentDues.ToString()));
                    else if (order.Customer.PaymentDues > remainingAmount)
                        objs.Add(("PaymentDues", null, SqlDbType.Money, remainingAmount.ToString()));

                }
                else if((double.Parse(receivedField.Text) - double.Parse(totalLabel.TextData))>0)
                    objs.Add(("PaymentDues", null, SqlDbType.Money, null));
                else
                    objs.Add(("PaymentDues", null, SqlDbType.Money, (-1*Math.Abs(double.Parse(totalLabel.TextData)-double.Parse(receivedField.Text))).ToString()));
            }
            else
                objs.Add(("PaymentDues", null, SqlDbType.Money, null));
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
            if(order.Customer !=null)
                builder.AppendLine($"Customer Name: {order.Customer.Name}");
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
            if(int.Parse(receivedField.Text)<int.Parse(totalLabel.TextData))
                builder.AppendLine($"Total Payable: {receivedField.Text} Rs");
            else
                builder.AppendLine($"Total Payable: {totalLabel.TextData} Rs");
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine("Thank you for Shopping here!".PadRight(10));

            builder.AppendLine("=========================================================");
            graphics.DrawString("Stationary Shop".PadLeft(25), new Font("Courier New", 18), brush, new PointF(startX, startY + Offset+10));
            graphics.DrawString(builder.ToString(), font, brush, new PointF(startX, startY + Offset));
        }

        private void searchCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            gridMain.Visibility = Visibility.Collapsed;
            getCustomerGrid.Visibility = Visibility.Visible;
        }

        private void newCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            gridMain.Visibility = Visibility.Collapsed;
            newCustomerFormGrid.Visibility = Visibility.Visible;
            customer = new Customer();
            newCustomerFormGrid.DataContext = customer;
        }

        private void ConfirmButton_Click_1(object sender, RoutedEventArgs e)
        {
            getCustomerGrid.Visibility = Visibility.Collapsed;
            gridMain.Visibility = Visibility.Visible;
            customer.Save(true);
            order.Customer = customer;
            customerLabel.TextData = order.Customer.Name;
        }

        private void CancelButton_Click_1(object sender, RoutedEventArgs e)
        {
            getCustomerGrid.Visibility = Visibility.Collapsed;
            gridMain.Visibility = Visibility.Visible;
        }
    }
}
