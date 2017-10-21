using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.caijunxiong.util.error
{
    public class ErrorCode
    {
        /// <summary>
        /// 最小长度过小
        /// </summary>
        public static string minLength = "minLength";

        /// <summary>
        /// 最大长度过长
        /// </summary>
        public static string maxLength = "maxLength";

        /// <summary>
        /// 必填,不能为空
        /// </summary>
        public static string required = "required";

        /// <summary>
        /// 参数错误
        /// </summary>
        public static string param = "param";

        /// <summary>
        /// 已存在
        /// </summary>
        public static string exist = "exist";

        /// <summary>
        /// 一般错误
        /// </summary>
        public static string general = "general";
    }
}
