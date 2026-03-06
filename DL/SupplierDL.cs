using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using StationeryStoreManagementSystem.BL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace StationeryStoreManagementSystem.DL
{
    static class SupplierDL
    {
        public static DataTable GetSuppliersView()
        {
            return DataHandler.FillDataTable(@"SELECT * FROM GetSuppliers_View");
        }
        public static Supplier GetSupplier(int id)
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT Supplier.Id
                                                          ,Name
                                                          ,Code
                                                    	  ,Contact
                                                    	  ,Email
                                                    	  ,StreetAddress
                                                    	  ,Town
                                                    	  ,l1.Value City
                                                    	  ,l2.Value Country
                                                    	  ,PostalCode
                                                    FROM Supplier
                                                    LEFT JOIN Lookup l1
                                                    ON l1.Id=City
                                                    LEFT JOIN Lookup l2
                                                    ON l2.Id=Country
                                                    WHERE Supplier.Id=" + id.ToString());
            List<object> args = Utils.GetArgs(reader);
            args.Add(ProductDL.GetSupplierProducts((int)args[0],false));
            return new Supplier(args);
;        }
        public static List<Supplier> GetProductSuppliers(Product product)
        {
            return GetProductSuppliers(product.Id);
        }
        public static List<Supplier> GetProductSuppliers(int ProductId)
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT Supplier.Id
                                                          ,Name
                                                          ,Code
                                                          ,Contact
                                                          ,Email
                                                          ,StreetAddress
                                                          ,Town
                                                    	  ,l1.Value City
                                                    	  ,l2.Value Country
                                                          ,PostalCode
                                                    FROM ProductSupplier
                                                    JOIN Supplier
                                                    ON Supplier.Id=ProductSupplier.SupplierId
                                                    LEFT JOIN Lookup l1
                                                    ON l1.Id=City
                                                    LEFT JOIN Lookup l2
                                                    ON l1.Id=Country
                                                    WHERE ProductSupplier.isDeleted=0 AND ProductId=" + ProductId.ToString());
            return DataHandler.ConstructObjects(reader, typeof(Supplier)).Cast<Supplier>().ToList();
        }
        public static void SaveSupplier(Supplier supplier,bool isAdd)
        {
            int? CountryId = supplier.Country==null? null: DataHandler.LookupData("Country").Where(x => x.Value == supplier.Country).Select(x => x.Key).First();
            int? cityId = supplier.City==null? null : DataHandler.LookupData($"City{supplier.Country}").Where(x => x.Value == supplier.City).Select(x => x.Key).First();
            List<(string, object)> args = new List<(string, object)>
            {
                (nameof(supplier.Name), supplier.Name),
                (nameof(supplier.Code), supplier.Code),
                (nameof(supplier.Contact), supplier.Contact),
                (nameof(supplier.Email),supplier.Email),
                (nameof(supplier.StreetAddress),supplier.StreetAddress),
                (nameof(supplier.Town),supplier.Town),
                (nameof(supplier.City),cityId),
                (nameof(supplier.Country),CountryId),
                (nameof(supplier.PostalCode),supplier.PostalCode)
            };
            if (isAdd == true)
            {
                supplier.Id = (int)DataHandler.InsertDataSPReturn(args, "stpInsertSupplier");
            }
            else
            {
                args.Add(("UpdatedOn",("CURRENT_TIMESTAMP",true)));
                DataHandler.UpdateData(args, supplier.InitialArgs, supplier.GetType().Name, (nameof(supplier.Id), supplier.Id));
            }
            List<int> newIds = new List<int>();
            List<int> deleteIds = new List<int>();
            newIds = supplier.Products.Select(x => x.Id).ToList();
            if (supplier.InitialArgs != null && supplier.InitialArgs.Count!=0)
            {
                List<int> prevIds = ((List<Product>)supplier.InitialArgs[9]).Select(x => x.Id).ToList();
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
                    record.SetInt32(0, supplier.Id);
                    record.SetInt32(1, x);
                    return record;
                });
                DataHandler.BulkDataExecuteSP("ProductSuppliers", "udtt_ProductSuppliers", "stpInsertProductSuppliers", valueInsert);
            }
            if (deleteIds.Count > 0)
            {
                var valueDelete = deleteIds.Select(x =>
                {
                    SqlDataRecord record = new SqlDataRecord(sqlMetas);
                    record.SetInt32(0, supplier.Id);
                    record.SetInt32(1, x);
                    return record;
                });
                DataHandler.BulkDataExecuteSP("ProductSuppliers", "udtt_ProductSuppliers", "stpDeleteProductSuppliers", valueDelete);
            }
        }
        public static void DeleteSupplier(int id)
        {
            DataHandler.DeleteDataSP("stpDeleteSupplier", ("Id", id));
        }
    }
}
