using MNepalWeb.Models;
using MNepalWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MNepalWeb.App_Start
{
    public static class ApplicationInitilize
    {
      
        private static string _WCFUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["WCF"];
        private static string _APIUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["API"];
        public static string WCFUrl { get { return _WCFUrl; } }
        public static string APIUrl { get { return _APIUrl; } }

        public static string[] NcellStarters = { "980","982", "981" };
        public static string[] NTCStarters = { "983","984", "985", "986" };
        public static string[] ADSLStarters = { "01","02","03","04","05","06","07","08","09"};

    }


    public class MyActionFilterAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if(!filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.HttpContext.Request.RequestType.ToUpper()=="GET")
            {

                if(filterContext.Controller.ControllerContext.RouteData.Values["controller"].ToString().ToUpper()!="LOGIN" &&
                   filterContext.Controller.ControllerContext.RouteData.Values["controller"].ToString().ToUpper() != "NAVIGATION")
                {
                    //check if the user is allowd to acces the controler action
                    var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                    var action = filterContext.ActionDescriptor.ActionName;
                    var ip = filterContext.HttpContext.Request.UserHostAddress;
                    var URL = filterContext.HttpContext.Request.Url.PathAndQuery;

                    MNAdminLog log = new MNAdminLog();
                    log.IPAddress = ip; //PublicIP
                    log.URL = URL;
                    log.Message = " ";
                    log.Action = "BROWSE";

                    if (HttpContext.Current.Session["UniqueId"] != null)
                        log.UniqueId = HttpContext.Current.Session["UniqueId"].ToString() + "|" + filterContext.HttpContext.Session.SessionID;
                    else
                        log.UniqueId = filterContext.HttpContext.Session.SessionID;

                    if (HttpContext.Current.Session["UserBranch"] != null)
                        log.Branch = HttpContext.Current.Session["UserBranch"].ToString();
                    else
                        log.Branch = "000";

                    if (HttpContext.Current.Session["UserName"]!= null)
                        log.UserId = HttpContext.Current.Session["UserName"].ToString();
                    else
                        log.UserId = "IIS";

                    if (HttpContext.Current.Session["LOGGED_USERTYPE"] != null)
                        log.UserType = HttpContext.Current.Session["LOGGED_USERTYPE"].ToString();
                    else
                        log.UserType = "SERVER";
                    log.TimeStamp = DateTime.Now;
                    LoginUtils.LogAction(log);

                   // var user = GetUserName(filterContext.HttpContext);
                   // CheckUser(controller,action);

                }
            }
            //var fooCookie = filterContext.HttpContext.Request.Cookies["foo"];

            //if (fooCookie == null || fooCookie.Value != "foo bar")
            //{
            //    filterContext.Result = new HttpUnauthorizedResult();
            //}
        }
    }
}