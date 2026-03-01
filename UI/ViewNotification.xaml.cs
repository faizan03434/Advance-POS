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
using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;

namespace StationeryStoreManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for ViewNotification.xaml
    /// </summary>
    public partial class ViewNotification : AbstractEntryForm
    {
        public ViewNotification(ManageEntity callingInstance, object Id):base(callingInstance)
        {
            InitializeComponent();
            Notification notification = NotificationDL.GetNotification((int)Id);
            if (notification != null)
            {
                if (notification.Sender == null)
                    fromLabel.TextData = "System Generated";
                else
                    fromLabel.TextData = notification.Sender.Name;
                timestampLabel.TextData = notification.Timestamp;
                content.Text = notification.Content;
                notification.Save(false);
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }
    }
}
