using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.caijunxiong.util.assist
{
    public class GlobalConfig
    {

        /// <summary>
        /// 缓存保存在哪个数据库中
        /// </summary>
        public int cacheDefaultDb = 0;

        /// <summary>
        /// session缓存键名
        /// </summary>
        public string cacheSessionKey = "session";

        /// <summary>
        /// token有效时间(小时)
        /// </summary>
        public int tokenValidTime = 24;

        /// <summary>
        /// 刷新token有效时间,token过期后还可以保留多久(小时)
        /// </summary>
        public int tokenValidRefreshTime = 120;

        /// <summary>
        /// 每页数据大小
        /// </summary>
        public int pageSize = 20;

        /// <summary>
        /// 超级角色
        /// </summary>
        public string superRole = "admin";


        /// <summary>
        /// 默认登录类型
        /// </summary>
        public string defaultLoginType = "pc";

        /// <summary>
        /// 保存每种登录类型对应的最大人数
        /// </summary>
        public Dictionary<string, int> maxUser = new Dictionary<string, int>();

        
    }
}
