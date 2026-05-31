using Microsoft.Data.SqlClient;
using StationeryStoreManagementSystem.BL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace StationeryStoreManagementSystem.DL
{
    static class NotificationDL
    {
        //public static DataTable GetNotifications_View()
        //{
        //    List<object> list = new List<object>();
        //    if (Utils.CurrentEmployee == null)
        //        return new DataTable();
        //    return DataHandler.FillDataTable($"SELECT V.Id, [From], IsViewed, Notification FROM GetNotifications_View V JOIN Employee E ON V.UserId = E.Id WHERE E.Id = {Utils.CurrentEmployee.Id}");
        //}


        // 1. Dashboard ke liye: Sirf wo alerts jo Acknowledge/View nahi hue (IsViewed = 0)
        public static DataTable GetActiveNotifications()
        {
            if (Utils.CurrentEmployee == null)
                return new DataTable();

            // 'AND IsViewed = 0' lagaya hai taake refresh karne par tab tak gayab na ho jab tak user acknowledge na kare
            return DataHandler.FillDataTable($@"
        SELECT V.Id, [From], IsViewed, Notification 
        FROM GetNotifications_View V 
        JOIN Employee E ON V.UserId = E.Id 
        WHERE E.Id = {Utils.CurrentEmployee.Id} AND IsViewed = 0");
        }

        // 2. Bell Icon ke liye: Pichle 30 din ki saari history (IsViewed 0 ho ya 1)
        public static DataTable GetNotificationHistory()
        {
            if (Utils.CurrentEmployee == null)
                return new DataTable();

            // Yahan humne "CASE WHEN ViewedAt IS NULL THEN 0 ELSE 1 END AS IsViewed" lagaya hai
            // Taake aapke frontend ko 'IsViewed' wala column milta rahay aur wo crash na ho.
            return DataHandler.FillDataTable($@"
        SELECT 
            N.Id, 
            E.FirstName + ' ' + E.LastName AS [From], 
            CASE WHEN N.ViewedAt IS NULL THEN 0 ELSE 1 END AS IsViewed, 
            N.Content AS [Notification],
            N.AddedOn
        FROM Notification N 
        JOIN [User] E ON N.AddedBy = E.Id
        WHERE N.UserId = {Utils.CurrentEmployee.Id} 
        AND DATEDIFF(day, N.AddedOn, GETDATE()) <= 30 
        ORDER BY N.AddedOn DESC");
        }
        public static Notification GetNotification(int id)
        {
            SqlDataReader reader = Utils.ReadData(@"SELECT N.Id, Content AS [Notification], AddedBy [From], N.UserId, AddedOn 
                                                   FROM Notification N 
                                                   WHERE Id=" + id.ToString());
            List<object> args = Utils.GetArgs(reader);
            if (args.Count != 0)
            {
                if (args[2] != null)
                    args[2] = EmployeeDL.GetEmployee((int)args[2]);
                if (args[3] != null)
                    args[3] = EmployeeDL.GetEmployee((int)args[3]);
                if (args[4] != null)
                    args[4] = args[4].ToString();
                return new Notification(args);
            }
            else
                return null;
        }
        public static void SaveNotification(Notification N, bool isAdd = false)
        {
            if (isAdd == true)
            {
                List<(string, object)> args = new List<(string, object)>
                {
                    ("UserId", N.Receiver.Id),
                    ("Content", N.Content),
                    ("AddedBy", N.Sender.Id)
                };
                DataHandler.InsertDataSP(args, "stpInsertNotification");
            }
            else
            {
                Utils.ExecuteQuery($"EXEC stpUpdateNotification @id = {N.Id}");
            }
        }
    }
}
