using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.caijunxiong.util.assist;

namespace com.caijunxiong.api.helper.pemission
{
    /// <summary>
    /// 权限操作类,用于判定用户是否有相应的权限
    /// </summary>
    public class PermissionHelper
    {
        /// <summary>
        /// 根据菜单编码判断当前用户是否拥有该菜单权限
        /// </summary>
        /// <param name="code">菜单编码</param>
        /// <returns></returns>
        public static bool HasMenuPermission(string code)
        {
            GlobalConfig globalConfig = HttpContext.Current.Application[Constant.globalConfigKey] as GlobalConfig;
            if (globalConfig != null)
            {
                UserSession us = HttpContext.Current.Session[globalConfig.userSessionKey] as UserSession;
                if (us != null && us.menuCodes.Contains(code))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据菜单ID判断当前用户是否拥有该菜单权限
        /// </summary>
        /// <param name="id">菜单id</param>
        /// <returns></returns>
        public static bool HasMenuPermission(int id)
        {
            GlobalConfig globalConfig = HttpContext.Current.Application[Constant.globalConfigKey] as GlobalConfig;
            if (globalConfig != null)
            {
                UserSession us = HttpContext.Current.Session[globalConfig.userSessionKey] as UserSession;
                if (us != null && us.menuIds.Contains(id))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据权限编码判断当前用户是否拥有该权限
        /// </summary>
        /// <param name="code">权限编码</param>
        /// <returns></returns>
        public static bool HasSystemPermission(string code)
        {
            GlobalConfig globalConfig = HttpContext.Current.Application[Constant.globalConfigKey] as GlobalConfig;
            if (globalConfig != null)
            {
                UserSession us = HttpContext.Current.Session[globalConfig.userSessionKey] as UserSession;
                if (us != null && us.privilegeCodes.Contains(code))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据权限ID判断当前用户是否拥有该权限
        /// </summary>
        /// <param name="id">权限ID</param>
        /// <returns></returns>
        public static bool HasSystemPermission(int id)
        {
            GlobalConfig globalConfig = HttpContext.Current.Application[Constant.globalConfigKey] as GlobalConfig;
            if (globalConfig != null)
            {
                UserSession us = HttpContext.Current.Session[globalConfig.userSessionKey] as UserSession;
                if (us != null && us.privilegeIds.Contains(id))
                {
                    return true;
                }
            }
            return false;
        }
    }
}