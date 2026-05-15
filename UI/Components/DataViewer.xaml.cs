using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StationeryStoreManagementSystem.UI.Components
{
    /// <summary>
    /// Interaction logic for DataHandler.xaml
    /// </summary>
    public partial class DataViewer : UserControl
    {
        public delegate void DataGridButtonPressedEventHandler(DataGrid dataGrid,int selectedIndex);
        public event RoutedEventHandler? AddButtonClicked;
        public event DataGridButtonPressedEventHandler? SelectButtonClicked;
        public event DataGridButtonPressedEventHandler? EditButtonClicked;
        public event DataGridButtonPressedEventHandler? DeleteButtonClicked;





        public object AddButtonContent
        {
            get { return GetValue(AddButtonContentProperty); }
            set { SetValue(AddButtonContentProperty, value); }
        }
        public static readonly DependencyProperty AddButtonContentProperty =
            DependencyProperty.Register("AddButtonContent",
                                        typeof(object),
                                        typeof(DataViewer),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));





        private bool row1Visbility
        {
            get { return (bool)GetValue(row1VisbilityProperty); }
            set { SetValue(row1VisbilityProperty, value); }
        }
        private static readonly DependencyProperty row1VisbilityProperty =
            DependencyProperty.Register("row1Visbility", typeof(bool), typeof(DataViewer), new PropertyMetadata(false));



        public bool IsAdd
        {
            get { return (bool)GetValue(IsAddProperty); }
            set {
                SetValue(IsAddProperty, value);
                if (value == true)
                    Addbutton.Visibility = Visibility.Visible;
                else
                    Addbutton.Visibility = Visibility.Collapsed;
                SetStackPanel();
            }
        }
        public static readonly DependencyProperty IsAddProperty =
            DependencyProperty.Register("IsAdd", typeof(bool), typeof(DataViewer), new PropertyMetadata(false));



        public bool IsEdit
        {
            get { return (bool)GetValue(IsEditProperty); }
            set { 
                SetValue(IsEditProperty, value);
                SetEditDeleteColumn();
            }
        }
        public static readonly DependencyProperty IsEditProperty =
            DependencyProperty.Register("IsEdit", typeof(bool), typeof(DataViewer), new PropertyMetadata(false));



        public bool IsDelete
        {
            get { return (bool)GetValue(IsDeleteProperty); }
            set { 
                SetValue(IsDeleteProperty, value);
                SetEditDeleteColumn();
            }
        }
        public static readonly DependencyProperty IsDeleteProperty =
            DependencyProperty.Register("IsDelete", typeof(bool), typeof(DataViewer), new PropertyMetadata(false));



        public bool IsSelect
        {
            get { return (bool)GetValue(IsSelectProperty); }
            set { 
                SetValue(IsSelectProperty, value);
                if (value == true)
                    SelectColumn.Visibility = Visibility.Visible;
                else
                    SelectColumn.Visibility = Visibility.Collapsed;
            }
        }

        // Using a DependencyProperty as the backing store for IsSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectProperty =
            DependencyProperty.Register("IsSelect", typeof(bool), typeof(DataViewer), new PropertyMetadata(false));




        public List<string> SearchAttributes
        {
            get { return (List<string>)GetValue(SearchAttributesProperty); }
            set { 
                SetValue(SearchAttributesProperty, value);
                if (value == null)
                    searchBar.Visibility = Visibility.Collapsed;
                else
                    searchBar.Visibility = Visibility.Visible;
                SetStackPanel();
            }
        }
        public static readonly DependencyProperty SearchAttributesProperty =
            DependencyProperty.Register("SearchAttributes",
                                        typeof(List<string>),
                                        typeof(DataViewer),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public IEnumerable ItemSource
        {
            get { return (IEnumerable)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register("ItemSource",
                                        typeof(IEnumerable),
                                        typeof(DataViewer),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public DataViewer()
        {
            InitializeComponent();
        }

        public void SetBindings(List<(string,string)> bindings)
        {
            for (int i = bindings.Count - 1; i >= 0; i--)
            {
                DataGridTextColumn column = new DataGridTextColumn();
                column.Header = bindings[i].Item1;
                column.Binding = new Binding(bindings[i].Item2);
                datagrid.Columns.Insert(0, column);
            }
        }
        private void searchBar_SearchRequested(object sender, EventArgs e)
        {
            string filterString = searchBar.FilterString;
            ((DataView)datagrid.ItemsSource).RowFilter = filterString;
        }
        private void SetEditDeleteColumn()
        {
            EditColumn.Visibility = Visibility.Collapsed;
            DeleteColumn.Visibility = Visibility.Collapsed;
            EditDeleteColumn.Visibility = Visibility.Collapsed;
            if (IsEdit == true && IsDelete == true)
                EditDeleteColumn.Visibility = Visibility.Visible;
            else if (IsEdit == true)
                EditColumn.Visibility = Visibility.Visible;
            else if (IsDelete == true)
                DeleteColumn.Visibility = Visibility.Visible;
        }
        private void SetStackPanel()
        {
            if (IsAdd == false && SearchAttributes == null)
            {
                row1Visbility = false;

            }
            else
            {
                row1Visbility = true;
            }
        }
        public void Refresh(DataTable source)
        {
            if (source == null)
                return;
            datagrid.ItemsSource = source.DefaultView;
            if (source.DataSet != null)
            {
                string filterString = searchBar.FilterString;
                ((DataView)datagrid.ItemsSource).RowFilter = filterString;
            }
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddButtonClicked?.Invoke(this, e);
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectButtonClicked?.Invoke(datagrid, datagrid.SelectedIndex);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditButtonClicked?.Invoke(datagrid, datagrid.SelectedIndex);

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteButtonClicked?.Invoke(datagrid, datagrid.SelectedIndex);
        }
    }
}
