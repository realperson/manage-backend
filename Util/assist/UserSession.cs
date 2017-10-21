using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.caijunxiong.util.assist
{
    /// <summary>
    /// 用户session
    /// </summary>
    [Serializable]
    public class UserSession
    {
        /// <summary>
        /// ID
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 登录类型
        /// </summary>
        public string login_type { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 网站ID
        /// </summary>
        public int site_id { get; set; }

        /// <summary>
        /// 状态(0:正常 1:禁用 2:停用 3:锁定)
        /// </summary>
        public int state { get; set; }

        /// <summary>
        /// 令牌
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// 有效时间
        /// </summary>
        public System.DateTime time { get; set; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string refresh_token { get; set; }

        /// <summary>
        /// 菜单ID列表
        /// </summary>
        public List<int> menuIds { get; set; }

        /// <summary>
        /// 菜单编码列表
        /// </summary>
        public List<string> menuCodes { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        public List<int> privilegeIds { get; set; }

        /// <summary>
        /// 权限编码列表
        /// </summary>
        public List<string> privilegeCodes { get; set; }


        public UserSession()
        {
            
        }

    }
}
