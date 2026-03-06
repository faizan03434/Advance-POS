using StationeryStoreManagementSystem.BL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Windows.Media.Imaging;

namespace StationeryStoreManagementSystem.DL
{
    static class EmployeeDL
    {
        public static DataTable GetEmployeessView()
        {
            List<object> list = new List<object>();
            return DataHandler.FillDataTable(@"SELECT * FROM GetEmployees_View");
        }

        public static Employee GetEmployee(int id)
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT U.Id, U.FirstName, U.LastName, G.Value AS Gender, U.CNIC, U.DateOfBirth, U.Contact, UA.Email, L.Value City, U.Town, U.StreetAddress, U.PostalCode, ES.Salary, RL.Value AS Role
                                                    FROM UserAccount UA
                                                    JOIN [User] U ON UA.UserId = U.Id
                                                    JOIN Employee E ON U.Id = E.Id
                                                    JOIN (
                                                        SELECT EmployeeId, MAX(AddedOn) AS MaxCreatedOn
                                                        FROM EmployeeSalary
                                                        GROUP BY EmployeeId
                                                    ) AS ME ON ME.EmployeeId = U.Id
                                                    JOIN Lookup L ON L.Id=U.City
                                                    JOIN EmployeeSalary ES ON ES.EmployeeId = ME.EmployeeId AND ES.AddedOn = ME.MaxCreatedOn
                                                    JOIN Lookup RL ON E.Role = RL.Id
                                                    JOIN Lookup SL ON E.Status = SL.Id
                                                    JOIN Lookup G ON U.Gender = G.Id
                                                    WHERE SL.Value = 'Active' AND U.Id = " + id.ToString());

            Employee e = null;
            if (reader.Read())
            {
                string role = reader["Role"].ToString();

                if (role == "Admin")
                {
                    e = (Admin)DataHandler.ConstructObject(reader, typeof(Admin),false);
                }
                else
                {
                    e = (Cashier)DataHandler.ConstructObject(reader, typeof(Cashier),false);
                }
            }
            return e;
        }
        public static List<Cashier> GetCashiers()
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT U.Id, U.FirstName, U.LastName, G.Value AS Gender, U.CNIC, U.DateOfBirth, U.Contact, UA.Email, L.Value City, U.Town, U.StreetAddress, U.PostalCode, ES.Salary
                                                    FROM UserAccount UA
                                                    JOIN [User] U ON UA.UserId = U.Id
                                                    JOIN Employee E ON U.Id = E.Id
                                                    JOIN (
                                                    SELECT EmployeeId, MAX(AddedOn) AS MaxCreatedOn
                                                    FROM EmployeeSalary
                                                    GROUP BY EmployeeId
                                                    ) AS ME ON ME.EmployeeId = U.Id
                                                    JOIN Lookup L ON L.Id=U.City
                                                    JOIN EmployeeSalary ES ON ES.EmployeeId = ME.EmployeeId AND ES.AddedOn = ME.MaxCreatedOn
                                                    JOIN Lookup RL ON E.Role = RL.Id
                                                    JOIN Lookup SL ON E.Status = SL.Id
                                                    JOIN Lookup G ON U.Gender = G.Id
                                                    WHERE RL.Value = 'Cashier'");
            return DataHandler.ConstructObjects(reader, typeof(Cashier)).Cast<Cashier>().ToList();
        }
        public static void SaveEmployee(Employee E, bool IsAdd)
        {
            int role = DataHandler.LookupData("Role").Where(x => x.Value == E.DetermineRole(E)).Select(X => X.Key).FirstOrDefault();
            int? gender = E.Gender == null ? null : DataHandler.LookupData("Gender").Where(x => x.Value == E.Gender).Select(X => X.Key).First();
            int? cityId = E.City == null ? null : DataHandler.LookupData("CityPakistan").Where(x => x.Value == E.City).Select(X => X.Key).First();
            List<(string, object)> args = new List<(string, object)>
            {
                (nameof(E.FirstName), E.FirstName),
                (nameof(E.LastName), E.LastName),
                (nameof(gender), gender),
                (nameof(E.CNIC), E.CNIC),
                (nameof(E.DateOfBirth), E.DateOfBirth),
                (nameof(E.Contact), E.Contact),
                (nameof(E.City),cityId),
                (nameof(E.Town),E.Town),
                (nameof(E.StreetAddress),E.StreetAddress),
                (nameof(E.PostalCode),E.PostalCode),
            };
            if (IsAdd == true)
            {
                args.Add((nameof(E.Username), E.Username));
                args.Add((nameof(E.Email), E.Email));
                args.Add((nameof(E.Password), E.Password));
                args.Add(("Role", role));
                 args.Add((nameof(E.Salary), E.Salary));
                DataHandler.InsertDataSP(args, "stpInsertEmployee");
            }
            else
            {
                string email = E.InitialArgs[6].ToString();
                E.InitialArgs.RemoveAt(11);
                E.InitialArgs.RemoveAt(7);
                args.Add(("UpdatedOn", ("CURRENT_TIMESTAMP", true)));
                DataHandler.UpdateData(args, E.InitialArgs, "[User]", (nameof(E.Id), E.Id));
                args = new List<(string, object)>
                {
                    (nameof(E.Email), E.Email)
                };
                DataHandler.UpdateData(args, new List<object> { email }, "UserAccount", ("UserId", E.Id));
                args = new List<(string, object)>
                {
                    ("EmployeeId", E.Id),
                    (nameof(E.Salary), E.Salary),
                    ("AddedOn", ("CURRENT_TIMESTAMP", true))
                };
                DataHandler.InsertData(args, "EmployeeSalary");
            }
        }
        public static void DeleteEmployee(int id)
        {
            DataHandler.DeleteDataSP("stpDeleteEmployee", (nameof(id), id));
        }
    }
}
