using MNepalWeb.Models;
using MNepalWeb.Utilities;
using MNepalWeb.ViewModel;
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
using MNepalWeb.App_Start;

namespace MNepalWeb.Controllers
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





    }
}