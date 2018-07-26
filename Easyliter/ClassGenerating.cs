using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easyliter
{
    /// <summary>
    /// ** 实生成生类
    /// ** 创始时间：2015-5-13
    /// ** 修改时间：-
    /// ** 作者：sunkaixuan
    /// ** qq：610262374 欢迎交流,共同提高 ,命名语法等写的不好的地方欢迎大家的给出宝贵建议
    /// </summary>
    public class ClassGenerating
    {
        /// <summary>
        /// 根据DataTable获取实体类的字符串
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string DataTableToClass(DataTable dt,string tableName,string nameSpace)
        {
            StringBuilder reval = new StringBuilder();
            StringBuilder propertiesValue = new StringBuilder();
            string replaceGuid = Guid.NewGuid().ToString();
            foreach (DataColumn r in dt.Columns)
            {
                propertiesValue.AppendFormat("           public {0} {1} {2}", r.DataType, r.ColumnName, "{get;set;}\r\n");

            }
            reval.AppendFormat(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace  {2}{{

 public class {0}{{
{1}
  }}
}} ", tableName, propertiesValue,nameSpace);


            return reval.ToString();
        }
    }
}
