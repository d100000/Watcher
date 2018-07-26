using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Linq.Expressions;
namespace Easyliter
{
    /// <summary>
    /// ** 公共类
    /// ** 创始时间：2015-5-13
    /// ** 修改时间：-
    /// ** 作者：sunkaixuan
    /// ** qq：610262374 欢迎交流,共同提高 ,命名语法等写的不好的地方欢迎大家的给出宝贵建议
    /// </summary>
    internal class Common
    {

        /// <summary>
        /// 将dataTable转成List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> List<T>(DataTable dt)
        {
            if (dt == null) return new List<T>();
            var list = new List<T>();
            Type t = typeof(T);
            var plist = new List<PropertyInfo>(typeof(T).GetProperties());

            foreach (DataRow item in dt.Rows)
            {
                T s = System.Activator.CreateInstance<T>();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    //PropertyInfo info = plist.Find(p => String.Equals(p.Name, dt.Columns[i].ColumnName, StringComparison.OrdinalIgnoreCase));
                    PropertyInfo info = plist.Find(p => p.Name == dt.Columns[i].ColumnName);
                    if (info != null)
                    {
                        if (!Convert.IsDBNull(item[i]))
                        {
                            try
                            {
                                info.SetValue(s, item[i], null);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }

                        }
                    }
                }
                list.Add(s);
            }
            return list;
        }


        /// <summary>
        /// 将数组转为 '1','2' 这种格式的字符串 用于 where id in(  )
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string ToJoinSqlInVal(object[] array)
        {
            if (array == null || array.Length == 0)
            {
                return "''";
            }
            else
            {
                //if (array.GetType().Name != "Object[]")
                if (!(array[0] is Array))             
                {
                    return string.Join( ",", array.Where( c => c != null ).Select( c => "'" + (c + "").Replace( "'", "''" ) + "'" ) );//除止SQL注入
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    //foreach (var item in array)
                    //{
                        var arrId = (long[])array[0];
                    if (arrId.Count()>0)
                    {
                        foreach (var Id in arrId)
                        {
                            var temp = Id;//无奈的临时方案
                            if (temp != 99999999)
                            {
                                sb.AppendFormat( " '{0}',", temp );
                            }
                        }
                    }


                    //}
                    string res = sb.ToString();
                    if (!string.IsNullOrEmpty( res ))
                    {
                        return res.Substring( 0, res.Length - 1 );
                    }
                    else
                    {
                        return "";
                    }

                }
            }
        }

        public static SQLiteParameter[] GetObjectToSQLiteParameter(object obj)
        {
            List<SQLiteParameter> listParams = new List<SQLiteParameter>();
            var propertiesObj = obj.GetType().GetProperties();
            string replaceGuid = Guid.NewGuid().ToString();
            foreach (PropertyInfo r in propertiesObj)
            {
                listParams.Add(new SQLiteParameter("@" + r.Name, r.GetValue(obj, null).ToString()));
            }

            return listParams.ToArray();
        }

        public static Dictionary<string, string> GetObjectToDictionary(object obj)
        {
            Dictionary<string, string> reval = new Dictionary<string, string>();
            var propertiesObj = obj.GetType().GetProperties();
            string replaceGuid = Guid.NewGuid().ToString();
            foreach (PropertyInfo r in propertiesObj)
            {
                var val=r.GetValue(obj, null);
                reval.Add(r.Name, val == null ? "" : val.ToString());
            }

            return reval;
        }

        public static string BinarExpressionProvider(Expression left, Expression right, ExpressionType type)
        {
            string sb = "(";
            //先处理左边
            sb += ExpressionRouter(left, false);
            sb += ExpressionTypeCast(type);
            //再处理右边
            string tmpStr = ExpressionRouter(right, true);
            if (tmpStr == "null")
            {
                if (sb.EndsWith(" ="))
                    sb = sb.Substring(0, sb.Length - 2) + " is null";
                else if (sb.EndsWith("<>"))
                    sb = sb.Substring(0, sb.Length - 2) + " is not null";
            }
            else
                sb += "'"+tmpStr+"'";
            return sb += ")";
        }
        //表达式路由计算 
        static string ExpressionRouter(Expression exp, bool isRight)
        {
            string sb = string.Empty;
            if (exp is BinaryExpression)
            {
                BinaryExpression be = ((BinaryExpression)exp);
                return BinarExpressionProvider(be.Left, be.Right, be.NodeType);
            }
            else if (exp is MemberExpression)
            {
                if (!isRight)
                {
                    MemberExpression me = ((MemberExpression)exp);
                    return me.Member.Name;
                }
                else
                {
                    return Expression.Lambda(exp).Compile().DynamicInvoke() + "";
                }
            }
            else if (exp is NewArrayExpression)
            {
                NewArrayExpression ae = ((NewArrayExpression)exp);
                StringBuilder tmpstr = new StringBuilder();
                foreach (Expression ex in ae.Expressions)
                {
                    tmpstr.Append(ExpressionRouter(ex, false));
                    tmpstr.Append(",");
                }
                return tmpstr.ToString(0, tmpstr.Length - 1);
            }
            else if (exp is MethodCallExpression)
            {
                MethodCallExpression mce = (MethodCallExpression)exp;
                if (mce.Method.Name == "Like")
                    return string.Format("({0} like {1})", ExpressionRouter(mce.Arguments[0], false), ExpressionRouter(mce.Arguments[1], false));
                else if (mce.Method.Name == "NotLike")
                    return string.Format("({0} Not like {1})", ExpressionRouter(mce.Arguments[0], false), ExpressionRouter(mce.Arguments[1], false));
                else if (mce.Method.Name == "In")
                    return string.Format("{0} In ({1})", ExpressionRouter(mce.Arguments[0], false), ExpressionRouter(mce.Arguments[1], false));
                else if (mce.Method.Name == "NotIn")
                    return string.Format("{0} Not In ({1})", ExpressionRouter(mce.Arguments[0], false), ExpressionRouter(mce.Arguments[1], false));
            }
            else if (exp is ConstantExpression)
            {
                ConstantExpression ce = ((ConstantExpression)exp);
                if (ce.Value == null)
                    return "null";
                else if (ce.Value is ValueType)
                    return ce.Value.ToString();
                else if (ce.Value is string || ce.Value is DateTime || ce.Value is char)
                    return string.Format("{0}", ce.Value.ToString());
            }
            else if (exp is UnaryExpression)
            {
                UnaryExpression ue = ((UnaryExpression)exp);
                var mexp = (MemberExpression)ue.Operand;
                object value = (mexp.Expression as ConstantExpression).Value;
                string name = mexp.Member.Name;
                System.Reflection.FieldInfo info = value.GetType().GetField(name);
                object obj = info.GetValue(value);
                return obj + "";
            }
            return null;
        }
        static string ExpressionTypeCast(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Equal:
                    return " =";
                case ExpressionType.GreaterThan:
                    return " >";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " Or ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                default:
                    return null;
            }
        }
    }



}
