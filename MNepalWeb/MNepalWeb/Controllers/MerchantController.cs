using MNepalWeb.Models;
using MNepalWeb.UserModels;
using MNepalWeb.Utilities;
using MNepalWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MNepalWeb.Controllers
{
    public class MerchantController : Controller
    {
       
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>


        #region
        // GET: Merchant
        [HttpGet]
       
        public ActionResult Statement()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.MType = "";

                MerStatement para = new MerStatement();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                ReportUserModel rep = new ReportUserModel();
               
                return View(new MerchantSmtVM
                {
                    Parameter = para,
                    SmtInfo = new List<SmtInfo>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        [HttpPost]

        public ActionResult Statement(MerchantSmtVM model)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.MType = "";

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

            string start = "", end = "";

            if (!string.IsNullOrEmpty(model.Parameter.StartDate))

            {
                if (!model.Parameter.StartDate.IsValidDate())
                {
                    ViewBag.Error = "Not valid start date";
                    return View(new MerchantSmtVM
                    {
                        Parameter = new MerStatement(),
                        SmtInfo = new List<SmtInfo>()

                    });
                }
                start = DateTime.ParseExact(model.Parameter.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                  .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(model.Parameter.EndDate))
            {
                if (!model.Parameter.EndDate.IsValidDate())
                {
                    ViewBag.Error = "Not valid end date";
                    return View(new MerchantSmtVM
                    {
                        Parameter = new MerStatement(),
                        SmtInfo = new List<SmtInfo>()

                    });
                }
                end = DateTime.ParseExact(model.Parameter.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            }

            ViewBag.Error = "";
            ViewBag.message = "T";


            model.Parameter.StartDate = start;
            model.Parameter.EndDate = end;


            ReportUserModel rep = new ReportUserModel();
            List<SmtInfo> report = rep.MerchantSmt(model.Parameter);
            MerchantSmtVM vm = new MerchantSmtVM();
            vm.SmtInfo= report;
            MerStatement ms = new MerStatement();
            ms.MobileNo = model.Parameter.MobileNo;
            ms.StartDate = model.Parameter.StartDate;
            ms.EndDate = model.Parameter.EndDate;
                      
            vm.Parameter = ms;
            return View(vm);


        }



        #endregion
    }
}