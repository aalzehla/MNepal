using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ThailiMNepalAgent.App_Start;
using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Utilities;

namespace ThailiMNepalAgent.Controllers
{
    public class CustomerSupportController : Controller
    {
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //ViewBag.bankBalance = (string)Session["bankBal"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
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

                    ViewBag.IsRejected = userInfo.IsRejected;
                    ViewBag.hasKYC = userInfo.hasKYC;
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //POST: FundTransfer/FundTransferTran
        [HttpPost]
        public async Task<ActionResult> CustomerSupportInsert(CustomerSupport customerSupport)
        {
            string MobileNumber, Name, Email, Remarks, Category;
            HttpPostedFileBase Image= customerSupport.Image;
            //HttpPostedFileBase Image= uploadedfile;
            MobileNumber = (string)Session["LOGGED_USERNAME"];
            Name = (string)Session["LOGGEDUSER_NAME"];
            Email = customerSupport.Email;
            Category = customerSupport.Category;
            Remarks = customerSupport.Remarks;
            string ImageName;
            if (Image != null)
            {
                ImageName = ParseCv(Image);
            }
            else
            {
                ImageName = null;
            }
            
            
            HttpResponseMessage _res = new HttpResponseMessage();
            using (HttpClient client = new HttpClient())
            {

                var action = "querystmt.svc/CustomerSupport";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("MobileNumber", MobileNumber),
                        new KeyValuePair<string, string>("Name", Name),
                        new KeyValuePair<string, string>("Email", Email),
                        new KeyValuePair<string, string>("Category", Category),
                        new KeyValuePair<string, string>("Remarks", Remarks),
                        new KeyValuePair<string, string>("Image", ImageName)
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
                        if(responseCode == 200)
                        {
                            this.TempData["message_class"] = "success_info";
                            this.TempData["agent_message"] = "Your request has been received.";
                            return RedirectToAction("Index", "AgentDashboard");
                        }
                        else
                        {
                            this.TempData["message_class"] = "failure_info";
                            this.TempData["agent_message"] = "Your request has not been received.";
                            return RedirectToAction("Index", "AgentDashboard");
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
                            this.TempData["message_class"] = "failure_info";
                            this.TempData["agent_message"] = "Your request has not been received.";
                            return RedirectToAction("Index", "AgentDashboard");
                            return Json(new { responseCode = responseCode, responseText = responsetext },
                        JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            this.TempData["message_class"] = "failure_info";
                            this.TempData["agent_message"] = "Your request has not been received.";
                            return RedirectToAction("Index", "AgentDashboard");
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
        #region For image encode and resize
        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        public string ParseCv(HttpPostedFileBase cvFile)
        {
            //WebImage img = new WebImage(cvFile.InputStream);
            //img.Resize(440, 300, true, true);
            var fileName = System.IO.Path.GetFileName(cvFile.FileName);
            string randomFileName = Path.GetExtension(fileName);

            Image image = Image.FromStream(cvFile.InputStream, true, true);
            Image img = resizeImage(image, new Size(300, 400));
            using (MemoryStream m = new MemoryStream())
            {
                img.Save(m, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] imageBytes = m.ToArray();

                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }

            
        }

        public string ReturnFileName(HttpPostedFileBase file, string mobile)
        {

            if (file == null)
                return null;

            if (IsImage(file) == false)
                return null;

            int iFileSize = file.ContentLength;
            if (iFileSize > 1048576)
                return null;

            var fileName = System.IO.Path.GetFileName(file.FileName);
            string randomFileName = mobile + "_" + Path.GetFileNameWithoutExtension(fileName) +
                                "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".jpeg";
            return randomFileName;
        }

        private bool IsImage(HttpPostedFileBase file)
        {
            //Checks for image type... you could also do filename extension checks and other things
            return ((file != null) && System.Text.RegularExpressions.Regex.IsMatch(file.ContentType, "image/\\S+") && (file.ContentLength > 0));
        }
        #endregion
    }
}