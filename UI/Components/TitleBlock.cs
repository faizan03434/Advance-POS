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
            FontSize = 20;
            Padding = new Thickness(10);
            VerticalAlignment = VerticalAlignment.Center;
            InitializeComponent();
        }
    }
}
