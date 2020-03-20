using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CustApp.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult ContactUs()
        {
            return View();
        }
    }
}