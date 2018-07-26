using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Easyliter
{
    public static class Extensions
    {
        public static El_Queryable<T> Where<T>(this El_Queryable<T> queryable, Expression<Func<T, bool>> expression)
        {
            string whereStr = string.Empty;

            if (expression.Body is BinaryExpression)
            {
                BinaryExpression be = ((BinaryExpression)expression.Body);
                whereStr = " and " + Common.BinarExpressionProvider(be.Left, be.Right, be.NodeType);
            }
            queryable.QueryItemList.Last().AppendValues.Add(whereStr);
            return queryable;
        }

        public static El_Queryable<T> JoinWhere<T>(this El_Queryable<T> queryable, string joinWhere)
        {
            string whereStr = string.Empty;
            whereStr = "where 1=1 and " + joinWhere;
            queryable.QueryItemList.Last().AppendValues.Add(whereStr);
            return queryable;
        }

        public static El_Queryable<T> Skip<T>(this El_Queryable<T> queryable, int skipNum)
        {
            var item = queryable.QueryItemList.Last();
            if (item.Skip == null)
            {
                item.Skip = skipNum;
            }
            else
            {
                throw new Exception("不能对同一张表多次skip");
            }
            return queryable;
        }
        public static El_Queryable<T> Take<T>(this El_Queryable<T> queryable, int takeNum)
        {
            var item = queryable.QueryItemList.Last();
            if (item.Take == null)
            {
                item.Take = takeNum;
            }
            else
            {
                throw new Exception("不能对同一张表多次takeNum");
            }
            return queryable;
        }
        public static El_Queryable<T> Select<T>(this El_Queryable<T> queryable, string selectRow)
        {
            if (selectRow != null)
            {
                var item = queryable.QueryItemList.Last();
                item.SelectRow = selectRow;
            }
            return queryable;
        }
        public static El_Queryable<T> OrderBy<T>(this El_Queryable<T> queryable, El_Sort sortType, string sortField)
        {
            var isAsc = sortType == El_Sort.asc;
            var item = queryable.QueryItemList.Last();
            var isFirst = item.OrderBy == null;
            if (isFirst)
            {
                item.OrderBy = " order by ";
            }
            else
            {
                item.OrderBy += ",";
            }
            item.OrderBy += isAsc ? string.Format(" {0} asc", sortField) : string.Format(" {0} desc", sortField);
            item.OrderBy = item.OrderBy;
            return queryable;
        }

        public static El_Queryable<T> Join<T, JoinT>(this El_Queryable<T> queryable, bool isLeft = false) where JoinT : new()
        {
            var items = queryable.QueryItemList;
            items.Add(new EL_QueryItem() { TableName = new JoinT().GetType().Name, Join = isLeft ? " left join " : " inner join ", AppendValues = new List<string>() });
            return queryable;
        }

        public static El_Queryable<T> On<T>(this El_Queryable<T> queryable, string on)
        {
            var item = queryable.QueryItemList.Last();
            item.On = on;
            return queryable;
        }


        public static string ToSql<T>(this El_Queryable<T> entity) where T : new()
        {
            StringBuilder sql = new StringBuilder();
            if (entity == null || entity.QueryItemList.Count == 0)
            {
                return string.Format("select * from {0}", new T().GetType().Name);
            }
            else
            {
                foreach (var r in entity.QueryItemList)
                {
                    if (r == entity.QueryItemList.First())
                    {
                        if (entity.QueryItemList.Count == 1)
                        {
                            sql.AppendFormat(" select {1} from {0} where 1=1 ", r.TableName, r.SelectRow == null ? "*" : r.SelectRow);
                        }
                        else
                        {
                            var selectRow = entity.QueryItemList.Last().SelectRow;
                            sql.AppendFormat(" select {1} from {0} ", r.TableName, selectRow == null ? "*" : selectRow);
                        }

                    }
                    else
                    {
                        sql.AppendFormat(" {0} {1} on {2}", r.Join, r.TableName, r.On);
                    }
                    foreach (var appendValue in r.AppendValues)
                    {
                        sql.Append(appendValue);
                    }
                    sql.Append(r.OrderBy);
                    if (r.Skip == null && r.Take == null)
                    {

                    }
                    else if (r.Skip != null && r.Take != null)
                    {
                        sql.AppendFormat(" limit {0},{1} ", r.Skip, r.Take);
                    }
                    else if (r.Take != null)
                    {
                        sql.AppendFormat(" limit 0,{0} ", r.Take);
                    }
                    else
                    {
                        sql.AppendFormat("limit {0},{1} ", r.Skip, int.MaxValue);
                    }
                }
                return sql.ToString();
            }
        }

        public static List<T> ToList<T>(this El_Queryable<T> entity) where T : new()
        {
            string sql = entity.ToSql();
            return entity.e.Select<T>(sql);

        }

        public static int Count<T>(this El_Queryable<T> entity) where T : new()
        {
            string sql = entity.ToSql();
            sql = Regex.Replace(sql, "select(.*?)from", "select count(*) from ");
            var count=entity.e.GetInt(sql);
            return count;

        }
        public static DataTable ToDataTable<T>(this El_Queryable<T> entity) where T : new()
        {
            string sql = entity.ToSql();
            return entity.e.GetDataTable(sql);

        }

        public static List<NewT> ToNewList<T, NewT>(this El_Queryable<T> entity)
            where NewT : new()
            where T : new()
        {
            string sql = entity.ToSql();
            return entity.e.Select<NewT>(sql);

        }

        public static T Single<T>(this El_Queryable<T> entity) where T : new()
        {
            string sql = entity.ToSql();
            return entity.e.Select<T>(sql).Single();

        }
        public static T First<T>(this El_Queryable<T> entity) where T : new()
        {
            string sql = entity.ToSql();
            sql = Regex.Replace(sql, "limit.*", "limit 0,1");
            return entity.e.Select<T>(sql).Single();

        }
    }
}
