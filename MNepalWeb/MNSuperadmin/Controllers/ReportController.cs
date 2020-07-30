using MNSuperadmin.Models;
using MNSuperadmin.UserModels;
using MNSuperadmin.Utilities;
using MNSuperadmin.Helper;
using MNSuperadmin.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Text.RegularExpressions;

namespace MNSuperadmin.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class ReportController : Controller
    {


        #region Super Admin Activities
        [HttpGet]
        public ActionResult SuperAdminActivityReport()
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
                ViewBag.COC = Session["COC"];
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        public ContentResult AdminActivityTable(DataTableAjaxPostModel model, string UserName, string BranchCode, string StartDate, string EndDate, string change, string ToExcel)
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
            var result = new List<MNAdminActivity>();
            if (Session["AdminActivity"] != null && ParaChanged == "F")
            {
                result = Session["AdminActivity"] as List<MNAdminActivity>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.GetSuperAdminActivity(UserName, BranchCode, StartDate, EndDate);
                Session["AdminActivity"] = result;

            }
            if (ToExcel == "T")
            {

                DataTable excel = ToDataTable<MNAdminActivity>(result);
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
            var res = FilterAndSort<MNAdminActivity>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                resultset.UserName = item.UserName;
                resultset.Name = item.Name;
                resultset.Role = item.Role;
                resultset.BranchCode = item.BranchCode;
                resultset.Description = item.Description;
                resultset.Code = item.Code;
                resultset.Remarks = string.Join(" ", item.Remarks, item.Description);
                resultset.TimeStamp = item.TimeStamp;
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
        #endregion

        #region Customer Login Activities
        [HttpGet]
        public ActionResult CustomerLoginActivities()
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
                ViewBag.COC = Session["COC"];
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        public ContentResult CustomerLoginActivityTable(DataTableAjaxPostModel model, string UserName, string BranchCode, string StartDate, string EndDate, string change, string ToExcel)
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
            var result = new List<CustomerLoginLog>();
            if (Session["CustomerLoginLog"] != null && ParaChanged == "F")
            {
                result = Session["CustomerLoginLog"] as List<CustomerLoginLog>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.GetCustomerLoginActivities(UserName, StartDate, EndDate);
                Session["CustomerLoginLog"] = result;

            }
            if (ToExcel == "T")
            {

                DataTable excel = ToDataTable<CustomerLoginLog>(result);
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
            var res = FilterAndSort<CustomerLoginLog>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                resultset.UserName = item.UserName;
                resultset.Description = item.Description;
                resultset.SNo = item.SNo;
                resultset.Status = item.Status;
                resultset.Date = item.Date;
                resultset.PrivateIP = item.PrivateIP;
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
        #endregion

        #region View Report RegisteredAgents
        // GET: Report/RegisteredAgents
        [HttpGet]
        public ActionResult RegisteredAgents()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end

            //string Parameter;
            //Parameter.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
            //Parameter.EndDate = DateTime.Now.ToString("dd/MM/yyyy");

            ViewBag.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.EndDate = DateTime.Now.ToString("dd/MM/yyyy");



            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpGet]
        public ActionResult AgentViewDetail(string StartDate, string EndDate, string MobileNo)
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
                ViewBag.Message = "No Result Found !!";
                UserInfo userInfo = new UserInfo();
                userInfo.ContactNumber1 = MobileNo;
                userInfo.StartDate = StartDate;
                userInfo.EndDate = EndDate;

                if (!string.IsNullOrEmpty(StartDate))
                {

                    userInfo.StartDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture); ;
                }
                if (!string.IsNullOrEmpty(EndDate))
                {

                    userInfo.EndDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                               .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture); ;
                }

                List<UserInfo> AgentStatus = new List<UserInfo>();


                //if ((userInfo.ContactNumber1 != "") && (SearchCol == "Mobile Number"))
                //{
                DataTable dtableAgentStatusByMobileNo = ReportUserModel.GetAgentDetail(userInfo.ContactNumber1, userInfo.StartDate, userInfo.EndDate);
                if (dtableAgentStatusByMobileNo != null && dtableAgentStatusByMobileNo.Rows.Count > 0)
                {
                    UserInfo regobj = new UserInfo();
                    regobj.ClientCode = dtableAgentStatusByMobileNo.Rows[0]["ClientCode"].ToString();
                    regobj.UserName = dtableAgentStatusByMobileNo.Rows[0]["AgentName"].ToString();
                    //regobj.Address = dtableAgentStatusByMobileNo.Rows[0]["Location"].ToString();

                    regobj.Status = dtableAgentStatusByMobileNo.Rows[0]["Status"].ToString();
                    regobj.ContactNumber1 = dtableAgentStatusByMobileNo.Rows[0]["AgentMobileNo"].ToString();
                    //regobj.GoodBaln = dtableAgentStatusByMobileNo.Rows[0]["Balance"].ToString();

                    regobj.HasKYC = dtableAgentStatusByMobileNo.Rows[0]["KYCstatus"].ToString();
                    regobj.CreatedDate = dtableAgentStatusByMobileNo.Rows[0]["CreatedDate"].ToString();
                    regobj.CreatedBy = dtableAgentStatusByMobileNo.Rows[0]["RegisteredBy"].ToString();
                    regobj.ApprovedBy = dtableAgentStatusByMobileNo.Rows[0]["ApprovedBy"].ToString();

                    AgentStatus.Add(regobj);
                    ViewData["dtableAgentStatus"] = dtableAgentStatusByMobileNo;

                }

                return View(AgentStatus);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }






        //public ActionResult ViewAgentDetails(string MobileNo, string StartDate, string EndDate)
        //{
        //    string userName = (string)Session["LOGGED_USERNAME"];
        //    string clientCode = (string)Session["LOGGEDUSER_ID"];
        //    string name = (string)Session["LOGGEDUSER_NAME"];
        //    string userType = (string)Session["LOGGED_USERTYPE"];

        //    TempData["userType"] = userType;

        //    if (TempData["userType"] != null)
        //    {
        //        this.ViewData["userType"] = this.TempData["userType"];
        //        ViewBag.UserType = this.TempData["userType"];
        //        ViewBag.Name = name;

        //        AgentProfileInfo agentobj = new AgentProfileInfo();
        //        DataTable dtableAgentByMobileNo = ReportUserModel.GetAgentDetail(MobileNo, StartDate, EndDate);
        //        if (dtableAgentByMobileNo.Rows.Count == 1)
        //        {

        //            agentobj.ClientCode = dtableAgentByMobileNo.Rows[0]["ClientCode"].ToString();
        //            agentobj.AgentName = dtableAgentByMobileNo.Rows[0]["AgentName"].ToString();
        //            agentobj.Location = dtableAgentByMobileNo.Rows[0]["Location"].ToString();
        //            agentobj.Status = dtableAgentByMobileNo.Rows[0]["Status"].ToString();
        //            agentobj.AgentMobileNo = dtableAgentByMobileNo.Rows[0]["AgentMobileNo"].ToString();
        //            agentobj.Balance = dtableAgentByMobileNo.Rows[0]["Balance"].ToString();
        //            agentobj.IsApproved = dtableAgentByMobileNo.Rows[0]["IsApproved"].ToString();
        //            agentobj.WalletNumber = dtableAgentByMobileNo.Rows[0]["WalletNumber"].ToString();


        //        }
        //        return View(agentobj);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //}

        #endregion

        #region Report FinancialReport
        public ActionResult FinancialReport()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                ViewBag.Error = "";
                FinancialReport para = new FinancialReport();
                para.MobileNumber = "";
                return View(new FinancialReportVM
                {
                    Parameter = para,
                    FinancialReportActivity = new List<FinancialReportActivity>(),
                });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ContentResult FinancialReportTable(DataTableAjaxPostModel model, string ContactNumber1, string change, string ToExcel)
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

            FinancialReport fr = new FinancialReport();
            fr.MobileNumber = ContactNumber1;
            ParaChanged = change;

            var result = new List<FinancialReportActivity>();
            if (Session["FinancialReportActivity"] != null && ParaChanged == "F")
            {
                result = Session["FinancialReportActivity"] as List<FinancialReportActivity>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.GetFinancialReportActivity(fr);
                Session["FinancialReportActivity"] = result;

            }
            if (ToExcel == "T")
            {

                DataTable excel = ToDataTable<FinancialReportActivity>(result);
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_FinancialReport.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<FinancialReportActivity>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultListobj = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                resultset.Date = item.Date;
                resultset.CustomerName = item.CustomerName;
                resultset.MobileNumber = item.MobileNumber;
                resultset.TargetNumber = item.TargetNumber;
                resultset.TranID = item.TranID;
                resultset.Amount = item.Amount.ToString();
                //resultset.Amount = String.Format("{0:0.00}", item.Amount);
                resultset.TransactionType = item.TransactionType;
                resultset.Status = item.Status;
                resultset.Message = item.Message;


                resultListobj.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultListobj
            });
            return Content(convert, "application/json");

        }

        #endregion

        #region Report NonFinancialReport
        public ActionResult NonFinancialReportView()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ContentResult NonFinancialReportTable(DataTableAjaxPostModel model, string ContactNumber1, string change, string ToExcel)
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

            ParaChanged = change;
            var result = new List<NonFinancialReport>();
            if (Session["NonFinancialReport"] != null && ParaChanged == "F")
            {
                result = Session["NonFinancialReport"] as List<NonFinancialReport>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.GetNonFinancialReport(ContactNumber1);
                Session["NonFinancialReport"] = result;

            }
            if (ToExcel == "T")
            {

                DataTable excel = ToDataTable<NonFinancialReport>(result);
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_NonFinancialReport.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<NonFinancialReport>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                resultset.Date = item.Date;
                resultset.CustomerName = item.CustomerName;
                resultset.MobileNumber = item.MobileNumber;
                resultset.TransID = item.TransID;
                resultset.TransactionType = item.TransactionType;
                resultset.Status = item.Status;

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

        #endregion

        //#region Report TransactionReportOld
        //public ActionResult TransactionReport()
        //{
        //    string userName = (string)Session["LOGGED_USERNAME"];
        //    string clientCode = (string)Session["LOGGEDUSER_ID"];
        //    string name = (string)Session["LOGGEDUSER_NAME"];
        //    string userType = (string)Session["LOGGED_USERTYPE"];

        //    TempData["userType"] = userType;
        //    //Check Role link start    
        //    RoleChecker roleChecker = new RoleChecker();
        //    bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
        //    //Check Role link end


        //    if (TempData["userType"] != null&& checkRole)
        //    {
        //        this.ViewData["userType"] = this.TempData["userType"];
        //        ViewBag.UserType = this.TempData["userType"];
        //        ViewBag.Name = name;

        //        return View();
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //}

        //[HttpPost]
        //public ContentResult TransactionReportTable(DataTableAjaxPostModel model, string ContactNumber1, string change, string ToExcel)
        //{
        //    if (!Request.IsAjaxRequest())
        //    {
        //        Response.Write("Invalid Execution");
        //        return Content("");
        //    }

        //    int filteredResultsCount;
        //    int totalResultsCount;
        //    string ParaChanged = "T";
        //    string convert;

        //    ParaChanged = change;
        //    var result = new List<TransactionReport>();
        //    if (Session["TranscationReport"] != null && ParaChanged == "F")
        //    {
        //        result = Session["TransactionReport"] as List<TransactionReport>;
        //    }
        //    else
        //    {
        //        ReportUserModel rep = new ReportUserModel();
        //        result = rep.GetTransactionReport(ContactNumber1);
        //        Session["TranscationReport"] = result;

        //    }
        //    if (ToExcel == "T")
        //    {

        //        DataTable excel = ToDataTable<TransactionReport>(result);
        //        string Handler = Guid.NewGuid().ToString();
        //        TempData[Handler] = excel;
        //        string FileName = DateTime.Now + "TranscationReport.xls";
        //        convert = JsonConvert.SerializeObject(new
        //        {
        //            FileGuid = Handler,
        //            FileName = FileName
        //        });
        //        return Content(convert, "application/json");
        //    }
        //    var res = FilterAndSort<TransactionReport>(model, result, out totalResultsCount, out filteredResultsCount);
        //    var resultList = new List<dynamic>();
        //    foreach (var item in res)
        //    {
        //        dynamic resultset = new ExpandoObject();

        //        resultset.Date = item.Date;
        //        resultset.AgentName = item.AgentName;
        //        resultset.AgentMobileNumber = item.AgentMobileNumber;
        //        resultset.Services = item.Services;
        //        resultset.TransactionCount = item.TransactionCount;
        //        resultset.CommissionEarned = item.CommissionEarned;
        //        resultset.Status = item.Status;

        //        resultList.Add(resultset);
        //    }
        //    convert = JsonConvert.SerializeObject(new
        //    {
        //        draw = model.draw,
        //        recordsTotal = totalResultsCount,
        //        recordsFiltered = filteredResultsCount,
        //        data = resultList
        //    });
        //    return Content(convert, "application/json");

        //}

        //#endregion

        #region Customer Log
        public ActionResult CustomerAccountLog()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;
            //Check Role link start    
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(System.Reflection.MethodBase.GetCurrentMethod().Name, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.Error = "";
                ViewBag.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                ViewBag.EndDate = DateTime.Now.ToString("dd/MM/yyyy");


                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        public ContentResult CustomerAccountLogTable(DataTableAjaxPostModel model, string UserName, string StartDate, string EndDate, string change, string ToExcel)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            string ParaChanged = "T";
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
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture); ;
            ac.EndDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            ParaChanged = change;
            var result = new List<CustomerLog>();
            if (Session["CustomerLog"] != null && ParaChanged == "F")
            {
                result = Session["CustomerLog"] as List<CustomerLog>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.CustomerAccountLog(ac);
                Session["CustomerLog"] = result;
            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<CustomerLog>(result);
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_CustomerAccountLog.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<CustomerLog>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.TranDate = item.TranDate.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.ServiceType = item.ServiceType;
                resultset.Amount = item.Amount;
                resultset.Source = item.Source;
                resultset.Destination = item.Destination;
                resultset.DestinationAccount = item.DestinationAccount.Trim() == "" ? "~~~" : item.DestinationAccount;
                resultset.Reciver = item.Reciever.Trim() == "" ? "~~~" : item.Reciever;
                resultset.Description = item.Description;
                resultset.ResponseCode = item.ResponseCode;
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

        #endregion

        #region Summary
        [HttpGet]
        public ActionResult Summary()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.MType = "";

                MerchantPay para = new MerchantPay();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                ReportUserModel rep = new ReportUserModel();
                para.MerchantTypeList = rep.GetMerchantsType();

                ViewBag.MerchantCategory = MerchantUtils.GetMerchantsType();
                return View(new MerchantVM
                {
                    Parameter = para,
                    MerchantInfo = new List<MerchantInfo>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        [HttpPost]
        public ActionResult Summary(MerchantVM model)
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
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay(),
                        MerchantInfo = new List<MerchantInfo>()

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
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay { MerchantTypeList = new List<ViewModel.Merchants>() },
                        MerchantInfo = new List<MerchantInfo>()

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
            List<MerchantInfo> report = rep.SummaryDetails(model.Parameter);
            MerchantVM vm = new MerchantVM();
            vm.MerchantInfo = report;
            vm.Parameter = model.Parameter;
            return View(vm);


        }

        static readonly string[] Months = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        public ContentResult SummaryTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string Service, string MerchantName, string GroupBy, string change, string ToExcel)
        {
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
            MerchantPay ac = new MerchantPay();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            ac.Service = Service;
            ac.MerchantName = MerchantName;
            ac.GrpByDate = GroupBy;
            var result = new List<MerchantInfo>();
            if (Session["Summary"] != null && ParaChanged == "F")
            {
                result = Session["Summary"] as List<MerchantInfo>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.SummaryDetails(ac);
                Session["Summary"] = result;

            }
            if (ToExcel == "T")
            {

                List<MerchantInfo> modifiedMI = result.ToList();
                foreach (MerchantInfo item in result)
                {
                    int i = int.Parse(item.SalesMonth.Trim() == "" ? "0" : item.SalesMonth.Trim());
                    if (item.SalesMonth.Length <= 2 && item.SalesMonth.Length > 0)
                        item.SalesMonth = Months[i - 1];
                }

                DataTable excel = ToDataTable<MerchantInfo>(result);
                excel.Columns.Remove("DatenTime");
                excel.Columns.Remove("Amount");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_Summary.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<MerchantInfo>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.MerchantName = item.MerchantName;
                resultset.ServiceType = item.ServiceType;
                resultset.TotalAmount = item.TotalAmount;
                resultset.NoOfTran = item.NoOfTran;
                resultset.SalesYear = item.SalesYear;
                resultset.SalesMonth = item.SalesMonth;
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList,
                GroupedBy = ac.GrpByDate,
                StartDate = ac.StartDate,
                EndDate = ac.EndDate
            });
            return Content(convert, "application/json");

        }

        //for service type
        public string LoadDropDownMerchants(string value)
        {
            bool Where = false;
            if (value != "")
                Where = true;
            ReportUserModel rep = new ReportUserModel();
            string ddl = rep.GetMerchantsFromType(value, Where);
            return ddl;
        }
        #endregion

        #region Summary Details



        [HttpGet]
        public ActionResult SummaryDetails(string service, string startdate, string enddate, string mercname, string smonth, string syear, string groupby)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

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

                MerchantPay para = new MerchantPay();
                //para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                //para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                //ReportUserModel rep = new ReportUserModel();
                //para.MerchantTypeList = rep.GetMerchantsType();


                //if (!string.IsNullOrEmpty(startdate))
                //    {
                //    startdate = DateTime.ParseExact(startdate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                //                                 .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                //    }


                //if (!string.IsNullOrEmpty(enddate))
                //{
                //    enddate = DateTime.ParseExact(enddate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                //                                 .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                //}
                para.StartDate = startdate;
                para.EndDate = enddate;
                para.Service = service;
                para.MerchantName = mercname;
                para.SalesMonth = smonth;
                para.SalesYear = syear;
                para.GrpByDate = groupby;
                ReportUserModel rep = new ReportUserModel();
                List<MerchantInfo> report = rep.SummaryDetailList(para);
                return View(report);

            }
            else
            {
                return Content("Your session has expired. Please re-login and retry.");
            }

        }



        #endregion

        #region QueryExecutor
        [HttpGet]
        public ActionResult QueryExecutor()
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
                ExecuteQuery Model = new ExecuteQuery();
                Model.Data = new DataTable();
                Model.TimeOut = "600";
                return View(Model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QueryExecutor(ExecuteQuery model)
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


                string Query = model.Query.Trim();
                model.Data = new DataTable();

                if (string.IsNullOrEmpty(Query))
                {
                    model.Message = "Empty Query String";
                    return View(model);
                }

                string tempSql = Query.Replace('\"', '\'');

                string pattern = @"\bcreate\b";
                string patterni = @"\bCREATE TABLE #";
                string pattern1 = @"\bdelete\b";
                string pattern2 = @"\bDROP\b";
                string pattern2i = @"\bDROP TABLE #";
                string pattern3 = @"\btruncate\b";
                string pattern4 = @"\binsert\b";
                string pattern4i = @"\/\* Insert";
                string pattern4j = @"\bINSERT INTO #";
                string pattern5 = @"\bupdate\b";
                string pattern6 = @"\exec\b";
                string pattern6i = @"\execute\b";


                Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);
                Regex regi = new Regex(patterni, RegexOptions.IgnoreCase);
                Regex reg1 = new Regex(pattern1, RegexOptions.IgnoreCase);
                Regex reg2 = new Regex(pattern2, RegexOptions.IgnoreCase);
                Regex reg2i = new Regex(pattern2i, RegexOptions.IgnoreCase);
                Regex reg3 = new Regex(pattern3, RegexOptions.IgnoreCase);
                Regex reg4 = new Regex(pattern4, RegexOptions.IgnoreCase);
                Regex reg4i = new Regex(pattern4i, RegexOptions.IgnoreCase);
                Regex reg4j = new Regex(pattern4j, RegexOptions.IgnoreCase);
                Regex reg5 = new Regex(pattern5, RegexOptions.IgnoreCase);

                Regex reg6 = new Regex(pattern6, RegexOptions.IgnoreCase);
                Regex reg6i = new Regex(pattern6i, RegexOptions.IgnoreCase);

                Match m1, m1i, m2, m3, m3i, m4, m5, m5i, m5j, m6, mexec, mexecute;
                m1 = reg.Match(tempSql);
                m1i = regi.Match(tempSql);
                m2 = reg1.Match(tempSql);
                m3 = reg2.Match(tempSql);//DROP TABLE
                m3i = reg2i.Match(tempSql);//DROP TABLE #TEMP
                m4 = reg3.Match(tempSql);
                m5 = reg4.Match(tempSql);//insert
                m5i = reg4i.Match(tempSql);//* Insert;
                m5j = reg4j.Match(tempSql);//Insert into Temp
                m6 = reg5.Match(tempSql);

                mexec = reg6.Match(tempSql);
                mexecute = reg6i.Match(tempSql);

                if (tempSql.StartsWith("exec") || tempSql.StartsWith("execute"))
                {
                    model.Message = "Cannot execute stored procedure";
                    return View(model);
                }


                if (m2.Success || m4.Success || m6.Success)
                {
                    model.Message = "Empty Query String";
                    return View(model);
                }
                if (m1.Success)//For Controlling Create
                {
                    if (!(m1i.Success))
                    {
                        model.Message = "Cannot execute Create Query";
                        return View(model);
                    }
                }
                if (m5.Success)//For Controlling Insert
                {
                    if (!(m5i.Success || m5j.Success))
                    {
                        model.Message = "Cannot execute Insert Query";
                        return View(model);
                    }
                }
                if (m3.Success)//For Controlling Drop
                {
                    if (!m3i.Success)
                    {
                        model.Message = "Cannot execute Drop Query";
                        return View(model);
                    }
                }
                if (String.IsNullOrEmpty(model.TimeOut.Trim()))
                {
                    model.TimeOut = "600";
                }
                ReportUserModel rep = new ReportUserModel();
                int Timeout = 0;
                if (!int.TryParse(model.TimeOut, out Timeout))
                {
                    Timeout = 600;
                }
                var dt = rep.ExecuteQuery(Query, Timeout);
                model.Data = dt;
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion

        #region TransactionReport
        [HttpGet]
        public ActionResult TransactionReport()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.MType = "";

                MerchantPay para = new MerchantPay();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                ReportUserModel rep = new ReportUserModel();
                para.MerchantTypeList = rep.GetMerchantsType();

                ViewBag.MerchantCategory = MerchantUtils.GetServiceType();
                return View(new MerchantVM
                {
                    Parameter = para,
                    MerchantInfo = new List<MerchantInfo>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        [HttpPost]
        public ActionResult TransactionReport(MerchantVM model)
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
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay(),
                        MerchantInfo = new List<MerchantInfo>()

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
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay { MerchantTypeList = new List<ViewModel.Merchants>() },
                        MerchantInfo = new List<MerchantInfo>()

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
            List<MerchantInfo> report = rep.SummaryDetails(model.Parameter);
            MerchantVM vm = new MerchantVM();
            vm.MerchantInfo = report;
            vm.Parameter = model.Parameter;
            return View(vm);


        }

        public ContentResult TransactionTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string Service, string ContactNumber1, string GroupBy, string change, string ToExcel)
        {
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
            MerchantPay ac = new MerchantPay();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            ac.Service = Service;
            ac.MerchantName = "";
            ac.GrpByDate = GroupBy;
            var result = new List<TransactionReport>();
            if (Session["TransactionReport"] != null && ParaChanged == "F")
            {
                result = Session["TransactionReport"] as List<TransactionReport>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.GetTransactionReport(ContactNumber1, ac);
                Session["TransactionReport"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<TransactionReport>(result);
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "TranscationReport.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<TransactionReport>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                resultset.Date = item.Date;
                resultset.AgentName = item.AgentName;
                resultset.AgentMobileNumber = item.AgentMobileNumber;
                resultset.Services = item.Services;
                resultset.TransactionCount = item.TransactionCount;
                resultset.CommissionEarned = item.CommissionEarned;
                resultset.Status = item.Status;

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


        #endregion

        #region Merchant Payment
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult MerchantPay()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.MType = "";

                MerchantPay para = new MerchantPay();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                ReportUserModel rep = new ReportUserModel();
                para.MerchantTypeList = rep.GetMerchantsType();
                return View(new MerchantVM
                {
                    Parameter = para,
                    MerchantInfo = new List<MerchantInfo>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        public ContentResult MerchantPaymentTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string MerchantType, string MerchantName, string Status, string change, string ToExcel)
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
            MerchantPay ac = new MerchantPay();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            ac.SourceMobileNo = SourceMobileNo;
            ac.MerchantType = MerchantType;
            ac.MerchantName = MerchantName;
            ac.Status = Status;
            var result = new List<MerchantInfo>();
            if (Session["MerchantPayment"] != null && ParaChanged == "F")
            {
                result = Session["MerchantPayment"] as List<MerchantInfo>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.MerchantDetails(ac);
                Session["MerchantPayment"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<MerchantInfo>(result);
                excel.Columns.Remove("TotalAmount");
                excel.Columns.Remove("NoOfTran");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_MerchantPayment.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            decimal SumAmount = result.Sum(x => x.Amount);
            var res = FilterAndSort<MerchantInfo>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.DatenTime = item.DatenTime.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.TxnID = item.TxnID;
                resultset.ReferenceNo = item.ReferenceNo;
                resultset.InitMobileNo = item.InitMobileNo;
                resultset.MerchantType = item.MerchantType;
                resultset.MerchantName = item.MerchantName;
                resultset.Amount = item.Amount;
                resultset.Status = item.Status;
                resultset.Message = item.Message;
                resultset.TranType = item.TranType;
                resultset.Name = item.Name;
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList,
                Sum = SumAmount
            });

            return Content(convert, "application/json");

        }

        [HttpPost]
        public ActionResult MerchantPay(MerchantVM model)
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
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay(),
                        MerchantInfo = new List<MerchantInfo>()

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
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay { MerchantTypeList = new List<ViewModel.Merchants>() },
                        MerchantInfo = new List<MerchantInfo>()

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
            List<MerchantInfo> report = rep.MerchantDetails(model.Parameter);
            MerchantVM vm = new MerchantVM();
            vm.MerchantInfo = report;
            MerchantPay pay = new MerchantPay();
            pay.MerchantTypeList = rep.GetMerchantsType();
            pay.StartDate = model.Parameter.StartDate;
            pay.EndDate = model.Parameter.EndDate;
            pay.MerchantName = model.Parameter.MerchantName;
            pay.MerchantType = model.Parameter.MerchantType;
            pay.Status = model.Parameter.Status;
            vm.Parameter = pay;
            return View(vm);


        }

        public string LoadDropDownMerchantsPaypoint(string value)
        {
            bool Where = false;
            if (value != "")
                Where = true;
            ReportUserModel rep = new ReportUserModel();
            string ddl = rep.GetMerchantsFromTypeMerchant(value, Where);
            return ddl;
        }

        #endregion

        #region Fund Transfer 
        [HttpGet]

        public ActionResult FundTranRep()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                TopUp para = new TopUp();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                return View(new FundTxnVm
                {
                    Parameter = para,
                    FundTransfer = new List<FundTransfer>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }
        public ContentResult FundTransferTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string TranId, string SourceMobileNo, string DestMobileNo, string FundTfrType, string Status, string change, string ToExcel)
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
            TopUp ac = new TopUp();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            ac.TranID = TranId;
            ac.DestMobileNo = DestMobileNo;
            ac.SourceMobileNo = SourceMobileNo;
            ac.FTType = FundTfrType;
            ac.Status = Status;
            var result = new List<FundTransfer>();
            if (Session["FundTransfer"] != null && ParaChanged == "F")
            {
                result = Session["FundTransfer"] as List<FundTransfer>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.FundTxnDetails(ac);
                Session["FundTransfer"] = result;
            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<FundTransfer>(result);
                //ExtraUtility.DataTableToInMemExcel(excel, "TEST.xls");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_FundTransferReport.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            decimal SumAmount = result.Sum(x => x.Amount);
            var res = FilterAndSort<FundTransfer>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.DatenTime = item.DatenTime.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.TxnID = item.TxnID;
                resultset.FTType = item.FTType;
                resultset.Source = item.SourceMobileNo;
                resultset.Destination = item.DestMobileNo;
                resultset.Amount = item.Amount;
                resultset.Status = item.Status;
                resultset.Message = item.Message;
                resultset.ReferenceNo = item.ReferenceNo;
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList,
                Sum = SumAmount
            });
            return Content(convert, "application/json");

        }
        [HttpPost]
        public ActionResult FundTranRep(FundTxnVm model)
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
                    return View(new FundTxnVm
                    {
                        Parameter = new TopUp(),
                        FundTransfer = new List<FundTransfer>()

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
                    return View(new TopUpRepVM
                    {
                        Parameter = new TopUp(),
                        TopUpInfo = new List<TopUpInfo>()

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
            List<FundTransfer> report = rep.FundTxnDetails(model.Parameter);
            FundTxnVm vm = new FundTxnVm();
            vm.FundTransfer = report;
            vm.Parameter = model.Parameter;
            return View(vm);

        }

        #endregion

        #region Fund Transfer EBanking
        [HttpGet]

        public ActionResult FundTranEBankingRep()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                TopUp para = new TopUp();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                return View(new FundTxnVm
                {
                    Parameter = para,
                    FundTransfer = new List<FundTransfer>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }
        public ContentResult FundTransferEBankingTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string TranId, string SourceMobileNo, string DestMobileNo, string FundTfrType, string Status, string change, string ToExcel)
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
            TopUp ac = new TopUp();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            ac.TranID = TranId;
            ac.DestMobileNo = DestMobileNo;
            ac.SourceMobileNo = SourceMobileNo;
            ac.FTType = FundTfrType;
            ac.Status = Status;
            var result = new List<FundTransfer>();
            if (Session["FundTransfer"] != null && ParaChanged == "F")
            {
                result = Session["FundTransfer"] as List<FundTransfer>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.FundTxnEBankingDetails(ac);
                Session["FundTransfer"] = result;
            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<FundTransfer>(result);
                //ExtraUtility.DataTableToInMemExcel(excel, "TEST.xls");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_FundTransferReport.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            decimal SumAmount = result.Sum(x => x.Amount);
            var res = FilterAndSort<FundTransfer>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.DatenTime = item.DatenTime.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.TxnID = item.TxnID;
                resultset.FTType = item.FTType;
                resultset.Source = item.SourceMobileNo;
                resultset.Destination = item.DestMobileNo;
                resultset.Amount = item.Amount;
                resultset.Status = item.Status;
                resultset.Message = item.Message;
                resultset.PaymentReferenceNumber = item.PaymentReferenceNumber;
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList,
                Sum = SumAmount
            });
            return Content(convert, "application/json");

        }
        [HttpPost]
        public ActionResult FundTranEBankingRep(FundTxnVm model)
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
                    return View(new FundTxnVm
                    {
                        Parameter = new TopUp(),
                        FundTransfer = new List<FundTransfer>()

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
                    return View(new TopUpRepVM
                    {
                        Parameter = new TopUp(),
                        TopUpInfo = new List<TopUpInfo>()

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
            List<FundTransfer> report = rep.FundTxnDetails(model.Parameter);
            FundTxnVm vm = new FundTxnVm();
            vm.FundTransfer = report;
            vm.Parameter = model.Parameter;
            return View(vm);

        }

        #endregion

        #region TopUp Report
        [HttpGet]
        public ActionResult TopUpRep()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                TopUp para = new TopUp();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                ReportUserModel rep = new ReportUserModel();
                ViewBag.MerchantList = rep.GetMerchantbyCategory("1"); //Topup=1
                return View(new TopUpRepVM
                {
                    Parameter = para,
                    TopUpInfo = new List<TopUpInfo>()

                });

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        public ContentResult TopUpTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string RequestType, string DestinationMobile, string TranId, string Status, string change, string ToExcel)
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
            TopUp ac = new TopUp();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            ac.SourceMobileNo = SourceMobileNo;
            ac.RequestType = RequestType;
            ac.DestMobileNo = DestinationMobile;
            ac.TranID = TranId;
            ac.Status = Status;
            var result = new List<TopUpInfo>();
            if (Session["TopUp"] != null && ParaChanged == "F")
            {
                result = Session["TopUp"] as List<TopUpInfo>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.TopUpDetails(ac);
                Session["TopUp"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<TopUpInfo>(result);
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_TopUp.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            decimal SumAmount = result.Sum(x => x.Amount);
            var res = FilterAndSort<TopUpInfo>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.DatenTime = item.DatenTime.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.TxnID = item.TxnID;
                resultset.InitMobileNo = item.InitMobileNo;
                resultset.DestMobileNo = item.DestMobileNo;
                resultset.ServiceType = item.ServiceType;
                resultset.Amount = item.Amount;
                resultset.Status = item.Status;
                resultset.Message = item.Message;
                resultset.ReferenceNo = item.ReferenceNo;
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList,
                Sum = SumAmount

            });
            return Content(convert, "application/json");

        }

        [HttpPost]
        public ActionResult TopUpRep(TopUpRepVM model)
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
                    return View(new TopUpRepVM
                    {
                        Parameter = new TopUp(),
                        TopUpInfo = new List<TopUpInfo>()

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
                    return View(new TopUpRepVM
                    {
                        Parameter = new TopUp(),
                        TopUpInfo = new List<TopUpInfo>()

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
            List<TopUpInfo> report = rep.TopUpDetails(model.Parameter);
            TopUpRepVM vm = new TopUpRepVM();
            vm.TopUpInfo = report;
            vm.Parameter = model.Parameter;
            return View(vm);


        }

        #endregion

        #region EBanking Cash Load 
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult EBankingCashLoad()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.MType = "";

                MerchantPay para = new MerchantPay();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                ReportUserModel rep = new ReportUserModel();
                para.MerchantTypeList = rep.GetMerchantsType();
                return View(new MerchantVM
                {
                    Parameter = para,
                    MerchantInfo = new List<MerchantInfo>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        public ContentResult EBankingCashLoadTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string change, string ToExcel)
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
            MerchantPay ac = new MerchantPay();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            var result = new List<EBankingTran>();
            if (Session["MerchantPayment"] != null && ParaChanged == "F")
            {
                result = Session["MerchantPayment"] as List<EBankingTran>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                //result = rep.EBankingTranDetails(ac);
                result = rep.EBankingCashLoad(ac);
                Session["MerchantPayment"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<EBankingTran>(result);
                //excel.Columns.Remove("TotalAmount");
                //excel.Columns.Remove("NoOfTran");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_EBankingCashLoadReport.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            decimal SumAmount = result.Sum(x => x.Amount);
            var res = FilterAndSort<EBankingTran>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                //resultset.EBDate = item.EBDate.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.EID = item.EID;
                resultset.ClientCode = item.ClientCode;
                resultset.PaymentReferenceNumber = item.PaymentReferenceNumber;
                resultset.ItemCode = item.ItemCode;
                resultset.Amount = item.Amount;
                DateTime date = Convert.ToDateTime(item.EBDate, CultureInfo.InvariantCulture);
                resultset.EBDate = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.ReferenceNo = item.ReferenceNo;
                resultset.Status = item.Status;

                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList,
                Sum = SumAmount
            });

            return Content(convert, "application/json");

        }


        [HttpPost]
        public ActionResult EBankingCashLoad(MerchantVM model)
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
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay(),
                        MerchantInfo = new List<MerchantInfo>()

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
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay { MerchantTypeList = new List<ViewModel.Merchants>() },
                        MerchantInfo = new List<MerchantInfo>()

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
            List<MerchantInfo> report = rep.MerchantDetails(model.Parameter);
            MerchantVM vm = new MerchantVM();
            vm.MerchantInfo = report;
            MerchantPay pay = new MerchantPay();
            pay.MerchantTypeList = rep.GetMerchantsType();
            pay.StartDate = model.Parameter.StartDate;
            pay.EndDate = model.Parameter.EndDate;
            pay.MerchantName = model.Parameter.MerchantName;
            pay.MerchantType = model.Parameter.MerchantType;
            pay.Status = model.Parameter.Status;
            vm.Parameter = pay;
            return View(vm);


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




        #region Registered Customer Report
        [HttpGet]
        public ActionResult RegisterCusReport()
        {

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            //string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            //RoleChecker roleChecker = new RoleChecker();
            //bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null /*&& checkRole*/)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                ////For ProfileName
                //List<MNCustProfile> ProfileList = new List<MNCustProfile>();
                //List<SelectListItem> ItemProfile = new List<SelectListItem>();
                //ProfileList = ReportUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

                //foreach (MNCustProfile Cust in ProfileList)
                //{
                //    ItemProfile.Add(new SelectListItem
                //    {
                //        Text = Cust.ProfileCode,
                //        Value = Cust.ProfileCode
                //    });
                //}

                //ViewBag.ProfileList = new SelectList(ItemProfile, "Value", "Text");

                CustReport para = new CustReport();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                return View(new RegCusDetailVM
                {
                    Parameter = para,
                    CustomerData = new List<CustomerData>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        public ContentResult RegisterCustomerTable(DataTableAjaxPostModel model, string MobileNumber, string StartDate, string EndDate, string Status, string change, string ToExcel)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }

            int filteredResultsCount;
            int totalResultsCount;
            string ParaChanged = "T";
            CustReport ac = new CustReport();
            string convert;
            string Start = "";
            string End = "";
            if (Session["UserName"] == null)
            {

                convert = JsonConvert.SerializeObject(new
                {
                    draw = model.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = "Session Expired, Please re-login."
                });

                return Content(convert, "application/json");

            }
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
                Start = DateTime.ParseExact(StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture); ;
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
                End = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                           .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            ac.MobileNo = MobileNumber;
            ac.StartDate = Start;
            ac.EndDate = End;
            ac.Status = Status;
            ac.UserName = (string)Session["UserName"];
            ParaChanged = change;
            var result = new List<CustomerData>();
            if (Session["CustomerData"] != null && ParaChanged == "F")
            {
                result = Session["CustomerData"] as List<CustomerData>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.CustomerDetail(ac);
                Session["CustomerData"] = result;
            }

            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<CustomerData>(result);
                //ExtraUtility.DataTableToInMemExcel(excel, "TEST.xls");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_CustomerData.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<CustomerData>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.MobileNo = item.MobileNo;
                resultset.CustomerName = item.CustomerName;
                //resultset.ProfileName = item.ProfileName;
                resultset.CreatedDate = item.CreatedDate;
                //resultset.ExpiryDate = item.ExpiryDate;
                resultset.Status = item.Status;
                resultset.Approved = item.Approved;
                resultset.CreatedBy = item.CreatedBy;
                resultset.ApprovedBy = item.ApprovedBy;
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
        #endregion









        #region Auto Reversal Transaction Report
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult ReversalTransactionReport()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.MType = "";

                MerchantPay para = new MerchantPay();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                ReportUserModel rep = new ReportUserModel();
                para.MerchantTypeList = rep.GetMerchantsType();
                return View(new MerchantVM
                {
                    Parameter = para,
                    MerchantInfo = new List<MerchantInfo>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        public ContentResult AutoReversalReportTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string MerchantType, string change, string ToExcel)
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
            MerchantPay ac = new MerchantPay();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            ac.SourceMobileNo = SourceMobileNo;
            ac.MerchantType = MerchantType;
            //ac.MerchantName = MerchantName;
            //ac.Status = Status;
            var result = new List<MerchantInfo>();
            if (Session["MerchantPayment"] != null && ParaChanged == "F")
            {
                result = Session["MerchantPayment"] as List<MerchantInfo>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.ReversalDetails(ac);
                Session["MerchantPayment"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<MerchantInfo>(result);
                excel.Columns.Remove("TotalAmount");
                excel.Columns.Remove("NoOfTran");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_MerchantPayment.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            decimal SumAmount = result.Sum(x => x.Amount);
            var res = FilterAndSort<MerchantInfo>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                //resultset.DatenTime = item.DatenTime.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.EnteredAt = item.EnteredAt;
                resultset.TxnID = item.TxnID;
                resultset.ReferenceNo = item.ReferenceNo;
                resultset.InitMobileNo = item.InitMobileNo;
                resultset.MerchantType = item.MerchantType;
                resultset.MerchantName = item.MerchantName;
                resultset.Amount = item.Amount;
                resultset.Status = item.Status;
                resultset.Message = item.Message;
                resultset.TranType = item.TranType;
                resultset.Name = item.Name;
                resultset.PayMode = item.PayMode;               
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList,
                Sum = SumAmount
            });

            return Content(convert, "application/json");

        }

        [HttpPost]
        public ActionResult ReversalTransactionReport(MerchantVM model)
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
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay(),
                        MerchantInfo = new List<MerchantInfo>()

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
                    return View(new MerchantVM
                    {
                        Parameter = new MerchantPay { MerchantTypeList = new List<ViewModel.Merchants>() },
                        MerchantInfo = new List<MerchantInfo>()

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
            List<MerchantInfo> report = rep.ReversalDetails(model.Parameter);
            MerchantVM vm = new MerchantVM();
            vm.MerchantInfo = report;
            MerchantPay pay = new MerchantPay();
            pay.MerchantTypeList = rep.GetMerchantsType();
            pay.StartDate = model.Parameter.StartDate;
            pay.EndDate = model.Parameter.EndDate;
            //pay.MerchantName = model.Parameter.MerchantName;
            pay.MerchantType = model.Parameter.MerchantType;
            //pay.Status = model.Parameter.Status;
            //pay.PayMode = model.Parameter.PayMode;
            vm.Parameter = pay;
            return View(vm);


        }

        //public string LoadDropDownMerchantsPaypoint(string value)
        //{
        //    bool Where = false;
        //    if (value != "")
        //        Where = true;
        //    ReportUserModel rep = new ReportUserModel();
        //    string ddl = rep.GetMerchantsFromTypeMerchant(value, Where);
        //    return ddl;
        //}

        #endregion






        #region Reversal Report Auto by System
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult ManualReversalTransaction()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.MType = "";

                MerchantPay para = new MerchantPay();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                ReportUserModel rep = new ReportUserModel();
                para.MerchantTypeList = rep.GetMerchantsType();
                return View(new MerchantVM
                {
                    Parameter = para,
                    MerchantInfo = new List<MerchantInfo>()

                });



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        public ContentResult ManualReversalReportTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string MerchantType, string change, string ToExcel)
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
            MerchantPay ac = new MerchantPay();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            ac.SourceMobileNo = SourceMobileNo;
            ac.MerchantType = MerchantType;
            var result = new List<MerchantInfo>();
            if (Session["MerchantPayment"] != null && ParaChanged == "F")
            {
                result = Session["MerchantPayment"] as List<MerchantInfo>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.ManualReversalDetails(ac);
                Session["MerchantPayment"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<MerchantInfo>(result);
                excel.Columns.Remove("TotalAmount");
                excel.Columns.Remove("NoOfTran");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_MerchantPayment.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            decimal SumAmount = result.Sum(x => x.Amount);
            var res = FilterAndSort<MerchantInfo>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.EnteredAt = item.EnteredAt;
                resultset.TxnID = item.TxnID;
                resultset.ReferenceNo = item.ReferenceNo;
                resultset.InitMobileNo = item.InitMobileNo;
                resultset.MerchantType = item.MerchantType;
                resultset.MerchantName = item.MerchantName;
                resultset.Amount = item.Amount;
                resultset.Status = item.Status;
                resultset.Message = item.Message;
                resultset.TranType = item.TranType;
                resultset.Name = item.Name;
                resultset.PayMode = item.PayMode;
                resultset.VerifiedBy = item.VerifiedBy;
                resultset.ApprovedBy = item.ApprovedBy;
                resultList.Add(resultset);
            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = resultList,
                Sum = SumAmount
            });

            return Content(convert, "application/json");

        }

        //[HttpPost]
        //public ActionResult ManualReversalTransactionReport(MerchantVM model)
        //{
        //    string userName = (string)Session["LOGGED_USERNAME"];
        //    string clientCode = (string)Session["LOGGEDUSER_ID"];
        //    string name = (string)Session["LOGGEDUSER_NAME"];
        //    string userType = (string)Session["LOGGED_USERTYPE"];

        //    TempData["userType"] = userType;

        //    if (TempData["userType"] != null)
        //    {
        //        this.ViewData["userType"] = this.TempData["userType"];
        //        ViewBag.UserType = this.TempData["userType"];
        //        ViewBag.Name = name;
        //        ViewBag.MType = "";

        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }

        //    string start = "", end = "";

        //    if (!string.IsNullOrEmpty(model.Parameter.StartDate))

        //    {
        //        if (!model.Parameter.StartDate.IsValidDate())
        //        {
        //            ViewBag.Error = "Not valid start date";
        //            return View(new MerchantVM
        //            {
        //                Parameter = new MerchantPay(),
        //                MerchantInfo = new List<MerchantInfo>()

        //            });
        //        }
        //        start = DateTime.ParseExact(model.Parameter.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
        //                                          .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
        //    }
        //    if (!string.IsNullOrEmpty(model.Parameter.EndDate))
        //    {
        //        if (!model.Parameter.EndDate.IsValidDate())
        //        {
        //            ViewBag.Error = "Not valid end date";
        //            return View(new MerchantVM
        //            {
        //                Parameter = new MerchantPay { MerchantTypeList = new List<ViewModel.Merchants>() },
        //                MerchantInfo = new List<MerchantInfo>()

        //            });
        //        }
        //        end = DateTime.ParseExact(model.Parameter.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
        //                                            .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

        //    }

        //    ViewBag.Error = "";
        //    ViewBag.message = "T";


        //    model.Parameter.StartDate = start;
        //    model.Parameter.EndDate = end;


        //    ReportUserModel rep = new ReportUserModel();
        //    List<MerchantInfo> report = rep.MerchantDetails(model.Parameter);
        //    MerchantVM vm = new MerchantVM();
        //    vm.MerchantInfo = report;
        //    MerchantPay pay = new MerchantPay();
        //    pay.MerchantTypeList = rep.GetMerchantsType();
        //    pay.StartDate = model.Parameter.StartDate;
        //    pay.EndDate = model.Parameter.EndDate;
        //    pay.MerchantType = model.Parameter.MerchantType;
           
        //    vm.Parameter = pay;
        //    return View(vm);


        //}

        #endregion


    }


}