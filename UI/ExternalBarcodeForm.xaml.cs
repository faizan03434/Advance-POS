using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Windows.Compatibility;

namespace StationeryStoreManagementSystem.UI
{
    public partial class ExternalBarcodeForm : AbstractEntryForm, IValidationFields
    {
        // ── State ───────────────────────────────────────────────────────
        private Product _product;
        private Bitmap _capturedBarcodeImage;

        // Two DataTables that back the two grids
        private DataTable _selectedTable;    // suppliers already chosen
        private DataTable _availableTable;   // suppliers still available

        // Track which row in _selectedTable is being priced right now
        private int _pricingRowIndex = -1;

        // In-memory price store: supplierId → (cost, retail, discount, qty)
        private readonly Dictionary<int, (double cost, double retail, double discount, int qty)>
            _prices = new Dictionary<int, (double, double, double, int)>();

        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(6)
        };

        // ── Constructor ─────────────────────────────────────────────────
        public ExternalBarcodeForm(ManageEntity callingInstance) : base(callingInstance)
        {
            InitializeComponent();
            _product = new Product();

            // Dropdowns
            var companies = CompanyDL.GetCompanies();
            var categories = CategoryDL.GetCategories();
            CompanyField.ItemSource = companies;
            CompanyField.DisplayPathName = "Name";
            CategoryField.ItemSource = categories;
            CategoryField.DisplayPathName = "Name";

            // Load all suppliers from DB
            DataTable allSuppliers = SupplierDL.GetSuppliersView();

            // _selectedTable  — starts empty, same schema
            _selectedTable = allSuppliers.Clone();
            // _availableTable — starts with all suppliers
            _availableTable = allSuppliers.Copy();

            // ── Build text columns for SuppliersDataGrid (selected) ────
            string[] priceHeaders = { "Cost Price", "Retail Price", "Discount", "Qty" };
            foreach (var h in priceHeaders)
                _selectedTable.Columns.Add(h, typeof(string));

            AddTextColumns(SuppliersDataGrid,
                new[] { "Name", "Contact" },
                new[] { "Name", "Contact" });
            AddTextColumns(SuppliersDataGrid,
                priceHeaders, priceHeaders);

            // ── Build text columns for AvailableSuppliersGrid ──────────
            AddTextColumns(AvailableSuppliersGrid,
                new[] { "Name", "Contact", "Email", "City" },
                new[] { "Name", "Contact", "Email", "City" });

            SuppliersDataGrid.ItemsSource = _selectedTable.DefaultView;
            AvailableSuppliersGrid.ItemsSource = _availableTable.DefaultView;

            DataContext = _product;
        }

        // Helper: add read-only text columns to a DataGrid
        private static void AddTextColumns(DataGrid dg,
            string[] headers, string[] bindings)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                dg.Columns.Add(new DataGridTextColumn
                {
                    Header = headers[i],
                    Binding = new System.Windows.Data.Binding(bindings[i]),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                });
            }
        }

        // ── Select supplier from available list ─────────────────────────
        private void SelectSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableSuppliersGrid.SelectedItem is not DataRowView drv) return;
            DataRow src = drv.Row;

            // Add to selected table (price columns start empty)
            DataRow newRow = _selectedTable.NewRow();
            for (int i = 0; i < src.Table.Columns.Count; i++)
                newRow[i] = src[i];
            newRow["Cost Price"] = "—";
            newRow["Retail Price"] = "—";
            newRow["Discount"] = "—";
            newRow["Qty"] = "—";
            _selectedTable.Rows.Add(newRow);

            // Remove from available
            _availableTable.Rows.Remove(src);
        }

        // ── Remove supplier from selected list ──────────────────────────
        private void RemoveSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersDataGrid.SelectedItem is not DataRowView drv) return;
            DataRow row = drv.Row;

            // Put back in available (only original columns)
            DataRow back = _availableTable.NewRow();
            for (int i = 0; i < _availableTable.Columns.Count; i++)
                back[i] = row[i];
            _availableTable.Rows.Add(back);

            // Remove price entry
            int suppId = (int)row[0];
            _prices.Remove(suppId);

            _selectedTable.Rows.Remove(row);

            // Hide price panel if it was open for this row
            PricePanel.Visibility = Visibility.Collapsed;
            _pricingRowIndex = -1;
        }

        // ── Set Price button ────────────────────────────────────────────
        private void SetPriceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersDataGrid.SelectedItem is not DataRowView drv) return;
            _pricingRowIndex = _selectedTable.Rows.IndexOf(drv.Row);

            string supplierName = drv.Row["Name"]?.ToString() ?? "";
            PricePanelTitle.Text = $"Set price for: {supplierName}";

            // Pre-fill if already set
            int suppId = (int)drv.Row[0];
            if (_prices.TryGetValue(suppId, out var existing))
            {
                PriceField.TextBoxText.Text = existing.cost.ToString("F2");
                RetailPriceField.TextBoxText.Text = existing.retail.ToString("F2");
                DiscountField.TextBoxText.Text = existing.discount.ToString("F2");
                InitialQtyField.TextBoxText.Text = existing.qty.ToString();
            }
            else
            {
                PriceField.TextBoxText.Text = "";
                RetailPriceField.TextBoxText.Text = "";
                DiscountField.TextBoxText.Text = "0";
                InitialQtyField.TextBoxText.Text = "0";
            }

            PricePanel.Visibility = Visibility.Visible;
            PriceField.TextBoxText.Focus();
        }

        // ── Save Price ──────────────────────────────────────────────────
        private void SavePriceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_pricingRowIndex < 0 ||
                _pricingRowIndex >= _selectedTable.Rows.Count) return;

            if (!double.TryParse(PriceField.TextBoxText.Text, out double cost) || cost <= 0)
            {
                MessageBox.Show("Enter a valid Cost Price.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!double.TryParse(RetailPriceField.TextBoxText.Text, out double retail) || retail <= 0)
            {
                MessageBox.Show("Enter a valid Retail Price.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            double.TryParse(DiscountField.TextBoxText.Text, out double discount);
            int.TryParse(InitialQtyField.TextBoxText.Text, out int qty);

            DataRow row = _selectedTable.Rows[_pricingRowIndex];
            int suppId = (int)row[0];

            // Store in dictionary
            _prices[suppId] = (cost, retail, discount, qty);

            // Update display columns
            row["Cost Price"] = cost.ToString("F2");
            row["Retail Price"] = retail.ToString("F2");
            row["Discount"] = discount.ToString("F2");
            row["Qty"] = qty.ToString();

            PricePanel.Visibility = Visibility.Collapsed;
            _pricingRowIndex = -1;
        }

        // ── Camera scan ─────────────────────────────────────────────────
        private void ScanBarcodeBtn_Click(object sender, RoutedEventArgs e)
        {
            var win = new BarcodeScanOverlay();
            win.Owner = Window.GetWindow(this);
            bool? result = win.ShowDialog();
            if (result == true && !string.IsNullOrEmpty(win.ScannedValue))
            {
                _capturedBarcodeImage = win.CapturedBarcodeImage;
                ApplyScannedBarcode(win.ScannedValue);
            }
        }

        private void ApplyScannedBarcode(string barcode)
        {
            barcode = barcode.Trim();
            _product.ExternalBarcode = barcode;
            ExternalBarcodeField.TextBoxText.Text = barcode;
            ShowBarcodePreview(barcode);
            _ = RunLookupAsync(barcode);
        }

        // ── Manual lookup ───────────────────────────────────────────────
        private void LookupBarcodeBtn_Click(object sender, RoutedEventArgs e)
        {
            string raw = ExternalBarcodeField.TextBoxText.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(raw))
            {
                SetLookupStatus("Please enter or scan a barcode first.", "#DC2626");
                return;
            }
            _product.ExternalBarcode = raw;
            ShowBarcodePreview(raw);
            _ = RunLookupAsync(raw);
        }

        // ── Online lookup ───────────────────────────────────────────────
        private async Task RunLookupAsync(string barcode)
        {
            Product existing = ProductDL.GetProductByExternalBarcode(barcode);
            if (existing != null)
            {
                SetLookupStatus(
                    $"⚠  Already registered as \"{existing.Name}\" (Code: {existing.Code}).",
                    "#DC2626");
                return;
            }

            SetLookupStatus("🔍  Looking up online...", "#3B82F6");
            LookupBarcodeBtn.IsEnabled = false;
            ScanBarcodeBtn.IsEnabled = false;

            try
            {
                BarcodeProductInfo info = null;
                if (IsNumericBarcode(barcode))
                    info = await FetchFromOpenFoodFactsAsync(barcode);

                if (info != null)
                {
                    if (!string.IsNullOrEmpty(info.Name) &&
                        string.IsNullOrWhiteSpace(_product.Name))
                    {
                        _product.Name = info.Name;
                        NameField.TextBoxText.Text = info.Name;
                    }
                    if (!string.IsNullOrEmpty(info.Category))
                        TryMatchCategory(info.Category);

                    if (string.IsNullOrWhiteSpace(_product.Code))
                    {
                        string code = GenerateUniqueCode(info.Name ?? barcode, barcode);
                        if (code != null)
                        {
                            _product.Code = code;
                            CodeField.TextBoxText.Text = code;
                        }
                    }
                    string msg = $"✓  Found: \"{info.Name}\"";
                    if (!string.IsNullOrEmpty(info.Brand)) msg += $" by {info.Brand}";
                    SetLookupStatus(msg + " — review and save.", "#16A34A");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_product.Code))
                    {
                        string code = GenerateUniqueCode(null, barcode);
                        if (code != null)
                        {
                            _product.Code = code;
                            CodeField.TextBoxText.Text = code;
                        }
                    }
                    SetLookupStatus(
                        "ℹ  Not found online. Code auto-generated — fill Name manually.",
                        "#92400E");
                }
            }
            catch
            {
                if (string.IsNullOrWhiteSpace(_product.Code))
                {
                    string code = GenerateUniqueCode(null, barcode);
                    if (code != null)
                    {
                        _product.Code = code;
                        CodeField.TextBoxText.Text = code;
                    }
                }
                SetLookupStatus("⚠  No internet. Code auto-generated — fill details manually.",
                    "#92400E");
            }
            finally
            {
                LookupBarcodeBtn.IsEnabled = true;
                ScanBarcodeBtn.IsEnabled = true;
            }
        }

        private async Task<BarcodeProductInfo> FetchFromOpenFoodFactsAsync(string barcode)
        {
            try
            {
                string url = $"https://world.openfoodfacts.org/api/v0/product/{barcode}.json";
                string json = await _http.GetStringAsync(url);
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                if (!root.TryGetProperty("status", out JsonElement st) || st.GetInt32() != 1)
                    return null;
                if (!root.TryGetProperty("product", out JsonElement prod))
                    return null;

                var info = new BarcodeProductInfo();
                if (prod.TryGetProperty("product_name_en", out JsonElement en) &&
                    en.GetString()?.Length > 0)
                    info.Name = TitleCase(en.GetString());
                else if (prod.TryGetProperty("product_name", out JsonElement pn) &&
                         pn.GetString()?.Length > 0)
                    info.Name = TitleCase(pn.GetString());

                if (prod.TryGetProperty("brands", out JsonElement br) &&
                    br.GetString()?.Length > 0)
                    info.Brand = br.GetString().Split(',')[0].Trim();

                if (prod.TryGetProperty("categories_tags", out JsonElement cats) &&
                    cats.GetArrayLength() > 0)
                {
                    string raw = cats[0].GetString() ?? "";
                    info.Category = raw.Replace("en:", "").Replace("-", " ").Trim();
                }
                return (info.Name != null || info.Brand != null) ? info : null;
            }
            catch { return null; }
        }

        // ── Save ────────────────────────────────────────────────────────
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors()) return;

            // Build supplier + stock lists from selected table
            var suppliers = new List<Supplier>();
            var stocks = new List<Stock>();
            var stockChanges = new List<(int, int, string)>();

            foreach (DataRow row in _selectedTable.Rows)
            {
                int suppId = (int)row[0];
                suppliers.Add(new Supplier(suppId));

                if (_prices.TryGetValue(suppId, out var p))
                {
                    var sup = new Supplier(suppId);
                    stocks.Add(new Stock(sup, p.cost, p.retail, p.discount, p.qty));

                    // Only log if qty > 0
                    if (p.qty > 0)
                        stockChanges.Add((suppId, p.qty, "Initial stock"));
                }
            }

            _product.Suppliers = suppliers;
            _product.Stocks = stocks;

            // Save captured barcode image
            if (_capturedBarcodeImage != null &&
                !string.IsNullOrWhiteSpace(_product.ExternalBarcode))
            {
                try
                {
                    string dir = "barcodes";
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    string path = Path.Combine(dir, $"{_product.ExternalBarcode}.png");
                    _capturedBarcodeImage.Save(path,
                        System.Drawing.Imaging.ImageFormat.Png);
                }
                catch { /* non-fatal */ }
            }

            _product.Save(isAdd: true);

            // Save stock changes with actual qty from _prices
            if (stockChanges.Count > 0)
                ProductDL.SaveStockChanges(_product, stockChanges);

            ProductDL.SavePrices(_product);

            NavigateCallingForm();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => NavigateCallingForm();

        // ── Validation ──────────────────────────────────────────────────
        public bool HasValidationErrors()
        {
            ExternalBarcodeField.TextBoxText
                .GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            NameField.TextBoxText
                .GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            CodeField.TextBoxText
                .GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            scrollViewer.ScrollToTop();

            if (string.IsNullOrWhiteSpace(_product.ExternalBarcode))
            {
                SetLookupStatus("Barcode value is required.", "#DC2626");
                return true;
            }
            return Validation.GetHasError(NameField.TextBoxText)
                || Validation.GetHasError(CodeField.TextBoxText);
        }

        // ── Helpers ─────────────────────────────────────────────────────
        private void ShowBarcodePreview(string value)
        {
            try
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = { Width = 300, Height = 55, Margin = 2 }
                };
                var bmp = writer.Write(value);
                using var ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                BarcodePreviewImage.Source = bi;
                BarcodePreviewImage.Visibility = Visibility.Visible;
            }
            catch { }
        }

        private void SetLookupStatus(string msg, string hexColor)
        {
            LookupStatusText.Text = msg;
            LookupStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)
                System.Windows.Media.ColorConverter.ConvertFromString(hexColor));
            LookupStatusText.Visibility = Visibility.Visible;
        }

        private bool IsNumericBarcode(string b)
        {
            if (b.Length < 8) return false;
            foreach (char c in b) if (!char.IsDigit(c)) return false;
            return true;
        }

        private string TitleCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0]) +
                               words[i].Substring(1).ToLower();
            return string.Join(" ", words);
        }

        private void TryMatchCategory(string fetched)
        {
            if (CategoryField.ItemSource is not List<Category> cats) return;
            foreach (var cat in cats)
            {
                if (cat.Name.IndexOf(fetched, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    fetched.IndexOf(cat.Name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    CategoryField.SelectedItem = cat;
                    return;
                }
            }
        }

        private string GenerateUniqueCode(string productName, string barcode)
        {
            string baseCode;
            if (!string.IsNullOrWhiteSpace(productName))
            {
                string letters = "";
                foreach (char c in productName.ToUpper())
                    if (char.IsLetterOrDigit(c))
                    {
                        letters += c;
                        if (letters.Length == 5) break;
                    }
                baseCode = letters.PadRight(5, '0').Substring(0, 5);
            }
            else
            {
                string tail = barcode.Length >= 5
                    ? barcode.Substring(barcode.Length - 5)
                    : barcode.PadLeft(5, '0');
                baseCode = tail.Substring(0, 5).ToUpper();
            }

            if (!ProductDL.IsCodeTaken(baseCode)) return baseCode;
            for (int i = 1; i <= 99; i++)
            {
                string sfx = i.ToString();
                string candidate = baseCode.Substring(0, 5 - sfx.Length) + sfx;
                if (!ProductDL.IsCodeTaken(candidate)) return candidate;
            }
            return null;
        }

        private void ExpiryDatePicker_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_product != null && ExpiryDatePicker.SelectedDate.HasValue)
                _product.ExpiryDate = ExpiryDatePicker.SelectedDate.Value;
        }

        private class BarcodeProductInfo
        {
            public string Name { get; set; }
            public string Brand { get; set; }
            public string Category { get; set; }
        }
    }
}
