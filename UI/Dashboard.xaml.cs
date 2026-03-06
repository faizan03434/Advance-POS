using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;

namespace StationeryStoreManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        List<string> reports = new List<string>()
            {
                "Today Sales Report",
                "Weekly Sales Report",
                "Monthly Sales Report",
                "Employees Sales Report"
            };

        public Dashboard()
        {
            InitializeComponent();
            Setup();
        }
        private void Setup()
        {
            if (Utils.CurrentEmployee is Admin)
            {
                Reports_cb.ItemSource = reports;
                AdminChart();
            }
            else if (Utils.CurrentEmployee is Cashier)
            {
                CashierChart();
            }
        }
        private void AdminChart()
        {
            SqlDataReader daily = Utils.ReadData(@"SELECT DATEPART(HOUR, O.Timestamp) AS Hour, SUM(OD.Price * OD.Quantity) AS TotalSales, SUM((OD.Price - PL.Price) * OD.Quantity) AS TotalProfit
                                                    FROM [Order] O
                                                    INNER JOIN OrderDetail OD ON O.Id = OD.OrderId
                                                    INNER JOIN PriceLog PL ON OD.ProductId = PL.ProductId AND OD.SupplierId = PL.SupplierId AND PL.AddedOn = (SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId = OD.ProductId AND SupplierId = OD.SupplierId)
                                                    WHERE CONVERT(date, O.Timestamp) = CONVERT(date, GETDATE())
                                                    GROUP BY DATEPART(HOUR, O.Timestamp)
                                                    ORDER BY Hour");

            List<KeyValuePair<int, double>> DailySaleData = new List<KeyValuePair<int, double>>();
            List<KeyValuePair<int, double>> DailyProfitData = new List<KeyValuePair<int, double>>();

            while (daily.Read()) 
            {
                int hour = daily.GetInt32(0);
                double sale = (double)daily.GetDecimal(1);
                double profit = (double)daily.GetDecimal(2);

                DailySaleData.Add(new KeyValuePair<int, double>(hour, sale));
                DailyProfitData.Add(new KeyValuePair<int, double>(hour, profit));
            }
            DailySale.DataContext = DailySaleData;
            DailyProfit.DataContext = DailyProfitData;

            SqlDataReader weekly = Utils.ReadData(@"SELECT CASE 
                                                            WHEN DATEPART(WEEKDAY, O.Timestamp) = 1 THEN 'Sunday'
                                                            WHEN DATEPART(WEEKDAY, O.Timestamp) = 2 THEN 'Monday'
                                                            WHEN DATEPART(WEEKDAY, O.Timestamp) = 3 THEN 'Tuesday'
                                                            WHEN DATEPART(WEEKDAY, O.Timestamp) = 4 THEN 'Wednesday'
                                                            WHEN DATEPART(WEEKDAY, O.Timestamp) = 5 THEN 'Thursday'
                                                            WHEN DATEPART(WEEKDAY, O.Timestamp) = 6 THEN 'Friday'
                                                            WHEN DATEPART(WEEKDAY, O.Timestamp) = 7 THEN 'Saturday'
                                                        END AS WeekdayName, SUM(OD.Price * OD.Quantity) AS TotalSales, SUM((OD.Price - PL.Price) * OD.Quantity) AS TotalProfit
                                                    FROM [Order] O
                                                    INNER JOIN OrderDetail OD ON O.Id = OD.OrderId
                                                    INNER JOIN PriceLog PL ON OD.ProductId = PL.ProductId AND OD.SupplierId = PL.SupplierId AND PL.AddedOn = (SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId = OD.ProductId AND SupplierId = OD.SupplierId)
                                                    WHERE DATEPART(WEEK, O.Timestamp) = DATEPART(WEEK, GETDATE())
                                                    GROUP BY DATEPART(WEEKDAY, O.Timestamp)
                                                    ORDER BY DATEPART(WEEKDAY, O.Timestamp)");

            List<KeyValuePair<string, double>> WeeklySaleData = new List<KeyValuePair<string, double>>();
            List<KeyValuePair<string, double>> WeeklyProfitData = new List<KeyValuePair<string, double>>();

            while (weekly.Read())
            {
                string week = weekly.GetString(0);
                double sale = (double)weekly.GetDecimal(1);
                double profit = (double)weekly.GetDecimal(2);

                WeeklySaleData.Add(new KeyValuePair<string, double>(week, sale));
                WeeklyProfitData.Add(new KeyValuePair<string, double>(week, profit));
            }
            WeeklySale.DataContext = WeeklySaleData;
            WeeklyProfit.DataContext = WeeklyProfitData;

            SqlDataReader monthly = Utils.ReadData(@"SELECT DATEPART(WEEK, O.Timestamp) AS WeekNumber, SUM(OD.Price * OD.Quantity) AS TotalSales, SUM((OD.Price - PL.Price) * OD.Quantity) AS TotalProfit
                                                    FROM [Order] O
                                                    INNER JOIN OrderDetail OD ON O.Id = OD.OrderId
                                                    INNER JOIN PriceLog PL ON OD.ProductId = PL.ProductId AND OD.SupplierId = PL.SupplierId AND PL.AddedOn = (SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId = OD.ProductId AND SupplierId = OD.SupplierId)
                                                    WHERE DATEPART(MONTH, O.Timestamp) = DATEPART(MONTH, GETDATE())
                                                    GROUP BY DATEPART(WEEK, O.Timestamp)
                                                    ORDER BY WeekNumber");

            List<KeyValuePair<int, double>> MonthlySaleData = new List<KeyValuePair<int, double>>();
            List<KeyValuePair<int, double>> MonthlyProfitData = new List<KeyValuePair<int, double>>();

            while (monthly.Read())
            {
                int month = monthly.GetInt32(0);
                double sale = (double)monthly.GetDecimal(1);
                double profit = (double)monthly.GetDecimal(2);

                MonthlySaleData.Add(new KeyValuePair<int, double>(month, sale));
                MonthlyProfitData.Add(new KeyValuePair<int, double>(month, profit));
            }
            MonthlySale.DataContext = MonthlySaleData;
            MonthlyProfit.DataContext = MonthlyProfitData;
        }

        private void CashierChart() 
        {
            daily_row.Height = new GridLength(0);
            weekly_row.Height = new GridLength(0);
            report_row.Height = new GridLength(0);
            SqlDataReader monthly = Utils.ReadData($"SELECT DATEPART(WEEK, O.Timestamp) AS WeekNumber, SUM(OD.Price * OD.Quantity) AS TotalSales, SUM((OD.Price - PL.Price) * OD.Quantity) AS TotalProfit"+
                                                    $" FROM [Order] O"+
                                                    $" INNER JOIN OrderDetail OD ON O.Id = OD.OrderId"+
                                                    $" INNER JOIN PriceLog PL ON OD.ProductId = PL.ProductId AND OD.SupplierId = PL.SupplierId AND PL.AddedOn = (SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId = OD.ProductId AND SupplierId = OD.SupplierId)"+
                                                    $" WHERE DATEPART(MONTH, O.Timestamp) = DATEPART(MONTH, GETDATE()) AND O.EmployeeId = {Utils.CurrentEmployee.Id}"+
                                                    $" GROUP BY DATEPART(WEEK, O.Timestamp)"+
                                                    $" ORDER BY WeekNumber");

            List<KeyValuePair<int, double>> MonthlySaleData = new List<KeyValuePair<int, double>>();
            List<KeyValuePair<int, double>> MonthlyProfitData = new List<KeyValuePair<int, double>>();

            while (monthly.Read())
            {
                int month = monthly.GetInt32(0);
                double sale = (double)monthly.GetDecimal(1);
                double profit = (double)monthly.GetDecimal(2);

                MonthlySaleData.Add(new KeyValuePair<int, double>(month, sale));
                MonthlyProfitData.Add(new KeyValuePair<int, double>(month, profit));
            }
            MonthlySale.DataContext = MonthlySaleData;
            MonthlyProfit.DataContext = MonthlyProfitData;
        }
        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            List<(string, string)> bindings = new List<(string, string)>
            {
                ("From","From"),
                ("IsViewed","IsViewed"),
                ("Notification","Notification")
            };
            if (GlobalSettings.DisplayIds == true)
                bindings.Insert(0, ("Id", "Id"));
            ((Border)Parent).Child = new UI.ManageEntity("Manage Notifications",
                                                typeof(Notification).Name,
                                                NotificationDL.GetNotifications_View,
                                                bindings,
                                                new List<string> { "From" },
                                                typeof(NotificationForm),
                                                false,
                                                false,
                                                null,
                                                typeof(ViewNotification));
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (Reports_cb.SelectedItem != null)
                Process.Start("CrystalReportApp.exe", Reports_cb.SelectedItem.ToString());
        }
    }
}
