using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace StationeryStoreManagementSystem.UI.Components
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : UserControl
    {
        public enum Type
        {
            Message,
            YesNo
        }
        public enum MessageBoxResult
        {
            Yes,
            No,
            Ok,
            Cancel,
            None
        }
        public MessageBoxResult Result { get; set; }
        public static MessageBoxResult Show(string message, string title = "",Type type=Type.Message,int fontSize = 18)
        {
            MessageBox messageBox = new MessageBox();
            messageBox.titleblock.Text = title;
            messageBox.messageblock.FontSize = fontSize;
            messageBox.messageblock.Text = message;
            Utils.CurrentMainWindow.overlay.Child = messageBox;
            Utils.CurrentMainWindow.overlay.Visibility = Visibility.Visible;
            if(type==Type.YesNo)
            {
                messageBox.yesno.Visibility = Visibility.Visible;
            }
            else if(type==Type.Message)
            {
                messageBox.confirmation.Visibility = Visibility.Visible;
            }
            while (messageBox.Result == MessageBoxResult.None)
            {
                if (messageBox.Dispatcher.HasShutdownStarted ||
                    messageBox.Dispatcher.HasShutdownFinished)
                {
                    break;
                }

                messageBox.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new ThreadStart(delegate { }));
                Thread.Sleep(20);
            }
            Utils.CurrentMainWindow.overlay.Child = null;
            Utils.CurrentMainWindow.overlay.Visibility = Visibility.Hidden;
            return messageBox.Result;
        }
        private MessageBox()
        {
            InitializeComponent();
            Result = MessageBoxResult.None;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Ok;

        }
    }
}
