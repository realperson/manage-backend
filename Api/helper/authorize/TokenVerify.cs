using com.caijunxiong.dal;
using com.caijunxiong.util.assist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.caijunxiong.api.helper.authorize
{
    /// <summary>
    /// token验证
    /// </summary>
    public class TokenVerify
    {

        private static ManageEntities db = new ManageEntities();

        /// <summary>
        /// 判断token是否有效
        /// </summary>
        /// <param name="pToken">token</param>
        /// <returns></returns>
        public static bool verifyToken(string pToken)
        {
            bool isValid = false;
            //查找token
            GlobalConfig globalConfig = HttpContext.Current.Application[Constant.globalConfigKey] as GlobalConfig;
            RedisHelper redis = new RedisHelper(globalConfig.cacheDefaultDb);
            UserSession us = redis.HashGet<UserSession>(globalConfig.cacheSessionKey, pToken);
            Token token;
            if (us == null)
            {
                //缓存中没有该token的记录
                //查询数据库
                token = db.Tokens.FirstOrDefault((r) => r.token == pToken);
                if (token != null)
                {
                    //判断是否有效
                    if (token.time >= DateTime.Now)
                    {
                        isValid = true;

                        us = TokenVerify.generateUserSessionByUserId(token.user_id); //根据用户ID生成用户session数据
                        HttpContext.Current.Session[globalConfig.userSessionKey] = us;
                        //更新缓存
                        us.id = token.id;
                        us.user_id = token.user_id;
                        us.token = token.token;
                        us.refresh_token = token.refresh_token;
                        us.time = token.time;
                        us.login_type = token.login_type;
                        us.state = token.state;
                        redis.HashSet<UserSession>(globalConfig.cacheSessionKey, token.token, us);
                    }
                }
            }
            else
            {
                //判断是否有效
                if (us.time >= DateTime.Now)
                {
                    isValid = true;
                    HttpContext.Current.Session[globalConfig.userSessionKey] = us;
                }
            }
            return isValid;
        }


        /// <summary>
        /// 根据用户ID生成用户session数据
        /// </summary>
        /// <param name="pUserId">用户ID</param>
        /// <returns></returns>
        public static UserSession generateUserSessionByUserId(int pUserId)
        {
            GlobalConfig globalConfig = HttpContext.Current.Application[Constant.globalConfigKey] as GlobalConfig;
            User userData = db.Users.FirstOrDefault((r) => r.id == pUserId);

            //获取该用户的用户角色
            var roleQuery = from ur in db.UserRoles
                            from r in db.Roles
                            where pUserId == ur.user_id && ur.role_id == r.id
                            orderby r.priority ascending
                            select r;
            List<Role> roleList = roleQuery.ToList();
            //判断是否超级角色
            bool isSuper = false;
            for (int i = 0; i < roleList.Count; i++)
            {
                if (roleList[i].code == globalConfig.superRole)
                {
                    isSuper = true;
                    break;
                }
            }

            //用户Session
            UserSession us = new UserSession();

            us.site_id = userData.site_id;
            us.name = userData.name;

            us.menuIds = new List<int>();
            us.menuCodes = new List<string>();
            us.privilegeIds = new List<int>();
            us.privilegeCodes = new List<string>();

            //获取该用户对应的权限
            if (isSuper)
            {
                //超级角色拥有所有权限

                //菜单权限
                List<Menu> menus = db.Menus.ToList();
                foreach (Menu m in menus)
                {
                    us.menuIds.Add(m.id);
                    us.menuCodes.Add(m.code);
                }

                //系统权限
                List<Privilege> privileges = db.Privileges.ToList();
                foreach (Privilege p in privileges)
                {
                    us.privilegeIds.Add(p.id);
                    us.privilegeCodes.Add(p.code);
                }
            }
            else
            {
                //获取该用户对应的所有角色拥有的菜单权限
                var menuPrivilegeQuery = from mp in db.MenuPrivileges
                                         from r in roleList
                                         where mp.owner_id == r.id && mp.type == "role"
                                         select mp;
                List<string> menuIds = new List<string>();
                foreach (MenuPrivilege mp in menuPrivilegeQuery)
                {
                    string[] ids = mp.menu_ids.Split(',');
                    menuIds.AddRange(ids);
                }
                //根据权限菜单ID获取菜单列表
                var menuQuery = from m in db.Menus
                                from id in menuIds
                                where m.id == int.Parse(id)
                                select m;
                foreach (Menu m in menuQuery)
                {
                    us.menuIds.Add(m.id);
                    us.menuCodes.Add(m.code);
                }

                //获取该用户对应的所有角色拥有的权限
                var systemPrivilegeQuery = from sp in db.SystemPrivileges
                                           from r in roleList
                                           where sp.owner_id == r.id && sp.type == "role"
                                           select sp;
                List<string> privilegeIds = new List<string>();
                foreach (SystemPrivilege sp in systemPrivilegeQuery)
                {
                    string[] ids = sp.privilege_ids.Split(',');
                    privilegeIds.AddRange(ids);
                }
                //根据权限ID获取权限列表
                var privilegeQuery = from p in db.Privileges
                                     from pid in privilegeIds
                                     where p.id == int.Parse(pid)
                                     select p;
                foreach (Privilege p in privilegeQuery)
                {
                    us.privilegeIds.Add(p.id);
                    us.privilegeCodes.Add(p.code);
                }
            }


            return us;
        }
    }
}