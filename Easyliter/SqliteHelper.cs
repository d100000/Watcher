using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Data.SqlClient;

namespace Easyliter
{
    /// <summary>
    /// ** SQLiteHelper
    /// ** 作者：未知
    /// ** 创始时间：未知
    /// ** 修改人：sunkaixuan
    /// ** 修改时间：2015-5-13
    /// ** qq：610262374 欢迎交流,共同提高 ,命名语法等写的不好的地方欢迎大家的给出宝贵建议
    /// </summary>
    internal class SQLiteHelper : IDisposable
    {
        private SQLiteConnection conn = null;
        protected SQLiteCommand cmd = null;
        protected SQLiteDataReader sdr = null;
        ///
        /// 构造函数
        ///
        public SQLiteHelper(string connStr)
        {
            conn = new SQLiteConnection(connStr);
        }
        ///
        /// 获取连接结果，未连接打开连接
        ///
        /// 连接结果
        protected SQLiteConnection GetConn()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            return conn;
        }

        ///
        /// 获取连接结果，未连接打开连接
        ///
        /// 连接结果
        protected void Close()
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        ///
        /// 该方法执行传入的增删改SQL语句
        ///
        /// 要执行的增删改SQL语句
        /// 返回更新的记录数
        public int ExecuteNonQuery(string sql)
        {
            int res;
            try
            {
                cmd = new SQLiteCommand(sql, GetConn());
                res = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                res = -1;
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return res;
        }
        ///
        /// 执行带参数的SQL增删改语句
        ///
        /// SQL增删改语句
        /// 参数集合
        /// 返回更新的记录数
        public int ExecuteNonQuery(string sql, SQLiteParameter[] paras)
        {
            int res;
            using (cmd = new SQLiteCommand(sql, GetConn()))
            {
                cmd.Parameters.AddRange(paras);
                res = cmd.ExecuteNonQuery();
            }
            return res;
        }
        ///
        /// 该方法执行传入的SQL查询语句
        ///
        /// SQL查询语句
        /// 返回数据集合
        public DataTable ExecuteQuery(string sql)
        {
            DataTable dt = new DataTable();
            cmd = new SQLiteCommand(sql, GetConn());

            using (sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {

                dt.Load(sdr);
            }
            return dt;
        }
        ///
        /// 执行带参数的SQL查询语句
        ///
        /// SQL查询语句
        /// 参数集合
        /// 返回数据集合
        public DataTable ExecuteQuery(string sql, SQLiteParameter[] paras)
        {
            DataTable dt = new DataTable();
            cmd = new SQLiteCommand(sql, GetConn());
            cmd.Parameters.AddRange(paras);

            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        /// <summary>
        /// 反回第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public string GetString(string sql, SQLiteParameter[] paras)
        {
            DataTable dt = new DataTable();
            cmd = new SQLiteCommand(sql, GetConn());
            cmd.Parameters.AddRange(paras);
            var reval = cmd.ExecuteScalar();
            return reval.ToString();
        }

        /// <summary>
        /// 反回第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public int GetInt(string sql, SQLiteParameter[] paras)
        {
            DataTable dt = new DataTable();
            cmd = new SQLiteCommand(sql, GetConn());
            cmd.Parameters.AddRange(paras);
            var reval = cmd.ExecuteScalar();
            return (int) Convert.ToInt64(reval);
        }

        ///
        /// 执行带参数的SQL查询判断语句
        ///
        /// SQL查询语句
        /// 参数集合
        /// 返回是否为空
        public bool BoolExecuteQuery(string sql, SQLiteParameter[] paras)
        {
            DataTable dt = new DataTable();
            cmd = new SQLiteCommand(sql, GetConn());
            cmd.Parameters.AddRange(paras);
            try
            {
                using (sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    dt.Load(sdr);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            DataRow[] rows = dt.Select();
            bool temp = false;
            if (rows.Length > 0)
            {
                temp = true;
            }
            return temp;
        }
        ///
        /// 该方法执行传入的SQL查询判断语句
        ///
        /// SQL查询语句
        /// 返回是否为空
        public bool BoolExecuteQuery(string sql)
        {
            DataTable dt = new DataTable();
            cmd = new SQLiteCommand(sql, GetConn());
            using (sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                dt.Load(sdr);
            }
            DataRow[] rows = dt.Select();
            bool temp = false;
            if (rows.Length > 0)
            {
                temp = true;
            }
            return temp;
        }

        public void Dispose()
        {
            Close();
        }

        private void AddDataTable(DataTable dtb)
        {

        }










    }

}