using CustApp.Connection;
using CustApp.Models;
using CustApp.Utilities;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net;
using System.Text;
using System.Web.Mvc;
using static CustApp.Models.Notifications;

namespace CustApp.Controllers
{
    public class NotificationsController : Controller
    {
        // GET: Notifications
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            //Check all three variables Session1, Session2, Cookie. If all the three are not null then procces futher
            if (Session["LOGGED_USERNAME"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                //Second Check, if Cookies we created has the same value as second session we've created
                //if (Request.Cookies["AuthToken"].Value == Session["AuthToken"].ToString())
                if (Session["AuthToken"].ToString().Equals(
                           Request.Cookies["AuthToken"].Value))
                {

                    if (TempData["userType"] != null)
                    {
                        DataTable dtableUserCheckFirstLogin = ProfileUtils.IsFirstLogin(clientCode);
                        if (dtableUserCheckFirstLogin != null && dtableUserCheckFirstLogin.Rows.Count > 0)
                        {
                            ViewBag.IsFirstLogin = dtableUserCheckFirstLogin.Rows[0]["IsFirstLogin"].ToString();
                            ViewBag.PinChanged = dtableUserCheckFirstLogin.Rows[0]["PinChanged"].ToString();
                            ViewBag.PassChanged = dtableUserCheckFirstLogin.Rows[0]["PassChanged"].ToString();
                        }

                        if (TempData["userType"] != null && ViewBag.IsFirstLogin == "F" && ViewBag.PinChanged == "T" && ViewBag.PassChanged == "T")
                        {
                            this.ViewData["userType"] = this.TempData["userType"];
                            ViewBag.UserType = this.TempData["userType"];
                            ViewBag.Name = name;
                            Session["bankbalance"] = "";
                            ViewBag.BankBal = Session["bankbalance"];

                            MNBalance availBaln = new MNBalance();
                            DataTable dtableUser = AvailBalnUtils.GetAvailBaln(clientCode);
                            if (dtableUser != null && dtableUser.Rows.Count > 0)
                            {
                                availBaln.amount = dtableUser.Rows[0]["AvailBaln"].ToString();

                                ViewBag.AvailBalnAmount = availBaln.amount;
                            }

                            UserInfo userInfo = new UserInfo();

                            //Check KYC
                            DataTable dtableUserCheckKYC = ProfileUtils.CheckKYC(userName);
                            if (dtableUserCheckKYC != null && dtableUserCheckKYC.Rows.Count > 0)
                            {
                                userInfo.hasKYC = dtableUserCheckKYC.Rows[0]["hasKYC"].ToString();
                                userInfo.IsRejected = dtableUserCheckKYC.Rows[0]["IsRejected"].ToString();

                                ViewBag.hasKYC = userInfo.hasKYC;
                                ViewBag.IsRejected = userInfo.IsRejected;
                            }

                            //Check Link Bank Account
                            DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
                            if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
                            {
                                userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                                ViewBag.HasBankKYC = userInfo.BankAccountNumber;
                            }

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

                            //calls api
                            var webClient = new WebClient();
                            var BasicAuthUserName = ConfigurationManager.AppSettings["BasicAuthUserName"];
                            var BasicAuthPassword = ConfigurationManager.AppSettings["BasicAuthPassword"];
                            string GetAllNotificationURL = ConfigurationManager.AppSettings["GetAllNotificationURL"];
                            string AuthorizationCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(BasicAuthUserName + ":" + BasicAuthPassword));
                            webClient.Headers.Add("Authorization", "Basic " + AuthorizationCredentials);
                            var json = webClient.DownloadString(GetAllNotificationURL);
                            RootObject objJson = JsonConvert.DeserializeObject<RootObject>(json);
                            return View(objJson);
                        }
                        else
                        {
                            Session["LOGGED_USERNAME"] = null;
                            return RedirectToAction("Index", "Login");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "Login");
                    }

                }
                else
                {
                    Session["LOGGED_USERNAME"] = null;
                    return RedirectToAction("Index", "Login");
                }
            }
            else
            {
                Session["LOGGED_USERNAME"] = null;
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult GetCurrentUserNotification()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            var notificationList = new List<NotificationModel>();
            Database database;
            DataTable dtableResult = null;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNGetUserNotification]"))
                {
                    database.AddInParameter(command, "@UserMobileNumber", DbType.String, userName);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "dtGetUserNotificationInfo");

                        if (dataset.Tables.Count > 0)
                        {
                            dtableResult = dataset.Tables["dtGetUserNotificationInfo"];
                        }


                        foreach (DataRow dr in dataset.Tables[0].Rows)
                        {
                            NotificationModel notifications = new NotificationModel();

                            notifications.title = dr["Title"].ToString();
                            notifications.text = dr["Text"].ToString();
                            notifications.imageName = dr["ImageName"].ToString();
                            notifications.redirectUrl = dr["RedirectUrl"].ToString();
                            notifications.pushNotificationDate = dr["NotificationDate"].ToString();
                            notifications.messageId = dr["MessageId"].ToString();
                            notifications.NotificationId = Convert.ToInt32(dr["Id"]);
                            notifications.ReadOn = string.IsNullOrEmpty(dr["ReadOn"].ToString()) ? (DateTime?)null : Convert.ToDateTime(dr["ReadOn"]);
                            notifications.IsRead = Convert.ToBoolean(dr["IsRead"]);
                            notificationList.Add(notifications);

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                database = null;
                Database.ClearParameterCache();
            }

            Session["UserNotification"] = notificationList;
            return null;
        }

        public int MarkAsRead(int notificationId, bool markAllAsRead = false)
        {
            string userName = (string)Session["LOGGED_USERNAME"];

            Database database;

            try
            {
                database = DatabaseConnection.GetDatabase();
                using (var command = database.GetStoredProcCommand("[s_MNMarkNotificationAsRead]"))
                {
                    database.AddInParameter(command, "@notificationId", DbType.String, notificationId);
                    database.AddInParameter(command, "@userMobileNumber", DbType.String, userName);
                    database.AddInParameter(command, "@markAllAsRead", DbType.String, markAllAsRead);
                    using (var dataset = new DataSet())
                    {
                        database.LoadDataSet(command, dataset, "RowsAffected");

                        if (dataset.Tables.Count > 0)
                        {

                            return Convert.ToInt32(dataset.Tables[0].Rows[0][0]);


                        }
                        return 0;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                database = null;
                Database.ClearParameterCache();
            }





        }

        public ActionResult GetNotifications()
        {
            //calls api
            var webClient = new WebClient();
            var BasicAuthUserName = ConfigurationManager.AppSettings["BasicAuthUserName"];
            var BasicAuthPassword = ConfigurationManager.AppSettings["BasicAuthPassword"];
            string GetAllNotificationURL = ConfigurationManager.AppSettings["GetAllNotificationURL"];
            string AuthorizationCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(BasicAuthUserName + ":" + BasicAuthPassword));
            webClient.Headers.Add("Authorization", "Basic " + AuthorizationCredentials);
            var json = webClient.DownloadString(GetAllNotificationURL);
            RootObject objJson = JsonConvert.DeserializeObject<RootObject>(json);
            webClient.Dispose();

            IList collection = (IList)objJson.notificationsList;

            Session["manoj"] = collection;
            return null;
        }
    }
}