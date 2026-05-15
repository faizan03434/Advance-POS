using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
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
    /// Interaction logic for CategoryForm.xaml
    /// </summary>
    public partial class CategoryForm : AbstractEntryForm, IValidationFields
    {
        private Category C;
        public CategoryForm(ManageEntity callingInstance,int id = -1):base(callingInstance)
        {
            InitializeComponent();

            if (id != -1)
            {
                button.Content = "Update";
                title.Title = "Edit Category";
                C = CategoryDL.GetCategory(id);
            }
            else
            {
                button.Content = "Add";
                title.Title = "Add Category";
                C = new Category();
            }
            C.Id = id;
            DataContext = C;
        }

        public bool HasValidationErrors()
        {
            category_name.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            category_gst.TextBoxText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            return Validation.GetHasError(category_name.TextBoxText)
                || Validation.GetHasError(category_gst.TextBoxText);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            if (HasValidationErrors())
                return;
            try
            {
                if (C.Id != -1)
                {
                    C.Save(false);
                }
                else
                {
                    C.Save(true);
                }
                NavigateCallingForm();
            }
            catch(SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    UI.Components.MessageBox.Show($"Category name \"{C.Name}\" already exists", "Error", UI.Components.MessageBox.Type.Message);
                }
            }
        }

        private void cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigateCallingForm();
        }
    }
}
