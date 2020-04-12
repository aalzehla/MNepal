using MNSuperadmin.Helper;
using MNSuperadmin.Models;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using static MNSuperadmin.Models.Notifications;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;

namespace MNSuperadmin.Controllers
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




                if (this.TempData["registration_message"] != null)
                {
                    this.ViewData["registration_message"] = this.TempData["registration_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                int id = TraceIdGenerator.GetID() + 1;
                string stringid = (id).ToString();//this.GetID() + 1
                string traceID = stringid.PadLeft(11, '0') + 'W';
                ViewBag.TraceID = traceID;

                UserInfo userInfo = new UserInfo();

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


            //api call here
            HttpResponseMessage _res = new HttpResponseMessage();
            string mobile = userName; //mobile is username
            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();

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
            string responsetext = "Unauthorized";
            bool result = false;

            // Serialize our concrete class into a JSON String
            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(payload));

            // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var PushNotificationUrl = System.Configuration.ConfigurationManager.AppSettings["PushNotificationUrl"];
            using (var httpClient = new HttpClient())
            {
                var BasicAuthUserName = ConfigurationManager.AppSettings["BasicAuthUserName"];
                var BasicAuthPassword = ConfigurationManager.AppSettings["BasicAuthPassword"];
                var byteArray = new UTF8Encoding().GetBytes(BasicAuthUserName + ":" + BasicAuthPassword);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                var httpResponse = await httpClient.PostAsync(PushNotificationUrl, httpContent);
                if (httpResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        result = true;
                        responseCode = (int)httpResponse.StatusCode;
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
                    catch (Exception ex)
                    {
                        return Json(new { responseCode = "400", responseText = ex.Message },
                            JsonRequestBehavior.AllowGet);
                    }
                }
               
                 else
                {
                    result = false;
                    responseCode = (int)httpResponse.StatusCode;
                    if (responsetext == null)
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


        }
        #endregion
    }
}