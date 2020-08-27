using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CustApp.Controllers
{
    public class HelpController : Controller
    {
        public readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        // GET: Help
        public ActionResult ContactUs()
        {
            try
            {
                string clientIp = (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ??
                   Request.ServerVariables["REMOTE_ADDR"]).Split(',')[0].Trim();
                var eventInfo = new LogEventInfo(LogLevel.Info, Logger.Name, "Message");
                eventInfo.Properties["UserName"] = "";
                eventInfo.Properties["UserIP"] = clientIp;
                Logger.Log(eventInfo);
            }
            catch (Exception ex)
            {

                var eventInfo = new LogEventInfo(LogLevel.Error, Logger.Name, ex.ToString());
                eventInfo.Properties["UserName"] = "";
                eventInfo.Properties["Exception"] = ex.Message;
                Logger.Log(eventInfo);
            }
            return View();
        }

        public ActionResult AboutUs()
        {
            return Redirect("http://mnepal.com/");
        }

        public ActionResult Career()
        {
            return View();
        }

        public ActionResult BlogOrNews()
        {
            return View();
        }

        public ActionResult Bank()
        {
            return View();
        }

    }
}