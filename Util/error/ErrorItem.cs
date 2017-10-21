using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.caijunxiong.util.assist;

namespace com.caijunxiong.util.error
{
    public class ErrorItem
    {
        /// <summary>
        /// 错误键名
        /// </summary>
        public string key = Constant.generalErrorKey;

        /// <summary>
        /// 错误编码
        /// </summary>
        public string code = ErrorCode.general;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string message = "";
    }
}
