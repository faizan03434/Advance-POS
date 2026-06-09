using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.DL
{
    static class ProductDL
    {
        public static DataTable GetProducts_View()
        {
           
            string query = @"
        SELECT 
            v.*, 
            p.DefaultDiscountPercent AS [Discount (%)],
            ISNULL((SELECT TOP 1 pl.DiscountAmount FROM PriceLog pl WHERE pl.ProductId = v.Id ORDER BY pl.AddedOn DESC), 0) AS [Discount (Rs)]
        FROM GetProducts_View v
        INNER JOIN Product p ON v.Id = p.Id";

            return DataHandler.FillDataTable(query);
        }

        public static Product GetProduct(int id)
        {
            // CHANGED: added ExternalBarcode to SELECT
            SqlDataReader reader = Utils.ReadData(@"SELECT Id,Name,Code,ExternalBarcode,CompanyId,ReorderThreshold,CategoryId,ExpiryDate
                                                    FROM Product WHERE Id=" + id.ToString());
            List<object> args = Utils.GetArgs(reader);
            if (args.Count != 0)
            {
                // args[4] = CompanyId, args[6] = CategoryId (shifted by ExternalBarcode at [3])
                if (args[4] != null) args[4] = CompanyDL.GetCompany((int)args[4]);
                if (args[6] != null) args[6] = CategoryDL.GetCategory((int)args[6]);
                args.Add(SupplierDL.GetProductSuppliers(id));
                reader = Utils.ReadData(@"SELECT p1.SupplierId,s1.Stock,p1.Price,p1.RetailPrice,p1.DiscountAmount
                                            FROM (SELECT SupplierId,ProductId,SUM(Stock) Stock
                                            FROM SupplierStock GROUP BY ProductId,SupplierId) s1
                                            RIGHT JOIN PriceLog p1 ON p1.SupplierId=s1.SupplierId AND p1.ProductId=s1.ProductId
                                            WHERE p1.AddedOn = (SELECT MAX(p2.AddedOn) FROM PriceLog p2
                                            WHERE p1.SupplierId=p2.SupplierId AND p2.ProductId=" + id.ToString() + ")");
                List<Stock> stocks = new List<Stock>();
                while (reader.Read())
                {
                    Stock stock = new Stock(((List<Supplier>)args[8]).Where(x => x.Id == reader.GetInt32(0)).FirstOrDefault()
                                           , reader.GetSqlMoney(2).ToDouble()
                                           , reader.GetSqlMoney(3).ToDouble()
                                           , reader.GetSqlMoney(4).ToDouble()
                                           , reader.IsDBNull(1) ? 0 : reader.GetInt32(1));
                    stocks.Add(stock);
                }
                args.Add(stocks);
                return new Product(args);
            }
            return null;
        }

        public static List<Product> GetProducts(List<int>? ids = null)
        {
            // Load all categories once for efficient lookup
            var categories = CategoryDL.GetCategories();
            var categoryMap = categories.ToDictionary(c => c.Id);

            SqlDataReader reader = Utils.ReadData(@"SELECT Id,Name,Code,ExternalBarcode,CompanyId,ReorderThreshold,CategoryId,ExpiryDate FROM Product WHERE IsDiscontinued=0");
            List<Product> products = new List<Product>();
            while (reader.Read())
            {
                Product p = new Product();
                p.Id = reader.GetInt32(0);
                p.Name = reader.GetString(1);
                p.Code = reader.GetString(2).Trim();
                p.ExternalBarcode = reader.IsDBNull(3) ? null : reader.GetString(3).Trim();
                p.ReorderThreshold = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5);
                p.ExpiryDate = reader.IsDBNull(7) ? null : (DateTime?)reader.GetDateTime(7);
                // Load Category object so Tax calculation works in Process Order
                if (!reader.IsDBNull(6))
                {
                    int catId = reader.GetInt32(6);
                    categoryMap.TryGetValue(catId, out var cat);
                    p.Category = cat;
                }
                products.Add(p);
            }
            return products;
        }

        // NEW METHOD: Find a product by its physical (external) barcode
        // Called from Order.AddProduct() when scanned code doesn't match 8-char system format
        public static Product GetProductByExternalBarcode(string barcode)
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT Id,Name,Code,ExternalBarcode,CompanyId,ReorderThreshold,CategoryId,ExpiryDate
                                                    FROM Product
                                                    WHERE ExternalBarcode='" + barcode.Replace("'", "''") + "' AND IsDiscontinued=0");
            List<object> args = Utils.GetArgs(reader);
            if (args.Count == 0) return null;

            if (args[4] != null) args[4] = CompanyDL.GetCompany((int)args[4]);
            if (args[6] != null) args[6] = CategoryDL.GetCategory((int)args[6]);

            int prodId = (int)args[0];
            args.Add(SupplierDL.GetProductSuppliers(prodId));

            reader = Utils.ReadData(@"SELECT p1.SupplierId,s1.Stock,p1.Price,p1.RetailPrice,p1.DiscountAmount
                                        FROM (SELECT SupplierId,ProductId,SUM(Stock) Stock
                                        FROM SupplierStock GROUP BY ProductId,SupplierId) s1
                                        RIGHT JOIN PriceLog p1 ON p1.SupplierId=s1.SupplierId AND p1.ProductId=s1.ProductId
                                        WHERE p1.AddedOn = (SELECT MAX(p2.AddedOn) FROM PriceLog p2
                                        WHERE p1.SupplierId=p2.SupplierId AND p2.ProductId=" + prodId.ToString() + ")");
            List<Stock> stocks = new List<Stock>();
            while (reader.Read())
            {
                Stock stock = new Stock(((List<Supplier>)args[8]).Where(x => x.Id == reader.GetInt32(0)).FirstOrDefault()
                                       , reader.GetSqlMoney(2).ToDouble()
                                       , reader.GetSqlMoney(3).ToDouble()
                                       , reader.GetSqlMoney(4).ToDouble()
                                       , reader.IsDBNull(1) ? 0 : reader.GetInt32(1));
                stocks.Add(stock);
            }
            args.Add(stocks);
            return new Product(args);
        }

        // NEW: check if a product code is already taken
        public static bool IsCodeTaken(string code)
        {
            SqlDataReader reader = Utils.ReadData(
                "SELECT COUNT(*) FROM Product WHERE Code='" + code.Replace("'", "''") + "' AND IsDiscontinued=0");
            if (!reader.Read()) return false;
            int count = reader.GetInt32(0);
            reader.Close();
            return count > 0;
        }

        // REPLACE existing GetSupplierProducts in DL/ProductDL.cs with this.
        // Only change: ExternalBarcode added to SELECT (column index 3).
        // All other logic is exactly the same as your original.

        public static List<Product> GetSupplierProducts(int supplierId, bool populateStock = true)
        {
            List<object> products = new List<object>();
            SqlDataReader reader = Utils.ReadData(@"SELECT ProductId
                                                  ,Product.Name
                                                  ,Product.Code
                                                  ,Product.ExternalBarcode
                                                  ,Product.CompanyId
                                                  ,Product.ReorderThreshold
                                                  ,Product.CategoryId
                                           FROM ProductSupplier
                                           JOIN Product
                                           ON Product.Id=ProductSupplier.ProductId
                                           WHERE ProductSupplier.isDeleted=0 AND SupplierId=" + supplierId.ToString());
            List<object> args;
            do
            {
                args = Utils.GetArgs(reader);
                if (args.Count != 0)
                    products.Add(args);
            }
            while (args != null && args.Count != 0);

            Supplier supplier = null;
            if (populateStock == true)
                supplier = SupplierDL.GetSupplier(supplierId);

            for (int i = 0; i < products.Count; i++)
            {
                var row = (List<object>)products[i];
                // row[0]=Id, row[1]=Name, row[2]=Code, row[3]=ExternalBarcode,
                // row[4]=CompanyId, row[5]=ReorderThreshold, row[6]=CategoryId
                if (row[4] != null)
                    row[4] = CompanyDL.GetCompany((int)row[4]);
                if (row[6] != null)
                    row[6] = CategoryDL.GetCategory((int)row[6]);
                row.Add(new List<Supplier>());
                products[i] = new Product(row);
                if (populateStock == true)
                    ((Product)products[i]).Stocks.Add(new Stock(supplier, 0, 0, 0, 0));
            }
            return products.Cast<Product>().ToList();
        }
        public static List<Stock> GetProductStocks(Product product)
        {
            // Ensure Suppliers list is populated before matching
            if (product.Suppliers == null || product.Suppliers.Count == 0)
                product.Suppliers = SupplierDL.GetProductSuppliers(product.Id);

            SqlDataReader reader = Utils.ReadData(@"SELECT p1.SupplierId,s1.Stock,p1.Price,p1.RetailPrice,p1.DiscountAmount
                                FROM (SELECT SupplierId,ProductId,SUM(Stock) Stock FROM SupplierStock GROUP BY ProductId,SupplierId) s1
                                RIGHT JOIN PriceLog p1 ON p1.SupplierId=s1.SupplierId AND p1.ProductId=s1.ProductId
                                WHERE p1.AddedOn = (SELECT MAX(p2.AddedOn) FROM PriceLog p2
                                WHERE p1.SupplierId=p2.SupplierId AND p2.ProductId=" + product.Id.ToString() + ")");
            List<Stock> stocks = new List<Stock>();
            while (reader.Read())
            {
                int supplierId = reader.GetInt32(0);
                // First try to find supplier in already-loaded list
                var supplier = product.Suppliers?.FirstOrDefault(x => x.Id == supplierId);
                // If not found in list, load directly from DB (handles newly linked suppliers)
                if (supplier == null)
                    supplier = SupplierDL.GetSupplier(supplierId);
                if (supplier != null)
                    stocks.Add(new Stock(supplier, reader.GetSqlMoney(2).ToDouble(), reader.GetSqlMoney(3).ToDouble(),
                                         reader.GetSqlMoney(4).ToDouble(), reader.IsDBNull(1) ? 0 : reader.GetInt32(1)));
            }
            return stocks;
        }

        public static void SaveStockChanges(Product product, List<(int, int, string)> values)
        {
            if (values.Count == 0) return;
            SqlMetaData[] sqlMetas = new SqlMetaData[]
            {
                new SqlMetaData("SupplierId",SqlDbType.Int),
                new SqlMetaData("ProductId",SqlDbType.Int),
                new SqlMetaData("Quantity",SqlDbType.Int),
                new SqlMetaData("Description",SqlDbType.NVarChar,100),
            };
            var items = values.Select(x =>
            {
                SqlDataRecord record = new SqlDataRecord(sqlMetas);
                record.SetInt32(0, product.Id);
                record.SetInt32(1, x.Item1);
                record.SetInt32(2, x.Item2);
                record.SetSqlString(3, x.Item3);
                return record;
            });
            DataHandler.BulkDataExecuteSP("StockChanges", "udtt_StockChanges", "stpInsertStockChanges", items);
        }

        // Save prices to PriceLog via stpInsertProductSupplierPrice
        // Called after product.Save() so product.Id is valid
        public static void SavePrices(Product product)
        {
            if (product.Stocks == null || product.Stocks.Count == 0) return;

            SqlMetaData[] sqlMetas = new SqlMetaData[]
            {
                new SqlMetaData("ProductId",   SqlDbType.Int),
                new SqlMetaData("SupplierId",  SqlDbType.Int),
                new SqlMetaData("Price",       SqlDbType.Money),
                new SqlMetaData("RetailPrice", SqlDbType.Money),
                new SqlMetaData("DiscountAmount", SqlDbType.Money),
            };

            var items = product.Stocks
                .Where(s => s.Supplier != null && s.RetailPrice > 0)
                .Select(s =>
                {
                    SqlDataRecord record = new SqlDataRecord(sqlMetas);
                    record.SetInt32(0, product.Id);
                    record.SetInt32(1, s.Supplier.Id);
                    record.SetDecimal(2, (decimal)s.Price);
                    record.SetDecimal(3, (decimal)s.RetailPrice);
                    record.SetDecimal(4, (decimal)s.DiscountAmount);
                    return record;
                }).ToList();

            if (items.Count == 0) return;

            DataHandler.BulkDataExecuteSP(
                "ProductSupplierPrice",
                "udtt_ProductSupplierPrice",
                "stpInsertProductSupplierPrice",
                items);
        }

        public static void GenerateBarcodes()
        {
            List<Product> products = GetProducts();
            foreach (var product in products)
            {
                // CHANGED: if external barcode exists, save that image; skip system barcode gen
                if (!string.IsNullOrWhiteSpace(product.ExternalBarcode))
                {
                    string path = $"barcodes/{product.ExternalBarcode}.png";
                    if (!File.Exists(path))
                        Utils.GenerateBarcode(product.ExternalBarcode);
                    continue;
                }

                product.Suppliers = SupplierDL.GetProductSuppliers(product);
                foreach (var supplier in product.Suppliers)
                {
                    string barcodeKey = product.Code + supplier.Code;
                    if (!File.Exists($"barcodes/{barcodeKey}.png"))
                        Utils.GenerateBarcode(barcodeKey);
                }
            }
        }

        public static void Save(Product product, bool isAdd = false)
        {
            List<(string, object)> args = new List<(string, object)>
            {
                ("Name",             product.Name),
                ("Code",             product.Code),
                ("CompanyId",        product.Company?.Id        ?? (object)DBNull.Value),
                ("ReorderThreshold", product.ReorderThreshold   ?? (object)DBNull.Value),
                ("CategoryId",       product.Category?.Id       ?? (object)DBNull.Value),
                ("ExpiryDate",       product.ExpiryDate.HasValue ? (object)product.ExpiryDate.Value : DBNull.Value),
                ("ExternalBarcode",  string.IsNullOrWhiteSpace(product.ExternalBarcode) ? (object)DBNull.Value : product.ExternalBarcode)
            };
            if (isAdd)
            {
                // SP returns SCOPE_IDENTITY() — capture it so product.Id is valid
                // for any FK-dependent inserts (SupplierStock, PriceLog) that follow
                object newId = DataHandler.InsertDataSPReturn(args, "stpInsertProduct");
                if (newId != null && newId != DBNull.Value)
                    product.Id = Convert.ToInt32(newId);
            }
            else
            {
                args.Add(("Id", product.Id));
                DataHandler.InsertDataSP(args, "stpUpdateProduct");
            }
        }

        public static void DeleteProduct(int id)
        {
            DataHandler.DeleteDataSP("stpDeleteProduct", ("Id", id));
        }

        // ===== ALERT METHODS (unchanged) =====
        public static List<LowStockDto> GetLowStockProducts()
        {
            var result = new List<LowStockDto>();
            try
            {
                var reader = Utils.ReadData(@"
                    SELECT P.Id, P.Name, P.Code, P.ReorderThreshold,
                           ISNULL(SUM(SS.Stock),0) - ISNULL(SUM(OD.Quantity),0) AS CurrentStock
                    FROM Product P
                    LEFT JOIN SupplierStock SS ON P.Id = SS.ProductId
                    LEFT JOIN OrderDetail OD ON P.Id = OD.ProductId
                    WHERE P.IsDiscontinued = 0
                      AND P.ReorderThreshold IS NOT NULL
                    GROUP BY P.Id, P.Name, P.Code, P.ReorderThreshold
                    HAVING (ISNULL(SUM(SS.Stock),0) - ISNULL(SUM(OD.Quantity),0)) <= P.ReorderThreshold");
                while (reader.Read())
                    result.Add(new LowStockDto
                    {
                        ProductId = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        ProductCode = reader.GetString(2).Trim(),
                        ReorderThreshold = reader.GetInt32(3),
                        CurrentStock = reader.GetInt32(4)
                    });
            }
            catch { }
            return result;
        }

        public static List<ExpiringDto> GetExpiringProducts(int daysAhead)
        {
            var result = new List<ExpiringDto>();
            try
            {
                var reader = Utils.ReadData($@"
                    SELECT Id, Name, Code, ExpiryDate,
                           DATEDIFF(DAY, GETDATE(), ExpiryDate) AS DaysLeft
                    FROM Product
                    WHERE IsDiscontinued = 0
                      AND ExpiryDate IS NOT NULL
                      AND ExpiryDate >= GETDATE()
                      AND DATEDIFF(DAY, GETDATE(), ExpiryDate) <= {daysAhead}");
                while (reader.Read())
                    result.Add(new ExpiringDto
                    {
                        ProductId = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        ProductCode = reader.GetString(2).Trim(),
                        ExpiryDate = reader.GetDateTime(3),
                        DaysUntilExpiry = reader.GetInt32(4)
                    });
            }
            catch { }
            return result;
        }

        public static void ApplyExpiryDiscount(int productId, double discountPercent)
        {
            Utils.ExecuteQuery($@"
                UPDATE PriceLog SET
                    DiscountAmount = ROUND(RetailPrice * {discountPercent} / 100.0, 2)
                WHERE ProductId = {productId}
                  AND AddedOn = (SELECT MAX(AddedOn) FROM PriceLog pl2 WHERE pl2.ProductId = {productId} AND pl2.SupplierId = PriceLog.SupplierId)");
        }

        public static (decimal revenue, decimal profit, int orders) GetTodayKpis()
        {
            try
            {
                var reader = Utils.ReadData(@"
                    SELECT ISNULL(SUM(OD.Price * OD.Quantity),0) AS Revenue,
                           ISNULL(SUM((OD.Price - PL.Price) * OD.Quantity),0) AS Profit,
                           COUNT(DISTINCT O.Id) AS Orders
                    FROM [Order] O
                    INNER JOIN OrderDetail OD ON O.Id = OD.OrderId
                    INNER JOIN PriceLog PL ON OD.ProductId=PL.ProductId AND OD.SupplierId=PL.SupplierId
                        AND PL.AddedOn=(SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId=OD.ProductId AND SupplierId=OD.SupplierId)
                    WHERE CONVERT(date, O.Timestamp) = CONVERT(date, GETDATE())");
                if (reader.Read())
                    return (reader.GetDecimal(0), reader.GetDecimal(1), reader.GetInt32(2));
            }
            catch { }
            return (0, 0, 0);
        }

        public static int GetLowStockCount()
        {
            try
            {
                var result = Utils.ExecuteQueryScalar(@"
                    SELECT COUNT(*) FROM Product P
                    WHERE P.IsDiscontinued=0 AND P.ReorderThreshold IS NOT NULL
                    AND (SELECT ISNULL(SUM(SS.Stock),0)-ISNULL(SUM(OD.Quantity),0)
                         FROM SupplierStock SS LEFT JOIN OrderDetail OD ON OD.ProductId=SS.ProductId
                         WHERE SS.ProductId=P.Id) <= P.ReorderThreshold");
                return result != null ? (int)result : 0;
            }
            catch { return 0; }
        }

        public static List<(string Name, double Sales)> GetTopProductsToday()
        {
            var result = new List<(string, double)>();
            try
            {
                var reader = Utils.ReadData(@"
                    SELECT TOP 5 P.Name, SUM(OD.Price * OD.Quantity) AS Sales
                    FROM [Order] O
                    INNER JOIN OrderDetail OD ON O.Id = OD.OrderId
                    INNER JOIN Product P ON P.Id = OD.ProductId
                    WHERE CONVERT(date, O.Timestamp) = CONVERT(date, GETDATE())
                    GROUP BY P.Name ORDER BY Sales DESC");
                while (reader.Read())
                    result.Add((reader.GetString(0), (double)reader.GetDecimal(1)));
            }
            catch { }
            return result;
        }

        public static List<(string Category, double Sales)> GetCategorySalesToday()
        {
            var result = new List<(string, double)>();
            try
            {
                var reader = Utils.ReadData(@"
                    SELECT C.Name, SUM(OD.Price * OD.Quantity) AS Sales
                    FROM [Order] O
                    INNER JOIN OrderDetail OD ON O.Id = OD.OrderId
                    INNER JOIN Product P ON P.Id = OD.ProductId
                    INNER JOIN Category C ON C.Id = P.CategoryId
                    WHERE CONVERT(date, O.Timestamp) = CONVERT(date, GETDATE())
                    GROUP BY C.Name ORDER BY Sales DESC");
                while (reader.Read())
                    result.Add((reader.GetString(0), (double)reader.GetDecimal(1)));
            }
            catch { }
            return result;
        }
    }
}
