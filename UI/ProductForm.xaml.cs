using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StationeryStoreManagementSystem.UI
{
    // ── Converter: show "Stock" button only in Edit mode ────────────────
    public class EditVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }

    public partial class ProductForm : AbstractEntryForm, IValidationFields
    {
        // ── Static converter instance referenced from XAML ───────────────
        public static readonly EditVisibilityConverter EditVisConverter
            = new EditVisibilityConverter();

        // ── State ────────────────────────────────────────────────────────
        public Product product;
        private bool _isEdit = false;

        // Two DataTables backing the two grids
        private DataTable _selectedTable;
        private DataTable _availableTable;

        // Which row in _selectedTable is being priced right now
        private int _pricingRowIndex = -1;

        // In-memory price + qty store: supplierId → (cost, retail, discount, qty)
        private readonly Dictionary<int, (double cost, double retail, double discount, int qty)>
            _prices = new Dictionary<int, (double, double, double, int)>();

        // ── Constructor ──────────────────────────────────────────────────
        public ProductForm(ManageEntity callingInstance, int id = -1)
            : base(callingInstance)
        {
            InitializeComponent();

            // Mark the DataGrid tag so the converter knows edit vs add mode
            SuppliersDataGrid.Tag = (id != -1);

            // Dropdowns
            var companies  = CompanyDL.GetCompanies();
            var categories = CategoryDL.GetCategories();
            CompanyField.ItemSource      = companies;
            CompanyField.DisplayPathName = "Name";
            CategoryField.ItemSource      = categories;
            CategoryField.DisplayPathName = "Name";

            // Load all suppliers
            DataTable allSuppliers = SupplierDL.GetSuppliersView();

            if (id != -1)
            {
                // ── EDIT mode ────────────────────────────────────────────
                _isEdit = true;
                titleBlock.Text        = "Edit Product";
                ConfirmButton.Content  = "Update Product";

                product = ProductDL.GetProduct(id);
                if (product.Company  != null) product.Company  = companies.Find(x => x.Id == product.Company.Id);
                if (product.Category != null) product.Category = categories.Find(x => x.Id == product.Category.Id);

                // Split suppliers: already assigned vs still available
                var assignedIds = product.Suppliers.Select(s => s.Id).ToHashSet();

                _selectedTable  = allSuppliers.Clone();
                _availableTable = allSuppliers.Clone();

                foreach (DataRow row in allSuppliers.Rows)
                {
                    int sid = (int)row[0];
                    if (assignedIds.Contains(sid))
                        _selectedTable.Rows.Add(row.ItemArray);
                    else
                        _availableTable.Rows.Add(row.ItemArray);
                }

                // Pre-fill _prices from existing stocks
                if (product.Stocks != null)
                {
                    foreach (var s in product.Stocks)
                    {
                        if (s.Supplier != null)
                            _prices[s.Supplier.Id] = (s.Price, s.RetailPrice, s.DiscountAmount, s.Quantity);
                    }
                }
            }
            else
            {
                // ── ADD mode ─────────────────────────────────────────────
                product         = new Product();
                _selectedTable  = allSuppliers.Clone();   // empty
                _availableTable = allSuppliers.Copy();    // all suppliers
            }

            if (product.Stocks == null) product.Stocks = new List<Stock>();

            // ── Build price display columns on _selectedTable ────────────
            _selectedTable.Columns.Add("Cost Price",   typeof(string));
            _selectedTable.Columns.Add("Retail Price", typeof(string));
            _selectedTable.Columns.Add("Discount",     typeof(string));
            _selectedTable.Columns.Add("Qty",          typeof(string));

            // Refresh price display for pre-filled rows (edit mode)
            foreach (DataRow row in _selectedTable.Rows)
            {
                int sid = (int)row[0];
                if (_prices.TryGetValue(sid, out var p))
                {
                    row["Cost Price"]   = p.cost.ToString("F2");
                    row["Retail Price"] = p.retail.ToString("F2");
                    row["Discount"]     = p.discount.ToString("F2");
                    row["Qty"]          = p.qty.ToString();
                }
                else
                {
                    row["Cost Price"] = row["Retail Price"] = row["Discount"] = row["Qty"] = "—";
                }
            }

            // ── SuppliersDataGrid columns ────────────────────────────────
            AddTextCols(SuppliersDataGrid,
                new[] { "Name", "Cost Price", "Retail Price", "Discount", "Qty" },
                new[] { "Name", "Cost Price", "Retail Price", "Discount", "Qty" });
            SuppliersDataGrid.ItemsSource = _selectedTable.DefaultView;

            // ── AvailableSuppliersGrid columns ───────────────────────────
            AddTextCols(AvailableSuppliersGrid,
                new[] { "Name", "Contact", "Email" },
                new[] { "Name", "Contact", "Email" });
            AvailableSuppliersGrid.ItemsSource = _availableTable.DefaultView;

            DataContext = product;
        }

        private static void AddTextCols(DataGrid dg, string[] headers, string[] bindings)
        {
            for (int i = 0; i < headers.Length; i++)
                dg.Columns.Add(new DataGridTextColumn
                {
                    Header     = headers[i],
                    Binding    = new System.Windows.Data.Binding(bindings[i]),
                    IsReadOnly = true,
                    Width      = new DataGridLength(1, DataGridLengthUnitType.Star)
                });
        }

        // ── Select supplier ──────────────────────────────────────────────
        private void SelectSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableSuppliersGrid.SelectedItem is not DataRowView drv) return;
            DataRow src = drv.Row;

            DataRow newRow = _selectedTable.NewRow();
            for (int i = 0; i < src.Table.Columns.Count; i++)
                newRow[i] = src[i];
            newRow["Cost Price"] = newRow["Retail Price"] = newRow["Discount"] = newRow["Qty"] = "—";
            _selectedTable.Rows.Add(newRow);
            _availableTable.Rows.Remove(src);
        }

        // ── Remove supplier ──────────────────────────────────────────────
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersDataGrid.SelectedItem is not DataRowView drv) return;
            DataRow row = drv.Row;

            DataRow back = _availableTable.NewRow();
            for (int i = 0; i < _availableTable.Columns.Count; i++)
                back[i] = row[i];
            _availableTable.Rows.Add(back);

            _prices.Remove((int)row[0]);
            _selectedTable.Rows.Remove(row);
            PricePanel.Visibility = Visibility.Collapsed;
            _pricingRowIndex = -1;
        }

        // ── Set Price button ─────────────────────────────────────────────
        private void SetPriceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersDataGrid.SelectedItem is not DataRowView drv) return;
            _pricingRowIndex = _selectedTable.Rows.IndexOf(drv.Row);

            PricePanelTitle.Text = $"Set price for: {drv.Row["Name"]}";

            int suppId = (int)drv.Row[0];
            if (_prices.TryGetValue(suppId, out var ex))
            {
                PriceField.TextBoxText.Text       = ex.cost.ToString("F2");
                RetailPriceField.TextBoxText.Text = ex.retail.ToString("F2");
                DiscountField.TextBoxText.Text    = ex.discount.ToString("F2");
                InitialQtyField.TextBoxText.Text  = ex.qty.ToString();
            }
            else
            {
                PriceField.TextBoxText.Text       = "";
                RetailPriceField.TextBoxText.Text = "";
                DiscountField.TextBoxText.Text    = "0";
                InitialQtyField.TextBoxText.Text  = "0";
            }

            PricePanel.Visibility = Visibility.Visible;
            PriceField.TextBoxText.Focus();
        }

        // ── Save Price ───────────────────────────────────────────────────
        private void SavePriceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_pricingRowIndex < 0 || _pricingRowIndex >= _selectedTable.Rows.Count) return;

            if (!double.TryParse(PriceField.TextBoxText.Text, out double cost) || cost <= 0)
            { MessageBox.Show("Enter a valid Cost Price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!double.TryParse(RetailPriceField.TextBoxText.Text, out double retail) || retail <= 0)
            { MessageBox.Show("Enter a valid Retail Price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            double.TryParse(DiscountField.TextBoxText.Text, out double discount);
            int.TryParse(InitialQtyField.TextBoxText.Text, out int qty);

            DataRow row   = _selectedTable.Rows[_pricingRowIndex];
            int suppId    = (int)row[0];
            _prices[suppId] = (cost, retail, discount, qty);

            row["Cost Price"]   = cost.ToString("F2");
            row["Retail Price"] = retail.ToString("F2");
            row["Discount"]     = discount.ToString("F2");
            row["Qty"]          = qty.ToString();

            PricePanel.Visibility = Visibility.Collapsed;
            _pricingRowIndex = -1;
        }

        // ── Edit Stock (edit mode only) ──────────────────────────────────
        private void EditStockButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersDataGrid.SelectedItem is not DataRowView drv) return;
            object[] arr = drv.Row.ItemArray;
            ((Border)Parent).Child = new EditStockForm(this, product, (int)arr[0], (string)arr[1]);
        }

        // ── Confirm / Save ───────────────────────────────────────────────
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors()) return;

            var suppliers = new List<Supplier>();
            var stocks    = new List<Stock>();
            var stockChanges = new List<(int suppId, int qty, string desc)>();

            foreach (DataRow row in _selectedTable.Rows)
            {
                int suppId = (int)row[0];
                suppliers.Add(new Supplier(suppId));

                if (_prices.TryGetValue(suppId, out var p))
                {
                    var sup = new Supplier(suppId);
                    stocks.Add(new Stock(sup, p.cost, p.retail, p.discount, p.qty));
                    if (p.qty != 0)
                        stockChanges.Add((suppId, p.qty, "Initial stock"));
                }
            }

            product.Suppliers = suppliers;
            product.Stocks    = stocks;

            product.Save(!_isEdit);
            ProductDL.SaveStockChanges(product, stockChanges);
            ProductDL.SavePrices(product);

            NavigateCallingForm();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => NavigateCallingForm();

        public bool HasValidationErrors()
        {
            NameField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            CodeField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            scrollViewer.ScrollToTop();
            return Validation.GetHasError(NameField.TextBoxText)
                || Validation.GetHasError(CodeField.TextBoxText);
        }

        private void ExpiryDatePicker_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (product != null && ExpiryDatePicker.SelectedDate.HasValue)
                product.ExpiryDate = ExpiryDatePicker.SelectedDate.Value;
        }

        // ── Legacy compatibility — EditStockForm writes here ────────────
        // Keep this public list so EditStockForm (edit mode) still works
        public List<(int, int, string)> stockChanges { get; } = new List<(int, int, string)>();
    }
}
