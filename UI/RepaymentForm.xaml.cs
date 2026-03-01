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
    /// Interaction logic for RepaymentForm.xaml
    /// </summary>
    public partial class RepaymentForm : AbstractEntryForm ,IValidationFields
    {
        private Customer C;
        public RepaymentForm(ManageEntity callingInstance, int id = -1) : base(callingInstance)
        {
            InitializeComponent();
            C = CustomerDL.GetCustomer(id);
            if (C != null)
            {
                DuesLabel.TextData = Math.Abs((C.PaymentDues)).ToString();
                List<(string, string)> bindings = new List<(string, string)> {
                ("Timestamp","Timestamp"),
                ("Amount","Amount"),
                ("Type","Type")};
                logdatagrid.SetBindings(bindings);
                logdatagrid.ItemSource = CustomerDL.GetCustomerRepaymentsView(id).DefaultView;
            }
            DataContext = C;
        }


        private void RepayButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidationErrors())
                return;
            if (C.PaymentDues < 0)
            {
                if ((-C.PaymentDues) >= Convert.ToDouble(RepaymentAmount.Text))
                {
                    List<(string, object)> args = new List<(string, object)>
                    {
                        ("CustomerId", C.Id),
                        ("Amount" , Convert.ToDouble(RepaymentAmount.Text))
                    };
                    DataHandler.InsertDataSP(args, "stpInsertPaymentDues");
                    C.PaymentDues += Convert.ToDouble(RepaymentAmount.Text);
                    DuesLabel.TextData = Math.Abs((C.PaymentDues)).ToString();
                    logdatagrid.Refresh(CustomerDL.GetCustomerRepaymentsView(C.Id));
                    RepaymentAmount.Text = "";
                }
                else
                    UI.Components.MessageBox.Show("Pending Dues are Lower", "Error", UI.Components.MessageBox.Type.Message);
            }
            else
            {
                UI.Components.MessageBox.Show("No! Pending Dues", "Error", UI.Components.MessageBox.Type.Message);
            }
        }
        public bool HasValidationErrors()
        {
            RepaymentAmount.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            return Validation.GetHasError(RepaymentAmount.TextBoxText);
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }
    }
}
