using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static StationeryStoreManagementSystem.UI.Controls.TextEntry;

namespace StationeryStoreManagementSystem.UI.Controls
{
    /// <summary>
    /// Interaction logic for ComboBoxEntry.xaml
    /// </summary>
    public partial class ComboBoxEntry : UserControl
    {
        public class EmptyItemValidate : ValidationRule
        {
            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                if (value!=null)
                {
                    return new ValidationResult(true, null);
                }

                return new ValidationResult(false, "*required field empty");
            }
        }
        public event SelectionChangedEventHandler SelectionChanged;
        public string LabelText
        {
            get => TextBlockLabel.Text;
            set
            {
                TextBlockLabel.Text = value;
            }
        }
        public bool ReadOnly { get; set; }
        public List<string> Items
        {
            get => (List<string>)ComboBox1.ItemsSource;
            set
            {
                ComboBox1.ItemsSource = value;
            }
        }


        public IEnumerable ItemSource
        {
            get { return (IEnumerable)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register("ItemSource",
                                        typeof(IEnumerable),
                                        typeof(ComboBoxEntry),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public SqlDataReader ItemsRead
        {
            set
            {
                List<string> items = new List<string>();
                while (value.Read())
                {
                    items.Add(value.GetString(0));
                }
                value.Close();
                Items = items;
            }
        }
        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue",
                                        typeof(object),
                                        typeof(ComboBoxEntry),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath",
                                        typeof(string),
                                        typeof(ComboBoxEntry),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));




        public bool IsRequired
        {
            get { return (bool)GetValue(IsRequiredProperty); }
            set { SetValue(IsRequiredProperty, value); }
        }
        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register("IsRequired", typeof(bool), typeof(ComboBoxEntry), new PropertyMetadata(false,IsRequiredCallback));

        private static void IsRequiredCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            Binding binding = BindingOperations.GetBinding(((ComboBoxEntry)dependencyObject).ComboBox1, ComboBox.SelectedValueProperty);
            binding.ValidationRules.Clear();
            if (((ComboBoxEntry)dependencyObject).IsRequired == true)
                binding.ValidationRules.Add(new EmptyItemValidate() { ValidatesOnTargetUpdated = true });
        }

        public object SelectedItem
        {
            get {
                return GetValue(SelectedItemProperty); }
            set {
                SetValue(SelectedItemProperty, value);
                ComboBox1.SelectedItem = value;
            }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem",
                                        typeof(object),
                                        typeof(ComboBoxEntry),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));




        public string DisplayPathName
        {
            get { return (string)GetValue(DisplayPathNameProperty); }
            set { SetValue(DisplayPathNameProperty, value); }
        }

        public static readonly DependencyProperty DisplayPathNameProperty =
            DependencyProperty.Register("DisplayPathName",
                                        typeof(string),
                                        typeof(ComboBoxEntry),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                                        typeof(string),
                                        typeof(ComboBoxEntry),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public ComboBoxEntry()
        {
            InitializeComponent();
        }

        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }
    }
}
