using Microsoft.Data.SqlClient;
using StationeryStoreManagementSystem;
using System;
using System.Data;
using System.Collections;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using StationeryStoreManagementSystem.DL;
using StationeryStoreManagementSystem.BL;
using Microsoft.IdentityModel.Tokens;

namespace StationeryStoreManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public event EventHandler LoginClicked;
        string username;
        string password;
        public Login()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            username = username_tb.Text;
            password = password_tb.Text;
            int? id = IsValid();

            if (id != null)
            {
                ((MainWindow)((Grid)((Grid)((Border)Parent).Parent).Parent).Parent).col1.Width = new GridLength(200, GridUnitType.Pixel);
                ((Border)Parent).Child = null;
                Utils.CurrentEmployee = EmployeeDL.GetEmployee((int)id);
                InsertLog();
                LoginClicked?.Invoke(this, e);
            }
            else
            {
                UI.Components.MessageBox.Show("Invalid Credentials", "Error", UI.Components.MessageBox.Type.Message);
            }
        }
        public int? IsValid()
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                return null;
            Utils.CloseReader();
            var conn = Configuration.getInstance().getConnection();
            string query = "EXEC CheckCredentials @Username, @Password, @UserId OUTPUT";
            SqlCommand command = new SqlCommand(query, conn);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);
            command.Parameters.Add("@UserId", SqlDbType.Int).Direction = ParameterDirection.Output;
            command.ExecuteNonQuery();
            if(command.Parameters["@UserId"].Value.GetType()==typeof(DBNull))
                    return null;
            int Id = Convert.ToInt32(command.Parameters["@UserId"].Value);
            return Id;
        }
        private void InsertLog()
        {
            List<(string, object)> args = new List<(string, object)>
            {
                ("UserId", Utils.CurrentEmployee.Id)
            };
            DataHandler.InsertDataSP(args, "stpInsertLoginTime");
        }
    }
}
