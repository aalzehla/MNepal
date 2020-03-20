using MNepalWeb.Models;
using MNepalWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MNepalWeb.NTServices;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace MNepalWeb.Controllers
{
    public class TopUpController : Controller
    {
        // GET: TopUp
        //ADSL
        //Volume Based
        //Unlimited
        //LandLine
        //Ncell Topup
        //Ncell  Prepaid
        //Ncell  PostPaid

        //NTC topup
        //PostPaid
        //Prepaid
        //CDMA


        public ActionResult Index()
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
            }
                return View();
        }

        [HttpGet]
        public ActionResult NTPrepaid()
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
            }

            return View();
        }

       

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult NTPrepaid(NTTopUpVM model)
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
            }

            if (string.IsNullOrEmpty(model.Amount.ToString()))
            {
                ModelState.AddModelError("Amount", "Amount cannot be empty");
            }
            if(string.IsNullOrEmpty(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phone number cannot be empty");
            }
            if(model.Amount<10)
            {
                ModelState.AddModelError("Amount", "Cannot topup with amount less than 10");
            }
            if (model.Amount > 5000)
            {
                ModelState.AddModelError("Amount", "Cannot topup with amount more than than 5000");
            }
            if (ModelState.IsValid)
            {
                NTTopUp top = new NTTopUp();
                top.amount = model.Amount.ToString();
                top.targetNumber = model.PhoneNumber;
                top.channel = "26";
                top.hold1 = "997000010000605";
                top.optType = "3";
                top.phoneType = "5";
                top.sourceDealer = "10000605";
                top.sourceNumber = "9841567999";

                string Parameter = GetQueryString(top);
                NTSoapSoapClient client = new NTServices.NTSoapSoapClient();
                
                string json=client.eTopupTransfer(Parameter);
                ViewBag.TestString = json;
                return View("Test");

            }
            return View(model);
        }
       
        [HttpGet]
        public ActionResult GetRetailerBalance()
        {

            NTSoapSoapClient client = new NTSoapSoapClient();

            string dealerbalance=client.getDealerBalance("9841567999");

            ViewBag.TestString = dealerbalance;
            return View("Test");
        }
        
        [HttpGet]
        public ActionResult GetDealerBalance()
        {

            NTSoapSoapClient client = new NTSoapSoapClient();

            string dealerbalance = client.getDealerBalance("9851166554");

            return View();
        }

        [HttpGet]
        public ActionResult DealerToRetailer()
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
            }

            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DealerToRetailer(NTTopUpVM model)
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
            }

            NTTopUp top = new NTTopUp();
            top.amount = model.Amount.ToString() ;
            top.targetNumber = model.PhoneNumber;
            top.channel = "26"; //26 MPOS  //27 WEB
            top.hold1 = "997000010000604";
            top.optType = "1";
            top.phoneType = "5";
            top.sourceDealer = "10000604";
            top.sourceNumber = "9851166554";
            top.targetDealer = "10000605";

            string Parameter = GetQueryString(top);
            NTSoapSoapClient client = new NTServices.NTSoapSoapClient();
            string json = client.eTopupTransfer(Parameter);
            eTopUpTransferResponse response = JsonConvert.DeserializeObject<eTopUpTransferResponse>(json);
            
            return View();
        }
        private string GetQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return String.Join("&", properties.ToArray());
        }
    }
}