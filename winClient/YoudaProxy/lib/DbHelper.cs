using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace YoudaProxy.lib
{
    public class DbHelper
    {
        public static Tuple<int, int> findFirstKeyWords(string script, int startIdx, string[] words)
        {
            Dictionary<int, int> wordsIndex = new Dictionary<int, int>();
            for (int i = 0; i < words.Length; i++)
            {
                wordsIndex[i] = script.IndexOf(words[i], startIdx);
            }
            int r = -1;
            int minPos = -1;
            foreach (int idx in wordsIndex.Keys)
            {
                int pos = wordsIndex[idx];
                if (pos == -1)
                {
                    continue;
                }
                if (minPos == -1)
                {
                    minPos = pos;
                    r = idx;
                }
                else if (minPos > pos)
                {
                    minPos = pos;
                    r = idx;
                }
            }
            return new Tuple<int, int>(r, minPos);
        }

        static OleDbParameter[] prepareParametersForSql(string sql, object[] parameterValues)
        {
            OleDbParameter[] commandParameters = new OleDbParameter[parameterValues.Length];
            List<string> psNames = new List<string>();
            int start = sql.IndexOf("@");
            string[] endWords = new string[] { " ", ",", ")" };
            while (start != -1)
            {
                string pname = string.Empty;
                Tuple<int, int> ret = findFirstKeyWords(sql, start, endWords);
                if (ret.Item2 == -1)
                {
                    pname = sql.Substring(start);
                }
                else
                {
                    pname = sql.Substring(start, ret.Item2 - start);
                }
                if (!psNames.Contains(pname))
                {
                    psNames.Add(pname);
                }
                start = sql.IndexOf("@", start + 1);
            }
            for (int i = 0; i < parameterValues.Length; i++)
            {
                commandParameters[i] = new OleDbParameter();
                commandParameters[i].ParameterName = psNames[i];
                commandParameters[i].Value = parameterValues[i];
            }
            return commandParameters;
        }

        static string constr = @"Provider = Microsoft.Jet.OLEDB.4.0; Data Source = " + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.mdb");

        static DataSet execDataSet(OleDbCommand cmd)
        {
            var conn = new OleDbConnection(constr);
            cmd.Connection = conn;

            OleDbDataAdapter adapter = new OleDbDataAdapter();
            adapter.SelectCommand = cmd;
            var ds = new DataSet();
            adapter.Fill(ds);

            return ds;
        }

        public static DataSet ExecDataSet(string sql, params object[] parameterValues)
        {
            OleDbCommand cmd = new OleDbCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                cmd.Parameters.AddRange(prepareParametersForSql(sql, parameterValues));
            }
            return execDataSet(cmd);
        }

        public static DataTable ExecDataTable(string sql, params object[] parameterValues)
        {
            return ExecDataSet(sql, parameterValues).Tables[0];
        }

        public static DataRow ExecDataRow(string sql, params object[] parameterValues)
        {
            return ExecDataTable(sql, parameterValues).Rows[0];
        }

        public static string ExecScalar(string sql, params object[] parameterValues)
        {
            return ExecDataRow(sql, parameterValues)[0].ToString();
        }

        public static void ExecNonQuery(string sql, params object[] parameterValues)
        {
            var conn = new OleDbConnection(constr);
            conn.Open();
            var cmd = new OleDbCommand(sql, conn);
            if (parameterValues != null && parameterValues.Length > 0)
            {
                cmd.Parameters.AddRange(prepareParametersForSql(sql, parameterValues));
            }
            var str = cmd.ExecuteNonQuery();
            conn.Close();
        }

        public static void MapDataRowToObject(DataRow fc, object target)
        {
            PropertyInfo[] properties = target.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name.ToLower();
                if (fc.Table.Columns.Contains(propertyName))
                {
                    if (!(fc[propertyName] is System.DBNull))
                    {
                        object dataValue = null;
                        string sourceVal = fc[propertyName].ToString();
                        switch (property.PropertyType.ToString())
                        {
                            case ("System.Int16"):
                            case ("System.Int32"):
                            case ("System.Int64"):
                            case ("System.UInt16"):
                            case ("System.UInt32"):
                            case ("System.UInt64"):
                                {
                                    dataValue = int.Parse(sourceVal);
                                    break;
                                }
                            case ("System.Double"):
                                {
                                    dataValue = double.Parse(sourceVal);
                                    break;
                                }
                            case ("System.Decimal"):
                                {
                                    dataValue = decimal.Parse(sourceVal);
                                    break;
                                }
                            case ("System.Boolean"):
                                {
                                    dataValue = bool.Parse(sourceVal);
                                    break;
                                }
                            case ("System.DateTime"):
                                {
                                    dataValue = DateTime.Parse(sourceVal);
                                    break;
                                }
                            default:
                                dataValue = sourceVal;
                                break;
                        }
                        property.SetValue(target, dataValue, null);
                    }
                }
            }
        }

        public static T MapDataRowToObject<T>(DataRow fc) where T : class, new()
        {
            if (fc == null)
                return null;
            T item = new T();
            MapDataRowToObject(fc, item);
            return item;
        }

        public static void Save<T>(T item) where T : BaseEntity, new()
        {
            PropertyInfo idField = null;
            StringBuilder sql = new StringBuilder();
            List<object> paras = new List<object>();
            Type type = item.GetType();
            PropertyInfo[] properties = type.GetProperties();
            List<string> arr_Fields = new List<string>();
            foreach (PropertyInfo field in properties)
            {
                //Check if there is ignore attribute
                string fieldName = field.Name;
                if (fieldName == "Id")
                {
                    idField = field;
                }
                else
                {
                    arr_Fields.Add(fieldName);
                    paras.Add(field.GetValue(item));
                }
            }
            if (item.Id == 0)
            {
                sql.Append("insert into ");
                sql.Append(type.Name);
                sql.Append("(");
                int idx = 0;
                foreach (string f in arr_Fields)
                {
                    if (idx > 0)
                    {
                        sql.Append(",");
                    }
                    sql.Append(f);
                    idx++;
                }
                sql.Append(") values (");
                idx = 0;
                foreach (string f in arr_Fields)
                {
                    if (idx > 0)
                    {
                        sql.Append(",");
                    }
                    sql.Append("@" + f);
                    idx++;
                }
                sql.Append(");");

                var conn = new OleDbConnection(constr);
                OleDbCommand cmd = new OleDbCommand(sql.ToString(), conn);
                cmd.Parameters.AddRange(prepareParametersForSql(cmd.CommandText, paras.ToArray()));
                conn.Open();
                cmd.ExecuteNonQuery();
                cmd.CommandText = "SELECT @@IDENTITY";
                string id = cmd.ExecuteScalar().ToString();
                conn.Close();
                idField.SetValue(item, int.Parse(id));
            }
            else
            {
                sql.Append("update " + type.Name + " set ");
                int idx = 0;
                foreach (PropertyInfo field in properties)
                {
                    if (field.Name == "Id")
                        continue;
                    if (idx > 0)
                    {
                        sql.Append(",");
                    }
                    sql.Append(field.Name + "=@" + field.Name);
                    idx++;
                }
                sql.Append(" where id = " + idField.GetValue(item).ToString());
                ExecNonQuery(sql.ToString(), paras.ToArray());
            }
        }

        public static T Get<T>(int id) where T : BaseEntity, new()
        {
            Type type = typeof(T);
            return GetBySql<T>("Select * From " + type.Name + " where Id=" + id, null);
        }

        public static void Delete<T>(int id) where T : BaseEntity, new()
        {
            Type type = typeof(T);
            ExecNonQuery("Delete from " + type.Name + " where Id=" + id);
        }

        public static T GetBySql<T>(string sql, params object[] paras) where T : BaseEntity, new()
        {
            DataRow dr = ExecDataRow(sql, paras);
            return MapDataRowToObject<T>(dr);
        }
    }
}
