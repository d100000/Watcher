using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Annotations;
using Helper;
using Watcher.Entity;

namespace Watcher.db
{
    public class RecordDbService : DbBase
    {

        public RecordDbService()
        {
            CheckRecordDb();
        }

        /// <summary>
        /// 写入新的数据
        /// </summary>
        /// <param name="thisProcess"></param>
        /// <param name="lastProcess"></param>
        /// <param name="beginTime"></param>
        public recode_info AddNewRecord(Process thisProcess, Process lastProcess)
        {
            var recordDate=new recode_info();
            recordDate.this_prosses_id = thisProcess.Id.ToString();
            recordDate.this_prosses_title = thisProcess.MainWindowTitle;
            recordDate.this_prosses_name = thisProcess.ProcessName;
            if (lastProcess == null)
            {
                recordDate.last_prosses_id = "";
                recordDate.last_prosses_title = "";
            }
            else
            {
                recordDate.last_prosses_id = lastProcess.Id.ToString();
                recordDate.last_prosses_title = lastProcess.MainWindowTitle;
            }
            recordDate.begin_time = Common.GetTimeStamp(DateTime.Now); 
            recordDate.end_time = Common.GetTimeStamp(DateTime.Now);
            recordDate.date = Common.GetDateInt(DateTime.Now);
            recordDate.create_time = Common.GetTimeStamp(DateTime.Now).ToString();
            return Insert(recordDate);
        }

        #region common
        public List<recode_info> QueryById(int id)
        {
            return GetDBClient().Select<recode_info>(x => x.record_id == id);
        }

        public List<recode_info> QueryByDate(int date)
        {
            return GetDBClient().Select<recode_info>(x => x.date >= date);
        }

        public List<recode_info> QueryByDate(long date)
        {
            return GetDBClient().Select<recode_info>(x => x.date == date);
        }

        public recode_info Insert(recode_info entity)
        {
            var generatedId = GetDBClient().Insert<recode_info>(entity, true);
            entity.record_id = int.Parse((string)generatedId);
            return entity;
        }

        public void Update(recode_info entity)
        {
            GetDBClient().Update<recode_info>(entity, new { record_id = entity.record_id });
        }

        public void Update(object value,object where)
        {
            GetDBClient().Update<recode_info>(value, where);
        }

        public void Remove(long id)
        {
            GetDBClient().Delete<recode_info>("record_id", new[] { id });
        }

        public void Remove(long[] ids)
        {
            GetDBClient().Delete<recode_info>("record_id", ids);
        }


        /// <summary>
        /// 
        /// 
        /// </summary>
        public void CheckRecordDb()
        {
            OpenConnect();
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = MDbConnection;
            Boolean testTableExists = false;
            cmd.CommandText = "SELECT * FROM sqlite_master WHERE type='table' and name='recode_info'";

            using (SQLiteDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    testTableExists = true;
                }
            }
            if (!testTableExists)
            {
                string sql = _recordInfoSql;
                SQLiteCommand command = new SQLiteCommand(sql, MDbConnection);
                command.ExecuteNonQuery();
            }
            else
            {
                return;
            }
        }

        private string _recordInfoSql = "CREATE TABLE [recode_info](  [record_id] integer  PRIMARY KEY autoincrement,[uid] INT(32),   [date] INT(8),   [begin_time] INT(14),   [end_time] INT(14),[spend_time] INT(14),   [last_prosses_id] VARCHAR(64),   [last_prosses_title] VARCHAR(126),   [this_prosses_id] VARCHAR(64),   [this_prosses_title] VARCHAR(126), [this_prosses_name] VARCHAR(126),  [create_time] VARCHAR(14),[remark] VARCHAR(1024));  CREATE UNIQUE INDEX [record_id] ON [recode_info]([record_id]);  CREATE INDEX [date_index] ON [recode_info]([date]);  CREATE INDEX [time_index] ON [recode_info](  [begin_time],   [end_time]); ";



        #endregion 

    }
}
