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
    static class CompanyDL
    {
        public static DataTable GetCompanies_View()
        {
            List<object> list = new List<object>();
            return DataHandler.FillDataTable(@"SELECT * FROM GetCompanies_View");
        }
        public static List<Company> GetCompanies()
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT Id,Name FROM Company WHERE IsDeleted=0");
            return DataHandler.ConstructObjects(reader, typeof(Company)).Cast<Company>().ToList();
        }

        public static void InsertCompany(Company C)
        {
            string query = $"EXEC stpInsertCompany @Name = {C.Name}";
            Utils.ExecuteQuery(query);
        }

        public static void SaveCompany(Company C, bool isAdd = false)
        {
            List<(string, object)> args = new List<(string, object)>
            {
                (nameof(C.Name), C.Name)
            };
            if (isAdd == true)
            {
                DataHandler.InsertDataSP(args, "stpInsertCompany");
            }
            else
            {
                args.Add(("UpdatedOn", ("CURRENT_TIMESTAMP", true)));
                DataHandler.UpdateData(args, C.InitialArgs, C.GetType().Name, (nameof(C.Id), C.Id));
            }
        }
        public static Company GetCompany(int id)
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT Id, Name
                                                    FROM Company
                                                    WHERE IsDeleted=0 AND Id=" + id.ToString());
            return (Company)DataHandler.ConstructObject(reader, typeof(Company));
        }
        public static void DeleteCompany(int id)
        {
            DataHandler.DeleteDataSP("stpDeleteCompany", ("Id", id));
        }
    }
}
