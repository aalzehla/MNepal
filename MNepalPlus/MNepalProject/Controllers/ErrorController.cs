using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MNepalProject.Controllers
{
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public ActionResult Invalid_Request()
        {
            TempData["antiforegerymsg"] = @"Sorry, your request could not be processed. you are already logged in ";
            return View();
        } 
   
        [AllowAnonymous]
        public ActionResult ApplicationError()
        {
            TempData["errmsg"] = @"error error ";
            return View();
        }
    }
}