using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CustApp.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Error404(string error)
        {
            return View();
        }
    }
}