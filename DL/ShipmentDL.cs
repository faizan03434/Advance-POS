using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using StationeryStoreManagementSystem.BL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.DL
{
    static class ShipmentDL
    {
        public static (int,List<Product>) GetShipment(DateTime datetime)
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT ProductId,SupplierId,Stock
                                                    FROM SupplierStock
                                                    WHERE AddedOn=CONVERT(DATETIME,'" + datetime.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture)+"')");
            List<(int,int)> ids = new List<(int, int)>();
            int SupplierId = -1;
            while(reader.Read())
            {
                if (SupplierId == -1)
                    SupplierId = reader.GetInt32(1);
                ids.Add((reader.GetInt32(0),reader.GetInt32(2)));
            }
            List<Product> products = ProductDL.GetProducts(ids.Select(x=>x.Item1).ToList());
            Supplier supplier = SupplierDL.GetSupplier(SupplierId);
            for(int i=0;i<ids.Count;i++)
            {
                products[i].Stocks.Add(new Stock(supplier, 0, 0, 0, ids[i].Item2));
            }
            return (SupplierId, products);
        }
        public static DataTable GetShipmentsView()
        {
            return DataHandler.FillDataTable(@"SELECT AddedOn,Id,Name FROM GetShipments_View");
        }
        public static void AddShipment(int SupplierId,List<Product> products)
        {
            SqlMetaData[] sqlMetas = new SqlMetaData[]
            {
                    new SqlMetaData("SupplierId",SqlDbType.Int),
                    new SqlMetaData("ProductId",SqlDbType.Int),
                    new SqlMetaData("Stock",SqlDbType.Int),
            };
            var values = products.Select(x=>
                {
                    SqlDataRecord record = new SqlDataRecord(sqlMetas);
                    record.SetInt32(0, SupplierId);
                    record.SetInt32(1, x.Id);
                    record.SetInt32(2, x.SupplierQuantity);
                    return record;
                });
            DataHandler.BulkDataExecuteSP("Shipment", "udtt_Shipment", "stpInsertShipment", values);
        }
    }
}
