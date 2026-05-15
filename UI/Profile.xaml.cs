using Microsoft.Data.SqlClient;
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
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Profile : UserControl
    {
        public event EventHandler LogoutClicked;
        public Profile()
        {
            InitializeComponent();
            DataContext = Utils.CurrentEmployee;
            name.Text = Utils.CurrentEmployee.FirstName + " " + Utils.CurrentEmployee.LastName;
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ((Border)Parent).Child = null;
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LogoutClicked?.Invoke(this, e);
            InsertLog();
        }
        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (password_box.Text != null) 
            { 
                Utils.CloseReader();
                var conn = Configuration.getInstance().getConnection();
                string query = "EXEC ResetPassword @UserId, @NewPassword";
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@UserId", Utils.CurrentEmployee.Id);
                command.Parameters.AddWithValue("@NewPassword", password_box.Text);
                command.ExecuteNonQuery();
                UI.Components.MessageBox.Show("Password is Changed to: " + password_box.Text, "Information", UI.Components.MessageBox.Type.Message);
                password_box.Text = "";
            }
            else
            {
                UI.Components.MessageBox.Show("New PasswordBox is empty", "Error", UI.Components.MessageBox.Type.Message);
            }
        }

        private void InsertLog()
        {
            List<(string, object)> args = new List<(string, object)>
            {
                ("UserId", Utils.CurrentEmployee.Id)
            };
            DataHandler.InsertDataSP(args, "stpInsertLogoutTime");
        }
    }
}
