using CustApp.App_Start;
using CustApp.Models;
using CustApp.Settings;
using CustApp.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.SessionState;

namespace CustApp.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class PinResetController : Controller
    {
        // GET: PinReset
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["pin_messsage"] != null)
            {
                this.ViewData["pin_messsage"] = this.TempData["pin_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                ///start milayako
                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }

                //end milayako
                UserInfo userInfo = new UserInfo();

                //Check KYC
                DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                {
                    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                    userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

                    ViewBag.IsRejected = userInfo.IsRejected;
                    ViewBag.hasKYC = userInfo.hasKYC;
                }

                DataSet DSet = ProfileUtils.GetCusDetailProfileInfoDS(clientCode);

                DataTable dtableUser = DSet.Tables["dtUserInfo"];
                DataTable dKYC = DSet.Tables["dtKycDetail"];
                DataTable dDoc = DSet.Tables["dtKycDoc"];
                if (dtableUser != null && dtableUser.Rows.Count > 0)
                {
                    userInfo.ContactNumber1 = dtableUser.Rows[0]["ContactNumber1"].ToString();
                    userInfo.ContactNumber2 = dtableUser.Rows[0]["ContactNumber2"].ToString();

                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.ContactNumber2 = userInfo.ContactNumber2;
                    ViewBag.SelfReg = userInfo.SelfRegistered;
                    //added//
                }

                if (dKYC != null && dKYC.Rows.Count > 0)
                {
                    userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();

                    ViewBag.CustStatus = userInfo.CustStatus;
                }

                if (dDoc != null && dDoc.Rows.Count > 0)
                {
                    userInfo.Document = dDoc.Rows[0]["DocType"].ToString();
                    userInfo.FrontImage = dDoc.Rows[0]["FrontImage"].ToString();
                    userInfo.BackImage = dDoc.Rows[0]["BackImage"].ToString();
                    userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();

                    ViewBag.DocType = userInfo.Document;
                    ViewBag.FrontImage = userInfo.FrontImage;
                    ViewBag.BackImage = userInfo.BackImage;
                    ViewBag.PassportImage = userInfo.PassportImage;

                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // POST: PinReset
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo userInfo = new UserInfo();
                string oldPin = collection["txtOldPin"].ToString();
                string newPin = collection["txtNewPin"].ToString();
                string retypePin = collection["txtRetypePin"].ToString();

                userInfo.OPIN = oldPin;
                userInfo.PIN = newPin;
                userInfo.ClientCode = clientCode;

                string displayMessage = null;
                string messageClass = null;

                try
                {
                    if ((newPin != "") && (retypePin != "") && (oldPin != ""))
                    {
                        if (newPin == retypePin)
                        {
                            bool isUpdated = PinUtils.UpdateUserPinInfo(userInfo);

                            if (isUpdated)
                            {
                                displayMessage = isUpdated
                                                     ? "Your T-PIN has been successfully changed."/*PIN has successfully been updated.*/
                                                     : "Your T-PIN doesn't match to the New T-PIN."; /*ERROR::Please Enter Correct old pin number!*/
                                messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                                FormsAuthentication.SignOut();
                                Session.RemoveAll();
                                //Session.Abandon();//Clear the session
                                //return RedirectToAction("Index", "Login");
                            }
                            else
                            {
                                displayMessage = "Your T-PIN doesn't match to the Old T-PIN.";
                                messageClass = CssSetting.FailedMessageClass;
                                this.TempData["pin_messsage"] = displayMessage;
                                this.TempData["message_class"] = messageClass;
                                Session.Timeout = 1;
                                return RedirectToAction("Index", "PinReset");
                            }
                        }
                    }
                    else
                    {
                        displayMessage = "Pin is Empty";
                        messageClass = CssSetting.FailedMessageClass;
                        this.TempData["pin_messsage"] = displayMessage;
                        this.TempData["message_class"] = messageClass;
                        Session.Timeout = 1;
                        return RedirectToAction("Index", "PinReset");

                    }

                }
                catch (Exception ex)
                {
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["login_message"] = displayMessage;
                this.TempData["message_class"] = messageClass;
                Session.Timeout = 1;
                return RedirectToAction("Index", "Login");

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        
        public ActionResult ResetThailiPin()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            if (this.TempData["pin_messsage"] != null)
            {
                this.ViewData["pin_messsage"] = this.TempData["pin_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                ///start milayako
                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }

                //end milayako
                UserInfo userInfo = new UserInfo();

                //Check KYC
                DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                {
                    userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                    userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

                    ViewBag.IsRejected = userInfo.IsRejected;
                    ViewBag.hasKYC = userInfo.hasKYC;
                }

                DataSet DSet = ProfileUtils.GetCusDetailProfileInfoDS(clientCode);

                DataTable dtableUser = DSet.Tables["dtUserInfo"];
                DataTable dKYC = DSet.Tables["dtKycDetail"];
                DataTable dDoc = DSet.Tables["dtKycDoc"];
                if (dtableUser != null && dtableUser.Rows.Count > 0)
                {
                    userInfo.ContactNumber1 = dtableUser.Rows[0]["ContactNumber1"].ToString();
                    userInfo.ContactNumber2 = dtableUser.Rows[0]["ContactNumber2"].ToString();

                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.ContactNumber2 = userInfo.ContactNumber2;
                    ViewBag.SelfReg = userInfo.SelfRegistered;
                    //added//
                }

                if (dKYC != null && dKYC.Rows.Count > 0)
                {
                    userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();

                    ViewBag.CustStatus = userInfo.CustStatus;
                }

                if (dDoc != null && dDoc.Rows.Count > 0)
                {
                    userInfo.Document = dDoc.Rows[0]["DocType"].ToString();
                    userInfo.FrontImage = dDoc.Rows[0]["FrontImage"].ToString();
                    userInfo.BackImage = dDoc.Rows[0]["BackImage"].ToString();
                    userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();

                    ViewBag.DocType = userInfo.Document;
                    ViewBag.FrontImage = userInfo.FrontImage;
                    ViewBag.BackImage = userInfo.BackImage;
                    ViewBag.PassportImage = userInfo.PassportImage;

                }

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        [HttpPost]
        public async Task<ActionResult> ResetThailiPin(FormCollection collection)
        {
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                UserInfo userInfo = new UserInfo();
                string mobileNumber = collection["mobileNumber"].ToString();


                //api call
                HttpResponseMessage _res = new HttpResponseMessage();
                using (var client = new HttpClient())
                {
                    //var re = Request;
                    //var headers = re.Headers;
                    ////client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAALb0J21uJQkW66f/5tn51oAAAAAACAAAAAAAQZgAAAAEAACAAAACujThGwSJ93BGelKQrjJ748rl2+xgpln4Cd1rtWKH6dgAAAAAOgAAAAAIAACAAAAA2q7bCrhkwFnASE36rJgfFlhqqZBTGliq5KiTPnbWiDSAAAACoZSqNV4SH5AwklADAn7cxHloBR7Ft7KW2Z24p/TnjWkAAAAC4IjJh6LMIy/8zKvr5r7/yuPlDtSnmTH/sVYxmDyBQ6JeDlR56+2dHFAvgF3i83Bt/SYP4dGZ9zpQjc1MzVIir");
                    //var AuthUsername = System.Configuration.ConfigurationManager.AppSettings["AuthUsername"];
                    //var AuthPassword = System.Configuration.ConfigurationManager.AppSettings["AuthPassword"];
                    //client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["DematBaseAddress"]);
                    //var GetPendingPayments = System.Configuration.ConfigurationManager.AppSettings["GetPendingPayments"];
                    ////var content = new StringContent("application/json");
                    //var byteArray = new UTF8Encoding().GetBytes(AuthUsername + ":" + AuthPassword);
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    //dematGet.BoId = dmat.dCode;

                    //var jsonObject = JsonConvert.SerializeObject(dematGet);
                    //var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");

                    var action = "reset.svc/ResetThailiPin";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]{
                    new KeyValuePair<string,string>("UserName",mobileNumber),
                    new KeyValuePair<string,string>("clientCode",clientCode),
                    new KeyValuePair<string,string>("name",name)

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

                }

            }
            else
            {
                return RedirectToAction("Index", "UserDashboard");
            }
           
        }
    }
}