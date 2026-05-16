using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using StationeryStoreManagementSystem.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;

namespace StationeryStoreManagementSystem.UI
{
    public partial class Dashboard : UserControl
    {
        private static readonly string[] DonutColors = {
            "#1565C0","#059669","#7C3AED","#DC2626","#F59E0B","#0891B2","#BE185D","#16A34A"
        };

        List<string> reports = new List<string>
        {
            "Today Sales Report",
            "Weekly Sales Report",
            "Monthly Sales Report",
            "Employees Sales Report",
            "Expiring Products Report"
        };

        public Dashboard()
        {
            InitializeComponent();
            todayDateText.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            Loaded += Dashboard_Loaded;
        }

        private void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            AlertService.AlertsUpdated += OnAlertsUpdated;
            Setup();
        }

        private void Setup()
        {
            if (Utils.CurrentEmployee is Admin)
            {
                Reports_cb.ItemSource = reports;
                report_row.Visibility = Visibility.Visible;
                alertsPanel.Visibility = Visibility.Visible;
                LoadKpis();
                AdminChart();
                LoadCategoryDonut();
                LoadAlerts(AlertService.CurrentAlerts.Where(a => !a.IsAcknowledged).ToList());
            }
            else if (Utils.CurrentEmployee is Cashier)
            {
                daily_row.Visibility = Visibility.Collapsed;
                weekly_row.Visibility = Visibility.Collapsed;
                report_row.Visibility = Visibility.Collapsed;
                alertsPanel.Visibility = Visibility.Collapsed;
                LoadKpis();
                CashierChart();
            }
        }

        private void LoadKpis()
        {
            try
            {
                var (revenue, profit, orders) = ProductDL.GetTodayKpis();
                kpiRevenue.Text = $"Rs. {revenue:N0}";
                kpiProfit.Text = $"Rs. {profit:N0}";
                kpiOrders.Text = orders.ToString();
                int lowStock = ProductDL.GetLowStockCount();
                kpiLowStock.Text = lowStock.ToString();
                kpiLowStock.Foreground = lowStock > 0
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#059669"));
            }
            catch { }
        }

        private void AdminChart()
        {
            try
            {
                // Daily
                SqlDataReader daily = Utils.ReadData(@"SELECT DATEPART(HOUR, O.Timestamp) AS Hour,
                    SUM(OD.Price * OD.Quantity) AS TotalSales,
                    SUM((OD.Price - PL.Price) * OD.Quantity) AS TotalProfit
                    FROM [Order] O
                    INNER JOIN OrderDetail OD ON O.Id = OD.OrderId
                    INNER JOIN PriceLog PL ON OD.ProductId=PL.ProductId AND OD.SupplierId=PL.SupplierId
                        AND PL.AddedOn=(SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId=OD.ProductId AND SupplierId=OD.SupplierId)
                    WHERE CONVERT(date, O.Timestamp) = CONVERT(date, GETDATE())
                    GROUP BY DATEPART(HOUR, O.Timestamp) ORDER BY Hour");

                var dailySale = new List<KeyValuePair<string, double>>();
                var dailyProfit = new List<KeyValuePair<string, double>>();
                while (daily.Read())
                {
                    string h = daily.GetInt32(0).ToString("00") + ":00";
                    dailySale.Add(new KeyValuePair<string, double>(h, (double)daily.GetDecimal(1)));
                    dailyProfit.Add(new KeyValuePair<string, double>(h, (double)daily.GetDecimal(2)));
                }
                DailySale.DataContext = dailySale;
                DailyProfit.DataContext = dailyProfit;

                // Weekly
                SqlDataReader weekly = Utils.ReadData(@"SELECT
                    CASE DATEPART(WEEKDAY, O.Timestamp)
                        WHEN 1 THEN 'Sun' WHEN 2 THEN 'Mon' WHEN 3 THEN 'Tue'
                        WHEN 4 THEN 'Wed' WHEN 5 THEN 'Thu' WHEN 6 THEN 'Fri' WHEN 7 THEN 'Sat' END AS Day,
                    SUM(OD.Price * OD.Quantity) AS TotalSales,
                    SUM((OD.Price - PL.Price) * OD.Quantity) AS TotalProfit
                    FROM [Order] O
                    INNER JOIN OrderDetail OD ON O.Id = OD.OrderId
                    INNER JOIN PriceLog PL ON OD.ProductId=PL.ProductId AND OD.SupplierId=PL.SupplierId
                        AND PL.AddedOn=(SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId=OD.ProductId AND SupplierId=OD.SupplierId)
                    WHERE DATEPART(WEEK, O.Timestamp)=DATEPART(WEEK, GETDATE())
                    GROUP BY DATEPART(WEEKDAY, O.Timestamp) ORDER BY DATEPART(WEEKDAY, O.Timestamp)");

                var weeklySale = new List<KeyValuePair<string, double>>();
                var weeklyProfit = new List<KeyValuePair<string, double>>();
                while (weekly.Read())
                {
                    weeklySale.Add(new KeyValuePair<string, double>(weekly.GetString(0), (double)weekly.GetDecimal(1)));
                    weeklyProfit.Add(new KeyValuePair<string, double>(weekly.GetString(0), (double)weekly.GetDecimal(2)));
                }
                WeeklySale.DataContext = weeklySale;
                WeeklyProfit.DataContext = weeklyProfit;

                // Monthly
                SqlDataReader monthly = Utils.ReadData(@"SELECT 'Wk '+CAST(DATEPART(WEEK, O.Timestamp) AS NVARCHAR) AS Wk,
                    SUM(OD.Price * OD.Quantity) AS TotalSales,
                    SUM((OD.Price - PL.Price) * OD.Quantity) AS TotalProfit
                    FROM [Order] O
                    INNER JOIN OrderDetail OD ON O.Id = OD.OrderId
                    INNER JOIN PriceLog PL ON OD.ProductId=PL.ProductId AND OD.SupplierId=PL.SupplierId
                        AND PL.AddedOn=(SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId=OD.ProductId AND SupplierId=OD.SupplierId)
                    WHERE DATEPART(MONTH, O.Timestamp)=DATEPART(MONTH, GETDATE())
                    GROUP BY DATEPART(WEEK, O.Timestamp) ORDER BY DATEPART(WEEK, O.Timestamp)");

                var monthlySale = new List<KeyValuePair<string, double>>();
                var monthlyProfit = new List<KeyValuePair<string, double>>();
                while (monthly.Read())
                {
                    monthlySale.Add(new KeyValuePair<string, double>(monthly.GetString(0), (double)monthly.GetDecimal(1)));
                    monthlyProfit.Add(new KeyValuePair<string, double>(monthly.GetString(0), (double)monthly.GetDecimal(2)));
                }
                MonthlySale.DataContext = monthlySale;
                MonthlyProfit.DataContext = monthlyProfit;
            }
            catch { }
        }

        private void LoadCategoryDonut()
        {
            try
            {
                var categories = ProductDL.GetCategorySalesToday();
                if (categories.Count == 0) { categoryDonutLabel.Text = "No sales today"; return; }
                DrawDonut(categoryDonutCanvas, categories, 100, 40);
                var legendItems = categories.Select((c, i) => new { Color = DonutColors[i % DonutColors.Length], Label = $"{c.Category}: Rs.{c.Sales:N0}" }).ToList();
                categoryLegend.ItemsSource = legendItems;
            }
            catch { }
        }

        private static void DrawDonut(Canvas canvas, List<(string Name, double Sales)> data, double outerR, double innerR)
        {
            canvas.Children.Clear();
            double total = data.Sum(d => d.Sales);
            double cx = canvas.Width / 2, cy = canvas.Height / 2;
            double startAngle = -Math.PI / 2;

            for (int i = 0; i < data.Count; i++)
            {
                double sweepAngle = data[i].Sales / total * 2 * Math.PI;
                var path = CreateDonutSlice(cx, cy, outerR, innerR, startAngle, sweepAngle,
                    (Color)ColorConverter.ConvertFromString(DonutColors[i % DonutColors.Length]));
                canvas.Children.Add(path);
                startAngle += sweepAngle;
            }

            // Center label
            var pct = new System.Windows.Controls.TextBlock
            {
                Text = $"{data.Count} cats",
                FontSize = 11, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.DimGray),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(pct, cx - 22);
            Canvas.SetTop(pct, cy - 8);
            canvas.Children.Add(pct);
        }

        private static System.Windows.Shapes.Path CreateDonutSlice(double cx, double cy, double outerR, double innerR,
            double startAngle, double sweepAngle, Color color)
        {
            bool isLarge = sweepAngle > Math.PI;
            double endAngle = startAngle + sweepAngle;

            Point outerStart = new Point(cx + outerR * Math.Cos(startAngle), cy + outerR * Math.Sin(startAngle));
            Point outerEnd = new Point(cx + outerR * Math.Cos(endAngle), cy + outerR * Math.Sin(endAngle));
            Point innerStart = new Point(cx + innerR * Math.Cos(endAngle), cy + innerR * Math.Sin(endAngle));
            Point innerEnd = new Point(cx + innerR * Math.Cos(startAngle), cy + innerR * Math.Sin(startAngle));

            var fig = new PathFigure { StartPoint = outerStart, IsClosed = true };
            fig.Segments.Add(new ArcSegment(outerEnd, new Size(outerR, outerR), 0, isLarge, SweepDirection.Clockwise, true));
            fig.Segments.Add(new LineSegment(innerStart, true));
            fig.Segments.Add(new ArcSegment(innerEnd, new Size(innerR, innerR), 0, isLarge, SweepDirection.Counterclockwise, true));

            return new System.Windows.Shapes.Path
            {
                Data = new PathGeometry(new[] { fig }),
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2
            };
        }

        private void CashierChart()
        {
            try
            {
                SqlDataReader monthly = Utils.ReadData($@"SELECT 'Wk '+CAST(DATEPART(WEEK, O.Timestamp) AS NVARCHAR),
                    SUM(OD.Price*OD.Quantity), SUM((OD.Price-PL.Price)*OD.Quantity)
                    FROM [Order] O
                    INNER JOIN OrderDetail OD ON O.Id=OD.OrderId
                    INNER JOIN PriceLog PL ON OD.ProductId=PL.ProductId AND OD.SupplierId=PL.SupplierId
                        AND PL.AddedOn=(SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId=OD.ProductId AND SupplierId=OD.SupplierId)
                    WHERE DATEPART(MONTH,O.Timestamp)=DATEPART(MONTH,GETDATE()) AND O.EmployeeId={Utils.CurrentEmployee.Id}
                    GROUP BY DATEPART(WEEK,O.Timestamp) ORDER BY DATEPART(WEEK,O.Timestamp)");

                var data = new List<KeyValuePair<string, double>>();
                while (monthly.Read())
                    data.Add(new KeyValuePair<string, double>(monthly.GetString(0), (double)monthly.GetDecimal(1)));
                MonthlySale.DataContext = data;
            }
            catch { }
        }

        private void LoadAlerts(List<AlertItem> alerts)
        {
            alertsContainer.Children.Clear();
            if (alerts.Count == 0)
            {
                alertsContainer.Children.Add(new TextBlock { Text = "No active alerts.", Foreground = new SolidColorBrush(Colors.Gray), FontSize = 13 });
                alertBadge.Visibility = Visibility.Collapsed;
                return;
            }

            int unread = alerts.Count(a => !a.IsAcknowledged);
            if (unread > 0) { alertBadge.Visibility = Visibility.Visible; alertBadgeCount.Text = unread.ToString(); }
            else alertBadge.Visibility = Visibility.Collapsed;

            foreach (var alert in alerts.OrderByDescending(a => a.Severity).ThenBy(a => a.IsAcknowledged))
            {
                var card = CreateAlertCard(alert);
                alertsContainer.Children.Add(card);
            }
        }

        private Border CreateAlertCard(AlertItem alert)
        {
            bool isExpiry = alert.Type == AlertItem.AlertType.Expiring;
            string bgColor = alert.Severity == "Critical" ? "#FEE2E2" : "#FEF9C3";
            string borderColor = alert.Severity == "Critical" ? "#FECACA" : "#FDE68A";
            string iconColor = isExpiry ? "#F59E0B" : "#DC2626";

            var card = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderColor)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 4, 0, 4),
                Padding = new Thickness(14, 10, 14, 10),
                Opacity = alert.IsAcknowledged ? 0.5 : 1.0
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var icon = new TextBlock { Text = isExpiry ? "⏰" : "📦", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) };
            Grid.SetColumn(icon, 0);

            var info = new StackPanel();
            info.Children.Add(new TextBlock { Text = alert.Title, FontWeight = FontWeights.Bold, FontSize = 13, Foreground = new SolidColorBrush(Colors.DarkSlateGray) });
            info.Children.Add(new TextBlock { Text = alert.Message, FontSize = 12, Foreground = new SolidColorBrush(Colors.SlateGray), TextWrapping = TextWrapping.Wrap });
            Grid.SetColumn(info, 1);

            var btnPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };

            if (isExpiry && !alert.DiscountApplied)
            {
                var discBtn = new Button { Content = "Manage Discount", Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")), Foreground = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0), Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(0, 0, 0, 4), Cursor = System.Windows.Input.Cursors.Hand, FontSize = 11 };
                var capturedAlert = alert;

                discBtn.Click += (s, e) => {
                    // Popup open hoga aur code yahan ruk jayega jab tak popup band na ho
                    DiscountPopup popup = new DiscountPopup(capturedAlert.ProductId, capturedAlert.ProductName);
                    popup.Owner = Window.GetWindow(this);
                    popup.ShowDialog();

                    // Jaise hi Admin "Apply Discount" dabayega tou IsSuccess true ho jayega
                    if (popup.IsSuccess)
                    {
                        // 1. Naya bulletproof method call kiya jo DB mein exact amount save karega
                        AlertService.AcknowledgeAlert(capturedAlert.ProductId, capturedAlert.Type, $"Applied manual discount of Rs. {popup.NewDiscount}");

                        // 2. Dashboard ko refresh kar do taake ye card screen se permanently hat jaye
                        RefreshAlerts_Click(null, null);
                    }
                };
                btnPanel.Children.Add(discBtn);
            }

            var ackBtn = new Button { Content = alert.IsAcknowledged ? "✓ Noted" : "Acknowledge", Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(alert.IsAcknowledged ? "#D1D5DB" : "#6B7280")), Foreground = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0), Padding = new Thickness(10, 4, 10, 4), Cursor = System.Windows.Input.Cursors.Hand, FontSize = 11 };
            var capturedA = alert;
            ackBtn.Click += (s, e) => AlertService.AcknowledgeAlert(capturedA.ProductId, capturedA.Type);
            btnPanel.Children.Add(ackBtn);

            Grid.SetColumn(btnPanel, 2);
            grid.Children.Add(icon); grid.Children.Add(info); grid.Children.Add(btnPanel);
            card.Child = grid;
            return card;
        }

        private void OnAlertsUpdated(object? sender, List<AlertItem> alerts)
        {

            var activeAlerts = alerts.Where(a => !a.IsAcknowledged).ToList();
            Dispatcher.Invoke(() => LoadAlerts(activeAlerts));
        }

        private async void RefreshAlerts_Click(object sender, RoutedEventArgs e)
        {
            await AlertService.CheckAfterSaleAsync();

            var activeAlerts = AlertService.CurrentAlerts.Where(a => !a.IsAcknowledged).ToList();
            LoadAlerts(activeAlerts);

            LoadKpis();
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            var bindings = new List<(string, string)>
            {
                ("From","From"), ("IsViewed","IsViewed"), ("Notification","Notification")
            };
            if (GlobalSettings.DisplayIds) bindings.Insert(0, ("Id", "Id"));
            ((Border)Parent).Child = new ManageEntity("Manage Notifications", typeof(Notification).Name,
                NotificationDL.GetNotificationHistory, bindings, new List<string> { "From" },
                typeof(NotificationForm), false, false, null, typeof(ViewNotification));
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (Reports_cb.SelectedItem != null)
                Process.Start(new ProcessStartInfo("CrystalReportApp.exe", Reports_cb.SelectedItem.ToString()) { UseShellExecute = true });
        }
    }
}
