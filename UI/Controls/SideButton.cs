using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StationeryStoreManagementSystem.UI.Controls
{
    public class SideButton : Button
    {
        public SideButton() :base()
        {
            Margin = new System.Windows.Thickness(5, 10, 5, 0);
            Height = 40;
        }
    }
}
