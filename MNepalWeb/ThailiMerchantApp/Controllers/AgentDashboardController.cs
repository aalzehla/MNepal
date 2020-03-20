using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThailiMerchantApp.Models;
using ThailiMerchantApp.ViewModel;
using ThailiMerchantApp.UserModels;
using ThailiMerchantApp.Utilities;
using System.Globalization;
using System.Data;
using Newtonsoft.Json;
using System.Dynamic;
using System.Reflection;

using System.Text.RegularExpressions;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using ThailiMerchantApp.App_Start;
using System.IO;
using ThailiMerchantApp.Helper;
using System.Threading.Tasks;

namespace ThailiMerchantApp.Controllers {
    public class AgentDashboardController : Controller {
        public List<T> FilterAndSort<T>(DataTableAjaxPostModel model, List<T> source, out int TotalCount, out int Filtered)
        {
            int skip = model.start;
            int take = model.length;

            string sortBy = "";
            bool sortDir = true;
            var filter = source.AsQueryable();
            Func<T, Object> orderByFunc = null;
            if (model.order != null)
            {
                sortBy = model.columns[model.order[0].column].data;
                sortDir = model.order[0].dir.ToLower() == "asc";
                orderByFunc = p => p.GetType().GetProperty(sortBy).GetValue(p, null);
            }
            if (orderByFunc != null)
            {
                if (sortDir)
                    filter = filter.OrderBy(orderByFunc).AsQueryable();
                else
                    filter = filter.OrderByDescending(orderByFunc).AsQueryable();
            }
            TotalCount = source.Count;
            Filtered = filter.Count();
            var res = filter.Skip(skip).Take(take).ToList();

            return res;
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable();

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            foreach (var column in dataTable.Columns.Cast<DataColumn>().ToArray())
            {
                if (dataTable.AsEnumerable().All(dr => dr.IsNull(column)))
                    dataTable.Columns.Remove(column);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        
        [HttpGet]
        public ActionResult Index()

        {
            if (this.TempData["agent_message"] != null)
            {
                this.ViewData["agent_message"] = this.TempData["agent_message"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
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
                    MNBalance availBaln = new MNBalance();
                    DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                    if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                    {
                        availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                        ViewBag.AvailBalnAmount = availBaln.amount;
                    }

                    //Check Link Bank Account
                    string HasBankKYC = string.Empty;
                    UserInfo userInfo = new UserInfo();
                    DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
                    if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
                    {
                        userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                        ViewBag.HasBankKYC = userInfo.BankAccountNumber;
                        HasBankKYC = ViewBag.HasBankKYC;

                    }
                    //For Profile Pic//
                    //UserInfo userInfo = new UserInfo();
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

                    if (TempData["userType"] != null)
                    {
                        this.ViewData["userType"] = this.TempData["userType"];
                        ViewBag.UserType = this.TempData["userType"];
                        ViewBag.COC = Session["COC"];
                        return View();
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
 
        public ContentResult AgentDashboardTable(DataTableAjaxPostModel model, string ContactNumber1, string StartDate, string EndDate, string TranID, string change, string ToExcel)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            string ParaChanged = "T";
            string convert;
            if (!string.IsNullOrEmpty(StartDate))
            {
                if (!StartDate.IsValidDate())
                {

                    convert = JsonConvert.SerializeObject(new
                    {
                        draw = model.draw,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        error = "Not valid Start Date"
                    });

                    return Content(convert, "application/json");
                }
                StartDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            
            if (!string.IsNullOrEmpty(EndDate))
            {
                if (!EndDate.IsValidDate())
                {

                    convert = JsonConvert.SerializeObject(new
                    {
                        draw = model.draw,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        error = "Not valid End Date"
                    });

                    return Content(convert, "application/json");
                }
                EndDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            ParaChanged = change;
            var result = new List<MNAgentStatement>();
            if (Session["AdminActivity"] != null && ParaChanged == "F")
            {
                result = Session["AdminActivity"] as List<MNAgentStatement>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.GetAgentStatement(ContactNumber1, StartDate, EndDate, TranID);
                Session["AdminActivity"] = result;

            }
            if (ToExcel == "T")
            {

                DataTable excel = ToDataTable<MNAgentStatement>(result);
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_AdminActivity.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<MNAgentStatement>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                resultset.Date = item.Date;
                resultset.TranID = item.TranID;
               
                resultset.Description = item.Description;
                resultset.Debit = item.Debit;
                resultset.Credit = item.Credit;
                //resultset.Balance = item.Balance;

                //resultset.Remarks = string.Join(" ",item.Remarks,item.Description);
               // resultset.TimeStamp = item.TimeStamp;
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList
            });
            return Content(convert, "application/json");

        }

        public async Task<ActionResult> BankQuery()
        {
            string username = (string)Session["LOGGED_USERNAME"];
            string pin = "";
            if ((username != "") || (username != null))
            {

                string result = string.Empty;
                DataTable dtableMobileNo = CustomerUtils.GetUserProfileByMobileNo(username);
                if (dtableMobileNo.Rows.Count == 1)
                {
                    pin = dtableMobileNo.Rows[0]["PIN"].ToString();
                }

            }

            HttpResponseMessage _res = new HttpResponseMessage();

            string mobile = username; //mobile is username


            TraceIdGenerator _tig = new TraceIdGenerator();
            var tid = _tig.GenerateTraceID();

            using (HttpClient client = new HttpClient())
            {

                var action = "query.svc/balance?tid=" + tid + "&sc=22&mobile=" + mobile + "&sa=1&pin=" + pin + "&src=web";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                //var content = new FormUrlEncodedContent(new[]{
                //        new KeyValuePair<string, string>("tid", tid),
                //        new KeyValuePair<string,string>("sc","22"),
                //        new KeyValuePair<string, string>("mobile",mobile),
                //        new KeyValuePair<string, string>("sa", "1"),
                //        new KeyValuePair<string,string>("pin", pin),
                //        new KeyValuePair<string,string>("src","web")
                //    });
                try
                {
                    _res = await client.GetAsync(uri);
                    string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                    _res.ReasonPhrase = responseBody;
                    string errorMessage = string.Empty;
                    int responseCode = 0;
                    string message = string.Empty;
                    string responsetext = string.Empty;
                    string responsedateTime = string.Empty;
                    bool result = false;
                    string ava = string.Empty;
                    string avatra = string.Empty;
                    string avamsg = string.Empty;

                    if (_res.IsSuccessStatusCode)
                    {
                        result = true;
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        responsedateTime = await _res.Content.ReadAsStringAsync();
                        message = _res.Content.ReadAsStringAsync().Result;
                        string respmsg = "";
                        string dateTime = "";
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
                        this.Session["bankSyncTime"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt");
                        dateTime = Session["bankSyncTime"].ToString();
                        this.Session["bankbal"] = respmsg;
                        ViewBag.AvailBankBalnAmount = (string)respmsg;
                        return Json(new { responseCode = responseCode, responseText = respmsg, responsedateTime = dateTime },
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
                            ViewBag.AvailBankBalnAmount = (string)item["StatusMessage"];
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


        public ActionResult WalletQuery()
        {
            string clientCode = (string)Session["LOGGEDUSER_ID"];

            MNBalance availBaln = new MNBalance();
            DataTable dtableUser = AvailBalnUtils.GetAvailBaln(clientCode);
            if (dtableUser != null && dtableUser.Rows.Count > 0)
            {
                string dateTime = "";
                availBaln.amount = dtableUser.Rows[0]["AvailBaln"].ToString();

                ViewBag.AvailBalnAmount = availBaln.amount;

                this.Session["walletSync"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt");
                dateTime = Session["walletSync"].ToString();

                return Json(new { responseCode = 200, responseText = availBaln.amount, responsedateTime = dateTime },
                       JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        public ActionResult GetLnkBankAcc()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                ViewData["userType"] = TempData["userType"];
                ViewBag.UserType = TempData["userType"];
                ViewBag.Name = name;
                string HasBankKYC = string.Empty;
                UserInfo userInfo = new UserInfo();

                //Check Link Bank Account
                DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
                if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
                {
                    userInfo.BankAccountNumber = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                    ViewBag.HasBankKYC = userInfo.BankAccountNumber;
                    HasBankKYC = ViewBag.HasBankKYC;

                }
                return Json(HasBankKYC, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
    }
}