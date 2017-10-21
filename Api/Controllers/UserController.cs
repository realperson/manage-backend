using com.caijunxiong.dal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.WebPages;
using com.caijunxiong.util;
using com.caijunxiong.util.assist;
using com.caijunxiong.util.error;

namespace com.caijunxiong.api.Controllers
{
    public class UserController : ApiController
    {

        private ManageEntities db = new ManageEntities();
        public HttpRequest httpRequest = HttpContext.Current.Request;

        // GET: api/User
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }


        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <param name="siteId">网站ID</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult Login(User user, int siteId = 0)
        {
            Result result = new Result();
            ErrorItem errorItem;

            user.name= user.name.Trim();
            user.password = Encryption.EncryptPassword(user.password);
            User userData = db.Users.FirstOrDefault((r) => r.name == user.name && r.password == user.password && r.site_id == siteId);
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
                DateTime now=DateTime.Now;
                //有该用户,生成token
                Token token=new Token();
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
                UserSession us=new UserSession();

                us.site_id = userData.site_id;
                us.login_type = token.login_type;
                us.name = userData.name;
                us.state = token.state;
                us.token = token.token;
                us.refresh_token = token.refresh_token;
                us.time = token.time;

                us.menuIds=new List<int>();
                us.menuCodes = new List<string>();
                us.privilegeIds=new List<int>();
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
                    List<string> privilegeIds=new List<string>();
                    foreach (SystemPrivilege sp in systemPrivilegeQuery)
                    {
                        string[] ids=sp.privilege_ids.Split(',');
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
                List<Token> tokenList = db.Tokens.Where((r) => r.user_id == userData.id).ToList();
                var query = from t in tokenList
                            where t.login_type== token.login_type
                            orderby t.time ascending
                            select t;
                if (query.Count() >= globalConfig.maxUser[token.login_type])
                {
                    //该类型登录已经超出了用户人数限制

                    //已有该登录项,将最初登录的用户踢出
                    Token firstToken = tokenList[0];
                    string lastToken = firstToken.token;//要被删除的token值
                    DbEntityEntry<Token> entry = db.Entry<Token>(firstToken);
                    firstToken.time= token.time;
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
                        redis.HashSet<UserSession>(globalConfig.cacheSessionKey, token.token,us);
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
                    result.data=new Dictionary<string, string>();
                    result.data.Add("access_token", token.token);
                    result.data.Add("refresh_token", token.refresh_token);
                    //result.data.Add("time", token.time.ToString("yyyy-MM-dd HH:mm:ss"));
                    TimeSpan ts = token.time-now;
                    result.data.Add("expires_in", ts.TotalSeconds.ToString());
                    result.data.Add("menus", string.Join(",", us.menuIds.ToArray()));
                    result.data.Add("permissions", string.Join(",", us.privilegeCodes.ToArray()));
                    
                }
            }
            return Ok(result);
        }

        //---------------------------------------增删查改(START)

        /// <summary>
        /// 用户名是否存在
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="siteId">网站ID</param>
        /// <returns></returns>
        public bool isExist(string name, int siteId = 0)
        {
            return db.Users.FirstOrDefault((r) => r.name == name && r.site_id == siteId) != null;
        }

        /// <summary>
        /// 如果添加参数存在
        /// </summary>
        /// <returns></returns>
        private bool isAddParamsExist()
        {
            bool result = true;
            //            string[] param = { "role_id", "name", "password", "nickname" };
            string[] param = { "name", "password", "nickname" };
            for (int i = 0; i < param.Length; i++)
            {
                if (httpRequest[param[i]] == null)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }


        /// <summary>
        /// 如果编辑参数存在
        /// </summary>
        /// <returns></returns>
        private bool isEditParamsExist()
        {
            bool result = true;
            string[] param = { "id", "role_id", "name", "role_id", "password", "real_name" };
            for (int i = 0; i < param.Length; i++)
            {
                if (httpRequest[param[i]] == null)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        [HttpPost]
        public IHttpActionResult Add()
        {
            Result result = new Result();
            ErrorItem errorItem;
            if (isAddParamsExist())
            {
                //                UserRole ur = new UserRole();
                User record = new User();
                record.name = httpRequest["name"].ToString().Trim();
                record.password = httpRequest["password"].ToString().Trim();
                record.nickname = httpRequest["nickname"].ToString().Trim();
                record.site_id = 0;

                //用户名
                if (record.name.IsEmpty())
                {
                    result.success = false;

                    errorItem = new ErrorItem();
                    errorItem.key = "name";
                    errorItem.code = ErrorCode.required;
                    errorItem.message = "用户名不能为空";
                    result.errors.Add(errorItem);
                }
                else if (record.name.Length > 50)
                {
                    result.success = false;

                    errorItem = new ErrorItem();
                    errorItem.key = "name";
                    errorItem.code = ErrorCode.maxLength;
                    errorItem.message = "用户名长度不能大于50位";
                    result.errors.Add(errorItem);
                }
                else if (isExist(record.name, record.site_id))
                {
                    result.success = false;

                    errorItem = new ErrorItem();
                    errorItem.key = "name";
                    errorItem.code = ErrorCode.exist;
                    errorItem.message = "用户名已存在";
                    result.errors.Add(errorItem);
                }

                //密码
                if (record.password.IsEmpty())
                {
                    result.success = false;

                    errorItem = new ErrorItem();
                    errorItem.key = "password";
                    errorItem.code = ErrorCode.required;
                    errorItem.message = "密码不能为空";
                    result.errors.Add(errorItem);
                }
                else if (record.password.Length > 20)
                {
                    result.success = false;

                    errorItem = new ErrorItem();
                    errorItem.key = "password";
                    errorItem.code = ErrorCode.maxLength;
                    errorItem.message = "密码长度不能大于20位";
                    result.errors.Add(errorItem);
                }
                else if (record.password.Length < 6)
                {
                    result.success = false;

                    errorItem = new ErrorItem();
                    errorItem.key = "password";
                    errorItem.code = ErrorCode.minLength;
                    errorItem.message = "密码长度不能小于6位";
                    result.errors.Add(errorItem);
                }
                else
                {
                    record.password = Encryption.EncryptPassword(record.password);
                }

                //昵称
                if (record.nickname.IsEmpty())
                {
                    result.success = false;

                    errorItem = new ErrorItem();
                    errorItem.key = "nickname";
                    errorItem.code = ErrorCode.required;
                    errorItem.message = "昵称不能为空";
                    result.errors.Add(errorItem);
                }
                else if (record.nickname.Length > 50)
                {
                    result.success = false;

                    errorItem = new ErrorItem();
                    errorItem.key = "nickname";
                    errorItem.code = ErrorCode.maxLength;
                    errorItem.message = "昵称长度不能大于50位";
                    result.errors.Add(errorItem);
                }


                if (result.success)
                {
                    record.create_at = DateTime.Now;
                    record.deleted = false;
                    record.create_by = 0;
                    try
                    {
                        db.Users.Add(record);
                        db.SaveChanges();
                        result.msg = "添加用户成功";
                    }
                    catch (Exception e)
                    {
                        result.success = false;
                        result.msg = "添加用户失败";

                        errorItem = new ErrorItem();
                        errorItem.message = e.Message;
                        result.errors.Add(errorItem);
                    }
                }
                else
                {
                    result.msg = "添加用户失败";
                }

                //                ur.role_id = int.Parse(httpRequest["role_id"].ToString().Trim());

                //                if (Exist(record.name))
                //                {
                //                    result.success = false;
                //                    result.msg = "用户名已存在";
                //                }
                //                else
                //                {
                //                try
                //                                    {
                //                                        db.Users.Add(record);
                //                                        db.SaveChanges();
                //                
                //                
                //                                        //添加用户-角色映射
                ////                                        ur.user_id = record.id;
                ////                                        db.UserRoles.Add(ur);
                ////                                        db.SaveChanges();
                //                
                //                                        result.msg = "添加成功";
                //                                    }
                //                                    catch (Exception e)
                //                                    {
                //                                        result.success = false;
                //                                        result.msg = "添加失败:" + e.Message;
                //                                    }
                //                }
            }
            else
            {
                result.success = false;
                result.msg = "添加用户失败";

                errorItem = new ErrorItem();
                errorItem.code = ErrorCode.param;
                errorItem.message = "参数不正确";
                result.errors.Add(errorItem);
            }

            return Ok(result); 
        }

        // GET: api/User/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/User
        //        public void Post([FromBody]string value)
        //        {
        //        }

        // PUT: api/User/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/User/5
        public void Delete(int id)
        {
        }

        //---------------------------------------增删查改(END)

    }
}
