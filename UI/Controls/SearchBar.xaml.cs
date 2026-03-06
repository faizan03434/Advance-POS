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

namespace StationeryStoreManagementSystem.UI.Controls
{
    /// <summary>
    /// Interaction logic for SearchBar.xaml
    /// </summary>
    public partial class SearchBar : UserControl
    {
        public List<string> SearchAttributes
        {
            get { return (List<string>)GetValue(SearchAttributesProperty); }
            set { SetValue(SearchAttributesProperty, value); }
        }
        public static readonly DependencyProperty SearchAttributesProperty =
            DependencyProperty.Register("SearchAttributes",
                                        typeof(List<string>),
                                        typeof(SearchBar),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public string FilterString
        {
            get
            {
                if (SearchAttributes == null)
                    return "";
                string[] strings = SearchAttributes.ToArray();
                for (int i = 0; i < SearchAttributes.Count; i++)
                {
                    strings[i] = $"[{strings[i]}] LIKE '*{SearchTextBox.Text}*'";
                }
                string filter = String.Join(" OR ", strings);
                return filter;
            }
        }
        public event EventHandler SearchRequested;
        public SearchBar()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchRequested(this,e);
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Enter)
            {
                SearchRequested(this,e);
            }
        }
    }
}
