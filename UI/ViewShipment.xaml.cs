using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
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
    /// Interaction logic for ViewShipment.xaml
    /// </summary>
    public partial class ViewShipment : AbstractEntryForm
    {
        public ViewShipment(ManageEntity callingInstance,object Id):base(callingInstance)
        {
            InitializeComponent();
            int SupplierId;
            List<Product> products;
            (SupplierId,products) =  ShipmentDL.GetShipment((DateTime)Id);
            supplierNameLabel.TextData = SupplierDL.GetSupplier(SupplierId).Name;
            dataViewer.SetBindings(
            new List<(string, string)>
            {
                            ("Product Name","Name"),
                            ("Company Name","Company.Name"),
                            ("Category","Category.Name"),
                            ("Quantity","SupplierQuantity"),
            });
            dataViewer.datagrid.ItemsSource = products;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }
    }
}
