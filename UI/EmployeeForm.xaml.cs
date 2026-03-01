using Microsoft.Data.SqlClient;
using StationeryStoreManagementSystem.BL;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace StationeryStoreManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for EmployeeForm.xaml
    /// </summary>
    public partial class EmployeeForm : AbstractEntryForm , IValidationFields
    {
        private Employee E;
        public EmployeeForm(ManageEntity callingInstance, int id = -1) : base(callingInstance)
        {
            InitializeComponent();
            gender_cb.ItemSource = DataHandler.LookupData("Gender");
            gender_cb.DisplayPathName = "Value";
            role_cb.ItemSource = DataHandler.LookupData("Role");
            role_cb.DisplayPathName = "Value";
            city_cb.ItemSource = DataHandler.LookupData("CityPakistan");
            city_cb.DisplayPathName = "Value";
            if (id != -1)
            {
                cell1.Visibility = Visibility.Collapsed;
                cell2.Visibility = Visibility.Collapsed;
                ConfirmButton.Content = "Update";
                titleBlock.Title = "Edit Employee";
                Employee em = EmployeeDL.GetEmployee(id);
                if (em is Admin)
                {
                    E = (Admin)em;
                }
                else
                {
                    E = (Cashier)em;
                }
                gender_cb.SelectedItem = ((Dictionary<int, string>)gender_cb.ItemSource).Where(x => x.Value == E.Gender).FirstOrDefault();
                role_cb.SelectedItem = DataHandler.LookupData("Role").Where(x => x.Value == E.DetermineRole(E)).FirstOrDefault();
            }
            else
            {
                ResetButton.Visibility = Visibility.Collapsed;
                ConfirmButton.Content = "Add";
                titleBlock.Title = "Add Employee";
                E = new Employee();
            }
            E.Id = id;
            DataContext = E;
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors())
                return;
            if (E.Id != -1)
            {
                E.Save(false);
            }
            else
            {
                var role = role_cb.SelectedItem as KeyValuePair<int, string>?;
                if (role.Value.Value == "Admin")
                {
                    E = new Admin(E);
                }
                else
                {
                    E = new Cashier(E);
                }
                E.Save(true);
            }
            NavigateCallingForm();
        }
        public bool HasValidationErrors()
        {
            cell1.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            cell2.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            firstNameField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            cnicField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            contactField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            role_cb.ComboBox1.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            emailField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            salaryField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            scrollViewer.ScrollToTop();
            return Validation.GetHasError(cell1.TextBoxText)
                || Validation.GetHasError(cell2.TextBoxText)
                || Validation.GetHasError(firstNameField.TextBoxText)
                || Validation.GetHasError(cnicField.TextBoxText)
                || Validation.GetHasError(contactField.TextBoxText)
                || Validation.GetHasError(role_cb.ComboBox1)
                || Validation.GetHasError(emailField.TextBoxText)
                || Validation.GetHasError(salaryField.TextBoxText);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.CloseReader();
            var conn = Configuration.getInstance().getConnection();
            string query = "EXEC ResetPassword @UserId, @NewPassword OUTPUT";
            SqlCommand command = new SqlCommand(query, conn);
            command.Parameters.AddWithValue("@UserId", E.Id);
            command.Parameters.Add("@NewPassword", SqlDbType.NVarChar, 5).Direction = ParameterDirection.Output;
            command.ExecuteNonQuery();
            string newPassword = command.Parameters["@NewPassword"].Value.ToString();
            UI.Components.MessageBox.Show("Password Reset to: " + newPassword, "Information", UI.Components.MessageBox.Type.Message);
        }
    }
}
