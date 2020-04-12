using CustApp.Helper;
using CustApp.Models;
using CustApp.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static CustApp.Models.Notifications;

namespace CustApp.Controllers
{
    public class NotificationsController : Controller
    {
        DAL objdal = new DAL();
        // GET: Notifications
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

                ViewBag.SenderMobileNo = userName;

                int id = TraceIdGenerator.GetID() + 1;
                string stringid = (id).ToString();//this.GetID() + 1
                string traceID = stringid.PadLeft(11, '0') + 'W';
                ViewBag.TraceID = traceID;

                UserInfo userInfo = new UserInfo();


                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }


                //Check KYC
                DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                {
                    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                    userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

                    ViewBag.IsRejected = userInfo.IsRejected;

                    ViewBag.hasKYC = userInfo.hasKYC;
                }

                //Check Link Bank Account
                DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
                if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
                {
                    userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                    ViewBag.HasBankKYC = userInfo.BankAccountNumber;
                }

                //For Profile Picture
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #region "POST: Push Notifications"
        [HttpPost]

        public async Task<ActionResult> PushNotifications(NotificationModel notifications)
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





            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            string mobile = userName; //mobile is username
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();
            string tokenID = Session["TokenID"].ToString();


            var payload = new Notificationsobject
            {
                to = "/topics/message",
                data = new Data
                {
                    extra_information = notifications.extraInformation
                },
                notification = new Notification
                {
                    title = notifications.title,
                    text = notifications.text
                }
            };

            int responseCode = 0;
            string message = string.Empty;
            string responsetext = string.Empty;
            bool result = false;

            // Serialize our concrete class into a JSON String
            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(payload));

            // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var PushNotificationUrl = System.Configuration.ConfigurationManager.AppSettings["PushNotificationUrl"];

            using (var httpClient = new HttpClient())
            {
                var httpResponse = await httpClient.PostAsync(PushNotificationUrl, httpContent);

                try
                {
                    if (httpResponse.IsSuccessStatusCode)
                    {

                        result = true;
                        responseCode = (int)_res.StatusCode;
                        responsetext = await httpResponse.Content.ReadAsStringAsync();
                        message = httpResponse.Content.ReadAsStringAsync().Result;
                        string respmsg = "";
                        if (!string.IsNullOrEmpty(message))
                        {



                            int code = 200;
                            respmsg = "Success";
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
                        if (message == null)
                        {
                            return Json(new { responseCode = responseCode, responseText = responsetext },
                        JsonRequestBehavior.AllowGet);
                        }
                        else
                        {

                            return Json(new { responseCode = responseCode, responseText = responsetext },
                            JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { responseCode = "400", responseText = ex.Message },
                        JsonRequestBehavior.AllowGet);
                }
            }


        }
        #endregion
    }
}