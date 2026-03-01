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
    /// Interaction logic for DataLabel.xaml
    /// </summary>
    public partial class DataLabel : UserControl
    {


        public string LabelData
        {
            get { return (string)GetValue(LabelDataProperty); }
            set { SetValue(LabelDataProperty, value); }
        }
        public static readonly DependencyProperty LabelDataProperty =
            DependencyProperty.Register("LabelData",
                                        typeof(string),
                                        typeof(DataLabel),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public string TextData
        {
            get { return (string)GetValue(TextDataProperty); }
            set { SetValue(TextDataProperty, value); }
        }
        public static readonly DependencyProperty TextDataProperty =
            DependencyProperty.Register("TextData",
                                        typeof(string),
                                        typeof(DataLabel),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(DataLabel), new PropertyMetadata(12.0));



        public DataLabel()
        {
            InitializeComponent();
        }
    }
}
