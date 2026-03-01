using Microsoft.Data.SqlClient;
using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using StationeryStoreManagementSystem.UI.Controls;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace StationeryStoreManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for SupplierForm.xaml
    /// </summary>
    public partial class SupplierForm : AbstractEntryForm, IValidationFields
    {
        private Supplier supplier;
        public SupplierForm(ManageEntity callingInstance, int id=-1):base(callingInstance)
        {
            InitializeComponent();
            CountryField.ItemSource = DataHandler.LookupData("Country");
            CountryField.DisplayPathName = "Value";
            List<(string, string)> bindings = new List<(string, string)>()
            {
                ("Product Name","Name"),
                ("Company Name","Company"),
                ("Category","Category"),
                ("Price","Price")
            };
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            productsDataHandler2.SearchAttributes = new List<string>() { "Name" };
            productsDataHandler1.IsDelete = true;
            productsDataHandler2.IsSelect = true;
            productsDataHandler1.SetBindings(bindings);
            productsDataHandler2.SetBindings(bindings);
            DataTable table = ProductDL.GetProducts_View();
            DataTable table1 = table.Clone();
            productsDataHandler2.ItemSource = table.DefaultView;
            productsDataHandler1.ItemSource = table1.DefaultView;
            productsDataHandler2.SelectButtonClicked += ProductsDataHandler2_SelectButtonClicked;
            productsDataHandler1.DeleteButtonClicked += ProductsDataHandler1_DeleteButtonClicked;
            if (id != -1)
            {
                titleBlock.Text = "Edit Supplier";
                ConfirmButton.Content = "Update";
                supplier = SupplierDL.GetSupplier(id);
                CountryField.SelectedValue = ((Dictionary<int, string>)CountryField.ItemSource).Where(x => x.Value == supplier.Country).Select(x=>x.Value).FirstOrDefault();
                if (CountryField.SelectedValue != null)
                {
                    CityField.ItemSource = DataHandler.LookupData($"City{CountryField.SelectedValue}");
                    CityField.DisplayPathName = "Value";
                    CityField.SelectedValue = ((Dictionary<int, string>)CityField.ItemSource).Where(x => x.Value == supplier.City).Select(x => x.Value).FirstOrDefault();
                }
                List<int> initialProductsIds = supplier.Products.Select(x => x.Id).ToList();
                List<int> indexes = new List<int>();
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    if (initialProductsIds.Exists(x => x == (int)table.Rows[i].ItemArray[0]))
                    {
                        table1.Rows.Add(table.Rows[i].ItemArray);
                        indexes.Add(i);
                    }
                }
                indexes.Reverse();
                foreach (int index in indexes)
                    table.Rows.RemoveAt(index);
                isEdit = true;
            }
            else
            {
                supplier = new Supplier();
            }
            DataContext = supplier;
        }

        private void ProductsDataHandler1_DeleteButtonClicked(DataGrid dataGrid, int selectedIndex)
        {
            DataRow dataRow = ((DataRowView)dataGrid.SelectedItem).Row;
            ((DataView)productsDataHandler2.ItemSource).Table.Rows.Add(dataRow.ItemArray);
            ((DataView)productsDataHandler1.ItemSource).Table.Rows.Remove(dataRow);
        }

        private void ProductsDataHandler2_SelectButtonClicked(DataGrid dataGrid, int selectedIndex)
        {
            DataRow dataRow = ((DataRowView)dataGrid.SelectedItem).Row;
            ((DataView)productsDataHandler1.ItemSource).Table.Rows.Add(dataRow.ItemArray);
            ((DataView)productsDataHandler2.ItemSource).Table.Rows.Remove(dataRow);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors())
                return;
            List<Product> products = new List<Product>();
            var rows = ((DataView)productsDataHandler1.ItemSource).Table.Rows;
            foreach (DataRow row in rows)
            {
                products.Add(new Product((int)row.ItemArray[0]));
            }
            supplier.Products = products;
            try
            {
                supplier.Save(!isEdit);
                NavigateCallingForm();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    UI.Components.MessageBox.Show($"Supplier Code \"{supplier.Code}\" already exists", "Error", UI.Components.MessageBox.Type.Message);
                }
                else
                    throw ex;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }

        private void CountryField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxEntry)sender).ComboBox1.SelectedValue != null)
            {
                CityField.ItemSource = DataHandler.LookupData($"City{((ComboBoxEntry)sender).ComboBox1.SelectedValue}");
                CityField.DisplayPathName = "Value";
            }
        }
        public bool HasValidationErrors()
        {
            NameField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            CodeField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            ContactField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            scrollViewer.ScrollToTop();
            return Validation.GetHasError(NameField.TextBoxText)
                || Validation.GetHasError(CodeField.TextBoxText)
                || Validation.GetHasError(ContactField.TextBoxText);
        }
    }
}
