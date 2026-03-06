using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using StationeryStoreManagementSystem.BL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.DL
{
    static class ProductDL
    {
        public static DataTable GetProducts_View()
        {
            return DataHandler.FillDataTable(@"SELECT * FROM GetProducts_View");
        }
        public static Product GetProduct(int id)
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT Id
                                                        ,Name
                                                        ,Code
                                                        ,CompanyId
                                                        ,ReorderThreshold
                                                        ,CategoryId
                                                    FROM Product
                                                    WHERE Id=" + id.ToString());
            List<object> args = Utils.GetArgs(reader);
            if (args.Count != 0)
             {
                 if (args[3] != null)
                     args[3] = CompanyDL.GetCompany((int)args[3]);
                 if (args[5] != null)
                     args[5] = CategoryDL.GetCategory((int)args[5]);
                args.Add(SupplierDL.GetProductSuppliers(id));
                reader = Utils.ReadData(@"SELECT p1.SupplierId,s1.Stock,p1.Price,p1.RetailPrice,p1.DiscountAmount
                                                    FROM (SELECT SupplierId,ProductId,SUM(Stock) Stock
                                                    FROM SupplierStock
                                                    GROUP BY ProductId,SupplierId) s1
                                                    RIGHT JOIN PriceLog p1
                                                    ON p1.SupplierId=s1.SupplierId AND p1.ProductId=s1.ProductId
                                                    WHERE p1.AddedOn = (SELECT MAX(p2.AddedOn)
                                                    	                FROM PriceLog p2
                                                    	                WHERE p1.SupplierId=p2.SupplierId
                                                                        AND p2.ProductId=" + id.ToString()+")");
                List<Stock> stocks = new List<Stock>();    
                while(reader.Read())
                    {
                        Stock stock = new Stock(((List<Supplier>)args[6]).Where(x => x.Id == reader.GetInt32(0)).FirstOrDefault()
                                               , reader.GetSqlMoney(2).ToDouble()
                                               , reader.GetSqlMoney(3).ToDouble()
                                               , reader.GetSqlMoney(4).ToDouble()
                                               , reader.IsDBNull(1) ? 0 : reader.GetInt32(1));
                    stocks.Add(stock);
                    }
                args.Add(stocks);
                Product product = new Product(args);
                return product;
             }
             else
            return null;
        }
        public static void SaveStockChanges(Product product,List<(int, int, string)> values)
        {
            if (values.Count == 0)
                return;
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
        public static void GenerateBarcodes()
        {
            List<Product> products = GetProducts();
            foreach(var product in products)
            {
                product.Suppliers = SupplierDL.GetProductSuppliers(product);
                foreach(var supplier in product.Suppliers)
                {
                    if(!File.Exists($"barcodes/{product.Code}{supplier.Code}"))
                        Utils.GenerateBarcode(product.Code + supplier.Code);
                }
            }
        }
        public static List<Stock> GetProductStocks(Product product)
        {
            List<Stock> stocks = new List<Stock>();
            SqlDataReader reader = Utils.ReadData(@"SELECT p1.SupplierId,s1.Stock,p1.Price,p1.RetailPrice,p1.DiscountAmount
                                                    FROM (SELECT SupplierId,ProductId,SUM(Stock) Stock
                                                    FROM SupplierStock
                                                    GROUP BY ProductId,SupplierId) s1
                                                    RIGHT JOIN PriceLog p1
                                                    ON p1.SupplierId=s1.SupplierId AND p1.ProductId=s1.ProductId
                                                    WHERE p1.AddedOn = (SELECT MAX(p2.AddedOn)
                                                    	                FROM PriceLog p2
                                                    	                WHERE p1.SupplierId=p2.SupplierId 
                                                                        AND p2.ProductId=" + product.Id.ToString()+")");
            while(reader.Read())
            {
                Stock stock = new Stock(product.Suppliers.Where(x => x.Id == reader.GetInt32(0)).FirstOrDefault()
                                               , reader.GetSqlMoney(2).ToDouble()
                                               , reader.GetSqlMoney(3).ToDouble()
                                               , reader.GetSqlMoney(4).ToDouble()
                                               , reader.IsDBNull(1)? 0:reader.GetInt32(1));
                stocks.Add(stock);
            }
            return stocks;
        }
        public static List<Product> GetProducts(List<int>? ids = null)
        {
            List<object> products = new List<object>();
            SqlDataReader reader;
            if (ids == null)
            {
                reader = Utils.ReadData(@"SELECT Id
                                              ,Name
                                              ,Code
                                              ,CompanyId
                                              ,ReorderThreshold
                                              ,CategoryId
                                          FROM Product");
            }
            else
            {
                reader = Utils.ReadData(@"SELECT Id
                                              ,Name
                                              ,Code
                                              ,CompanyId
                                              ,ReorderThreshold
                                              ,CategoryId
                                          FROM Product
                                          WHERE Id IN (" + string.Join(',', ids) + ")");
            }
            List<object> args;
            do
            {
                args = Utils.GetArgs(reader);
                if (args.Count != 0)
                    products.Add(args);
            }
            while (args != null && args.Count != 0);
            for (int i = 0; i < products.Count; i++)
            {
                var row = (List<object>)products[i];
                if (row[3] != null)
                    row[3] = CompanyDL.GetCompany((int)row[3]);
                if (row[5] != null)
                    row[5] = CategoryDL.GetCategory((int)row[5]);
                row.Add(new List<Supplier>());
                products[i] = new Product(row);
            }
            return products.Cast<Product>().ToList();
        }
        public static List<Product> GetSupplierProducts(int supplierId,bool populateStock = true)
        {
            List<object> products = new List<object>();
            SqlDataReader reader = Utils.ReadData(@"SELECT ProductId
                                                    	  ,Product.Name
                                                          ,Product.Code
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
            while (args!=null && args.Count != 0);
            Supplier supplier = null;
            if (populateStock==true)
                 supplier = SupplierDL.GetSupplier(supplierId);
            for(int i=0;i<products.Count;i++)
            {
                var row = (List<object>)products[i];
                if (row[3] != null)
                    row[3] = CompanyDL.GetCompany((int)row[3]);
                if (row[5] != null)
                    row[5] = CategoryDL.GetCategory((int)row[5]);
                row.Add(new List<Supplier>());
                products[i] = new Product(row);
                if(populateStock==true)
                    ((Product)products[i]).Stocks.Add(new Stock(supplier, 0, 0, 0, 0));
            }
            return products.Cast<Product>().ToList();
        }
        public static void Save(Product product,bool isAdd) 
        {
            int? CompanyId = product.Company?.Id;
            int? CategoryId = product.Category?.Id;
            List<(string, object)> args = new List<(string, object)>
            {
                (nameof(product.Name),product.Name),
                (nameof(product.Code),product.Code),
                ("CompanyId",CompanyId),
                (nameof(product.ReorderThreshold),product.ReorderThreshold),
                ("CategoryId",CategoryId),
            };
            if(isAdd==true)
            {
                product.Id = (int)DataHandler.InsertDataSPReturn(args, "stpInsertProduct");
            }
            else
            {
                List<object> initialArgs = new List<object>(product.InitialArgs);
                initialArgs.RemoveAt(6);
                initialArgs.RemoveAt(5);
                initialArgs[2] = ((Company)initialArgs[2])?.Id;
                initialArgs[4] = ((Category)initialArgs[4])?.Id;
                args.Add(("UpdatedOn", ("CURRENT_TIMESTAMP", true)));
                DataHandler.UpdateData(args, initialArgs, product.GetType().Name, (nameof(product.Id), product.Id));

            }
            List<int> newIds = new List<int>();
            List<int> deleteIds = new List<int>();
            newIds = product.Suppliers.Select(x => x.Id).ToList();
            if (product.InitialArgs != null)
            {
                List<int> prevIds = ((List<Supplier>)product.InitialArgs[5]).Select(x => x.Id).ToList();
                deleteIds = prevIds.Except(newIds).ToList();
                newIds = newIds.Except(prevIds).ToList();
            }
            SqlMetaData[] sqlMetas = new SqlMetaData[]
                {
                    new SqlMetaData("SupplierId",SqlDbType.Int),
                    new SqlMetaData("ProductId",SqlDbType.Int)
                };
            if (newIds.Count > 0)
            {
                var valueInsert = newIds.Select(x =>
                {
                    SqlDataRecord record = new SqlDataRecord(sqlMetas);
                    record.SetInt32(0, x);
                    record.SetInt32(1, product.Id);
                    return record;
                });
                DataHandler.BulkDataExecuteSP("ProductSuppliers", "udtt_ProductSuppliers", "stpInsertProductSuppliers", valueInsert);
            }
            if (deleteIds.Count > 0)
            {
                var valueDelete = deleteIds.Select(x =>
                {
                    SqlDataRecord record = new SqlDataRecord(sqlMetas);
                    record.SetInt32(0, x);
                    record.SetInt32(1, product.Id);
                    return record;
                });
                DataHandler.BulkDataExecuteSP("ProductSuppliers", "udtt_ProductSuppliers", "stpDeleteProductSuppliers", valueDelete);
            }
            SqlMetaData[] sqlMetas3 = new SqlMetaData[]
            {
                    new SqlMetaData("ProductId",SqlDbType.Int),
                    new SqlMetaData("SupplierId",SqlDbType.Int),
                    new SqlMetaData("Price",SqlDbType.Money),
                    new SqlMetaData("RetailPrice",SqlDbType.Money),
                    new SqlMetaData("DiscountAmount",SqlDbType.Money),
            };
            if (product.Stocks != null && product.Stocks.Count > 0)
            {
                List<Stock> oldStocks = ((List<Stock>)product.InitialArgs[6]);
                var productSupplierPrices = product.Stocks.Where(x => oldStocks.Where(y => y.ToString() == x.ToString()).FirstOrDefault() == null).Select(x =>
                {
                    SqlDataRecord record = new SqlDataRecord(sqlMetas3);
                    record.SetInt32(0, product.Id);
                    record.SetInt32(1, x.Supplier.Id);
                    record.SetSqlMoney(2, (System.Data.SqlTypes.SqlMoney)x.Price);
                    record.SetSqlMoney(3, (System.Data.SqlTypes.SqlMoney)x.RetailPrice);
                    record.SetSqlMoney(4, (System.Data.SqlTypes.SqlMoney)x.DiscountAmount);
                    return record;
                });
                if (productSupplierPrices.Count() > 0)
                {
                    DataHandler.BulkDataExecuteSP("ProductSupplierPrice", "udtt_ProductSupplierPrice", "stpInsertProductSupplierPrice", productSupplierPrices);
                }
            }
        }
        public static void DeleteProduct(int id)
        {
            DataHandler.DeleteDataSP("stpDeleteProduct", ("Id", id));
        }
    }
}
