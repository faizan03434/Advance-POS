using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using StationeryStoreManagementSystem.UI;
using StationeryStoreManagementSystem.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace StationeryStoreManagementSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var handle = new WindowInteropHelper(this).Handle;
            if (DwmSetWindowAttribute(handle, 19, new[] { 1 }, 4) != 0)
                DwmSetWindowAttribute(handle, 20, new[] { 1 }, 4);
        }
        private List<SideButton> AdminBtns = new List<SideButton>()
        {
            new SideButton() { Content = "Dashboard"},
            new SideButton() { Content = "Process Order"},
            new SideButton() { Content = "Manage Companies" },
            new SideButton() { Content = "Manage Categories" },
            new SideButton() { Content = "Manage Products" },
            new SideButton() { Content = "Manage Suppliers" },
            new SideButton() { Content = "Manage Shipments" },
            new SideButton() { Content = "Manage Employees" },
            new SideButton() { Content = "Manage Customers" },
            new SideButton() { Content = "Manage Notifications" },
            new SideButton() { Content = "Manage Repayments"},
            new SideButton() { Content = "Profile"},
            new SideButton() { Content = "Settings"}
        };
        private List<SideButton> CashierBtns = new List<SideButton>()
        {
            new SideButton() { Content = "Dashboard"},
            new SideButton() { Content = "Process Order"},
            new SideButton() { Content = "Manage Customers"},
            new SideButton() { Content = "Manage Repayments"},
            new SideButton() { Content = "Profile"},
            new SideButton() { Content = "Settings"}
        };

        public MainWindow()
        {
            InitializeComponent();
            Utils.ExecuteQuery("SELECT 1");
            ProductDL.GenerateBarcodes();
            Utils.CurrentMainWindow = this;
            GlobalSettings.LoadSettings();
            InitializeLogin();
        }
        private void ManageSuppliersButton_Click(object sender, RoutedEventArgs e)
        {
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("Name","Name"),
                ("Contact","Contact"),
                ("Email","Email"),
                ("Street Address","StreetAddress"),
                ("Town","Town"),
                ("City","City"),
                ("Country","Country"),
                ("Postal Code","PostalCode")
             };
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            Content.Child = new UI.ManageEntity("Manage Suppliers",
                                                typeof(Supplier).Name,
                                                SupplierDL.GetSuppliersView,
                                                bindings,
                                                new List<string> { "Name" },
                                                typeof(SupplierForm),
                                                true,
                                                true,
                                                SupplierDL.DeleteSupplier);
        }

        private void ManageCompaniesButton_Click(object sender, RoutedEventArgs e)
        {
            // Content.Child = new UI.ManageCompanies();
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("Company Name","Name")
             };
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            Content.Child = new UI.ManageEntity("Manage Companies",
                                                typeof(Company).Name,
                                                CompanyDL.GetCompanies_View,
                                                bindings,
                                                new List<string> { "Name" },
                                                typeof(CompanyForm),
                                                true,
                                                true,
                                                CompanyDL.DeleteCompany);
        }

        private void ManageCategoriesButton_Click(Object sender, RoutedEventArgs e)
        {
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("Name","Name"),
                ("GST", "GST")
             };
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            Content.Child = new UI.ManageEntity("Manage Categories",
                                                typeof(Category).Name,
                                                CategoryDL.GetCategories_View,
                                                bindings,
                                                new List<string> { "Name" },
                                                typeof(CategoryForm),
                                                true,
                                                true,
                                                CategoryDL.DeleteCategory);
        }

        private void ManageEmployeesButton_Click(Object sender, RoutedEventArgs e)
        {
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("Username","Username"),
                ("Name","Name"),
                ("CNIC","CNIC"),
                ("Contact","Contact"),
                ("Role","Role"),
                ("Gender","Gender")
            };
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            Content.Child = new UI.ManageEntity("Manage Employees",
                                                typeof(Employee).Name,
                                                EmployeeDL.GetEmployeessView,
                                                bindings,
                                                new List<string> { "Name" },
                                                typeof(EmployeeForm),
                                                true,
                                                true,
                                                EmployeeDL.DeleteEmployee);
        }

        private void ManageProductButton_Click(object sender, RoutedEventArgs e)
        {
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("Name","Name"),
                ("Code","Code"),
                ("Price","Price"),
                ("Retail Rs","RetailPrice"),
                ("Discount Rs","DiscountAmount"),
                ("Company","Company"),
                ("Category","Category"),
                ("No. Suppliers","[No. Suppliers]"),
                ("Q.ty","Stock")
             };
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            Content.Child = new UI.ManageEntity("Manage Products",
                                                typeof(Product).Name,
                                                ProductDL.GetProducts_View,
                                                bindings,
                                                new List<string> { "Name" },
                                                typeof(ProductForm),
                                                true,
                                                true,
                                                ProductDL.DeleteProduct);
        }

        private void ManageShipmentsButton_Click(object sender, RoutedEventArgs e)
        {
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("Name","Name"),
                ("Timestamp","AddedOn"),
             };
            Content.Child = new UI.ManageEntity("Manage Shipments",
                                                "Shipment",
                                                ShipmentDL.GetShipmentsView,
                                                bindings,
                                                new List<string> { "Name" },
                                                typeof(ShipmentForm),
                                                true,
                                                false,
                                                null,
                                                typeof(ViewShipment));
        }

        private void ManageCustomersButton_Click(object sender, RoutedEventArgs e)
        {
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("CNIC","CNIC"),
                ("Name","Name"),
                ("Contact","Contact"),
                ("Gender","Gender")
            };
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            Content.Child = new UI.ManageEntity("Manage Customers",
                                                typeof(Customer).Name,
                                                CustomerDL.GetCustomersView,
                                                bindings,
                                                new List<string> { "Name" },
                                                typeof(CustomerForm),
                                                true,
                                                true);
        }

        private void ManageNotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("From","From"),
                ("IsViewed","IsViewed"),
                ("Notification","Notification")
            };
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            Content.Child = new UI.ManageEntity("Manage Notifications",
                                                typeof(Notification).Name,
                                                NotificationDL.GetNotifications_View,
                                                bindings,
                                                new List<string> { "From" },
                                                typeof(NotificationForm),
                                                true,
                                                false,
                                                null,
                                                typeof(ViewNotification));

        }
        private void ManageRepaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("Customer Name","Customer Name"),
                ("CNIC","CNIC"),
                ("Contact","Contact"),
                ("Gender", "Gender"),
                ("Registered On","Registered On"),
                ("Total Pending","Total Pending")
            };
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            Content.Child = new UI.ManageEntity("Accounts",
                                                "PaymentDues",
                                                CustomerDL.GetRepaymentsView,
                                                bindings,
                                                new List<string> { "Customer Name" },
                                                typeof(RepaymentForm),
                                                false,
                                                true);
        }
        private void InitializeLogin()
        {
            sideBar.Children.Clear();
            sideBar.Visibility = Visibility.Collapsed;
            col1.Width = new GridLength(0, GridUnitType.Star);
            Login login = new Login();
            Content.Child = login;
            login.LoginClicked += SetButtons;
        }

        private void SetButtons(object sender, EventArgs e)
        {
            sideBar.Visibility = Visibility.Visible;
            if (Utils.CurrentEmployee is Admin)
            {
                foreach (Button btn in AdminBtns)
                {
                    sideBar.Children.Add(btn);
                    btn.Click += ButtonClick;
                }
            }
            else if (Utils.CurrentEmployee is Cashier)
            {
                foreach (Button btn in CashierBtns)
                {
                    sideBar.Children.Add(btn);
                    btn.Click += ButtonClick;
                }
            }
            Content.Child = new Dashboard();
        }
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            switch (btn.Content.ToString())
            {
                case "Manage Suppliers":
                    ManageSuppliersButton_Click(sender, e);
                    break;
                case "Manage Companies":
                    ManageCompaniesButton_Click(sender, e);
                    break;
                case "Manage Categories":
                    ManageCategoriesButton_Click(sender, e);
                    break;
                case "Manage Employees":
                    ManageEmployeesButton_Click(sender, e);
                    break;
                case "Manage Products":
                    ManageProductButton_Click(sender, e);
                    break;
                case "Manage Shipments":
                    ManageShipmentsButton_Click(sender, e);
                    break;
                case "Manage Customers":
                    ManageCustomersButton_Click(sender, e);
                    break;
                case "Manage Notifications":
                    ManageNotificationsButton_Click(sender, e);
                    break;
                case "Manage Repayments":
                    ManageRepaymentsButton_Click(sender, e);
                    break;
                case "Settings":
                    SettingsButton_Click(sender, e);
                    break;
                case "Process Order":
                    ProcessOrderButton_Click(sender, e);
                    break;
                case "Dashboard":
                    DashboardButton_Click(sender, e);
                    break;
                case "Profile":
                    ProfileButton_Click(sender, e);
                    break;
            }
        }

        private void ProcessOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Content.Child = new ProcessOrder();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Content.Child = new Settings();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            Content.Child = new Dashboard();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = new Profile();
            Content.Child = profile;
            profile.LogoutClicked += Profile_LogoutClicked;
        }

        private void Profile_LogoutClicked(object? sender, EventArgs e)
        {
            sideBar.Children.Clear();
            sideBar.Visibility = Visibility.Collapsed;
            col1.Width = new GridLength(0, GridUnitType.Star);
            Login login = new Login();
            Content.Child = login;
            login.LoginClicked += SetButtons;
        }
    }
}
