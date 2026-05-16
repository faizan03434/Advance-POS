using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StationeryStoreManagementSystem.UI.Components
{
    public partial class TitleBlock : TextBlock
    {
        public string Title
        {
            get { return Text; }
            set { Text = value; }
        }
        public TitleBlock()
        {
            FontSize = 22;
            Padding = new Thickness(20, 0, 0, 0);
            VerticalAlignment = VerticalAlignment.Center;

            // Ye rahi wo do lines:
            this.Background = System.Windows.Media.Brushes.Transparent;
            this.Foreground = System.Windows.Media.Brushes.White;

            InitializeComponent();
        }
    }
}
