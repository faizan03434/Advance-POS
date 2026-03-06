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
    /// Interaction logic for ManageEntity.xaml
    /// </summary>
    public partial class ManageEntity : UserControl
    {

        private Type entryForm;
        private Type viewForm;
        private Delegate? deleteFunc;
        private Delegate getTable;
        public ManageEntity(string title,string entityName, Delegate getTable, List<(string, string)> bindings, List<string>? searchAttributes, Type? entryForm, bool isAdd, bool isEdit, Delegate? deleteFunc = null,Type? viewForm = null)
        {
            InitializeComponent();
            dataGridView.Addbutton.Content = $"Add {entityName}";
            dataGridView.SetBindings(bindings);
            dataGridView.SearchAttributes = searchAttributes;
            TitleBlock.Text = title;
            this.deleteFunc = deleteFunc;
            this.getTable = getTable;
            this.entryForm = entryForm;
            this.viewForm = viewForm;
            dataGridView.IsAdd = isAdd;
            dataGridView.IsEdit = isEdit;
            if(deleteFunc!=null)
                dataGridView.IsDelete = true;
            if (viewForm!=null)
                dataGridView.IsSelect = true;
            dataGridView.AddButtonClicked += DataGridView_AddButtonClicked;
            dataGridView.EditButtonClicked += DataGridView_EditButtonClicked;
            dataGridView.DeleteButtonClicked += DataGridView_DeleteButtonClicked;
            dataGridView.SelectButtonClicked += DataGridView_SelectButtonClicked;
            RefreshData();
        }

        private void DataGridView_DeleteButtonClicked(DataGrid dataGrid, int selectedIndex)
        {
            object[] id = { ((DataView)(dataGrid.ItemsSource))[selectedIndex].Row.ItemArray[0] };
            UI.Components.MessageBox.MessageBoxResult result = UI.Components.MessageBox.Show("Are you sure you want to delete?", "Confirmation", UI.Components.MessageBox.Type.YesNo);
            if (result == UI.Components.MessageBox.MessageBoxResult.Yes)
            {
                deleteFunc.DynamicInvoke(id);
            }
            RefreshData();
        }

        private void DataGridView_EditButtonClicked(DataGrid dataGrid, int selectedIndex)
        {
            object id = ((DataView)(dataGrid.ItemsSource))[selectedIndex].Row.ItemArray[0];
            ((Border)Parent).Child = (UIElement)Activator.CreateInstance(entryForm, new object[] { this, id });
        }
        private void DataGridView_SelectButtonClicked(DataGrid dataGrid, int selectedIndex)
        {
            object id = ((DataView)(dataGrid.ItemsSource))[selectedIndex].Row.ItemArray[0];
            ((Border)Parent).Child = (UIElement)Activator.CreateInstance(viewForm, new object[] { this, id });
        }
        private void DataGridView_AddButtonClicked(object sender, RoutedEventArgs e)
        {
            ((Border)Parent).Child = (UIElement)Activator.CreateInstance(entryForm, new object[] { this, -1 });
        }
        public void RefreshData()
        {
            dataGridView.Refresh((DataTable)getTable.DynamicInvoke());
        }
    }
}
