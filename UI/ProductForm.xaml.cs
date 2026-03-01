using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
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
    /// Interaction logic for ProductForm.xaml
    /// </summary>
    public partial class ProductForm : AbstractEntryForm, IValidationFields
    {
        public Product product;
        public List<(int, int, string)> stockChanges;
        private bool isEdit = false;
        public ProductForm(ManageEntity callingInstance, int id = -1) : base(callingInstance)
        {
            InitializeComponent();
            List<Company> companies = CompanyDL.GetCompanies();
            List<Category> categories = CategoryDL.GetCategories();
            CompanyField.ItemSource = companies;
            CompanyField.DisplayPathName = "Name";
            CategoryField.ItemSource = categories;
            CategoryField.DisplayPathName = "Name";
            List<(string, string)> bindings = new List<(string, string)> {
                ("Name","Name"),
                ("Contact","Contact"),
                ("Email","Email"),
                ("Street Address","StreetAddress"),
                ("Town","Town"),
                ("City","City"),
                ("Country","Country"),
                ("Postal Code","PostalCode")};
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            List<string> searchAttributes = new List<string>() { "Name" };
            suppliersDataHandler2.SearchAttributes = searchAttributes;
            suppliersDataHandler2.IsSelect = true;
            suppliersDataHandler2.SetBindings(bindings);
            DataTable table = SupplierDL.GetSuppliersView();
            DataTable table1 = table.Clone();
            suppliersDataHandler2.ItemSource = table.DefaultView;
            suppliersDataHandler2.SelectButtonClicked += SuppliersDataHandler2_SelectButtonClicked;
            if (id != -1)
            {
                titleBlock.Text = "Edit Product";
                ConfirmButton.Content = "Update";
                product = ProductDL.GetProduct(id);
                if (product.Company != null)
                    product.Company = companies.Find(x => x.Id == product.Company.Id);
                if (product.Category != null)
                    product.Category = categories.Find(x => x.Id == product.Category.Id);
                List<int> initialSupplierIds = product.Suppliers.Select(x => x.Id).ToList();
                List<int> indexes = new List<int>();
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    if (initialSupplierIds.Exists(x => x == (int)table.Rows[i].ItemArray[0]))
                    {
                        table1.Rows.Add(table.Rows[i].ItemArray);
                        indexes.Add(i);
                    }
                }
                indexes.Reverse();
                foreach (int index in indexes)
                    table.Rows.RemoveAt(index);
                EditColumn.Visibility = Visibility.Visible;
                AddColumn.Visibility = Visibility.Collapsed;
                isEdit = true;
            }
            else
                product = new Product();
            if (product.Stocks == null)
                product.Stocks = new List<Stock>();
            stockChanges = new List<(int, int, string)>();
            DataContext = product;



            List<(string, string)> bindings2 = new List<(string, string)> {
                ("Name","Name"),
                ("Contact","Contact"),
                ("Email","Email"),
                ("Street Address","StreetAddress"),
                ("Town","Town"),
                ("City","City"),
                ("Country","Country"),
                ("Postal Code","PostalCode")};
            for (int i = bindings.Count - 1; i >= 0; i--)
            {
                DataGridTextColumn column = new DataGridTextColumn();
                column.Header = bindings2[i].Item1;
                column.Binding = new System.Windows.Data.Binding(bindings2[i].Item2);
                column.IsReadOnly = true;
                SuppliersDataGrid.Columns.Insert(0, column);
            }
            SuppliersDataGrid.AutoGenerateColumns = false;
            SuppliersDataGrid.ItemsSource = new List<Product>();
            SuppliersDataGrid.CanUserAddRows = false;
            SuppliersDataGrid.ItemsSource = table1.DefaultView;
        }
        private void SuppliersDataHandler2_SelectButtonClicked(DataGrid dataGrid, int selectedIndex)
        {
            DataRow dataRow = ((DataRowView)dataGrid.SelectedItem).Row;
            ((DataView)SuppliersDataGrid.ItemsSource).Table.Rows.Add(dataRow.ItemArray);
            ((DataView)suppliersDataHandler2.ItemSource).Table.Rows.Remove(dataRow);
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors())
                return;
            List<Supplier> suppliers = new List<Supplier>();
            var filteredChanges = new List<(int, int, string)>();
            var rows = ((DataView)SuppliersDataGrid.ItemsSource).Table.Rows;
            foreach (DataRow row in rows)
            {
                suppliers.Add(new Supplier((int)row.ItemArray[0]));
                var stocksSupplier = stockChanges.Where(x => x.Item1 == (int)row.ItemArray[0]);
                foreach(var item in stocksSupplier)
                {
                    filteredChanges.Add(item);
                }
            }
            product.Suppliers = suppliers;
            product.Stocks = product.Stocks==null? null:product.Stocks.Where(x =>
            {
                if (x.Supplier == null) return false;
                return suppliers.Select(y => y.Id)
            .Contains(x.Supplier.Id);
            }).ToList();
            product.Save(!isEdit);
            ProductDL.SaveStockChanges(product,filteredChanges);
            NavigateCallingForm();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }
        public bool HasValidationErrors()
        {
            NameField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            CodeField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            scrollViewer.ScrollToTop();
            return Validation.GetHasError(NameField.TextBoxText)
                || Validation.GetHasError(CodeField.TextBoxText);
        }

        private void EditPriceButton_Click(object sender, RoutedEventArgs e)
        {
            object[] itemarray = ((DataRowView)SuppliersDataGrid.SelectedItem).Row.ItemArray;
            ((Border)Parent).Child = new ProductSupplierPriceForm(this, product, (int)itemarray[0], (string)itemarray[1]);
        }

        private void EditStockButton_Click(object sender, RoutedEventArgs e)
        {
            object[] itemarray = ((DataRowView)SuppliersDataGrid.SelectedItem).Row.ItemArray;
            ((Border)Parent).Child = new EditStockForm(this, product, (int)itemarray[0], (string)itemarray[1]);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DataRow dataRow = ((DataRowView)SuppliersDataGrid.SelectedItem).Row;
            ((DataView)suppliersDataHandler2.ItemSource).Table.Rows.Add(dataRow.ItemArray);
            ((DataView)SuppliersDataGrid.ItemsSource).Table.Rows.Remove(dataRow);
        }
    }
}