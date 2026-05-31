using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using StationeryStoreManagementSystem.DL;

namespace StationeryStoreManagementSystem.BL
{
    public class PredictiveAnalyticsBL
    {
        private class SalesData
        {
            public float Quantity { get; set; }
        }

        private class SalesForecast
        {
            public float[] Forecast { get; set; }
        }

        /// <summary>
        /// Forecasts when a product's stock will hit 0 using SSA (Singular Spectrum Analysis).
        /// </summary>
        /// <param name="productId">The ID of the product to analyze.</param>
        /// <param name="currentStock">The current total stock of the product.</param>
        /// <returns>The estimated date of depletion, or null if it cannot be determined.</returns>
        public static DateTime? ForecastStockDepletion(int productId, int currentStock)
        {
            if (currentStock <= 0) return DateTime.Now;

            // 1. Get historical daily sales
            string query = $@"
                SELECT CAST(SUM(od.Quantity) AS FLOAT) as DailyQty
                FROM OrderDetail od
                JOIN [Order] o ON od.OrderId = o.Id
                WHERE od.ProductId = {productId}
                GROUP BY CAST(o.Timestamp AS DATE)
                ORDER BY CAST(o.Timestamp AS DATE) ASC";

            DataTable dt = DataHandler.FillDataTable(query);
            if (dt.Rows.Count < 10) return null; // Need enough data for SSA

            List<SalesData> history = dt.AsEnumerable()
                .Select(row => new SalesData { Quantity = Convert.ToSingle(row["DailyQty"]) })
                .ToList();

            // 2. Setup ML.NET context
            MLContext mlContext = new MLContext();
            IDataView dataView = mlContext.Data.LoadFromEnumerable(history);

            // 3. Configure SSA forecasting
            // WindowSize: 7 (weekly seasonality), SeriesLength: total data points
            int windowSize = Math.Min(7, history.Count / 2);
            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(SalesForecast.Forecast),
                inputColumnName: nameof(SalesData.Quantity),
                windowSize: windowSize,
                seriesLength: history.Count,
                trainSize: history.Count,
                horizon: 30, // Forecast up to 30 days ahead
                confidenceLevel: 0.95f);

            // 4. Train and Forecast
            var model = forecastingPipeline.Fit(dataView);
            var forecastingEngine = model.CreateTimeSeriesEngine<SalesData, SalesForecast>(mlContext);
            var predictions = forecastingEngine.Predict();

            // 5. Calculate depletion date
            float remainingStock = currentStock;
            for (int i = 0; i < predictions.Forecast.Length; i++)
            {
                float forecastedDailySale = Math.Max(0, predictions.Forecast[i]);
                remainingStock -= forecastedDailySale;

                if (remainingStock <= 0)
                {
                    return DateTime.Now.AddDays(i + 1);
                }
            }

            // If it doesn't hit 0 in 30 days, we can estimate based on average of forecast
            float avgForecastedSale = predictions.Forecast.Average();
            if (avgForecastedSale > 0)
            {
                int remainingDays = (int)Math.Ceiling(remainingStock / avgForecastedSale);
                return DateTime.Now.AddDays(30 + remainingDays);
            }

            return null;
        }
    }
}
