using System;
using CustApp.Models;
using CustApp.Utilities;
using System.Web.Mvc;
using System.Data;
using System.Net.Http;
using CustApp.Helper;
using System.IO;
using CustApp.App_Start;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CustApp.Controllers
{
    public class DishHomeController : Controller
    {
        // GET: DishHome
        public ActionResult Index()
        {
            return View();
        }



        #region "POST: NEAPayment"
        [HttpPost]

        public async Task<ActionResult> DishHomePayment()
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;



            this.ViewData["userType"] = this.TempData["userType"];
            ViewBag.UserType = this.TempData["userType"];
            ViewBag.Name = name;


            MNBalance availBaln = new MNBalance();
            DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
            if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
            {
                availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                ViewBag.AvailBalnAmount = availBaln.amount;
            }

            //For Profile Picture
            UserInfo userInfo = new UserInfo();
            DataSet DSet = ProfileUtils.GetCusDetailProfileInfoDS(clientCode);
            DataTable dKYC = DSet.Tables["dtKycDetail"];
            DataTable dDoc = DSet.Tables["dtKycDoc"];
            if (dKYC != null && dKYC.Rows.Count > 0)
            {
                userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();
                ViewBag.CustStatus = userInfo.CustStatus;
            }
            if (dDoc != null && dDoc.Rows.Count > 0)
            {
                userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();
                ViewBag.PassportImage = userInfo.PassportImage;
            }


            HttpResponseMessage _res = new HttpResponseMessage();

            //string mobile = _DishHomeft.mobile; //mobile is username
            string mobile = userName; //mobile is username

            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();

            using (HttpClient client = new HttpClient())
            {

                var action = "paypoint.svc/request";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                //string token = Session["TokenID"].ToString();
                var content = new FormUrlEncodedContent(new[]{

                        new KeyValuePair<string, string>("vid", "14"),
                        new KeyValuePair<string,string>("sc","00"),
                        new KeyValuePair<string, string>("mobile","9803200158"),
                        new KeyValuePair<string, string>("amount", "139"),
                        new KeyValuePair<string,string>("da","9840066836"),
                        new KeyValuePair<string,string>("pin","1234"),
                        new KeyValuePair<string, string>("note", "Test"),
                        new KeyValuePair<string,string>("src","gprs"),
                        new KeyValuePair<string,string>("tokenID","7mfM2LjdyXsKYkKMqzmo7zHp2tc7jxnErBU"),

                        new KeyValuePair<string, string>("companyCode", "598"),
                        new KeyValuePair<string,string>("serviceCode", "1"),
                        //new KeyValuePair<string, string>("account",  _DishHomeft.SCNo),
                        //new KeyValuePair<string, string>("special1",_DishHomeft.DishHomeBranchName),
                        //new KeyValuePair<string,string>("special2", _DishHomeft.CustomerID),
                        new KeyValuePair<string, string>("tid", "4586957586"),

                    });
                _res = await client.PostAsync(new Uri(uri), content);
                string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                _res.ReasonPhrase = responseBody;
                string errorMessage = string.Empty;
                int responseCode = 0;
                string message = string.Empty;
                string responsetext = string.Empty;
                bool result = false;
                string ava = string.Empty;
                string avatra = string.Empty;
                string avamsg = string.Empty;
                try
                {
                    if (_res.IsSuccessStatusCode)
                    {
                        result = true;
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        message = _res.Content.ReadAsStringAsync().Result;
                        string respmsg = "";
                        if (!string.IsNullOrEmpty(message))
                        {
                            JavaScriptSerializer ser = new JavaScriptSerializer();
                            var json = ser.Deserialize<JsonParse>(responsetext);
                            message = json.d;
                            JsonParse myNames = ser.Deserialize<JsonParse>(json.d);
                            int code = Convert.ToInt32(myNames.StatusCode);
                            respmsg = myNames.StatusMessage;
                            if (code != responseCode)
                            {
                                responseCode = code;
                            }
                        }
                        return Json(new { responseCode = responseCode, responseText = respmsg },
                        JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        result = false;
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        dynamic json = JValue.Parse(responsetext);
                        message = json.d;
                        if (message == null)
                        {
                            return Json(new { responseCode = responseCode, responseText = responsetext },
                        JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            dynamic item = JValue.Parse(message);

                            return Json(new { responseCode = responseCode, responseText = (string)item["StatusMessage"] },
                            JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { responseCode = "400", responseText = ex.Message },
                        JsonRequestBehavior.AllowGet);
                }
                this.TempData["DishHome_messsage"] = result
                                                ? "Payment successfull." + message
                                                : "ERROR :: " + message;
                this.TempData["message_class"] = result ? "success_info" : "failed_info";

                return RedirectToAction("Index", "DishHome");
            }
        }
        #endregion
    }
}