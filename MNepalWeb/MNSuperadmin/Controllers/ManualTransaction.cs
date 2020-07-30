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
using System.Net.Http;
using System.IO;
using MNSuperadmin.App_Start;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace MNSuperadmin.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class ManualTransactionController : Controller
    {

        #region Manual EBanking
        #region EBanking
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult EBanking()
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

        public ContentResult EBankingTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string change, string ToExcel)
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
            var result = new List<EBankingTran>();
            if (Session["MerchantPayment"] != null && ParaChanged == "F")
            {
                result = Session["MerchantPayment"] as List<EBankingTran>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.EBankingTranDetails(ac);
                Session["MerchantPayment"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<EBankingTran>(result);
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
            var res = FilterAndSort<EBankingTran>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                //resultset.EBDate = item.EBDate.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);

                DateTime date = Convert.ToDateTime(item.EBDate, CultureInfo.InvariantCulture);
                resultset.EBDate = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.UserName = item.UserName;
                resultset.PaymentReferenceNumber = item.PaymentReferenceNumber;
                resultset.ItemCode = item.ItemCode;
                resultset.Amount = item.Amount;
                resultset.BID = item.BID;
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

        public ActionResult ViewEBankingDetails(string prn)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string mobile = (string)Session["Mobile"];

            if (this.TempData["custmodify_messsage"] != null)
            {
                this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                EBankingManualTran srInfo = new EBankingManualTran();

                EBankingTran result = new EBankingTran();

                ReportUserModel rep = new ReportUserModel();
                result = rep.EBankingTranDetails(prn);

                srInfo.PRN = result.PaymentReferenceNumber;
                srInfo.ItemCode = result.ItemCode;
                srInfo.Amount = result.Amount;

                DateTime date = Convert.ToDateTime(result.EBDate, CultureInfo.InvariantCulture);
                srInfo.Reqdate = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                srInfo.ReferenceNo = result.ReferenceNo;
                srInfo.MobileNo = result.UserName;
                srInfo.Name = result.Name;

                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult VerifyEBankingTransaction(EBankingManualTran eBankingManualTran)
        {
            if (eBankingManualTran.btnCommand == "Verify")
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                ManualTransactionUserModel mn = new ManualTransactionUserModel();

                int result = mn.verifyManualEBankingTrans(eBankingManualTran, userName);
                if(result == 100)
                {
                    this.TempData["EBanking_ManualTran_messsage"] = "Transaction successfully verified.";
                    this.TempData["message_class"] = "success_info";
                }
                else
                {
                    this.TempData["EBanking_ManualTran_messsage"] = "Transaction not verified.";
                    this.TempData["message_class"] = "failure_info";
                }
            }
            
            return RedirectToAction("EBanking", "ManualTransaction");
        }

        [HttpPost]
        public async Task<ActionResult> GetTransactionDetails(EBankingManualTran eBankingManualTran)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
           
                HttpResponseMessage _res = new HttpResponseMessage();

                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();
                string amount = eBankingManualTran.Amount.ToString();
                using (HttpClient client = new HttpClient())
                {

                    var action = "ft.svc/GetTransactionDetails";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("PREF", eBankingManualTran.PRN),
                        new KeyValuePair<string,string>("AMOUNT", amount),
                        new KeyValuePair<string, string>("AMT1", amount),
                        new KeyValuePair<string, string>("CustRefType", eBankingManualTran.ItemCode)
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
                            string BID = "";
                            string status = "";
                            if (!string.IsNullOrEmpty(message))
                            {
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                var json = ser.Deserialize<JsonParse>(responsetext);
                                message = json.d;
                                if(message == "Transaction Failure")
                                {
                                    BID = "00000";
                                    Session["BID"] = BID;
                                    status = "Transaction not verified";
                                    Session["status"] = status;
                                }
                                else
                                {
                                    BID = message.Substring(16, 6);
                                    Session["BID"] = BID;
                                    status = "Transaction verified succcessful";
                                    Session["status"] = status;
                                }
                            }
                            return Json(new { responseCode = responseCode, responseText = BID, status = status },
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
           
            return RedirectToAction("EBanking", "ManualTransaction");
        }

        [HttpPost]
        public ActionResult EBanking(MerchantVM model)
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

        #region EBanking Approval
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult EBankingApproval()
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

        public ContentResult EBankingApprovalTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string change, string ToExcel)
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
            var result = new List<EBankingTran>();
            if (Session["MerchantPayment"] != null && ParaChanged == "F")
            {
                result = Session["MerchantPayment"] as List<EBankingTran>;
            }
            else
            {
                ManualTransactionUserModel rep = new ManualTransactionUserModel();
                result = rep.EBankingVerifiedTranDetails(ac);
                Session["MerchantPayment"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<EBankingTran>(result);
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
            var res = FilterAndSort<EBankingTran>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                //resultset.EBDate = item.EBDate.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                DateTime date = Convert.ToDateTime(item.EBDate, CultureInfo.InvariantCulture);
                resultset.EBDate = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.UserName = item.UserName;
                resultset.PaymentReferenceNumber = item.PaymentReferenceNumber;
                resultset.VerifiedBy = item.VerifiedBy;
                resultset.Amount = item.Amount;
                resultset.VerifiedDate = item.VerifiedDate;
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

        public ActionResult ViewEBankingVerifiedDetails(string prn)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string mobile = (string)Session["Mobile"];

            if (this.TempData["custmodify_messsage"] != null)
            {
                this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                EBankingManualTran srInfo = new EBankingManualTran();

                EBankingTran result = new EBankingTran();

                ReportUserModel rep = new ReportUserModel();
                result = rep.EBankingTranDetails(prn);

                srInfo.PRN = result.PaymentReferenceNumber;
                srInfo.ItemCode = result.ItemCode;
                srInfo.Amount = result.Amount;

                DateTime date = Convert.ToDateTime(result.EBDate, CultureInfo.InvariantCulture);
                srInfo.Reqdate = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                srInfo.ReferenceNo = result.ReferenceNo;
                srInfo.MobileNo = result.UserName;
                srInfo.Name = result.Name;
                srInfo.BID = result.BID;
                srInfo.Status = result.Status;
                srInfo.FName = result.FName;
                srInfo.LName = result.LName;

                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> ManualEBankingTran(EBankingManualTran eBankingManualTran)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            if (eBankingManualTran.btnCommand == "Approve")
            {
                HttpResponseMessage _res = new HttpResponseMessage();

                TraceIdGenerator _tig = new TraceIdGenerator();
                var tid = _tig.GenerateTraceID();
                string amount = eBankingManualTran.Amount.ToString();
                using (HttpClient client = new HttpClient())
                {

                    var action = "ft.svc/manualTranEBanking";
                    var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
                    var content = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("PREF", eBankingManualTran.PRN),
                        new KeyValuePair<string,string>("AMOUNT", amount),
                        new KeyValuePair<string, string>("AMT1", amount),
                        new KeyValuePair<string, string>("FirstName", eBankingManualTran.FName),
                        new KeyValuePair<string, string>("LastName", eBankingManualTran.LName),
                        new KeyValuePair<string, string>("CustRefType", eBankingManualTran.ItemCode)
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
                            ManualTransactionUserModel mn = new ManualTransactionUserModel();
                            int res = mn.ApproveManualEBankingTrans(eBankingManualTran, userName);
                            if (responseCode != 200)
                            {
                                this.TempData["EBanking_ManualTran_messsage"] = respmsg;
                                this.TempData["message_class"] = "failed_info";
                            }
                            if (responseCode == 400)
                            {
                                this.TempData["EBanking_ManualTran_messsage"] = "Request cancelled for unverified transaction";
                                this.TempData["message_class"] = "success_info";
                            }
                            else
                            {
                                this.TempData["EBanking_ManualTran_messsage"] = "Transaction Successful";
                                this.TempData["message_class"] = "success_info";
                            }
                            return RedirectToAction("EBanking", "ManualTransaction");
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
                    this.TempData["fundTransfer_messsage"] = result
                                                    ? "Fund Transfer successfully." + message
                                                    : "ERROR :: " + message;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";

                    return RedirectToAction("EBanking", "ManualTransaction");
                }
            }
            else if (eBankingManualTran.btnCommand == "Reject")
            {
                ManualTransactionUserModel mn = new ManualTransactionUserModel();

                int result = mn.RejectManualEBankingTrans(eBankingManualTran, userName);
                if (result == 100)
                {
                    this.TempData["EBanking_ManualTran_messsage"] = "Transaction successfully rejected.";
                    this.TempData["message_class"] = "success_info";
                }
                else
                {
                    this.TempData["EBanking_ManualTran_messsage"] = "Transaction not rejected.";
                    this.TempData["message_class"] = "failure_info";
                }
            }

            return RedirectToAction("EBanking", "ManualTransaction");
        }

        [HttpPost]
        public ActionResult EBankingApproval(MerchantVM model)
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

        #region EBanking Rejected
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult EBankingRejected()
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

        public ContentResult EBankingRejectedTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string change, string ToExcel)
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
            var result = new List<EBankingTran>();
            if (Session["MerchantPayment"] != null && ParaChanged == "F")
            {
                result = Session["MerchantPayment"] as List<EBankingTran>;
            }
            else
            {
                ManualTransactionUserModel rep = new ManualTransactionUserModel();
                result = rep.EBankingRejectedTranDetails(ac);
                Session["MerchantPayment"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<EBankingTran>(result);
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
            var res = FilterAndSort<EBankingTran>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                //resultset.EBDate = item.EBDate.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                DateTime date = Convert.ToDateTime(item.EBDate, CultureInfo.InvariantCulture);
                resultset.EBDate = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.UserName = item.UserName;
                resultset.PaymentReferenceNumber = item.PaymentReferenceNumber;
                resultset.RejectedBy = item.RejectedBy;
                resultset.Amount = item.Amount;
                resultset.RejectedDate = item.RejectedDate;
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
        public ActionResult ViewEBankingRejectedDetails(string prn)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string mobile = (string)Session["Mobile"];

            if (this.TempData["custmodify_messsage"] != null)
            {
                this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                EBankingManualTran srInfo = new EBankingManualTran();

                EBankingTran result = new EBankingTran();

                ReportUserModel rep = new ReportUserModel();
                result = rep.EBankingTranDetails(prn);

                srInfo.PRN = result.PaymentReferenceNumber;
                srInfo.ItemCode = result.ItemCode;
                srInfo.Amount = result.Amount;

                DateTime date = Convert.ToDateTime(result.EBDate, CultureInfo.InvariantCulture);
                srInfo.Reqdate = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                srInfo.ReferenceNo = result.ReferenceNo;
                srInfo.MobileNo = result.UserName;
                srInfo.Name = result.Name;
                srInfo.Status = result.Status;
                srInfo.RejectRemarks = result.RejectedRemarks;

                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult RejectEBankingTransaction(EBankingManualTran eBankingManualTran)
        {
            if (eBankingManualTran.btnCommand == "Approve")
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                ManualTransactionUserModel mn = new ManualTransactionUserModel();

                int result = mn.ReVerifyManualEBankingTrans(eBankingManualTran, userName);
                if (result == 100)
                {
                    this.TempData["EBanking_ManualTran_messsage"] = "Transaction successfully verified.";
                    this.TempData["message_class"] = "success_info";
                }
                else
                {
                    this.TempData["EBanking_ManualTran_messsage"] = "Transaction not verified.";
                    this.TempData["message_class"] = "failure_info";
                }
            }

            return RedirectToAction("EBanking", "ManualTransaction");
        }
        [HttpPost]
        public ActionResult EBankingRejected(MerchantVM model)
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

        #endregion

        #region Manual Topup

        #region Verify
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult TopupVerify()
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

        public ContentResult TopupVerifyTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string change, string ToExcel)
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
            var result = new List<TopUpManualTran>();
            if (Session["TopupVerifyInfo"] != null && ParaChanged == "F")
            {
                result = Session["TopupVerifyInfo"] as List<TopUpManualTran>;
            }
            else
            {
                ManualTransactionUserModel rep = new ManualTransactionUserModel();
                result = rep.TopUpManualVerifyList(ac);
                Session["TopupVerifyInfo"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<TopUpManualTran>(result);
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
            var res = FilterAndSort<TopUpManualTran>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();

                //resultset.EBDate = item.Date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);

                DateTime date = Convert.ToDateTime(item.Date, CultureInfo.InvariantCulture);
                resultset.Date = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.MobNumber = item.MobNumber;
                resultset.PayMedium = item.PayMedium;
                resultset.MerchantName = item.MerchantName;
                resultset.Amount = item.Amount;
                resultset.PayMode = item.PayMode;
                resultset.RetrievalRef = item.RetrievalRef;
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

        public ActionResult ViewTopUpVerifyDetails(string retReferencce)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string mobile = (string)Session["Mobile"];

            if (this.TempData["custmodify_messsage"] != null)
            {
                this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                TopUpManualTran srInfo = new TopUpManualTran();

                TopUpManualTran result = new TopUpManualTran();

                ManualTransactionUserModel rep = new ManualTransactionUserModel();
                result = rep.TopUpManualVerifyList(retReferencce);

                DateTime date = Convert.ToDateTime(result.Date, CultureInfo.InvariantCulture);
                srInfo.Date = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                srInfo.MobNumber = result.MobNumber;
                srInfo.PayMedium = result.PayMedium;
                srInfo.MerchantName = result.MerchantName;
                srInfo.Amount = result.Amount;
                srInfo.PayMode = result.PayMode;
                srInfo.RetrievalRef = result.RetrievalRef;
                srInfo.UserName = result.UserName;
                srInfo.DestNumber = result.DestNumber;
                srInfo.RespDescription = result.RespDescription;
                srInfo.RespCode = result.RespCode;
                srInfo.RespStatus = result.RespStatus;
                srInfo.MerchantID = result.MerchantID;
               // await BankQuery();
                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        public ActionResult VerifyTopUpTransaction(TopUpManualTran TopUpManualTran)
        {
            if (TopUpManualTran.btnCommand == "Verify")
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                ManualTransactionUserModel mn = new ManualTransactionUserModel();

                int result = mn.verifyManualToUpTrans(TopUpManualTran, userName);
                if (result == 100)
                {
                    this.TempData["TopUp_ManualTran_messsage"] = "Transaction successfully verified.";
                    this.TempData["message_class"] = "success_info";
                }
                else
                {
                    this.TempData["TopUp_ManualTran_messsage"] = "Transaction not verified.";
                    this.TempData["message_class"] = "failure_info";
                }
            }

            return RedirectToAction("TopupVerify", "ManualTransaction");
        }

        [HttpPost]
        public ActionResult TopupVerify(MerchantVM model)
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

        #region Approval
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult TopupApproval()
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
        public ContentResult TopupApprovalTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string change, string ToExcel)
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
            var result = new List<TopUpManualTran>();
            if (Session["ApprovalInfo"] != null && ParaChanged == "F")
            {
                result = Session["ApprovalInfo"] as List<TopUpManualTran>;
            }
            else
            {
                ManualTransactionUserModel rep = new ManualTransactionUserModel();
                result = rep.TopUpVerifiedTranDetails(ac);
                Session["ApprovalInfo"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<TopUpManualTran>(result);
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
            var res = FilterAndSort<TopUpManualTran>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                //resultset.EBDate = item.EBDate.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                DateTime date = Convert.ToDateTime(item.Date, CultureInfo.InvariantCulture);
                resultset.Date = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.MobNumber = item.MobNumber;
                resultset.MerchantName = item.MerchantName;
                resultset.VerifiedBy = item.VerifiedBy;
                resultset.Amount = item.Amount;
                resultset.VerifiedDate = item.VerifiedDate;
                resultset.ReferenceNo = item.RetrievalRef;
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

        public async Task<ActionResult> ViewTopUpVerifiedDetails(string retRef)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string mobile = (string)Session["Mobile"];

            if (this.TempData["custmodify_messsage"] != null)
            {
                this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                TopUpManualTran srInfo = new TopUpManualTran();

                TopUpManualTran result = new TopUpManualTran();

                ManualTransactionUserModel rep = new ManualTransactionUserModel();
                result = rep.TopUpVerifiedTranDetails(retRef);

                DateTime date = Convert.ToDateTime(result.Date, CultureInfo.InvariantCulture);
                srInfo.Date = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                srInfo.MobNumber = result.MobNumber;
                srInfo.PayMedium = result.PayMedium;
                srInfo.MerchantName = result.MerchantName;
                srInfo.Amount = result.Amount;
                srInfo.PayMode = result.PayMode;
                srInfo.RetrievalRef = result.RetrievalRef;
                srInfo.UserName = result.UserName;
                srInfo.DestNumber = result.DestNumber;
                srInfo.RespDescription = result.RespDescription;
                srInfo.RespCode = result.RespCode;
                srInfo.RespStatus = result.RespStatus;
                srInfo.MerchantID = result.MerchantID;
                srInfo.VerifiedBy = result.VerifiedBy;
                srInfo.VerifiedDate = result.VerifiedDate;

                string merchantID = result.MerchantID;
                string amount = result.Amount.ToString();
                await BankQuery(merchantID,amount);

                srInfo.BalanceCheck = Session["resp"].ToString();
                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult TopupApprovalTran(TopUpManualTran TopUpManualTran)
        {
            if (TopUpManualTran.btnCommand == "Approve")
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                string merchantNumber = "";
                string serviceCode = "";
                if (TopUpManualTran.PayMedium == "Wallet")
                {
                    serviceCode = "10";
                }
                else if(TopUpManualTran.PayMedium == "Bank")
                {
                    serviceCode = "11";
                }

                DataTable dtableMobileNo = ManualTransactionUserModel.GetMerchantDetail(TopUpManualTran.MerchantID);
                if (dtableMobileNo.Rows.Count == 1)
                {
                    merchantNumber = dtableMobileNo.Rows[0]["UserName"].ToString();
                }


                ManualTransactionUserModel mn = new ManualTransactionUserModel();

                if (TopUpManualTran.MerchantID == "1")
                {
                    TopUpManualTran.MerchantName = "ADSL";
                }
                else if (TopUpManualTran.MerchantID == "2")
                {
                    TopUpManualTran.MerchantName = "NTC";
                }
                else if (TopUpManualTran.MerchantID == "7")
                {
                    TopUpManualTran.MerchantName = "Landline";
                }
                else if (TopUpManualTran.MerchantID == "10")
                {
                    TopUpManualTran.MerchantName = "NCELL";
                }

                //int result = mn.ManualTopUpTrans(TopUpManualTran, userName,merchantNumber,serviceCode);

                string respCode = "123";
                string respRetrievalRef = mn.ManualTopUpTrans1(TopUpManualTran, userName, merchantNumber, serviceCode);

                Thread.Sleep(3000);

                respCode = mn.GetResponseCode(respRetrievalRef);
                string resultmsg = "";
                
                resultmsg = mn.getResponseDescription(respCode);

                if (respCode ==  "000" || respCode == "00")
                {
                    int result1 = mn.ApproveManualToUpTrans(TopUpManualTran, userName);

                    if(result1 == 100)
                    {
                        string mobile = "";
                        string merchantName = "";
                        string name = mn.GetUserName(TopUpManualTran.MobNumber);

                        if (TopUpManualTran.MerchantID == "1" )
                        {
                            merchantName = "ADSL";
                        }
                        else if(TopUpManualTran.MerchantID == "2")
                        {
                            merchantName = "NTC";
                        }
                        else if (TopUpManualTran.MerchantID == "7")
                        {
                            merchantName = "Landline";
                        }
                        else if (TopUpManualTran.MerchantID == "10")
                        {
                            merchantName = "NCELL";
                        }

                        string messagereply = "Dear " + name + "," + "\n";
                        messagereply += merchantName + " successfully manual reverse NPR " + TopUpManualTran.Amount + "\n";
                        messagereply += "Thank You,MNepal";

                        var client = new WebClient();

                        mobile = TopUpManualTran.MobNumber;
                        //var content = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                        //    + "977" + mobile + "&Text=" + messagereply + "");

                        if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                        {
                            //FOR NCELL
                            var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                            + "977" + mobile + "&Text=" + messagereply + "");
                        }
                        else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                            || (mobile.Substring(0, 3) == "986"))
                        {
                            //FOR NTC
                            var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                                + "977" + mobile + "&Text=" + messagereply + "");
                        }

                        SMSLog log = new SMSLog();
                        log.SentBy = mobile;
                        log.Purpose = "Manual Reversal";
                        log.UserName = userName;
                        log.Message = messagereply;
                        CustomerUtils.LogSMS(log);

                        this.TempData["TopUp_ManualTran_messsage"] = "Transaction successfully approved.";
                        this.TempData["message_class"] = "success_info";
                    }
                }
                else
                {
                    this.TempData["TopUp_ManualTran_messsage"] = "Transaction not approved. " + resultmsg;
                    this.TempData["message_class"] = "failure_info";
                }
            }
            else if (TopUpManualTran.btnCommand == "Reject")
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                ManualTransactionUserModel mn = new ManualTransactionUserModel();

                int result = mn.RejectManualTopUpTrans(TopUpManualTran,userName);
                if (result == 100)
                {
                    this.TempData["TopUp_ManualTran_messsage"] = "Transaction successfully rejected.";
                    this.TempData["message_class"] = "success_info";
                }
                else
                {
                    this.TempData["TopUp_ManualTran_messsage"] = "Transaction not rejected.";
                    this.TempData["message_class"] = "failure_info";
                }
            }

            return RedirectToAction("TopupApproval", "ManualTransaction");
        }
        
        //[HttpPost]
        //public async Task<ActionResult> ManualTopUpTran(TopUpManualTran topUpManualTran)
        //{
        //    string userName = (string)Session["LOGGED_USERNAME"];
        //    if (eBankingManualTran.btnCommand == "Approve")
        //    {
        //        HttpResponseMessage _res = new HttpResponseMessage();

        //        TraceIdGenerator _tig = new TraceIdGenerator();
        //        var tid = _tig.GenerateTraceID();
        //        string amount = topUpManualTran.Amount.ToString();
        //        using (HttpClient client = new HttpClient())
        //        {

        //            var action = "ft.svc/manualTranEBanking";
        //            var uri = Path.Combine(ApplicationInitilize.WCFUrl, action);
        //            var content = new FormUrlEncodedContent(new[]{
        //                new KeyValuePair<string, string>("sc", eBankingManualTran.PRN),
        //                new KeyValuePair<string, string>("sourcemobile", eBankingManualTran.PRN),
        //                new KeyValuePair<string, string>("destmobile", eBankingManualTran.PRN),
        //                new KeyValuePair<string, string>("amount", eBankingManualTran.PRN),
        //                new KeyValuePair<string, string>("pin", eBankingManualTran.PRN),
        //                new KeyValuePair<string, string>("PREF", eBankingManualTran.PRN),
        //                new KeyValuePair<string, string>("PREF", eBankingManualTran.PRN),
        //                new KeyValuePair<string, string>("PREF", eBankingManualTran.PRN),
        //                new KeyValuePair<string, string>("PREF", eBankingManualTran.PRN),
        //                new KeyValuePair<string, string>("PREF", eBankingManualTran.PRN),


        //                new KeyValuePair<string,string>("AMOUNT", amount),
        //                new KeyValuePair<string, string>("AMT1", amount),
        //                new KeyValuePair<string, string>("FirstName", eBankingManualTran.FName),
        //                new KeyValuePair<string, string>("LastName", eBankingManualTran.LName),
        //                new KeyValuePair<string, string>("CustRefType", eBankingManualTran.ItemCode)
        //            });
        //            _res = await client.PostAsync(new Uri(uri), content);
        //            string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
        //            _res.ReasonPhrase = responseBody;
        //            string errorMessage = string.Empty;
        //            int responseCode = 0;
        //            string message = string.Empty;
        //            string responsetext = string.Empty;
        //            bool result = false;
        //            string ava = string.Empty;
        //            string avatra = string.Empty;
        //            string avamsg = string.Empty;
        //            try
        //            {
        //                if (_res.IsSuccessStatusCode)
        //                {
        //                    result = true;
        //                    responseCode = (int)_res.StatusCode;
        //                    responsetext = await _res.Content.ReadAsStringAsync();
        //                    message = _res.Content.ReadAsStringAsync().Result;
        //                    string respmsg = "";
        //                    if (!string.IsNullOrEmpty(message))
        //                    {
        //                        JavaScriptSerializer ser = new JavaScriptSerializer();
        //                        var json = ser.Deserialize<JsonParse>(responsetext);
        //                        message = json.d;
        //                        JsonParse myNames = ser.Deserialize<JsonParse>(json.d);
        //                        int code = Convert.ToInt32(myNames.StatusCode);
        //                        respmsg = myNames.StatusMessage;
        //                        if (code != responseCode)
        //                        {
        //                            responseCode = code;
        //                        }
        //                    }
        //                    ManualTransactionUserModel mn = new ManualTransactionUserModel();
        //                    int res = mn.ApproveManualEBankingTrans(eBankingManualTran, userName);
        //                    if (responseCode != 200)
        //                    {
        //                        this.TempData["EBanking_ManualTran_messsage"] = respmsg;
        //                        this.TempData["message_class"] = "failed_info";
        //                    }
        //                    if (responseCode == 400)
        //                    {
        //                        this.TempData["EBanking_ManualTran_messsage"] = "Request cancelled for unverified transaction";
        //                        this.TempData["message_class"] = "success_info";
        //                    }
        //                    else
        //                    {
        //                        this.TempData["EBanking_ManualTran_messsage"] = "Transaction Successful";
        //                        this.TempData["message_class"] = "success_info";
        //                    }
        //                    return RedirectToAction("EBanking", "ManualTransaction");
        //                    return Json(new { responseCode = responseCode, responseText = respmsg },
        //                    JsonRequestBehavior.AllowGet);
        //                }
        //                else
        //                {
        //                    result = false;
        //                    responseCode = (int)_res.StatusCode;
        //                    responsetext = await _res.Content.ReadAsStringAsync();
        //                    dynamic json = JValue.Parse(responsetext);
        //                    message = json.d;
        //                    if (message == null)
        //                    {
        //                        return Json(new { responseCode = responseCode, responseText = responsetext },
        //                    JsonRequestBehavior.AllowGet);
        //                    }
        //                    else
        //                    {
        //                        dynamic item = JValue.Parse(message);

        //                        return Json(new { responseCode = responseCode, responseText = (string)item["StatusMessage"] },
        //                        JsonRequestBehavior.AllowGet);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                return Json(new { responseCode = "400", responseText = ex.Message },
        //                    JsonRequestBehavior.AllowGet);
        //            }
        //            this.TempData["fundTransfer_messsage"] = result
        //                                            ? "Fund Transfer successfully." + message
        //                                            : "ERROR :: " + message;
        //            this.TempData["message_class"] = result ? "success_info" : "failed_info";

        //            return RedirectToAction("EBanking", "ManualTransaction");
        //        }
        //    }
        //    else if (eBankingManualTran.btnCommand == "Reject")
        //    {
        //        ManualTransactionUserModel mn = new ManualTransactionUserModel();

        //        int result = mn.RejectManualEBankingTrans(eBankingManualTran, userName);
        //        if (result == 100)
        //        {
        //            this.TempData["EBanking_ManualTran_messsage"] = "Transaction successfully rejected.";
        //            this.TempData["message_class"] = "success_info";
        //        }
        //        else
        //        {
        //            this.TempData["EBanking_ManualTran_messsage"] = "Transaction not rejected.";
        //            this.TempData["message_class"] = "failure_info";
        //        }
        //    }

        //    return RedirectToAction("EBanking", "ManualTransaction");
        //}
        
        [HttpPost]
        public ActionResult TopupApproval(MerchantVM model)
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

        #region Reject
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult TopupReject()
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

        public ContentResult TopupRejectTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string change, string ToExcel)
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
            var result = new List<TopUpManualTran>();
            if (Session["RejectedInfo"] != null && ParaChanged == "F")
            {
                result = Session["RejectedInfo"] as List<TopUpManualTran>;
            }
            else
            {
                ManualTransactionUserModel rep = new ManualTransactionUserModel();
                result = rep.TopUpRejectedTranDetails(ac);
                Session["RejectedInfo"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<TopUpManualTran>(result);
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
            var res = FilterAndSort<TopUpManualTran>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                //resultset.EBDate = item.EBDate.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                DateTime date = Convert.ToDateTime(item.Date, CultureInfo.InvariantCulture);
                resultset.Date = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                resultset.MobNumber = item.MobNumber;
                resultset.MerchantName = item.MerchantName;
                resultset.RejectedBy = item.RejectedBy;
                resultset.Amount = item.Amount;
                resultset.RejectedDate = item.RejectedDate;
                resultset.RejectRemarks = item.RejectRemarks;
                resultset.ReferenceNo = item.RetrievalRef;
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

        public async Task<ActionResult> ViewTopUpRejectedDetails(string retRef)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string mobile = (string)Session["Mobile"];

            if (this.TempData["custmodify_messsage"] != null)
            {
                this.ViewData["custmodify_messsage"] = this.TempData["custmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                TopUpManualTran srInfo = new TopUpManualTran();

                TopUpManualTran result = new TopUpManualTran();

                ManualTransactionUserModel rep = new ManualTransactionUserModel();
                result = rep.TopUpRejectedTranDetails(retRef);

                DateTime date = Convert.ToDateTime(result.Date, CultureInfo.InvariantCulture);
                srInfo.Date = date.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);
                srInfo.MobNumber = result.MobNumber;
                srInfo.PayMedium = result.PayMedium;
                srInfo.MerchantName = result.MerchantName;
                srInfo.Amount = result.Amount;
                srInfo.PayMode = result.PayMode;
                srInfo.RetrievalRef = result.RetrievalRef;
                srInfo.UserName = result.UserName;
                srInfo.DestNumber = result.DestNumber;
                srInfo.RespDescription = result.RespDescription;
                srInfo.RespCode = result.RespCode;
                srInfo.RespStatus = result.RespStatus;
                srInfo.MerchantID = result.MerchantID;
                srInfo.RejectedBy = result.RejectedBy;
                srInfo.RejectedDate = result.RejectedDate;
                srInfo.RejectRemarks = result.RejectRemarks;

                string merchantID = result.MerchantID;
                string amount = result.Amount.ToString();
                await BankQuery(merchantID, amount);

                srInfo.BalanceCheck = Session["resp"].ToString();
                return View(srInfo);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult RejectTopupTransaction(TopUpManualTran topUpManualTran)
        {
            if (topUpManualTran.btnCommand == "Approve")
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                ManualTransactionUserModel mn = new ManualTransactionUserModel();

                int result = mn.ReVerifyManualTopUpTrans(topUpManualTran, userName);
                if (result == 100)
                {
                    this.TempData["Reject_ManualTran_messsage"] = "Transaction successfully verified.";
                    this.TempData["message_class"] = "success_info";
                }
                else
                {
                    this.TempData["Reject_ManualTran_messsage"] = "Transaction not verified.";
                    this.TempData["message_class"] = "failure_info";
                }
            }

            return RedirectToAction("TopupReject", "ManualTransaction");
        }

        [HttpPost]
        public ActionResult TopupReject(MerchantVM model)
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

        public async Task<ActionResult> BankQuery(string merchantID,string Amount)
        {
            string username = (string)Session["LOGGED_USERNAME"];
            string pin = "";
            Session["resp"] = null;
            if ((username != "") || (username != null))
            {

                string result = string.Empty;
                DataTable dtableMobileNo = ManualTransactionUserModel.GetMerchantDetail(merchantID);
                if (dtableMobileNo.Rows.Count == 1)
                {
                    pin = dtableMobileNo.Rows[0]["PIN"].ToString();
                    username = dtableMobileNo.Rows[0]["UserName"].ToString();
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
                    int amt = int.Parse(Amount);

                    if (_res.IsSuccessStatusCode)
                    {
                        result = true;
                        responseCode = (int)_res.StatusCode;
                        responsetext = await _res.Content.ReadAsStringAsync();
                        responsedateTime = await _res.Content.ReadAsStringAsync();
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
                            decimal balance = decimal.Parse(respmsg);

                            if(balance<amt)
                            {
                                respmsg = "Insufficient Balance";
                                Session["resp"] = respmsg;
                            }
                            else if (balance > amt)
                            {
                                respmsg = "Sufficient Balance";
                                Session["resp"] = respmsg;
                            }

                            if (code != responseCode)
                            {
                                responseCode = code;
                            }
                        }
                        //this.Session["bankSyncTime"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt");
                        //dateTime = Session["bankSyncTime"].ToString();
                        //this.Session["bankbal"] = respmsg;
                        //ViewBag.AvailBankBalnAmount = (string)respmsg;
                        return Json(new { responseCode = responseCode, responseText = respmsg},
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
    }
}