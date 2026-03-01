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
using WPFMediaKit.DirectShow.Controls;

namespace StationeryStoreManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for GetCustomerForm.xaml
    /// </summary>
    public partial class GetCustomerForm : UserControl
    {
        ProcessOrder callingInstance;
        public GetCustomerForm(ProcessOrder callingInstance)
        {
            this.callingInstance = callingInstance;
            InitializeComponent();
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("CNIC","CNIC"),
                ("Name","Name"),
                ("Contact","Contact"),
                ("Gender","Gender")
            };
            dataGridView.SetBindings(bindings);
            dataGridView.SearchAttributes = new List<string>() { "Name" };
            dataGridView.IsSelect = true;
            dataGridView.Refresh(CustomerDL.GetCustomersView());
            dataGridView.SelectButtonClicked += DataGridView_SelectButtonClicked;
        }

        private void DataGridView_SelectButtonClicked(DataGrid dataGrid, int selectedIndex)
        {
            //object id = ((DataView)(dataGrid.ItemsSource))[selectedIndex].Row.ItemArray[0];
            //callingInstance.order.Customer = CustomerDL.GetCustomer((int)id);
            //((Border)Parent).Child = callingInstance;
        }
    }
}
