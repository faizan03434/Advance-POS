// NEW FILE: UI/ExternalBarcodeForm.xaml.cs

using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Windows.Compatibility;

namespace StationeryStoreManagementSystem.UI
{
    public partial class ExternalBarcodeForm : AbstractEntryForm, IValidationFields
    {
        private Product _product;

        public ExternalBarcodeForm(ManageEntity callingInstance) : base(callingInstance)
        {
            InitializeComponent();

            _product = new Product();

            // Populate combo boxes
            List<Company> companies = CompanyDL.GetCompanies();
            List<Category> categories = CategoryDL.GetCategories();
            CompanyField.ItemSource = companies;
            CompanyField.DisplayPathName = "Name";
            CategoryField.ItemSource = categories;
            CategoryField.DisplayPathName = "Name";

            // Supplier picker (same pattern as ProductForm)
            List<(string, string)> bindings = new List<(string, string)> {
                ("Name","Name"), ("Contact","Contact"), ("Email","Email"),
                ("Street Address","StreetAddress"), ("Town","Town"),
                ("City","City"), ("Country","Country"), ("Postal Code","PostalCode")};

            suppliersDataHandler.SearchAttributes = new List<string> { "Name" };
            suppliersDataHandler.IsSelect = true;
            suppliersDataHandler.SetBindings(bindings);

            DataTable allSuppliers = SupplierDL.GetSuppliersView();
            suppliersDataHandler.ItemSource = allSuppliers.DefaultView;
            suppliersDataHandler.SelectButtonClicked += SuppliersDataHandler_SelectButtonClicked;

            // Build supplier display columns in selected grid
            for (int i = bindings.Count - 1; i >= 0; i--)
            {
                var col = new DataGridTextColumn
                {
                    Header = bindings[i].Item1,
                    Binding = new System.Windows.Data.Binding(bindings[i].Item2),
                    IsReadOnly = true
                };
                SuppliersDataGrid.Columns.Insert(0, col);
            }
            SuppliersDataGrid.AutoGenerateColumns = false;
            SuppliersDataGrid.CanUserAddRows = false;
            SuppliersDataGrid.ItemsSource = allSuppliers.Clone().DefaultView; // empty table, same schema

            DataContext = _product;
        }

        // ── Supplier selection ──────────────────────────────────────────
        private void SuppliersDataHandler_SelectButtonClicked(DataGrid dg, int idx)
        {
            DataRow row = ((DataRowView)dg.SelectedItem).Row;
            ((DataView)SuppliersDataGrid.ItemsSource).Table.Rows.Add(row.ItemArray);
            ((DataView)suppliersDataHandler.ItemSource).Table.Rows.Remove(row);
        }

        private void RemoveSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            DataRow row = ((DataRowView)SuppliersDataGrid.SelectedItem).Row;
            ((DataView)suppliersDataHandler.ItemSource).Table.Rows.Add(row.ItemArray);
            ((DataView)SuppliersDataGrid.ItemsSource).Table.Rows.Remove(row);
        }

        // ── Step 1: Camera scan ─────────────────────────────────────────
        private void ScanBarcodeBtn_Click(object sender, RoutedEventArgs e)
        {
            // Reuse ProcessOrder camera logic: open a small overlay window that
            // reads one frame and returns the decoded string.
            var scanWindow = new BarcodeScanOverlay();
            scanWindow.ShowDialog();
            if (!string.IsNullOrEmpty(scanWindow.ScannedValue))
            {
                _product.ExternalBarcode = scanWindow.ScannedValue;
                ExternalBarcodeField.TextBoxText.Text = scanWindow.ScannedValue;
                ShowBarcodePreview(scanWindow.ScannedValue);
                SetLookupStatus("Barcode captured from camera.", "#16A34A");
            }
        }

        // ── Step 1: Lookup embedded data in barcode ─────────────────────
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

            // Many GS1/EAN barcodes embed product name or GTIN — try simple decode
            // Real-world barcodes usually do NOT embed names; we just prefill what we can.
            bool decoded = TryDecodeBarcodeData(raw, out string detectedName);
            if (decoded && !string.IsNullOrEmpty(detectedName))
            {
                if (string.IsNullOrEmpty(_product.Name))
                    _product.Name = detectedName;
                NameField.TextBoxText.Text = _product.Name;
                SetLookupStatus($"Product info detected: \"{detectedName}\". Fill in remaining fields.", "#16A34A");
            }
            else
            {
                SetLookupStatus("No embedded product data found in this barcode. Please fill in details manually.", "#92400E");
            }
        }

        // Minimal GS1 data extraction — expand as needed
        private bool TryDecodeBarcodeData(string raw, out string productName)
        {
            productName = null;
            // GS1-128 Application Identifier 10 = batch, 11 = date, 30 = qty, etc.
            // Many retail barcodes are pure GTINs with no embedded text.
            // This stub returns false; plug in a real GS1 parser if needed.
            return false;
        }

        private void ShowBarcodePreview(string value)
        {
            try
            {
                // Generate preview image in memory
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = { Width = 300, Height = 60, Margin = 2 }
                };
                var bitmap = writer.Write(value);
                using var ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                BarcodePreviewImage.Source = bitmapImage;
                BarcodePreviewImage.Visibility = Visibility.Visible;
            }
            catch { /* non-critical */ }
        }

        private void SetLookupStatus(string msg, string hexColor)
        {
            LookupStatusText.Text = msg;
            LookupStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor));
            LookupStatusText.Visibility = Visibility.Visible;
        }

        private void ExpiryDatePicker_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_product != null && ExpiryDatePicker.SelectedDate.HasValue)
                _product.ExpiryDate = ExpiryDatePicker.SelectedDate.Value;
        }

        // ── Save ────────────────────────────────────────────────────────
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors()) return;

            // Build supplier list from selected grid
            var suppliers = new List<Supplier>();
            foreach (DataRow row in ((DataView)SuppliersDataGrid.ItemsSource).Table.Rows)
                suppliers.Add(new Supplier((int)row.ItemArray[0]));

            _product.Suppliers = suppliers;
            if (_product.Stocks == null) _product.Stocks = new List<Stock>();

            // Save — Product.Save() will call Utils.GenerateBarcode(ExternalBarcode)
            _product.Save(isAdd: true);

            NavigateCallingForm();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }

        public bool HasValidationErrors()
        {
            ExternalBarcodeField.TextBoxText.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            NameField.TextBoxText.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            CodeField.TextBoxText.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            scrollViewer.ScrollToTop();

            if (string.IsNullOrWhiteSpace(_product.ExternalBarcode))
            {
                SetLookupStatus("Barcode value is required.", "#DC2626");
                return true;
            }
            return Validation.GetHasError(NameField.TextBoxText)
                || Validation.GetHasError(CodeField.TextBoxText);
        }
    }
}
