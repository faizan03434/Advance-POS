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
    /// Interaction logic for EditStockForm.xaml
    /// </summary>
    public partial class EditStockForm : UserControl , IValidationFields
    {
        ProductForm callingInstance;
        Product product;
        int supplierId;
        public EditStockForm(ProductForm callingInstance, Product product,int supplierId,string supplierName)
        {
            this.callingInstance = callingInstance;
            InitializeComponent();
            productName.TextData = product.Name;
            this.supplierName.TextData = supplierName;
            this.product = product;
            this.supplierId = supplierId;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors())
                return;
            callingInstance.stockChanges.Add((supplierId, int.Parse(quantityChangeField.Text), reasonField.Text));
            ((Border)Parent).Child = callingInstance;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ((Border)Parent).Child = callingInstance;
        }
        public bool HasValidationErrors()
        {
            quantityChangeField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            return Validation.GetHasError(quantityChangeField.TextBoxText);
        }
    }
}
