using Microsoft.IdentityModel.Tokens;
using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StationeryStoreManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for ShipmentForm.xaml
    /// </summary>
    public partial class ShipmentForm : AbstractEntryForm
    {
        private int supplierSelectionIndex = -1;
        public ShipmentForm(ManageEntity callingInstance,int _):base(callingInstance)
        {
            InitializeComponent();
            SupplierDataViewer.SetBindings(
                new List<(string, string)>
                { ("Name","Name"),
                  ("Contact","Contact"),
                  ("Email","Email")});
            SupplierDataViewer.SearchAttributes = new List<string> { "Name" };
            SupplierDataViewer.ItemSource = SupplierDL.GetSuppliersView().DefaultView;
            SupplierDataViewer.IsSelect = true;
            SupplierDataViewer.SelectButtonClicked += SupplierDataViewer_SelectButtonClicked;
            ProductDataViewer.IsSelect = true;
            ProductDataViewer.SearchAttributes = new List<string> { "Name" };
            ProductDataViewer.SetBindings(
                new List<(string, string)>
                {
                    ("Product Name","Name"),
                    ("Company Name","Company.Name"),
                    ("Category","Category.Name"),
                });
            ProductDataViewer.SelectButtonClicked += ProductDataViewer_SelectButtonClicked;
            List<(string, string,bool)> bindings = new List<(string, string,bool)>
            {
                    ("Product Name","Name",true),
                    ("Company Name","Company.Name",true),
                    ("Category", "Category.Name", true),
                    ("Quantity", "SupplierQuantity", false),
            };
            for (int i = bindings.Count - 1; i >= 0; i--)
            {
                DataGridTextColumn column = new DataGridTextColumn();
                column.Header = bindings[i].Item1;
                column.Binding = new System.Windows.Data.Binding(bindings[i].Item2);
                column.IsReadOnly = bindings[i].Item3;
                ProductDataGrid.Columns.Insert(0, column);
            }
            ProductDataGrid.AutoGenerateColumns = false;
            ProductDataGrid.ItemsSource = new List<Product>();
            ProductDataGrid.CanUserAddRows = false;
        }

        private void ProductDataViewer_SelectButtonClicked(DataGrid dataGrid, int selectedIndex)
        {
            List<Product> products = (List<Product>)dataGrid.ItemsSource;
            List<Product> products2 = (List<Product>)ProductDataGrid.ItemsSource;
            Product product = products[selectedIndex];
            products.Remove(product);
            products2.Add(product);
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = products;
            ProductDataGrid.ItemsSource = null;
            ProductDataGrid.ItemsSource = products2;
        }

        private void SupplierDataViewer_SelectButtonClicked(DataGrid dataGrid, int selectedIndex)
        {
            supplierSelectionIndex = selectedIndex;
            object[] row = (object[])((DataView)dataGrid.ItemsSource)[selectedIndex].Row.ItemArray;
            SupplierNameLabel.TextData = (string)row[1];
            ProductDataGrid.ItemsSource = new List<Product>();
            ProductDataViewer.datagrid.ItemsSource = ProductDL.GetSupplierProducts((int)row[0]);
        }

        private void ProductDataGridRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = ProductDataGrid.SelectedIndex;
            List<Product> products = (List<Product>)ProductDataGrid.ItemsSource;
            List<Product> products2 = (List<Product>)ProductDataViewer.datagrid.ItemsSource;
            Product product = products[selectedIndex];
            products.Remove(product);
            products2.Add(product);
            ProductDataGrid.ItemsSource = null;
            ProductDataGrid.ItemsSource = products;
            ProductDataViewer.datagrid.ItemsSource = null;
            ProductDataViewer.datagrid.ItemsSource = products2;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (SupplierNameLabel.TextData.IsNullOrEmpty())
                return;
            int SupplierId = (int)((DataView)SupplierDataViewer.datagrid.ItemsSource).Table.Rows[supplierSelectionIndex].ItemArray[0];
            List<Product> products = (List<Product>)ProductDataGrid.ItemsSource;
            ShipmentDL.AddShipment(SupplierId, products);
            NavigateCallingForm();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }
    }
}
