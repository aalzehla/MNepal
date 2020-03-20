using MNepalWeb.Models;
using MNepalWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MNepalWeb.Controllers
{
    public class LogOutController : Controller
    {
        // GET: LogOut
        public ActionResult Index()
        {

            /*Stamp Logout*/
            MNAdminLog log = new MNAdminLog();
            log.IPAddress = this.Request.UserHostAddress;
            log.URL = this.Request.Url.PathAndQuery;
            log.Message = " ";
            log.Action = "LOGOUT";
            if (Session["UniqueId"] != null)
                log.UniqueId = Session["UniqueId"].ToString() + "|" + HttpContext.Session.SessionID;
            else
                log.UniqueId = HttpContext.Session.SessionID;

            if (Session["UserBranch"] != null)
                log.Branch = Session["UserBranch"].ToString();
            else
                log.Branch = "000";

            if (Session["UserName"] != null)
                log.UserId =Session["UserName"].ToString();
            else
                log.UserId = "IIS";

            if (Session["LOGGED_USERTYPE"] != null)
                log.UserType = Session["LOGGED_USERTYPE"].ToString();
            else
                log.UserType = "SERVER";
       
            log.TimeStamp = DateTime.Now;

            //for getting ip address
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    log.PrivateIP = ip.ToString();
                }
            }
            LoginUtils.LogAction(log);

            if(Session["UserName"]!= null)
                LoginUtils.LogOutUser(Session["UserName"].ToString());


            //Clear Cookie
            FormsAuthentication.SignOut();
            Session.Abandon();//Clear the session
            return RedirectToAction("Index", "Login");
        }
    }
}