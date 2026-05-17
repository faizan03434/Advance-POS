using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;
using StationeryStoreManagementSystem.DL;

namespace StationeryStoreManagementSystem.UI
{
    public partial class NotificationWindow : Window
    {
        public NotificationWindow()
        {
            InitializeComponent();
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            LowStockListPanel.Children.Clear();
            DiscountListPanel.Children.Clear();

            // Asal Database se data utha raha hai (Latest pehle)
            string query = @"
        SELECT N.Content, N.AddedOn, (U.FirstName + ' ' + U.LastName) AS EmployeeName 
        FROM [dbo].[Notification] N
        INNER JOIN [dbo].[User] U ON N.AddedBy = U.Id 
        ORDER BY N.AddedOn DESC";

            try
            {
                using (SqlDataReader reader = Utils.ReadData(query))
                {
                    while (reader.Read())
                    {
                        string content = reader["Content"].ToString();
                        DateTime addedOn = Convert.ToDateTime(reader["AddedOn"]);
                        string empName = reader["EmployeeName"].ToString();

                        // "Is Viewed" ki jagah Date aur Time (e.g., 17 May 2026, 03:15 PM)
                        string timeString = addedOn.ToString("dd MMM yyyy, hh:mm tt") + $"  •  Action By: {empName}";

                        // Card Design
                        Border card = new Border
                        {
                            Background = new SolidColorBrush(Colors.White),
                            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0")),
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(5),
                            Padding = new Thickness(10),
                            Margin = new Thickness(0, 0, 0, 8)
                        };

                        StackPanel sp = new StackPanel();
                        sp.Children.Add(new TextBlock
                        {
                            Text = content,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                            TextWrapping = TextWrapping.Wrap
                        });

                        sp.Children.Add(new TextBlock
                        {
                            Text = timeString,
                            FontSize = 11,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                            Margin = new Thickness(0, 5, 0, 0)
                        });

                        card.Child = sp;

                        // Tabs mein alag alag karna
                        if (content.ToLower().Contains("discount"))
                        {
                            DiscountListPanel.Children.Add(card);
                        }
                        else
                        {
                            LowStockListPanel.Children.Add(card);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading notifications: " + ex.Message);
            }
        }
    }
}