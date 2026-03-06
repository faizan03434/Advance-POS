using StationeryStoreManagementSystem.BL;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for ProductSupplierPriceForm.xaml
    /// </summary>
    public partial class ProductSupplierPriceForm : UserControl , IValidationFields
    {
        private Stock stock;
        private ProductForm callingInstance;
        private bool isEdit = true;
        public ProductSupplierPriceForm(ProductForm callingInstance,Product product,int supplierId, string supplierName)
        {
            InitializeComponent();
            this.callingInstance = callingInstance;
            stock = product.Stocks?.Where(x => x.Supplier.Id == supplierId).FirstOrDefault();
            if (stock == null)
            {
                stock = new Stock(new Supplier() {Id = supplierId, Name = supplierName }, 0, 0, 0, 0);
                isEdit = false;
            }
            else
                CancelButton.Visibility = Visibility.Collapsed;
            DataContext = stock;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors())
                return;
            if(isEdit==false) 
            { 
            callingInstance.product.Stocks.Add(stock);
            }
            ((Border)Parent).Child = callingInstance;
        }

        public bool HasValidationErrors()
        {
            PriceField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            RetailPriceField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            DiscountAmountField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            return Validation.GetHasError(PriceField.TextBoxText)
                || Validation.GetHasError(RetailPriceField.TextBoxText)
                || Validation.GetHasError(DiscountAmountField.TextBoxText);
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ((Border)Parent).Child = callingInstance;
        }
    }
}
