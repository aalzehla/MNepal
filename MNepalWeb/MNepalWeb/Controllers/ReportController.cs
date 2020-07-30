using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MNepalWeb.Models;
using MNepalWeb.ViewModel;
using MNepalWeb.UserModels;
using MNepalWeb.Utilities;
using System.Globalization;
using System.Data;
using Newtonsoft.Json;
using System.Dynamic;
using System.Reflection;
using MNepalWeb.Settings;
using System.Text.RegularExpressions;
using MNepalWeb.Helper;

namespace MNepalWeb.Controllers {
    public class ReportController : Controller {
        #region Customer Detail Report
        // GET: Report
        public ActionResult CusReport()
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

            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.Error = "";
                CustReport para = new CustReport();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
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
        public ContentResult CusReportTable(DataTableAjaxPostModel model, string UserName, string StartDate, string EndDate, string change, string ToExcel)
        {
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
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture); ;
            ac.EndDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            ParaChanged = change;

            var result = new List<CustomerAccActivity>();
            if (Session["CustomerAccActivity"] != null && ParaChanged == "F")
            {
                result = Session["CustomerAccActivity"] as List<CustomerAccActivity>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.CustomerAccountActivity(ac);
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
                resultset.TransactionDate = item.TransactionDate.ToString("dd/MM/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);//"dd /MM/yyyy", CultureInfo.InvariantCulture);
                resultset.UserName = item.UserName;
                resultset.TransactionType = item.TransactionType;
                resultset.DestinationNo = item.DestinationNo;
                resultset.Amount = item.Amount;
                resultset.SMSStatus = item.SMSStatus;
                resultset.SMSSenderReply = item.SMSSenderReply;
                resultset.ErrorMessage = item.ErrorMessage;
                resultset.SMSTimeStamp = item.SMSTimeStamp;
                resultset.Name = item.Name;
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
        
        #region Customer Log
        public ActionResult CustomerAccountLog()
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

            if (TempData["userType"] != null&& checkRole)
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
        
        #region Merchants
        public ActionResult Merchants()
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

            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                ViewBag.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                ViewBag.EndDate = DateTime.Now.ToString("dd/MM/yyyy");

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

            ReportUserModel rep = new ReportUserModel();
            List<Merchants> MerchantList = rep.GetMerchants();
            return View(MerchantList);
        }
        public ActionResult MerchantsDetails(string ClientId, string StartDate, string EndDate)
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
            }
            else
            {
                return Content("Your Session has expired please login again to continue");
            }

            if (!StartDate.IsValidDate())
            {
                return Content("Not valid Start Date");
            }

            if (!EndDate.IsValidDate())
            {
                return Content("Not valid Start Date");
            }

            string start = DateTime.ParseExact(StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            string end = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                    .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            CustReport Params = new CustReport();
            Params.UserName = ClientId;
            Params.StartDate = start;
            Params.EndDate = end;

            ReportUserModel rep = new ReportUserModel();
            List<MerchantAcDetail> MerchantDetail = rep.MerchantAccDetail(Params);
            return View(MerchantDetail);
        }
        #endregion
        
        #region Registered Customer Report
        [HttpGet]
        public ActionResult RegisterCusReport()
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

                //For ProfileName
                List<MNCustProfile> ProfileList = new List<MNCustProfile>();
                List<SelectListItem> ItemProfile = new List<SelectListItem>();
                ProfileList = CusProfileUserModel.GetMNCustProfile().Where(x => x.ProfileStatus == 'A').ToList();

                foreach (MNCustProfile Cust in ProfileList)
                {
                    ItemProfile.Add(new SelectListItem
                    {
                        Text = Cust.ProfileCode,
                        Value = Cust.ProfileCode
                    });
                }

                ViewBag.ProfileList = new SelectList(ItemProfile, "Value", "Text");

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
        public ContentResult RegisterCustomerTable(DataTableAjaxPostModel model, string MobileNumber, string StartDate, string EndDate, string ProfileName, string Status, string Approved, string change, string ToExcel, string RegisterBy)
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
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture); //MM/dd/yyyy
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
            ac.ProfileCode = ProfileName;
            ac.Status = Status;
            string Approve = Approved;
            string Register = RegisterBy;
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
                result = rep.CustomerDetails(ac, Approve, Register);
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
                resultset.ProfileName = item.ProfileName;
                resultset.CreatedDate = item.CreatedDate;
                resultset.ExpiryDate = item.ExpiryDate;
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

            if (TempData["userType"] != null&&checkRole)
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

        public ContentResult TopUpTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string RequestType, string DestinationMobile, string TranId,string Status, string change, string ToExcel)
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
        
        #region Recharge Detail Report
        [HttpGet]
        //Using same models as for TopUp
        public ActionResult RechargeRep()
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

            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                TopUp para = new TopUp();
                para.StartDate = DateTime.Now.ToString("dd/MM/yyyy");
                para.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
                ReportUserModel rep = new ReportUserModel();
                ViewBag.MerchantList = rep.GetMerchantbyCategory("2"); //Topup=1
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

        public ContentResult RechargeCardTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string SourceMobileNo, string RequestType, string TranId, string Status, string change, string ToExcel)
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
            ac.TranID = TranId;
            ac.Status = Status;
            var result = new List<TopUpInfo>();
            if (Session["RechargeCard"] != null && ParaChanged == "F")
            {
                result = Session["RechargeCard"] as List<TopUpInfo>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.RechargeDetails(ac);
                Session["RechargeCard"] = result;

            }
            if (ToExcel == "T")
            {
                DataTable excel = ToDataTable<TopUpInfo>(result);
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_RechargeReport.xls";
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
                resultset.ServiceType = item.ServiceType;
                resultset.Amount = item.Amount;
                resultset.Status = item.Status;
                resultset.Message = item.Message;
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
        public ActionResult RechargeRep(TopUpRepVM model)
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
            List<TopUpInfo> report = rep.RechargeDetails(model.Parameter);
            TopUpRepVM vm = new TopUpRepVM();
            vm.TopUpInfo = report;
            vm.Parameter = model.Parameter;
            return View(vm);


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

            if (TempData["userType"] != null&& checkRole)
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
        public string LoadDropDownMerchants(string value)
        {
            bool Where = false;
            if (value != "")
                Where = true;
            ReportUserModel rep = new ReportUserModel();
            string ddl = rep.GetMerchantsFromType(value, Where);
            return ddl;
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

            if (TempData["userType"] != null&& checkRole)
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
        #endregion

        //test//
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


        #region Branchwise Report

        //public ActionResult BranchRep()
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
        //        ViewBag.BranchCode = "" ;


        //        BranchRep para = new BranchRep();
        //        ReportUserModel rep = new ReportUserModel();
        //        para.BranchList = rep.GetBranchCode();

        //        return View(new BranchVM
        //        {
        //            Parameter = para,
        //            BranchInfo = new List<BranchInfo>()

        //        });



        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }

        //}


        [HttpGet]

        public ActionResult BranchRep(BranchVM model)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];
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
                ViewBag.BranchCode = "";

                ViewBag.Error = "";
                ViewBag.message = "T";

                ReportUserModel rep = new ReportUserModel();
                List<BranchInfo> report = rep.BranchRepDetail(model.Parameter, bankCode);
                BranchVM vm = new BranchVM();
                vm.BranchInfo = report;
                BranchRep br = new BranchRep();
                br.BranchList = rep.GetBranchCode();
                vm.Parameter = br;
                return View(vm);




            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        #endregion
        
        #region CustomerExpiryList

        public ActionResult CustomerExpiryList()
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

            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                ViewBag.BranchCode = "";
                ViewBag.ProfileList = CusProfileUserModel.GetMNCustProfile().GroupBy(x=>x.ProfileCode,x=>x.ProfileCode).ToDictionary(x => x.Key, x => x.Key);
                return View();
              
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }
        //string StartDate, string EndDate, string Service, string MerchantName, string GroupBy, string change, string ToExcel
        public ContentResult CustomerExpiryListTable(DataTableAjaxPostModel model, string UserName, string Profile,string Status,string change, string ToExcel)
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
            var result = new List<MNExpiryCustomers>();
            if (Session["ExpiryCustomer"] != null && ParaChanged == "F")
            {
                result = Session["ExpiryCustomer"] as List<MNExpiryCustomers>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.GetExpiryCustomers(UserName,Profile,Status);
                Session["ExpiryCustomer"] = result;

            }
            if (ToExcel == "T")
            {

                DataTable excel = ToDataTable<MNExpiryCustomers>(result);
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_CustomerExpiry.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<MNExpiryCustomers>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.Name = item.Name;
                resultset.Address = item.Address;
                resultset.ContactNumber = item.ContactNumber;
                resultset.CreatedDate = item.CreatedDate.ToString("dd/MM/yyyy"); 
                resultset.ExpiryDate = item.ExpiryDate.ToString("dd/MM/yyyy");
                resultset.ProfileCode = item.ProfileCode;
                resultset.RenewPeriod = item.RenewPeriod + "Months";
                resultset.ExpiryPeriodInDays = item.ExpiryPeriodInDays +" Days";
                resultset.Expired = item.Expired.ToString().ToUpper();
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

        //Report Datatable

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

        public void GetReport(string Key, string Format, string FileName)
        {
            if (!string.IsNullOrEmpty(Key) && !string.IsNullOrEmpty(Format) && !string.IsNullOrEmpty(FileName))
            {
                if (ExtraUtility.HasSessionKey(Key))
                {
                    try
                    {
                        var data = Session[Key] as DataTable;
                        if (Format == "xlx")
                        {
                            ExtraUtility.ExportDataTableToExcel(data, FileName + ".xls");
                        }
                        else if (Format == "pdf")
                        {

                        }
                    }
                    catch
                    {
                        Response.Write("There was problem parsing your request. Please contact with Administrator.");
                    }
                }
                else
                {
                    Response.Write("Bad Request");
                }
            }
            else
            {
                Response.Write("Bad Request");
            }
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
        public void Download(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                DataTable result = TempData[fileGuid] as DataTable;
                ExtraUtility.ExportDataTableToExcel(result, fileName);
            }
            else {
                Response.Write("Bad Request");
            }
        }

        #region View Admin Details
        [HttpGet]

        public ActionResult AdminDetails()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];
            //Check Role link start          
            string methodlink = System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null&& checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;



                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                DataTable bank = dsBank.Tables[0];
                DataSet dsProfile = UserProfileUtils.GetDataSetPopulateProfileName();
                DataTable profile = dsProfile.Tables[0];

                List<UserInfo> registrationtList = new List<UserInfo>();
                DataTable dtblRegistration = ProfileUtils.GetAdminProfile();

                foreach (DataRow row in dtblRegistration.Rows)
                {
                    UserInfo regobj = new UserInfo();
                    regobj.ClientCode = row["ClientCode"].ToString();
                    regobj.Name = row["Name"].ToString();
                    regobj.Address = row["Address"].ToString();
                    regobj.PIN = row["PIN"].ToString();
                    regobj.Status = row["Status"].ToString();
                    regobj.ContactNumber1 = row["ContactNumber1"].ToString();
                    regobj.ContactNumber2 = row["ContactNumber2"].ToString();
                    regobj.EmailAddress = row["EmailAddress"].ToString();
                    regobj.UserName = row["UserName"].ToString();
                    regobj.UserType = row["userType"].ToString();
                    regobj.UserBranchName = row["UserBranchName"].ToString();
                    regobj.CreatedBy = row["CreatedBy"].ToString();
                    regobj.EmailAddress = row["EmailAddress"].ToString();
                    regobj.ProfileName = row["ProfileName"].ToString();
                    regobj.AProfileName = row["AProfileName"].ToString();
                    registrationtList.Add(regobj);
                }
                return View(registrationtList);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        #endregion


        [HttpGet]
        public ActionResult GetTransactionStatusSummary()
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
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
           
        }
        public ContentResult GetTransactionStatusSummaryTable(DataTableAjaxPostModel model, string Date)
        {
            if (!Request.IsAjaxRequest())
            {
                Response.Write("Invalid Execution");
                return Content("");
            }
            
            int totalResultsCount;
            string convert;
            if (!string.IsNullOrEmpty(Date))
            {
                if (!Date.IsValidDate())
                {

                    convert = JsonConvert.SerializeObject(new
                    {
                        draw = model.draw,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        error = "Not valid  Date"
                    });

                    return Content(convert, "application/json");
                }
                Date = DateTime.ParseExact(Date, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                Response.Write("Invalid Date");
                convert = JsonConvert.SerializeObject(new
                {
                    draw = model.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = "Invalid date"
                });
                return Content(convert, "application/json");
            }
           
            var result = new List<Tuple<string,string>>();
            ReportUserModel rep = new ReportUserModel();
            result = rep.GetTransactionStatusSummary(Date);

            totalResultsCount = result.Count;
            var res = result;
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.TransactionCount = item.Item1; //Item1 = transactioncount
                resultset.ResponseCode = item.Item2; //Item2 = responseCode
                resultList.Add(resultset);

            }
            convert = JsonConvert.SerializeObject(new
            {
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = 0,
                data = resultList
            });
            return Content(convert, "application/json");

        }


        [HttpGet]
        public ActionResult AdminActivity()
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
                ViewBag.COC = Session["COC"];
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


 
        public ContentResult AdminActivityTable(DataTableAjaxPostModel model, string UserName, string BranchCode,string StartDate, string EndDate, string change, string ToExcel)
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
                result = rep.GetAdminActivity(UserName, BranchCode, StartDate, EndDate);
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
                resultset.Remarks = string.Join(" ",item.Remarks,item.Description);
                resultset.TimeStamp = item.TimeStamp;
                resultset.Updates = item.Updates;
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

                if(userType!="admin")
                {
                    return RedirectToAction("Index","Login");

                }
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

                Match m1, m1i, m2, m3, m3i, m4, m5, m5i, m5j, m6,mexec,mexecute;
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

                if(tempSql.StartsWith("exec")|| tempSql.StartsWith("execute"))
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
                if(String.IsNullOrEmpty(model.TimeOut.Trim()))
                {
                    model.TimeOut = "600";
                }
                ReportUserModel rep = new ReportUserModel();
                int Timeout=0;
                if (!int.TryParse(model.TimeOut,out Timeout))
                {
                    Timeout = 600;
                }
                var dt = rep.ExecuteQuery(Query,Timeout);
                model.Data = dt;
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #region BankLink
        [HttpGet]
        public ActionResult BankLink()
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

                return View();



            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        [HttpPost]
        public ActionResult BankLink(BankLinkVM model)
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
                    return View(new BankLinkVM
                    {
                        Parameter = new BankLink(),
                        BankLinkInfo = new List<BankLinkInfo>()

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
                    return View(new BankLinkVM
                    {
                        Parameter = new BankLink(),
                        BankLinkInfo = new List<BankLinkInfo>()

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
            List<BankLinkInfo> report = rep.BankLinkDetails(model.Parameter);
            BankLinkVM vm = new BankLinkVM();
            vm.BankLinkInfo = report;
            vm.Parameter = model.Parameter;
            return View(vm);


        }

        public ContentResult BankLinkTable(DataTableAjaxPostModel model, string StartDate, string EndDate, string mobileNo, string Status, string change, string ToExcel)
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

            BankLink ac = new BankLink();
            ac.StartDate = StartDate;
            ac.EndDate = EndDate;
            ac.MobileNo = mobileNo;
            ac.Status = Status;

            var result = new List<BankLinkInfo>();
            if (Session["BankLinkInfo"] != null && ParaChanged == "F")
            {
                result = Session["BankLinkInfo"] as List<BankLinkInfo>;
            }
            else
            {
                ReportUserModel rep = new ReportUserModel();
                result = rep.BankLinkDetails(ac);
                Session["BankLinkInfo"] = result;

            }
            if (ToExcel == "T")
            {

                List<BankLinkInfo> modifiedMI = result.ToList();
                //foreach (BankLinkInfo item in result)
                //{
                //    int i = int.Parse(item.SalesMonth.Trim() == "" ? "0" : item.SalesMonth.Trim());
                //    if (item.SalesMonth.Length <= 2 && item.SalesMonth.Length > 0)
                //        item.SalesMonth = Months[i - 1];
                //}

                DataTable excel = ToDataTable<BankLinkInfo>(result);
                //excel.Columns.Remove("DatenTime");
                //excel.Columns.Remove("Amount");
                string Handler = Guid.NewGuid().ToString();
                TempData[Handler] = excel;
                string FileName = DateTime.Now + "_BankLinkSummary.xls";
                convert = JsonConvert.SerializeObject(new
                {
                    FileGuid = Handler,
                    FileName = FileName
                });
                return Content(convert, "application/json");
            }
            var res = FilterAndSort<BankLinkInfo>(model, result, out totalResultsCount, out filteredResultsCount);
            var resultList = new List<dynamic>();
            foreach (var item in res)
            {
                dynamic resultset = new ExpandoObject();
                resultset.RequestedDate = item.RequestedDate;
                resultset.MobileNumber = item.MobileNo;
                resultset.Name = item.CustomerName;
                resultset.AccountNumber = item.BankAccNo;
                resultset.Status = item.Status;
                resultset.VerifiedBy = item.VerifiedBy;
                resultset.VerifiedDate = item.VerifiedDate;
                resultset.ApprovedBy = item.ApprovedBy;
                resultset.ApprovedDate = item.ApprovedDate;
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

    }
}