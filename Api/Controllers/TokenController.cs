using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using com.caijunxiong.api.helper.authorize;
using com.caijunxiong.dal;
using com.caijunxiong.util;
using com.caijunxiong.util.assist;
using com.caijunxiong.util.error;
using Newtonsoft.Json.Linq;

namespace com.caijunxiong.api.Controllers
{
    public class TokenController : ApiController
    {
        private ManageEntities db = new ManageEntities();
        public HttpRequest httpRequest = HttpContext.Current.Request;


        // POST: api/Token
        [HttpPost]
        public IHttpActionResult Post(dynamic json)
        {
            Result result = new Result();
            ErrorItem errorItem;

            //判断参数是否存在
            int pSiteId = 0;
            string pGrantType = "password";
            string pName = "";
            string pPassword = "";
            string pRefreshToken = "";
            if (json.siteId != null)
            {
                pSiteId = int.Parse(json.siteId);
            }
            if (json.grant_type != null)
            {
                pGrantType = json.grant_type.ToString();
                if (pGrantType != "password" && pGrantType != "refresh_token")
                {
                    pGrantType = "password";
                }
            }
            if (pGrantType == "password")
            {
                if (json.name == null || json.password == null)
                {
                    //参数错误
                    result.success = false;
                    result.msg = "登录失败";

                    errorItem = new ErrorItem();
                    errorItem.code = ErrorCode.param;
                    errorItem.message = "参数错误";
                    result.errors.Add(errorItem);
                }
                else
                {
                    pName = json.name.ToString();
                    pPassword = json.password.ToString();
                    result = Login(pName, pPassword, pSiteId);
                }
            }else if (pGrantType == "refresh_token")
            {
                if (json.refresh_token == null)
                {
                    //参数错误
                    result.success = false;
                    result.msg = "登录失败";

                    errorItem = new ErrorItem();
                    errorItem.code = ErrorCode.param;
                    errorItem.message = "参数错误";
                    result.errors.Add(errorItem);

                }
                else
                {
                    pRefreshToken = json.refresh_token.ToString();
                    result = refreshToken(pRefreshToken);
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// 刷新token
        /// </summary>
        /// <param name="pToken">用于刷新的令牌</param>
        /// <returns></returns>
        public Result refreshToken(string pToken)
        {
            Result result = new Result();
            ErrorItem errorItem;

            //查找token
            Token token = db.Tokens.FirstOrDefault((r) => r.refresh_token == pToken);
            if (token == null)
            {
                //没有该用户
                result.success = false;
                result.msg = "刷新token失败";

                errorItem = new ErrorItem();
                errorItem.key = "refresh_token";
                errorItem.message = "没有该token";
                result.errors.Add(errorItem);
            }
            else
            {
                string lastToken = token.token;//缓存中要被删除的token值
                GlobalConfig globalConfig = HttpContext.Current.Application[Constant.globalConfigKey] as GlobalConfig;
                DateTime invalidTime = token.time.AddHours(globalConfig.tokenValidRefreshTime);

                RedisHelper redis = new RedisHelper(globalConfig.cacheDefaultDb);
                UserSession us = redis.HashGet<UserSession>(globalConfig.cacheSessionKey, lastToken);
                if (invalidTime >= DateTime.Now)
                {
                    if (us == null)
                    {
                        //缓存中没有该token的记录
                        us = TokenVerify.generateUserSessionByUserId(token.user_id);//根据用户ID生成用户session数据
                    }


                    //该token有效,刷新token并保存
                    DateTime now = DateTime.Now;
                    token.time = now.AddHours(globalConfig.tokenValidTime);//有效时间
                    string tokenKey = us.name + ":" + us.site_id + ":token:" + token.time.Ticks;
                    string tokenRefreshKey = us.name + ":" + us.site_id + ":refresh_token:" + token.time.Ticks;
                    token.token = Encryption.Encrypt(tokenKey);//令牌
                    token.refresh_token = Encryption.Encrypt(tokenRefreshKey);//刷新令牌

                    db.SaveChanges();
                    //更新缓存
                    us.id = token.id;
                    us.user_id = token.user_id;
                    us.token = token.token;
                    us.refresh_token = token.refresh_token;
                    us.time = token.time;
                    us.login_type = token.login_type;
                    us.state = token.state;
                    redis.HashDelete(globalConfig.cacheSessionKey, lastToken);
                    redis.HashSet<UserSession>(globalConfig.cacheSessionKey, token.token, us);

                    result.msg = "刷新token成功";
                    if (result.success)
                    {
                        //返回token和权限
                        result.data = new Dictionary<string, string>();
                        result.data.Add("access_token", token.token);
                        result.data.Add("refresh_token", token.refresh_token);
                        TimeSpan ts = token.time - now;
                        result.data.Add("expires_in", ts.TotalSeconds.ToString());
                        result.data.Add("menus", string.Join(",", us.menuIds.ToArray()));
                        result.data.Add("permissions", string.Join(",", us.privilegeCodes.ToArray()));
                    }
                }
                else
                {
                    //token已失效
                    result.success = false;
                    result.msg = "刷新token失败";

                    errorItem = new ErrorItem();
                    errorItem.key = "refresh_token";
                    errorItem.message = "refresh_token已失效";
                    result.errors.Add(errorItem);

                    //该token无效
                    if (us != null)
                    {
                        //清除缓存中的数据
                        redis.HashDelete(globalConfig.cacheSessionKey, lastToken);
                    }
                }
            }

            return result;
        }

        

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="pUserName">用户名</param>
        /// <param name="pPassword">密码</param>
        /// <param name="siteId">网站ID</param>
        /// <returns></returns>
        public Result Login(string pUserName,string pPassword, int siteId = 0)
        {
            Result result = new Result();
            ErrorItem errorItem;

            pUserName = pUserName.Trim();
            pPassword = Encryption.EncryptPassword(pPassword);
            User userData = db.Users.FirstOrDefault((r) => r.name == pUserName && r.password == pPassword && r.site_id == siteId);
            if (userData == null)
            {
                //没有该用户
                result.success = false;
                result.msg = "登录失败";

                errorItem = new ErrorItem();
                errorItem.message = "用户名或密码错误";
                result.errors.Add(errorItem);
            }
            else
            {
                GlobalConfig globalConfig = HttpContext.Current.Application[Constant.globalConfigKey] as GlobalConfig;
                DateTime now = DateTime.Now;
                //有该用户,生成token
                Token token = new Token();
                token.user_id = userData.id;//用户ID
                token.login_type = globalConfig.defaultLoginType;//登录类型
                token.time = now.AddHours(globalConfig.tokenValidTime);//有效时间
                string tokenKey = userData.name + ":" + userData.site_id + ":token:" + token.time.Ticks;
                string tokenRefreshKey = userData.name + ":" + userData.site_id + ":refresh_token:" + token.time.Ticks;
                token.token = Encryption.Encrypt(tokenKey);//令牌
                token.refresh_token = Encryption.Encrypt(tokenRefreshKey);//刷新令牌
                token.state = 0;//默认状态为0(正常)


                //获取该用户的用户角色
                var roleQuery = from ur in db.UserRoles
                                from r in db.Roles
                                where token.user_id == ur.user_id && ur.role_id == r.id
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

                //用户Session,保存在Redis数据库中
                UserSession us = new UserSession();

                us.site_id = userData.site_id;
                us.login_type = token.login_type;
                us.name = userData.name;
                us.state = token.state;
                us.token = token.token;
                us.refresh_token = token.refresh_token;
                us.time = token.time;

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


                //判断数据库是否已存在该登录项
                List<Token> tokenList = db.Tokens.Where((r) => r.user_id == userData.id).OrderBy((r) => r.time).ToList();
                var query = from t in tokenList
                            where t.login_type == token.login_type
                            select t;
                if (query.Count() >= globalConfig.maxUser[token.login_type])
                {
                    //该类型登录已经超出了用户人数限制

                    //已有该登录项,将最初登录的用户踢出
                    Token firstToken = tokenList[0];
                    string lastToken = firstToken.token;//要被删除的token值
                    DbEntityEntry<Token> entry = db.Entry<Token>(firstToken);
                    firstToken.time = token.time;
                    firstToken.token = token.token;
                    firstToken.refresh_token = token.refresh_token;
                    entry.State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        //更新redis缓存
                        us.id = firstToken.id;
                        RedisHelper redis = new RedisHelper(globalConfig.cacheDefaultDb);
                        redis.HashDelete(globalConfig.cacheSessionKey, lastToken);
                        redis.HashSet<UserSession>(globalConfig.cacheSessionKey, token.token, us);


                        result.msg = "登录成功";
                    }
                    catch (Exception e)
                    {
                        result.success = false;
                        result.msg = "登录失败";

                        errorItem = new ErrorItem();
                        errorItem.message = e.Message;
                        result.errors.Add(errorItem);
                    }
                }
                else
                {
                    //该类型登录未超出用户人数限制
                    //添加登录信息
                    try
                    {
                        db.Tokens.Add(token);
                        db.SaveChanges();
                        //更新redis缓存
                        us.id = token.id;
                        RedisHelper redis = new RedisHelper(globalConfig.cacheDefaultDb);
                        redis.HashSet<UserSession>(globalConfig.cacheSessionKey, token.token, us);
                        result.msg = "登录成功";
                    }
                    catch (Exception e)
                    {
                        result.success = false;
                        result.msg = "登录失败";

                        errorItem = new ErrorItem();
                        errorItem.message = e.Message;
                        result.errors.Add(errorItem);
                    }
                }


                if (result.success)
                {
                    //返回token和权限
                    result.data = new Dictionary<string, string>();
                    result.data.Add("token_type", globalConfig.defaultTokenType);
                    result.data.Add("access_token", token.token);
                    result.data.Add("refresh_token", token.refresh_token);
                    //result.data.Add("time", token.time.ToString("yyyy-MM-dd HH:mm:ss"));
                    TimeSpan ts = token.time - now;
                    result.data.Add("expires_in", ts.TotalSeconds.ToString());
                    result.data.Add("menus", string.Join(",", us.menuIds.ToArray()));
                    result.data.Add("permissions", string.Join(",", us.privilegeCodes.ToArray()));

                }
            }
            return result;
        }

    }
}
