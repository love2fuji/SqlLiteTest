using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace SqlLiteTest.Config
{
    public class SQLiteHelper
    {
        private static string PassWord = "***";
        public string OldPassword { get; set; }
        // app.config name="SQLitelocaldb"
        private static string connectionString = string.Format(ConfigurationManager.ConnectionStrings["SQLitelocaldb"].ConnectionString + "; password=" + PassWord);


        // app.config name="localdb"
        public static string SQLiteDbFilePath = ConfigurationManager.ConnectionStrings["localdb"].ConnectionString;

        public SQLiteHelper()
        {
            SetConnectionString(SQLiteDbFilePath, PassWord);
        }

        /// <summary>
        /// 根据数据源、密码、版本号设置连接字符串。
        /// </summary>
        /// <param name="dataSource">数据源。</param>
        /// <param name="passWord">密码。</param>
        /// <param name="version">版本号（缺省为3）。</param>
        public static void SetConnectionString(string dataSource, string passWord, int version = 3)
        {
            connectionString = string.Format("Data Source={0};Version={1};password={2}",
                dataSource, version, passWord);
        }

        /// <summary>
        /// 创建一个数据库文件。如果存在同名数据库文件，则会覆盖。
        /// </summary>
        /// <param name="dbName">数据库文件名。为null或空串时不创建。</param>
        /// <param name="password">（可选）数据库密码，默认为空。</param>
        /// <exception cref="Exception"></exception>
        public static void CreateDB(string dbName)
        {
            if (!string.IsNullOrEmpty(dbName))
            {
                try { SQLiteConnection.CreateFile(dbName); }
                catch (Exception) { throw; }
            }
        }

        /// <summary> 
        /// 对SQLite数据库执行 增 删 改操作，返回受影响的行数。 
        /// </summary> 
        /// <param name="sql">要执行的增删改的SQL语句。</param> 
        /// <param name="parameters">执行增删改语句所需要的参数，参数必须以它们在SQL语句中的顺序为准。</param> 
        /// <returns></returns> 
        /// <exception cref="Exception"></exception>
        public static int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            int affectedRows = 0;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    try
                    {
                        connection.Open();
                        command.CommandText = sql;
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        affectedRows = command.ExecuteNonQuery();
                    }
                    catch (Exception) { throw; }
                }
            }
            return affectedRows;
        }

        /// <summary>
        /// 批量处理数据操作语句。
        /// </summary>
        /// <param name="list">SQL语句集合。</param>
        /// <exception cref="Exception"></exception>
        public static void ExecuteNonQueryBatch(List<KeyValuePair<string, SQLiteParameter[]>> list)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try { conn.Open(); }
                catch { throw; }
                using (SQLiteTransaction tran = conn.BeginTransaction())
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        try
                        {
                            foreach (var item in list)
                            {
                                cmd.CommandText = item.Key;
                                if (item.Value != null)
                                {
                                    cmd.Parameters.AddRange(item.Value);
                                }
                                cmd.ExecuteNonQuery();
                            }
                            tran.Commit();
                        }
                        catch (Exception) { tran.Rollback(); throw; }
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，并返回第一个结果。
        /// </summary>
        /// <param name="sql">查询语句。</param>
        /// <returns>查询结果。</returns>
        /// <exception cref="Exception"></exception>
        public static object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.CommandText = sql;
                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        return cmd.ExecuteScalar();
                    }
                    catch (Exception) { throw; }
                }
            }
        }

        /// <summary> 
        /// 执行一个查询语句，返回一个包含查询结果的DataTable。 
        /// </summary> 
        /// <param name="sql">要执行的查询语句。</param> 
        /// <param name="parameters">执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准。</param> 
        /// <returns></returns> 
        /// <exception cref="Exception"></exception>
        public static DataTable ExecuteQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    DataTable data = new DataTable();
                    try { adapter.Fill(data); }
                    catch (Exception) { throw; }
                    return data;
                }
            }
        }

        /// <summary> 
        /// 执行一个查询语句，返回一个关联的SQLiteDataReader实例。 
        /// </summary> 
        /// <param name="sql">要执行的查询语句。</param> 
        /// <param name="parameters">执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准。</param> 
        /// <returns></returns> 
        /// <exception cref="Exception"></exception>
        public static SQLiteDataReader ExecuteReader(string sql, params SQLiteParameter[] parameters)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            try
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }
                connection.Open();
                return command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception) { throw; }
        }

        /// <summary> 
        /// 执行一个查询语句，返回一个包含查询结果的 DataSet。 
        /// </summary> 
        /// <param name="sql">要执行的查询语句。</param> 
        /// <param name="parameters">执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准。</param> 
        /// <returns></returns> 
        /// <exception cref="Exception"></exception>
        public static DataSet ExecuteQueryDataSet(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);

                    DataSet dataSet = new DataSet();

                    try
                    {
                        adapter.Fill(dataSet);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (connection != null)
                        {
                            if (connection.State == ConnectionState.Open)
                            {
                                connection.Close();
                            }
                        }
                    }
                    return dataSet;
                }
            }
        }

        /// <summary> 
        /// 查询数据库中的所有数据类型信息。
        /// </summary> 
        /// <returns></returns> 
        /// <exception cref="Exception"></exception>
        public static DataTable GetSchema()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return connection.GetSchema("TABLES");
                }
                catch (Exception) { throw; }
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="oldPassword">原密码</param>
        /// <param name="newPassword">新密码</param>
        public static bool ChangePassword(string oldPassword, string newPassword)
        {
            using (SQLiteConnection connection = new SQLiteConnection())
            {
                connection.ConnectionString = connectionString;

                if (oldPassword != null && oldPassword.Length > 0)
                {
                    connection.ConnectionString += ";Password=" + oldPassword;
                }
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("无法连接到数据库! 详细：" + ex.Message);
                }
                connection.ChangePassword(newPassword);
                connection.Close();
                return true;
            }
        }

    }
}
