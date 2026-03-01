using StationeryStoreManagementSystem.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StationeryStoreManagementSystem.UI
{
    public class AbstractEntryForm : UserControl
    {
        private ManageEntity callingInstance;
        protected bool isEdit = false;

        public AbstractEntryForm(ManageEntity callingInstance):base()
        {
            this.callingInstance = callingInstance;
        }
        protected void NavigateCallingForm()
        {
            ((Border)Parent).Child = callingInstance;
            callingInstance.RefreshData();
        }
    }
}
