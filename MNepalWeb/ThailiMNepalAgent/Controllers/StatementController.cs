using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Utilities;
using ThailiMNepalAgent.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using ThailiMNepalAgent.App_Start;
using ThailiMNepalAgent.Helper;
using Newtonsoft.Json;
using ThailiMNepalAgent.UserModels;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.Web.Security;

namespace ThailiMNepalAgent.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class StatementController : Controller
    {
        // GET: Statement
        [HttpGet]
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
                ViewBag.UserName = userName;


                // start milayako
                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }
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

                //end milayako


                //string BranchCode = string.Empty;
                //string AccountNumber = string.Empty;
                //UserInfo userobj = new UserInfo();

                //DataTable dtableStatement = StmtUtils.GetWalletORBankACNoByClientCode(clientCode);
                //if (dtableStatement != null && dtableStatement.Rows.Count > 0)
                //{
                //    {
                //        userobj.BranchCode = dtableStatement.Rows[0]["BranchCode"].ToString();
                //        userobj.WalletNumber = dtableStatement.Rows[0]["WalletNumber"].ToString();
                //        BranchCode = userobj.BranchCode;
                //        AccountNumber = userobj.WalletNumber;
                //    }
                //}
                //ViewBag.BranchCode = BranchCode;
                //ViewBag.AccountNumber = AccountNumber;
                //return RedirectToAction("MiniStatement","Statement");




                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        public ActionResult GetAcNo(string txtSourceAccount)
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

                List<UserInfo> CustomerAcNo = new List<UserInfo>();
                UserInfo userobj = new UserInfo();
                string SourceAccount = txtSourceAccount;
                string AccountNumber = string.Empty;
                if (SourceAccount != null)
                {
                    DataTable dtableStatement = StmtUtils.GetWalletORBankACNoByClientCode(clientCode);
                    if (dtableStatement != null && dtableStatement.Rows.Count > 0)
                    {
                        if (SourceAccount == "Wallet")
                        {
                            userobj.WalletNumber = dtableStatement.Rows[0]["WalletNumber"].ToString();
                            AccountNumber = userobj.WalletNumber;
                            CustomerAcNo.Add(userobj);
                        }
                        if (SourceAccount == "Bank")
                        {
                            userobj.BankAccountNumber = dtableStatement.Rows[0]["BankAccountNumber"].ToString();
                            AccountNumber = userobj.BankAccountNumber;
                            CustomerAcNo.Add(userobj);
                        }
                    }
                }
                ViewBag.AccountNumber = AccountNumber;
                return Json(AccountNumber, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpGet]
        public ActionResult StatementDetail(string txtBranchCode, string txtSourceAccount, string txtAccountNo, string txtStartDate, string txtEndDate)
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

                string SourceAccount = txtSourceAccount;
                StatementInfo stmtInfo = new StatementInfo();
                stmtInfo.MainCode = txtAccountNo;
                stmtInfo.StartDate = txtStartDate;
                stmtInfo.EndDate = txtEndDate;
                stmtInfo.BranchCode = txtBranchCode;

                List<StatementInfo> CustomerStatement = new List<StatementInfo>();

                if ((SourceAccount != null) && (stmtInfo.BranchCode != null) && (stmtInfo.BranchCode != "") &&
                    (stmtInfo.MainCode != null) && (stmtInfo.StartDate != null) &&
                    (stmtInfo.MainCode != "") && (stmtInfo.StartDate != "") && (stmtInfo.EndDate != ""))
                {
                    DataTable dtableStatement = StmtUtils.GetStatememt(stmtInfo.MainCode, stmtInfo.StartDate, stmtInfo.EndDate, stmtInfo.BranchCode);
                    if (dtableStatement != null && dtableStatement.Rows.Count > 0)
                    {
                        StatementInfo stmtobj = new StatementInfo();
                        stmtobj.TranDate = dtableStatement.Rows[0]["TranDate"].ToString();
                        stmtobj.Debit = dtableStatement.Rows[0]["Debit"].ToString();
                        stmtobj.Credit = dtableStatement.Rows[0]["Credit"].ToString();
                        stmtobj.Desc1 = dtableStatement.Rows[0]["Desc1"].ToString();
                        stmtobj.Balance = dtableStatement.Rows[0]["Balance"].ToString();

                        CustomerStatement.Add(stmtobj);
                        ViewData["dtableStatement"] = dtableStatement;
                    }
                    else
                    {

                    }
                }
                return View(CustomerStatement);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        //test

      
        [HttpGet]
        public async Task<ActionResult> MiniStatement()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string mobileno = userName;
            HttpResponseMessage _res = new HttpResponseMessage();
            List<MiniStatementVM> MiniStatements = new List<MiniStatementVM>();
            using (HttpClient client = new HttpClient())
            {
                // var uri = "http://27.111.30.126/MNepalApiTest/api/MiniStmtApi/Statement";
                //"http://192.168.6.80/MNepalApi/api/MiniStmtApi/Statement";//
                string action = "MiniStmtApi/Statement";
                var uri = Path.Combine(ApplicationInitilize.APIUrl, action);
                string Parameter = "?mobile=" + mobileno;
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


                _res = client.GetAsync(Parameter).Result;

                string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                _res.ReasonPhrase = responseBody;

                string errorMessage = string.Empty;

                int responseCode = 0;
                string message = string.Empty;
                string responsetext = string.Empty;
                bool result = false;

                if (_res.IsSuccessStatusCode)
                {
                    result = true;
                    responseCode = (int)_res.StatusCode;
                    responsetext = await _res.Content.ReadAsStringAsync();
                    dynamic json = JValue.Parse(responsetext);
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    MiniStatements = ser.Deserialize<List<MiniStatementVM>>(json.ToString());




                }
                return View(MiniStatements);
            }




        }
        
        ///start milayako 01
        [HttpGet]
        public ActionResult IndexNew()
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
              
                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }
                 

                if (this.ViewData["fundTransfer_messsage"] != null)
                {
                    this.ViewData["fundTransfer_messsage"] = this.TempData["fundTransfer_messsage"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                ViewBag.SenderMobileNo = userName;

                int id = TraceIdGenerator.GetID() + 1;
                string stringid = (id).ToString();//this.GetID() + 1
                string traceID = stringid.PadLeft(11, '0') + 'W';
                ViewBag.TraceID = traceID;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        } 

        [HttpGet]
        public async Task<ActionResult> IndexNewOne(StatementInfo _ft)
        { 

            string userName = (string)Session["LOGGED_USERNAME"];

            HttpResponseMessage _res = new HttpResponseMessage();

            string mobileno = _ft.mobileno; //mobile is username
            string StartDate = _ft.StartDate; //mobile is username
            string EndDate = _ft.EndDate; //mobile is username

            List<MiniStatementVM> MiniStatements = new List<MiniStatementVM>();

            using (HttpClient client = new HttpClient())
            {

                var action = "querystmt.svc/StatementByDate";//?mobile=" + mobileno + "&startdate="+ _ft.StartDate + "&enddate=" + _ft.EndDate + "";
                var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                //var content = new FormUrlEncodedContent(new[]{

                //        new KeyValuePair<string, string>("mobileno", _ft.mobileno),
                //        new KeyValuePair<string, string>("startdate", _ft.StartDate),
                //        new KeyValuePair<string, string>("enddate", _ft.EndDate),

                //        }).ToString();
                //_res = await client.GetAsync(new Uri(uri), content);

                string Parameter = "?mobile=" + mobileno + "&startdate=" + _ft.StartDate + "&enddate=" + _ft.EndDate + "";
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    _res = client.GetAsync(Parameter).Result;
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
               
                    if (_res.IsSuccessStatusCode)
                    {
                        result = true;
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        message = _res.Content.ReadAsStringAsync().Result;
                        //string respmsg = "";
                        if (!string.IsNullOrEmpty(message))
                        {
                            JavaScriptSerializer ser = new JavaScriptSerializer();
                            var json = ser.Deserialize<JsonParse>(responsetext);
                            message = json.d;
                            dynamic dyjson = JValue.Parse(responsetext);
                            MiniStatements = ser.Deserialize<List<MiniStatementVM>>(message); 
                        } 

                        return View(MiniStatements);

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
                //this.TempData["fundTransfer_messsage"] = result
                //                                ? "Fund Transfer successfully." + message
                //                                : "ERROR :: " + message;
                //this.TempData["message_class"] = result ? "success_info" : "failed_info";

                //return RedirectToAction("IndexNew", "Statement");
            }


        } 
        //end milayako 01
         
        

        #region Customer Detail Report
        // GET: Report
        public ActionResult CusReportNew()
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
                ViewBag.UserName = userName;
                ///start milayako
                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }
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

                ViewBag.Error = "";
                CustReport para = new CustReport();
                //para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                //para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");


                string sdate = Convert.ToDateTime(DateTime.Now.AddMonths(-1), CultureInfo.GetCultureInfo("en-US")).ToString("dd/MM/yyyy");
                string startdate = sdate.Replace("-", "/");
                para.StartDate = startdate.ToString();

                string edate = Convert.ToDateTime(DateTime.Now.AddDays(-1), CultureInfo.GetCultureInfo("en-US")).ToString("dd/MM/yyyy");
                string enddate = edate.Replace("-", "/");
                para.EndDate = enddate.ToString();




                return View(new CustomerAccountActivityVM
                {
                    Parameter = para,
                    CustomerAccActivity = new List<CustomerAccActivity>(),
                });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        public ContentResult CusReportTableNew(DataTableAjaxPostModel model, string StartDate, string EndDate, string change, string ToExcel)
        {
            string UserName = (string)Session["LOGGED_USERNAME"];

            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            String ParaChanged = "T";
            CustReport ac = new CustReport();
            string convert;
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


            ac.UserName = UserName;
            ac.StartDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); ;
            ac.EndDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ParaChanged = change;

            var result = new List<CustomerAccActivity>();
            if (Session["CustomerAccActivity"] != null && ParaChanged == "F")
            {
                result = Session["CustomerAccActivity"] as List<CustomerAccActivity>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.CustomerAccountActivityNew(ac);
                Session["CustomerAccActivity"] = result;
            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<CustomerAccActivity>(result);
                //ExtraUtility.DataTableToInMemExcel(excel, "TEST.xls");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "CustomerStatement.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<CustomerAccActivity>(model, result, out totalResultsCount, out filteredResultsCount);
            var obj = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.TranId = item.TranId;
                //resultset.Date = item.Date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);//"dd /MM/yyyy", CultureInfo.InvariantCulture);

                resultset.TimeStamp = item.TimeStamp;
                resultset.Desc1 = item.Desc1;
                resultset.Debit = item.Debit;
                resultset.Credit = item.Credit;
                resultset.Balance = item.Balance;
                resultset.Type = "Digital Wallet";
                resultset.Status = item.Status;
                obj.Add(resultset);

            }
            convert = JsonConvert.SerializeObject(new
            {

                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = obj
            });

            return Content(convert, "application/json");
        }

        #endregion


        

        #region Customer Detail Report bank account
        // GET: Report
        public ContentResult CusReportTableNewBnkAcc(DataTableAjaxPostModel model, string StartDate, string EndDate, string change, string ToExcel)
        {
            string UserName = (string)Session["LOGGED_USERNAME"];

            decimal bankbal;
            if (Session["bankbal"] == null)
            {
                bankbal = 0;
            }
            else {
                bankbal = Convert.ToDecimal(Session["bankbal"]);
            }
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            String ParaChanged = "T";
            CustReport ac = new CustReport();
            string convert;
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


            ac.UserName = UserName;
            ac.StartDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); ;
            ac.EndDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ParaChanged = change;

            var result = new List<CustomerAccActivity>();
            if (Session["CustomerAccActivity"] != null && ParaChanged == "F")
            {
                result = Session["CustomerAccActivity"] as List<CustomerAccActivity>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.CustomerAccountActivityNewBnkAcc(ac);
                Session["CustomerAccActivity"] = result;
            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<CustomerAccActivity>(result);
                //ExtraUtility.DataTableToInMemExcel(excel, "TEST.xls");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "CustomerAccActivity.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<CustomerAccActivity>(model, result, out totalResultsCount, out filteredResultsCount);
            var obj = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.TranId = item.TranId;
                //resultset.Date = item.Date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);//"dd /MM/yyyy", CultureInfo.InvariantCulture);
               // resultset.Date = item.Date;
                resultset.TimeStamp = item.TimeStamp;
                resultset.Desc1 = item.Desc1;
                resultset.Debit = item.Debit;
                resultset.Credit = item.Credit;
                resultset.Balnc = bankbal;
                if (item.Debit != 0)
                {
                    bankbal = bankbal + item.Debit;
                }
                else
                {
                    bankbal = bankbal - item.Credit;
                }
                
                resultset.Type = "Bank Account";
                resultset.Status = item.Status;
                obj.Add(resultset);

            }
            convert = JsonConvert.SerializeObject(new
            {

                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = obj
            });

            return Content(convert, "application/json");
        }

        #endregion
        

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

        //////

        public ActionResult logout()
        {

            /*Stamp Logout*/
            MNAdminLog log = new MNAdminLog();
            log.IPAddress = this.Request.UserHostAddress;
            log.URL = this.Request.Url.PathAndQuery;
            log.Message = " ";
            log.Action = "LOGOUT";
            if (Session["UniqueId"] != null)
                log.UniqueId = Session["UniqueId"].ToString() + "|" + HttpContext.Session.SessionID;
            else
                log.UniqueId = HttpContext.Session.SessionID;

            if (Session["UserBranch"] != null)
                log.Branch = Session["UserBranch"].ToString();
            else
                log.Branch = "000";

            if (Session["UserName"] != null)
                log.UserId = Session["UserName"].ToString();
            else
                log.UserId = "IIS";

            if (Session["LOGGED_USERTYPE"] != null)
                log.UserType = Session["LOGGED_USERTYPE"].ToString();
            else
                log.UserType = "SERVER";

            log.TimeStamp = DateTime.Now;
            LoginUtils.LogAction(log);

            if (Session["UserName"] != null)
                LoginUtils.LogOutUser(Session["UserName"].ToString());


            //Clear Cookie
            FormsAuthentication.SignOut();
            Session.Abandon();//Clear the session
            return RedirectToAction("Index", "Login");
        }




        [HttpGet]
        public void Download(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                DataTable result = TempData[fileGuid] as DataTable;
                ExtraUtility.ExportDataTableToExcel(result, fileName);
            }
            else
            {
                Response.Write("Bad Request");
            }
        }

        /////
    }
}