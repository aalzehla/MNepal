using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace CustApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            MvcHandler.DisableMvcResponseHeader = true;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Response.AddHeader("X-Frame-Options", "DENY");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        }

        protected void Application_EndRequest(Object sender, EventArgs e)
        {
            string authCookie = FormsAuthentication.FormsCookieName;
            //Request
            if (Response.Cookies.Count > 0)
            {
                foreach (string s in Response.Cookies.AllKeys)
                {
                    Response.Cookies[s].Secure = true;
                }
            }
            //Response
            if (Response.Cookies.Count > 0)
            {
                foreach (string sCookie in Response.Cookies)
                {
                    var httpCookie = Response.Cookies[sCookie];
                    if (httpCookie != null)
                    {
                        httpCookie.HttpOnly = true;

                    }
                    if (sCookie.Equals(authCookie))
                    {
                        Response.Cookies[sCookie].Path += ";HttpOnly";
                    }
                }
            }
        }

        private void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();

            if (ex is HttpAntiForgeryException)
            {
                Response.Clear();
                Server.ClearError(); //make sure you log the exception first
                Response.Redirect("/Error/Error404", true);
            }
        }

        protected void Application_PreSendRequestHeaders()
        {
            this.Response.AppendHeader("X-Frame-Options", "DENY");
        }

    }
}
