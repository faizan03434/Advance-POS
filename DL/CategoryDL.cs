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
                SqlDataReader reader = Utils.ReadData(@"SELECT Id
                                                   	   ,Name
                                                   	   ,(SELECT TOP 1 GST 
                                                   	     FROM TaxLog 
                                                   		 WHERE TaxLog.CategoryId=Category.Id 
                                                   		 ORDER BY AddedOn DESC) GST
                                                        FROM Category
                                                        WHERE IsDeleted=0");
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
                args.Add(("CategoryId",C.Id));
                args.Add(("GST", C.GST));
                args.Add(("AddedOn", ("CURRENT_TIMESTAMP",true)));
                DataHandler.InsertData(args, "TaxLog");
            }
        }

        public static Category GetCategory(int id)
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT C.Id, C.Name, T.GST
                                                    FROM Category C
                                                    JOIN (
                                                        SELECT CategoryId, MAX(AddedOn) AS MaxCreatedOn
                                                        FROM TaxLog
                                                        GROUP BY CategoryId
                                                    ) AS MD 
                                                    ON C.Id = MD.CategoryId
                                                    JOIN TaxLog T ON T.CategoryId = MD.CategoryId AND T.AddedOn = MD.MaxCreatedOn
                                                    WHERE IsDeleted=0 AND Id=" + id.ToString());
            return (Category)DataHandler.ConstructObject(reader, typeof(Category));
        }
        public static void DeleteCategory(int id)
        {
            DataHandler.DeleteDataSP("stpDeleteCategory", ("Id", id));
        }
    }
}
