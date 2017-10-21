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


        //---------------------------------------增删查改(START)

        /// <summary>
        /// 用户名是否存在
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="siteId">网站ID</param>
        /// <returns></returns>
        public bool isExist(string name, int siteId = 0)
        {
            return db.Users.FirstOrDefault((r) => r.name == name && r.site_id == siteId && !r.deleted) != null;
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
                record.name = httpRequest["name"].ToString().Trim().ToLower();
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
        [HttpPut]
        public void Put(dynamic json)
        {

        }

        // DELETE: api/User/5
        [HttpDelete]
        public IHttpActionResult Delete(dynamic json)
        {
            Result result = new Result();
            ErrorItem errorItem;
            int pId = 0;
            if (json.id != null)
            {
                pId = int.Parse(json.id.ToString());
                //删除相应的token和缓存
                var tokenQuery = from t in db.Tokens
                                 where t.user_id == pId
                                 select t;
                if (tokenQuery.Count() > 0)
                {
                    //删除缓存
                    GlobalConfig globalConfig = HttpContext.Current.Application[Constant.globalConfigKey] as GlobalConfig;
                    RedisHelper redis = new RedisHelper(globalConfig.cacheDefaultDb);
                    foreach (Token t in tokenQuery)
                    {
                        redis.HashDelete(globalConfig.cacheSessionKey, t.token);
                        //从token表中删除
                        db.Tokens.Attach(t);
                        db.Tokens.Remove(t);
                    }
                }
                //软删除
                User user = db.Users.FirstOrDefault((r) => r.id == pId);
                if (user != null)
                {
                    user.deleted = true;
                }
                db.SaveChanges();
                result.msg = "删除成功";
            }
            else
            {
                //参数错误
                result.success = false;
                result.msg = "删除失败";

                errorItem = new ErrorItem();
                errorItem.key = "id";
                errorItem.code = ErrorCode.param;
                errorItem.message = "参数错误";
                result.errors.Add(errorItem);
            }
            return Ok(result);
        }

        //---------------------------------------增删查改(END)

    }
}
