using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StationeryStoreManagementSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            string message;
            if (ex.GetType()==typeof(SqlException))
            {
                SqlException sqlException = ex as SqlException;
                message = $"Error {sqlException.Number} ";
                switch (sqlException.Number)
                {
                    case 2627:
                        message += "Unique Data Duplication Attempt";
                        break;
                    default:
                        message += "Unknown Database Server Exception";
                        break;
                }
                UI.Components.MessageBox.Show(message, "Error", UI.Components.MessageBox.Type.Message,20);
            }
            e.Handled = true;
        }
    }
}
