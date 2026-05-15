using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.DL
{
    internal class DataHandler
    {
        public static List<object> ConstructObjects(SqlDataReader reader, Type type)
        {
            List<object> args;
            List<object> objs = new List<object>();
            while (reader.Read())
            {
                args = new List<object>();
                var dbColumns = reader.GetColumnSchema();
                for (int i = 0; i < dbColumns.Count; i++)
                {
                    args.Add(Utils.NormalizeForORM(reader.GetValue(i)));
                }
                objs.Add(Activator.CreateInstance(type, args));
            }
            return objs;
        }
        public static object ConstructObject(SqlDataReader reader, Type type,bool isRead=true)
        {
            List<object> args;
            object obj;
            if (isRead==false || reader.Read())
            {
                args = new List<object>();
                var dbColumns = reader.GetColumnSchema();
                for (int i = 0; i < dbColumns.Count; i++)
                {
                    args.Add(Utils.NormalizeForORM(reader.GetValue(i)));
                }
                return Activator.CreateInstance(type, args);
            }
            else
                return null;
        }
        public static DataTable FillDataTable(string query)
        {
            Utils.CloseReader();
            var conn = Configuration.getInstance().getConnection();
            SqlCommand command = new SqlCommand(query, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }
        public static void AddData(List<(string, object)> args, string relation)
        {
            string attributes = string.Join(',', args.Select(x => x.Item1));
            string values = string.Join(',', args.Select(x =>
            {
                if (x.Item2 == null)
                    x.Item2 = "NULL";
                else if (x.Item2.GetType() == typeof(string))
                    x.Item2 = $"'{x.Item2}'";
                return x.Item2;
            }));
            Utils.ExecuteQuery($"INSERT INTO {relation}({attributes}) VALUES ({values})");
        }
        public static void UpdateData(List<(string, object)> args, List<object> initialArgs, string relation, (string, object) id)
        {
            List<string> updatedAttributes = new List<string>();
            for (int i = 0; i < initialArgs.Count; i++)
            {
                (string, object) struc = args[i];
                if (struc.Item2?.ToString() != initialArgs[i]?.ToString())
                {
                    updatedAttributes.Add(struc.Item1 + " = " + Utils.NormalizeForQuery(struc.Item2));
                }
            }
            for (int i = initialArgs.Count; i < args.Count; i++)
            {
                (string, object) struc = args[i];
                updatedAttributes.Add(struc.Item1 + " = " + Utils.NormalizeForQuery(struc.Item2));

            }
            if (updatedAttributes.Count != 0 && !updatedAttributes[0].Contains("CURRENT_TIMESTAMP"))
            {
                Utils.ExecuteQuery($"UPDATE {relation} SET {string.Join(',', updatedAttributes)} WHERE {id.Item1}={id.Item2}");

            }
        }
        public static void InsertDataSP(List<(string, object)> args, string stpName)
        {
            List<string> adjustedAttributes = new List<string>();
            for (int i = 0; i < args.Count; i++)
            {
                (string, object) struc = args[i];
                adjustedAttributes.Add("@" + struc.Item1 + " = " + Utils.NormalizeForQuery(struc.Item2));
            }
            if (adjustedAttributes.Count != 0)
                Utils.ExecuteQuery($"EXEC  {stpName} {string.Join(',', adjustedAttributes)}");
        }
        public static object InsertDataSPReturn(List<(string, object)> args, string stpName)
        {
            List<string> adjustedAttributes = new List<string>();
            for (int i = 0; i < args.Count; i++)
            {
                (string, object) struc = args[i];
                adjustedAttributes.Add("@" + struc.Item1 + " = " + Utils.NormalizeForQuery(struc.Item2));
            }
            if (adjustedAttributes.Count != 0)
                return Utils.ExecuteQueryScalar($"EXEC  {stpName} {string.Join(',', adjustedAttributes)}");
            else
                return null;
        }
        public static void BulkDataExecuteSP(string parameterName,string objectTypeName,string stpName,IEnumerable value)
        {
            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = $"@{parameterName}";
            parameter.TypeName = objectTypeName;
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.Value = value;
            SqlCommand command = new SqlCommand(stpName, Configuration.getInstance().getConnection());
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(parameter);
            Utils.CloseReader();
            command.ExecuteNonQuery();
        }
        public static object BulkDataExecuteSP( string stpName, List<(string,string?, SqlDbType,object)> objs)
        {

            SqlCommand command = new SqlCommand(stpName, Configuration.getInstance().getConnection());
            command.CommandType = CommandType.StoredProcedure;
            foreach(var obj in objs)
            {
                SqlParameter parameter = new SqlParameter();
                parameter.ParameterName = $"@{obj.Item1}";
                parameter.TypeName = obj.Item2;
                parameter.SqlDbType = obj.Item3;
                parameter.Value = obj.Item4;
                command.Parameters.Add(parameter);
            }
            Utils.CloseReader();
            return command.ExecuteScalar();
        }
        public static void DeleteDataSP(string stpName, (string, object) id)
        {
            Utils.ExecuteQuery($"EXEC {stpName} @{id.Item1} = {id.Item2}");
        }
        public static void InsertData(List<(string, object)> args, string relationName)
        {
            List<string> adjustedAttributes = new List<string>();
            for (int i = 0; i < args.Count; i++)
            {
                (string, object) struc = args[i];
                adjustedAttributes.Add(Utils.NormalizeForQuery(struc.Item2).ToString());
            }
            if (adjustedAttributes.Count != 0)
                Utils.ExecuteQuery($"INSERT INTO {relationName} VALUES ({string.Join(',', adjustedAttributes)}) ");
        }

        public static Dictionary<int,string> LookupData(string category)
        {
            Dictionary<int,string> keyValuePairs = new Dictionary<int, string>();
            SqlDataReader reader = Utils.ReadData($"SELECT Id, Value FROM Lookup WHERE Category = '{category}'");
            while (reader.Read())
            {
                keyValuePairs.Add(reader.GetInt32(0),reader.GetString(1));

            }
            return keyValuePairs;
        }
    }
}

