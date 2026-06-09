using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using StationeryStoreManagementSystem.DL;

namespace StationeryStoreManagementSystem.BL
{
    public class CustomerRetentionBL
    {
        public class CustomerSegment
        {
            public string CustomerName { get; set; }
            public int Recency { get; set; }
            public int Frequency { get; set; }
            public double Monetary { get; set; }
            public string Category { get; set; } // Loyal, At Risk, Lost
        }

        /// <summary>
        /// Calculates RFM (Recency, Frequency, Monetary) scores and segments customers.
        /// </summary>
        /// <returns>A list of segmented customers.</returns>
        public static List<CustomerSegment> CalculateRFM()
        {
            string query = @"
                SELECT 
                    o.CustomerName,
                    DATEDIFF(day, MAX(o.Timestamp), GETDATE()) AS Recency,
                    COUNT(DISTINCT o.Id) AS Frequency,
                    SUM(od.Price * od.Quantity) AS Monetary
                FROM [Order] o
                JOIN OrderDetail od ON o.Id = od.OrderId
                WHERE o.CustomerName IS NOT NULL AND o.CustomerName <> ''
                GROUP BY o.CustomerName";

            DataTable dt = DataHandler.FillDataTable(query);
            List<CustomerSegment> segments = new List<CustomerSegment>();

            if (dt.Rows.Count == 0) return segments;

            // 1. Convert to objects
            foreach (DataRow row in dt.Rows)
            {
                segments.Add(new CustomerSegment
                {
                    CustomerName = row["CustomerName"].ToString(),
                    Recency = Convert.ToInt32(row["Recency"]),
                    Frequency = Convert.ToInt32(row["Frequency"]),
                    Monetary = Convert.ToDouble(row["Monetary"])
                });
            }

            // 2. Simple Segmentation Logic
            // Loyal: Frequency > 5 AND Recency < 30
            // At Risk: Frequency > 2 AND Recency >= 30 AND Recency < 90
            // Lost: Recency >= 90 OR (Frequency <= 2 AND Recency >= 30)

            foreach (var s in segments)
            {
                if (s.Frequency > 5 && s.Recency < 30)
                {
                    s.Category = "Loyal";
                }
                else if (s.Recency >= 90 || (s.Frequency <= 2 && s.Recency >= 30))
                {
                    s.Category = "Lost";
                }
                else if (s.Frequency > 2 && s.Recency >= 30)
                {
                    s.Category = "At Risk";
                }
                else
                {
                    // Default to 'Regular' or similar if not fitting precisely, 
                    // but the prompt asked for Loyal, At Risk, Lost.
                    // Let's refine the logic to ensure everyone fits.
                    if (s.Recency < 30) s.Category = "Loyal"; // Recent customers are good
                    else if (s.Recency < 60) s.Category = "At Risk";
                    else s.Category = "Lost";
                }
            }

            return segments;
        }
    }
}
