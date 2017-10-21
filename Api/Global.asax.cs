﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using com.caijunxiong.util.assist;

namespace api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            readConfig();//获取配置信息
        }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        private void readConfig()
        {
            int tokenValidTime =int.Parse(ConfigurationManager.AppSettings["Token:ValidTime"]);
            int tokenValidRefreshTime = int.Parse(ConfigurationManager.AppSettings["Token:ValidRefreshTime"]);
            int pageSize = int.Parse(ConfigurationManager.AppSettings["Config:PageSize"]);
            string superRole = ConfigurationManager.AppSettings["Config:SuperRole"];
            string maxUser = ConfigurationManager.AppSettings["Config:MaxUser"];
            string defaultLoginType = ConfigurationManager.AppSettings["Config:defaultLoginType"];
            string cacheDefaultDb = ConfigurationManager.AppSettings["Config:CacheDefaultDb"];
            string cacheSessionKey = ConfigurationManager.AppSettings["Config:cacheSessionKey"];

            GlobalConfig config = new GlobalConfig();

            //保存每种登录类型对应的最大人数
            string[] typeMax = maxUser.Split(',');
            for (int i = 0; i < typeMax.Length; i++)
            {
                string[] currentMax = typeMax[i].Split(':');
                config.maxUser.Add(currentMax[0],int.Parse(currentMax[1]));
            }

            config.defaultLoginType = defaultLoginType;

            config.tokenValidTime = tokenValidTime;
            config.tokenValidRefreshTime = tokenValidRefreshTime;
            config.pageSize = pageSize;
            config.superRole = superRole;
            config.cacheDefaultDb = int.Parse(cacheDefaultDb);
            config.cacheSessionKey = cacheSessionKey;



            Application.Add(Constant.globalConfigKey, config);
        }
    }
}
