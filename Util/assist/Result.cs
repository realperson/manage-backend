using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.caijunxiong.util.assist
{
    public class Result
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool success = true;

      
        /// <summary>
        /// 结果提示
        /// </summary>
        public string msg = "";

        /// <summary>
        /// 是否跳转到指定的url
        /// </summary>
//        public string url = null;

        /// <summary>
        /// 数据集
        /// </summary>
        public Dictionary<string, string> data = null;

        /// <summary>
        /// 错误结果
        /// </summary>
        public ArrayList errors = new ArrayList();

        public Result()
        {
            
        }

    }
}
