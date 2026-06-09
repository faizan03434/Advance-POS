using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;

namespace StationeryStoreManagementSystem.UI
{
    public partial class BIDashboard : UserControl, System.ComponentModel.INotifyPropertyChanged
    {
        private ISeries[] _salesSeries;
        public ISeries[] SalesSeries 
        { 
            get => _salesSeries; 
            set { _salesSeries = value; OnPropertyChanged(nameof(SalesSeries)); } 
        }

        private Axis[] _xAxes;
        public Axis[] XAxes 
        { 
            get => _xAxes; 
            set { _xAxes = value; OnPropertyChanged(nameof(XAxes)); } 
        }

        private Axis[] _yAxes;
        public Axis[] YAxes 
        { 
            get => _yAxes; 
            set { _yAxes = value; OnPropertyChanged(nameof(YAxes)); } 
        }

        // Product Performance Chart
        private ISeries[] _productSeries;
        public ISeries[] ProductSeries
        {
            get => _productSeries;
            set { _productSeries = value; OnPropertyChanged(nameof(ProductSeries)); }
        }

        private Axis[] _productXAxes;
        public Axis[] ProductXAxes
        {
            get => _productXAxes;
            set { _productXAxes = value; OnPropertyChanged(nameof(ProductXAxes)); }
        }

        public BIDashboard()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // 1. Load Market Basket Analysis (Async)
                var marketBasketTask = Task.Run(() => AdvancedBIDataDL.GetFrequentlyBoughtTogether());
                
                // 2. Load Cashier Anomalies (Async)
                var anomaliesTask = Task.Run(() => AdvancedBIDataDL.GetCashierDiscountAnomalies());

                // 3. Get AI Briefing from Local Files (New System)
                var briefingTask = Task.Run(() => {
                    try
                    {
                        string reportDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AI_Reports");
                        if (!System.IO.Directory.Exists(reportDir)) return "No AI briefings generated yet. Click 'Refresh' to start.";

                        var directory = new System.IO.DirectoryInfo(reportDir);
                        var latestFile = directory.GetFiles("BI_Report_*.txt")
                                                 .OrderByDescending(f => f.CreationTime)
                                                 .FirstOrDefault();

                        if (latestFile != null)
                        {
                            return System.IO.File.ReadAllText(latestFile.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        return "Error reading latest AI report: " + ex.Message;
                    }
                    return "Generating master-level AI briefing... Ensure AI API is configured in Settings and AlertService is running.";
                });

                // 4. Load Sales History
                var salesHistoryTask = Task.Run(() => {
                    string query = @"
                        SELECT TOP 30 CAST(Timestamp AS DATE) as SaleDate, SUM(od.Price * od.Quantity) as DailyRevenue
                        FROM [Order] o
                        JOIN OrderDetail od ON o.Id = od.OrderId
                        WHERE Timestamp >= DATEADD(DAY, -30, GETDATE())
                        GROUP BY CAST(Timestamp AS DATE)
                        ORDER BY SaleDate ASC";
                    return DataHandler.FillDataTable(query);
                });

                // 5. Load Product Performance
                var productPerfTask = Task.Run(() => AdvancedBIDataDL.GetProductPerformance(30));

                // Wait for data
                DataTable marketBasketDt = await marketBasketTask;
                DataTable anomaliesDt = await anomaliesTask;
                string briefing = await briefingTask;
                DataTable salesHistoryDt = await salesHistoryTask;
                DataTable productPerfDt = await productPerfTask;

                // Update Charts
                UpdateSalesChart(salesHistoryDt);
                UpdateProductChart(productPerfDt);

                // Update UI - Market Basket: bind as anonymous objects for DataGrid column binding
                if (marketBasketDt.Rows.Count > 0)
                {
                    MarketBasketGrid.ItemsSource = marketBasketDt.Rows.Cast<DataRow>().Select(r => new
                    {
                        ProductA = r["ProductA"].ToString(),
                        ProductB = r["ProductB"].ToString(),
                        CoOccurrenceRate = Convert.ToDouble(r["CoOccurrenceRate"])
                    }).ToList();
                }
                else
                {
                    MarketBasketGrid.ItemsSource = null;
                }

                AIBriefingText.Text = briefing;

                var anomalyRows = anomaliesDt.Rows.Cast<DataRow>().Select(r => new {
                    CashierName = r["CashierName"].ToString(),
                    Message = $"Discount Rate: {Convert.ToDouble(r["CashierDiscountRate"]):P1} (Store Avg: {Convert.ToDouble(r["GlobalAvgDiscountRate"]):P1})"
                }).ToList();

                if (anomalyRows.Count > 0)
                    AnomaliesList.ItemsSource = anomalyRows;
                else
                    AnomaliesList.ItemsSource = new[] { new { CashierName = "No anomalies detected", Message = "All cashiers are within normal discount range." } };
            }
            catch (Exception ex)
            {
                AIBriefingText.Text = "Error loading BI data: " + ex.Message;
            }
        }

        private void UpdateSalesChart(DataTable dt)
        {
            var history = dt.Rows.Cast<DataRow>()
                .Select(r => Convert.ToDouble(r["DailyRevenue"]))
                .ToList();

            var dates = dt.Rows.Cast<DataRow>()
                .Select(r => (DateTime)r["SaleDate"])
                .ToList();

            if (history.Count == 0) return;

            // Forecast logic
            double lastVal = history.Last();
            DateTime lastDate = dates.Last();
            double avgGrowth = history.Count > 1 ? (history.Last() - history.First()) / history.Count : 0;
            
            var forecast = new List<double>();
            var allLabels = dates.Select(d => d.ToString("dd MMM")).ToList();

            for (int i = 0; i < history.Count - 1; i++) forecast.Add(double.NaN);
            forecast.Add(lastVal);
            
            for (int i = 1; i <= 7; i++) 
            {
                forecast.Add(Math.Max(0, lastVal + (avgGrowth * i)));
                allLabels.Add(lastDate.AddDays(i).ToString("dd MMM"));
            }

            SalesSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = history,
                    Name = "Actual Revenue",
                    Fill = null,
                    GeometrySize = 5,
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue, 3)
                },
                new LineSeries<double>
                {
                    Values = forecast,
                    Name = "Trend Projection",
                    Fill = null,
                    GeometrySize = 8,
                    Stroke = new SolidColorPaint(SKColors.Orange, 3) { PathEffect = new DashEffect(new float[] { 10, 5 }) }
                }
            };

            XAxes = new Axis[] { new Axis { Name = "Timeline", Labels = allLabels.ToArray(), LabelsRotation = 45 } };
            YAxes = new Axis[] { new Axis { Labeler = value => value.ToString("C0"), Name = "Revenue" } };
            
            OnPropertyChanged(nameof(SalesSeries));
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));
        }

        private void UpdateProductChart(DataTable dt)
        {
            var topProducts = dt.Rows.Cast<DataRow>().Take(5).ToList();
            var values = topProducts.Select(r => Convert.ToDouble(r["UnitsSold"])).ToList();
            var labels = topProducts.Select(r => r["Name"].ToString()).ToList();

            ProductSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = values,
                    Name = "Units Sold",
                    Fill = new SolidColorPaint(SKColors.MediumSeaGreen),
                    Padding = 2
                }
            };

            ProductXAxes = new Axis[] { new Axis { Labels = labels.ToArray(), LabelsRotation = 15 } };
            OnPropertyChanged(nameof(ProductSeries));
            OnPropertyChanged(nameof(ProductXAxes));
        }

        private async void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            AIBriefingText.Text = "Running Deep AI Analysis... This may take up to a minute depending on your provider.";
            
            try
            {
                // Trigger a fresh AI run in the background service
                await StationeryStoreManagementSystem.Services.AlertService.RunEndOfDayBriefingAsync();
                
                // Reload dashboard data
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                AIBriefingText.Text = "Refresh Error: " + ex.Message;
            }
            finally
            {
                RefreshButton.IsEnabled = true;
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

    }
}
