using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.caijunxiong.dal;
using com.caijunxiong.util;
using com.caijunxiong.util.assist;
using Newtonsoft.Json;

namespace com.caijunxiong.api.Pages
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //            string str=Encryption.EncryptPassword("123456");
            //                        string str= Encryption.HMACSHA1("123456123456", "key666888");
            //            Response.Write(str);
            //            Response.Write("<br>");
            //            str = Encryption.EncryptPassword("1234561234561234561234561234");
            //            Response.Write(str);
            //            Response.Write("<br>");
            //            Response.Write(str.Length);
            //
            //
            //            Response.Write("<br>");
            //            str = Encryption.MD5("1234561234561234561234561234");
            //            Response.Write(str);
            //            Response.Write("<br>");
            //            Response.Write(str.Length);

            //            var redis = new RedisHelper(1);
            //            redis.StringSet("test", "ha ha");
            //            string str = redis.StringGet("test");
            //            Response.Write(str);

//            GlobalConfig config = Application[Constant.globalConfigKey] as GlobalConfig;
            string str ;
            //= config.tokenValidTime+"";
            Response.Write("开始生成"+DateTime.Now.ToString("u"));
            Response.Write("<br>");

            //存入大量
            int count = 100;
            List<UserSession> arrays=new List<UserSession>();
            UserSession us;
            for (int i = 0; i < count; i++)
            {
                us = new UserSession();
                us.id = i + 1;
                arrays.Add(us);
//                arrays.Add(JsonConvert.SerializeObject(us));
                //                redis.HashSet("Session", (i + 1) + "", us);

                //                redis.StringSet("Session" + (i + 1) + "", us);
            }

            Response.Write("生成后" + DateTime.Now.ToString("u"));
            Response.Write("<br>");

            RedisHelper redis = new RedisHelper(2);
            
            for (int i = 0; i < count; i++)
            {
                //                us=new UserSession();
                //                us.id = i + 1;
                redis.HashSet<string>("Session", (i + 1) + "", "ha");
                //                 redis.HashSet("Session", (i + 1) + "", arrays[i]);
                //                redis.StringSet("Session"+ (i + 1) + "",us);
            }
            

            Response.Write("存入redis" + DateTime.Now.ToString("u"));
            Response.Write("<br>");

            //读取
//            redis.HashGet("Session", "1");
                        for (int i = 0; i < count; i++)
                        {
                            us = redis.HashGet<UserSession>("Session", (i + 1) + "");
//                            string u = redis.HashGet("Session", (i + 1) + "");
//                            UserSession us = (UserSession)JsonConvert.DeserializeObject(u, typeof(UserSession));
                Response.Write(us.id);
                Response.Write("<br>");
            }
            //
            Response.Write("读取redis"+DateTime.Now.ToString("u"));
            Response.Write("<br>");

            //            string str = "666";

        }
    }
}