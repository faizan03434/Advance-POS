using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace StationeryStoreManagementSystem.DL
{
    /// <summary>
    /// Data Layer class for advanced Business Intelligence analytical data extraction.
    /// </summary>
    public static class AdvancedBIDataDL
    {
        /// <summary>
        /// Identifies cashiers who apply percentage-based discounts 20% more often than the store average.
        /// Optimized using CTEs to calculate global average and per-cashier rates in a single pass.
        /// </summary>
        /// <returns>DataTable containing cashier names and their discount frequency metrics.</returns>
        public static DataTable GetCashierDiscountAnomalies()
        {
            // Analytical query:
            // 1. Calculate per-cashier stats (total items vs discounted items).
            // 2. Calculate global store average.
            // 3. Filter cashiers exceeding 1.2x (20% more) of the global average.
            string query = @"
                WITH CashierStats AS (
                    SELECT 
                        u.FirstName + ' ' + u.LastName AS CashierName,
                        COUNT(od.OrderId) AS TotalItemsSold,
                        SUM(CASE WHEN od.DiscountAmount > 0 THEN 1 ELSE 0 END) AS DiscountedItemsCount
                    FROM [Order] o
                    JOIN OrderDetail od ON o.Id = od.OrderId
                    JOIN [User] u ON o.EmployeeId = u.Id
                    GROUP BY u.Id, u.FirstName, u.LastName
                ),
                StoreStats AS (
                    SELECT 
                        CAST(SUM(DiscountedItemsCount) AS FLOAT) / NULLIF(SUM(TotalItemsSold), 0) AS GlobalAvgDiscountRate
                    FROM CashierStats
                )
                SELECT 
                    CashierName,
                    TotalItemsSold,
                    DiscountedItemsCount,
                    CAST(DiscountedItemsCount AS FLOAT) / NULLIF(TotalItemsSold, 0) AS CashierDiscountRate,
                    GlobalAvgDiscountRate
                FROM CashierStats, StoreStats
                WHERE (CAST(DiscountedItemsCount AS FLOAT) / NULLIF(TotalItemsSold, 0)) > (GlobalAvgDiscountRate * 1.2)
                ORDER BY CashierDiscountRate DESC";

            return DataHandler.FillDataTable(query);
        }

        /// <summary>
        /// Analyzes transaction history to find pairs of products frequently bought together.
        /// Returns pairs that appear in the same transaction more than 15% of the time (Support > 0.15).
        /// </summary>
        /// <returns>DataTable containing pairs of products and their co-occurrence frequency.</returns>
        public static DataTable GetFrequentlyBoughtTogether()
        {
            // Market Basket Analysis query:
            // 1. Get total order count for support calculation.
            // 2. Self-join OrderDetail to find pairs in the same OrderId.
            // 3. Filter by support threshold > 15%.
            string query = @"
                WITH TotalOrders AS (
                    SELECT COUNT(*) AS TotalCount FROM [Order]
                ),
                ProductPairs AS (
                    SELECT 
                        od1.ProductId AS ProductIdA, 
                        od2.ProductId AS ProductIdB,
                        COUNT(DISTINCT od1.OrderId) AS PairCount
                    FROM OrderDetail od1
                    JOIN OrderDetail od2 ON od1.OrderId = od2.OrderId AND od1.ProductId < od2.ProductId
                    GROUP BY od1.ProductId, od2.ProductId
                )
                SELECT 
                    p1.Name AS ProductA, 
                    p2.Name AS ProductB,
                    pp.PairCount,
                    CAST(pp.PairCount AS FLOAT) / NULLIF(t.TotalCount, 0) AS CoOccurrenceRate
                FROM ProductPairs pp
                CROSS JOIN TotalOrders t
                JOIN Product p1 ON pp.ProductIdA = p1.Id
                JOIN Product p2 ON pp.ProductIdB = p2.Id
                WHERE CAST(pp.PairCount AS FLOAT) / NULLIF(t.TotalCount, 0) > 0.15
                ORDER BY CoOccurrenceRate DESC";

            return DataHandler.FillDataTable(query);
        }

        public static DataTable GetProductPerformance(int days)
        {
            string query = $@"
                SELECT TOP 10
                    P.Name,
                    SUM(OD.Quantity) AS UnitsSold,
                    SUM(OD.Price * OD.Quantity) AS Revenue
                FROM OrderDetail OD
                JOIN [Order] O ON OD.OrderId = O.Id
                JOIN Product P ON OD.ProductId = P.Id
                WHERE O.Timestamp >= DATEADD(day, -{days}, GETDATE())
                GROUP BY P.Id, P.Name
                ORDER BY UnitsSold DESC";
            return DataHandler.FillDataTable(query);
        }

        public static DataTable GetSeasonalTrends()
        {
            string query = @"
                SELECT 
                    P.Name,
                    MONTH(O.Timestamp) AS SaleMonth,
                    SUM(OD.Quantity) AS MonthlyUnits
                FROM OrderDetail OD
                JOIN [Order] O ON OD.OrderId = O.Id
                JOIN Product P ON OD.ProductId = P.Id
                WHERE O.Timestamp >= DATEADD(year, -1, GETDATE())
                GROUP BY P.Name, MONTH(O.Timestamp)
                ORDER BY MonthlyUnits DESC";
            return DataHandler.FillDataTable(query);
        }

        public static DataTable GetDeadStock()
        {
            string query = @"
                SELECT P.Name, SUM(SS.Stock) as CurrentStock
                FROM Product P
                JOIN SupplierStock SS ON P.Id = SS.ProductId
                WHERE P.Id NOT IN (
                    SELECT DISTINCT ProductId 
                    FROM OrderDetail OD
                    JOIN [Order] O ON OD.OrderId = O.Id
                    WHERE O.Timestamp >= DATEADD(day, -90, GETDATE())
                )
                GROUP BY P.Id, P.Name
                HAVING SUM(SS.Stock) > 0";
            return DataHandler.FillDataTable(query);
        }
    }
}
