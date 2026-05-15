using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
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

namespace StationeryStoreManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for NotificationForm.xaml
    /// </summary>
    public partial class NotificationForm : AbstractEntryForm , IValidationFields
    {
        private Notification notification = new Notification();
        public NotificationForm(ManageEntity callingInstance, int id = -1) : base(callingInstance)
        {
            InitializeComponent();
            List<Cashier> employees = EmployeeDL.GetCashiers();
            EmployeeField.ItemSource = employees;
            EmployeeField.DisplayPathName = "Name";
            DataContext = notification;
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors())
                return;
            if (EmployeeField.SelectedItem == null)
            {
                List<Cashier> employees = EmployeeDL.GetCashiers();
                foreach (Cashier employee in employees)
                {
                    notification.Receiver = employee;
                    notification.Save(true);
                }
            }
            else
            {
                notification.Save(true);
            }
            NavigateCallingForm();
        }
        public bool HasValidationErrors()
        {
            ContentField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            EmployeeField.ComboBox1.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            return Validation.GetHasError(ContentField.TextBoxText)
                || Validation.GetHasError(EmployeeField.ComboBox1);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }
    }
}
