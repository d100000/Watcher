using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;


namespace Easyliter
{
    /// <summary>
    /// ** Easyliter
    /// ** 创始时间：2015-5-13
    /// ** 修改时间：-
    /// ** 作者：sunkaixuan
    /// ** qq：610262374 欢迎交流,共同提高 ,命名语法等写的不好的地方欢迎大家的给出宝贵建议
    /// </summary>
    public class Client
    {
        private string _connstr = null;
        public string message { get; private set; }
        public Client(string connstr)
        {
            _connstr = connstr;
        }

        /// <summary>
        /// 根据SQL查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="pars">例如 : new { id>0 }</param>
        /// <returns>查询结果</returns> 
        public List<T> Select<T>(string sql, object pars = null) where T : new()
        {
            DataTable dt = null;
            if (pars == null)
                dt = GetDataTable(sql);
            else
                dt = GetDataTable(sql, Common.GetObjectToSQLiteParameter(pars));
            var reval = Common.List<T>(dt);
            return reval;
        }

        public El_Queryable<T> Query<T>() where T : new()
        {
            El_Queryable<T> q = new El_Queryable<T>();
            q.QueryItemList = new List<EL_QueryItem>();
            var tEntity = new T();
            var type = tEntity.GetType();
            EL_QueryItem item = new EL_QueryItem()
            {
                TableName = type.Name,
                AppendValues = new List<string>()
            };
            q.e = new Client(_connstr);
            q.QueryItemList.Add(item);
            return q;
        }

        /// <summary>
        /// 根据拉姆达表达示查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpressions">查询表达示</param>
        /// <returns></returns>
        public List<T> Select<T>(params Expression<Func<T, bool>>[] whereExpressions) where T : new()
        {
            try
            {
                var tEntity = new T();
                var type = tEntity.GetType();
                string whereStr = string.Empty;
                if (whereExpressions != null && whereExpressions.Length > 0)
                {
                    foreach (var expression in whereExpressions)
                    {
                        if (expression.Body is BinaryExpression)
                        {
                            BinaryExpression be = ((BinaryExpression) expression.Body);
                            whereStr = " and " + Common.BinarExpressionProvider(be.Left, be.Right, be.NodeType);
                        }
                    }
                }
                string sql = string.Format("select * from {0} where 1=1  {1} ", type.Name, whereStr);
                var dt = GetDataTable(sql);
                var reval = Common.List<T>(dt);
                return reval;
            }
            catch(Exception ex)
            {
                
                return null;
            }
        }

        /// <summary>
        /// 判断DBproduct是否有重复
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public bool JudgeRepeat(List<string> datas)
        {
            if (datas.Count() != 2)
                return false;
            string sql = string.Format("select * from DBProduct where ProductInfo='{0}' and ProductType='{1}'", datas[0], datas[1]);
            var deleteRowCount = GetDataTable(sql);
            if (deleteRowCount.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据拉姆达表达示查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCount"></param>
        /// <param name="order">id asc 或者 id desc</param>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public List<T> SelectPage<T>(int pageIndex, int pageSize, ref int pageCount, string orderBy, params Expression<Func<T, bool>>[] expressions) where T : new()
        {

            var tEntity = new T();
            var type = tEntity.GetType();
            string whereStr = string.Empty;
            foreach (var expression in expressions)
            {
                if (expression.Body is BinaryExpression)
                {
                    BinaryExpression be = ((BinaryExpression)expression.Body);
                    whereStr = " and " + Common.BinarExpressionProvider(be.Left, be.Right, be.NodeType);
                }
            }
            string sql = string.Format("select * from {0} where 1=1  {1} order by {2}  LIMIT {3},{4} ", type.Name, whereStr, orderBy, (pageIndex - 1) * pageSize, pageSize);
            string sqlCount = string.Format("select count(*) from {0} where 1=1  {1} order by {2}   ", type.Name, whereStr, orderBy);

            var dt = GetDataTable(sql);
            pageCount = GetInt(sqlCount);
            var reval = Common.List<T>(dt);
            return reval;
        }


        /// <summary>
        /// 创建类
        /// </summary>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="fullPath">生成类的存放物理目录</param>
        /// <param name="tableNames">指定生成的表名，不填为生成所有表</param>
        /// <returns>执行结果</returns>
        public string CreateClass(string nameSpace, string dirPath, params string[] tableNames)
        {

            StringBuilder reval = new StringBuilder();
            if (nameSpace == null)
            {
                return "nameSpace不能为空";
            }
            if (dirPath == null)
            {
                return "DirPath不能为空";
            }
            if (!System.IO.Directory.Exists(dirPath))
            {
                return "找不到dicPath路径";
            }

            string sql = "select name from sqlite_master where type='table'  order by name";
            var dt = GetDataTable(sql);
            if (dt == null || dt.Rows.Count == 0)
            {
                throw new Exception("数据库没有可生成的表！");
            }
            else
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string tableName = dr["name"].ToString();
                    if (!tableName.Contains("_old") && tableName != "sqlite_sequence")
                    {
                        if (tableNames.Length == 0 || tableNames.Contains(tableName))
                        {
                            string classsql = string.Format("select  * from {0} Limit 1", tableName);
                            CreateClassBySql(nameSpace, dirPath, tableName, classsql);
                            reval.AppendFormat("表【{0}】：创建成功！\r\n", tableName);
                        }
                    }
                }
            }
            message = reval.ToString();
            return message;
        }
        /// <summary>
        /// 根据SQL创建类
        /// </summary>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="fullPath">生成类的存放物理目录</param>
        /// <param name="tableNames">指定生成的表名，不填为生成所有表</param>
        /// <param name="classsql">sql语句</param>
        public bool CreateClassBySql(string nameSpace, string dirPath, string tableName, string classsql)
        {
            DataTable classSource = GetDataTable(classsql);
            string classString = ClassGenerating.DataTableToClass(classSource, tableName, nameSpace);
            string calssPath = string.Format("{0}\\{1}.cs", dirPath.TrimEnd('\\'), tableName);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            if (!File.Exists(calssPath))
            {
                FileInfo file = new FileInfo(calssPath);
                using (FileStream stream = file.Create())
                {
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        //写入字符串     
                        writer.Write(classString);

                        //输出
                        writer.Flush();
                    }
                }
            }
            System.IO.File.WriteAllText(calssPath, classString, Encoding.UTF8);
            return true;
        }

        /// <summary>
        /// 插入
        /// 用法:
        /// Product p = new Product()
        /// {
        ///  category_id = 2,
        ///  sku = "sku",
        ///  title = "title"
        /// };
        /// e.Insert<Product>(p);
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="isIdentity">主键是否为自增长,true可以不填,false必填</param>
        /// <returns></returns>
        public object Insert<TEntity>(TEntity entity, bool isIdentity = true) where TEntity : class
        {
            Type type = entity.GetType();
            var primaryKeyName = GetPrimaryKey(type);
            List<SQLiteParameter> pars = new List<SQLiteParameter>();
            //2.获得实体的属性集合 
            PropertyInfo[] props = type.GetProperties();

            //实例化一个StringBuilder做字符串的拼接 
            StringBuilder sb = new StringBuilder();

            sb.Append("insert into " + type.Name + " (");

            //3.遍历实体的属性集合 
            foreach (PropertyInfo prop in props)
            {
                //EntityState,@EntityKey
                if (isIdentity == false || (isIdentity && prop.Name != primaryKeyName))
                {
                    //4.将属性的名字加入到字符串中 
                    sb.Append(prop.Name + ",");
                }
            }
            //**去掉最后一个逗号 
            sb.Remove(sb.Length - 1, 1);
            sb.Append(" ) values(");

            //5.再次遍历，形成参数列表"(@xx,@xx@xx)"的形式 
            foreach (PropertyInfo prop in props)
            {
                //EntityState,@EntityKey
                if (isIdentity == false || (isIdentity && prop.Name != primaryKeyName))
                {
                    sb.Append("@" + prop.Name + ",");
                    object val = prop.GetValue(entity, null);
                    if (val == null)
                        val = DBNull.Value;
                    pars.Add(new SQLiteParameter("@" + prop.Name, val));
                }
            }
            //**去掉最后一个逗号 
            sb.Remove(sb.Length - 1, 1);
            sb.Append(");select last_insert_rowid();");

            var sql = sb.ToString();
            var lastInsertRowId = GetString(sql, pars.ToArray());
            message = string.Format("插入成功");
            return lastInsertRowId;

        }


        /// <summary>
        /// 更新
        /// 用法:
        /// Update<ClassName>(new { name='小张',sex='男'},new {id=1})
        /// </summary>
        /// <typeparam name="TEntity">更新实体类型</typeparam>
        public bool Update<TEntity>(object rowObj, object whereObj) where TEntity : class
        {
            if (rowObj == null) { message = "rowObj不能为空"; return false; };
            if (whereObj == null) { message = "whereObj不能为空"; return false; };

            using (SQLiteHelper db = new SQLiteHelper(_connstr))
            {
                Type type = typeof(TEntity);
                string sql = string.Format(" update {0} set ", type.Name);
                Dictionary<string, string> rows = Common.GetObjectToDictionary(rowObj);
                foreach (var r in rows)
                {
                    sql += string.Format(" {0} =@{0}  ,", r.Key);
                }
                sql = sql.TrimEnd(',');
                sql += string.Format(" where  1=1  ");
                Dictionary<string, string> wheres = Common.GetObjectToDictionary(whereObj);
                foreach (var r in wheres)
                {
                    sql += string.Format(" and {0} =@{0}", r.Key);
                }
                List<SQLiteParameter> parsList = new List<SQLiteParameter>();
                parsList.AddRange(rows.Select(c => new SQLiteParameter("@" + c.Key, c.Value)));
                parsList.AddRange(wheres.Select(c => new SQLiteParameter("@" + c.Key, c.Value)));
                var updateRowCount = db.ExecuteNonQuery(sql, parsList.ToArray());
                message = string.Format("{0}行受影响", updateRowCount);
                return updateRowCount > 0;
            }
        }

        /// <summary>
        /// 批量删除
        /// 用法:
        /// Delete<T>(new int[]{1,2,3})
        /// 或者
        /// Delete<T>(3)
        /// </summary>
        /// <param name="oc"></param>
        /// <param name="whereIn">in的集合</param>
        /// <param name="whereIn">主键为实体的第一个属性</param>
        public bool Delete<TEntity>(params dynamic[] whereIn)
        {
            try
            {
                using (SQLiteHelper db = new SQLiteHelper(_connstr))
                {
                    Type type = typeof(TEntity);
                    string key = type.FullName;
                    bool isSuccess = false;
                    if (whereIn != null && whereIn.Length > 0)
                    {
                        string idList = Common.ToJoinSqlInVal( whereIn );
                        if (string.IsNullOrEmpty(idList))
                        {
                            return false;
                        }
                        string sql = string.Format("delete from {0} where {1} in ({2})", type.Name, GetPrimaryKey(type), idList );
                        int deleteRowCount = db.ExecuteNonQuery(sql);
                        message = string.Format("{0}行受影响", deleteRowCount);
                        isSuccess = deleteRowCount > 0;
                    }
                    return isSuccess;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 批量删除
        /// 用法:
        /// Delete<T>("pid",new int[]{1,2,3})
        /// 或者
        /// Delete<T>(3)
        /// </summary>
        /// <param name="oc"></param>
        /// <param name="whereIn">in的集合</param>
        /// <param name="whereIn">主键为实体的第一个属性</param>
        public bool Delete<TEntity>(string inFiled, params dynamic[] whereIn)
        {
            try
            {
                using (SQLiteHelper db = new SQLiteHelper(_connstr))
                {

                    Type type = typeof(TEntity);
                    string key = type.FullName;
                    bool isSuccess = false;
                    if (whereIn != null && whereIn.Length > 0)
                    {
                        string sql = string.Format("delete from {0} where {1} in ({2})", type.Name, inFiled, Common.ToJoinSqlInVal(whereIn));
                        int deleteRowCount = db.ExecuteNonQuery(sql);
                        message = string.Format("{0}行受影响", deleteRowCount);
                        isSuccess = deleteRowCount > 0;
                    }
                    return isSuccess;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        /// <summary>
        /// 该方法执行传入的增删改SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>返回影响行数</returns>
        public int ExecuteNonQuery(string sql, params SQLiteParameter[] pars)
        {
            using (SQLiteHelper db = new SQLiteHelper(_connstr))
            {
                return db.ExecuteNonQuery(sql, pars);
            }
        }

        /// <summary>
        /// 反回datatable
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string sql, params SQLiteParameter[] pars)
        {
            using (SQLiteHelper db = new SQLiteHelper(_connstr))
            {
                return db.ExecuteQuery(sql, pars);
            }
        }

        /// <summary>
        /// 反回第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string GetString(string sql, params SQLiteParameter[] pars)
        {
            using (SQLiteHelper db = new SQLiteHelper(_connstr))
            {
                return db.GetString(sql, pars);
            }
        }
        /// <summary>
        /// 反回第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int GetInt(string sql, params SQLiteParameter[] pars)
        {
            using (SQLiteHelper db = new SQLiteHelper(_connstr))
            {
                return db.GetInt(sql, pars);
            }
        }


        /// <summary>
        /// 获取主键
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetPrimaryKey(Type type)
        {
            return type.GetProperties()[0].Name;
        }
    }
}
