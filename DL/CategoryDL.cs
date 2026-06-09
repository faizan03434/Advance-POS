using Microsoft.Data.SqlClient;
using StationeryStoreManagementSystem.BL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.DL
{
    static class CategoryDL
    {
        public static DataTable GetCategories_View()
        {
            List<object> list = new List<object>();
            return DataHandler.FillDataTable(@"Select * from GetCategories_View");
        }

        public static List<Category> GetCategories()
        {
            // LEFT JOIN with TaxLog so categories WITHOUT a TaxLog entry still appear (GST = null = 0%)
            SqlDataReader reader = Utils.ReadData(@"
                SELECT Id,
                       Name,
                       (SELECT TOP 1 GST FROM TaxLog
                        WHERE TaxLog.CategoryId = Category.Id
                        ORDER BY AddedOn DESC) AS GST
                FROM Category
                WHERE IsDeleted = 0");
            return DataHandler.ConstructObjects(reader, typeof(Category)).Cast<Category>().ToList();
        }

        public static void SaveCategory(Category C, bool isAdd = false)
        {
            List<(string, object)> args = new List<(string, object)>
            {
                (nameof(C.Name), C.Name)
            };
            if (isAdd == true)
            {
                args.Add((nameof(C.GST), C.GST));
                DataHandler.InsertDataSP(args, "stpInsertCategory");
            }
            else
            {
                C.InitialArgs.RemoveAt(1);
                args.Add(("UpdatedOn", ("CURRENT_TIMESTAMP", true)));
                DataHandler.UpdateData(args, C.InitialArgs, C.GetType().Name, (nameof(C.Id), C.Id));
                args.Clear();
                args.Add(("CategoryId", C.Id));
                args.Add(("GST", C.GST));
                args.Add(("AddedOn", ("CURRENT_TIMESTAMP", true)));
                DataHandler.InsertData(args, "TaxLog");
            }
        }

        public static Category GetCategory(int id)
        {
            // Uses LEFT JOIN so category is returned even if no TaxLog entry exists
            SqlDataReader reader = Utils.ReadData(@"
                SELECT C.Id, C.Name,
                       (SELECT TOP 1 GST FROM TaxLog
                        WHERE TaxLog.CategoryId = C.Id
                        ORDER BY AddedOn DESC) AS GST
                FROM Category C
                WHERE C.IsDeleted = 0 AND C.Id = " + id.ToString());
            return (Category)DataHandler.ConstructObject(reader, typeof(Category));
        }

        public static void DeleteCategory(int id)
        {
            DataHandler.DeleteDataSP("stpDeleteCategory", ("Id", id));
        }
    }
}
