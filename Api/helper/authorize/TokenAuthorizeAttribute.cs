using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace com.caijunxiong.api.helper.authorize
{
    /// <summary>
    /// token验证属性
    /// </summary>
    public class TokenAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            string authorization = HttpContext.Current.Request.Headers["Authorization"];
            if (authorization == null)
            {
                HandleUnauthorizedRequest(actionContext);
            }
            else
            {
                string[] auths = authorization.Split(' ');
                if (auths.Length > 1)
                {
                    string token = auths[1];
                    if (!TokenVerify.verifyToken(token))
                    {
                        HandleUnauthorizedRequest(actionContext);
                    }
                }
                else
                {
                    HandleUnauthorizedRequest(actionContext);
                }
            }


        }
        protected override void HandleUnauthorizedRequest(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var challengeMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            challengeMessage.Headers.Add("WWW-Authenticate", "Basic");
            throw new System.Web.Http.HttpResponseException(challengeMessage);
        }
    }
}