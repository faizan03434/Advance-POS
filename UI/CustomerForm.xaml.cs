using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for CustomerForm.xaml
    /// </summary>
    public partial class CustomerForm : AbstractEntryForm , IValidationFields
    {
        private Customer customer;
        public CustomerForm(ManageEntity callingInstance, int id = -1) : base(callingInstance)
        {
            InitializeComponent();
            gender_cb.ItemSource = DataHandler.LookupData("Gender");
            gender_cb.DisplayPathName = "Value";
            city_cb.ItemSource = DataHandler.LookupData("CityPakistan");
            city_cb.DisplayPathName = "Value";
            if (id != -1)
            {
                ConfirmButton.Content = "Update";
                titleBlock.Title = "Edit Customer";
                customer = (Customer)CustomerDL.GetCustomer(id);
                gender_cb.SelectedItem = ((Dictionary<int, string>)gender_cb.ItemSource).Where(x => x.Value == customer.Gender).FirstOrDefault();
            }
            else
            {
                ConfirmButton.Content = "Add";
                titleBlock.Title = "Add Customer";
                customer = new Customer();
            }
            customer.Id = id;
            DataContext = customer;
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors())
                return;
            if (customer.Id != -1)
            {
                customer.Save(false);
            }
            else
            {
                customer.Save(true);
            }
            NavigateCallingForm();
        }
        public bool HasValidationErrors()
        {
            firstNameField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            cnicField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            contactField.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            scrollViewer.ScrollToTop();
            return Validation.GetHasError(firstNameField.TextBoxText)
                || Validation.GetHasError(cnicField.TextBoxText)
                || Validation.GetHasError(contactField.TextBoxText);
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }
    }
}
