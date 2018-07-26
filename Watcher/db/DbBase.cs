using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using  Easyliter;

namespace Watcher.db
{
    public class DbBase
    {

        public string SqLitePath = System.Environment.CurrentDirectory;
        public SQLiteConnection MDbConnection;

        /// <summary>
        /// EasyLiter
        /// </summary>
        private static Client _dbClient;

        static string db_path = Environment.CurrentDirectory + "\\test.db";
        static string connStr = @"Data Source=" + Environment.CurrentDirectory + "\\test.db" +
                         @";Initial Catalog=sqlite;Integrated Security=True;Max Pool Size=10";

        public DbBase()
        {

            if (!Directory.Exists(db_path))
            {
                SQLiteConnection.CreateFile(db_path);
            }


            MDbConnection = new SQLiteConnection(connStr);
            if (MDbConnection.State != System.Data.ConnectionState.Open)
            {
                MDbConnection.Open();
            }
        }

        #region

        public static Client GetDBClient()
        {
            if (_dbClient == null)
            {
                _dbClient = new Client(connStr);
            }
            return _dbClient;
        }


        #region base record_info database

        /// <summary>
        /// 
        /// </summary>
        public void OpenConnect()
        {
            if (MDbConnection.State != System.Data.ConnectionState.Open)
            {
                MDbConnection.Open();
            }
        } 

        #endregion

        #endregion
    }
}
