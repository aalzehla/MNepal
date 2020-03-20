using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using MNepalWeb.Models;
using MNepalWeb.Utilities;
using System.Web.SessionState;
using MNepalWeb.Helper;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class BranchSetupController : Controller
    {
        // GET: BranchSetup
        public ActionResult Index()
        {
          
            return View();
        }

        // GET: BranchSetup/CreateBranch
        public ActionResult CreateBranch()
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

                if (this.TempData["branch_message"] != null)
                {
                    this.ViewData["branch_message"] = this.TempData["branch_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                return this.View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpGet]
        public bool GetCheckingBranchCode(string BranchCode)
        {
            string bankCode = (string)Session["BankCode"];
            if ((BranchCode != "") || (BranchCode != null))
            {
                string result = string.Empty;
                DataTable dtableBranchCode = BranchUtils.GetCheckBranchCode(BranchCode,bankCode);
                if (dtableBranchCode.Rows.Count == 0)
                {
                    result = "Success";
                    return true;
                }
                else
                {
                    result = "AcType Already Exist";
                    return false;
                }
            }
            else
                return false;

        }


        // POST: BranchSetup/CreateBranch
        [HttpPost]
        public ActionResult CreateBranch(FormCollection collection)
        {
            try
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                string clientCode = (string)Session["LOGGEDUSER_ID"];
                string name = (string)Session["LOGGEDUSER_NAME"];
                string userType = (string)Session["LOGGED_USERTYPE"];
                string bankCode = (string)Session["BankCode"];

                TempData["userType"] = userType;

                if (TempData["userType"] != null)
                {
                    this.ViewData["userType"] = this.TempData["userType"];
                    ViewBag.UserType = this.TempData["userType"];
                    ViewBag.Name = name;

                    MNBranchTable branchInfo = new MNBranchTable
                    {
                        BranchName = collection["txtBranchName"].ToString(),
                        BranchCode = collection["txtBranchCode"],
                        BranchLocation = collection["txtBranchLocation"].ToString()
                    };

                    if (string.IsNullOrEmpty(collection["txtBranchName"]))
                    {
                        ModelState.AddModelError("BranchName", "*Please enter BranchName");
                    }
                    if (string.IsNullOrEmpty(collection["txtBranchCode"]))
                    {
                        ModelState.AddModelError("BranchCode", "*Please enter BranchCode");
                    }
                    if (string.IsNullOrEmpty(collection["txtBranchLocation"]))
                    {
                        ModelState.AddModelError("BranchLocation", "*Please enter BranchLocation");
                    }

                    if (!ViewData.ModelState.IsValid)
                    {
                        ViewData["branch_message"] = " *Validation Error.";
                        ViewData["message_class"] = "failed_info";
                        return View();
                    }

                    bool result = false;
                    string errorMessage = string.Empty;

                    //if ((branchInfo.BranchName != "") && (Convert.ToInt32(branchInfo.BranchCode) != 0) 
                    //    && (branchInfo.BranchLocation != ""))


                    if ((branchInfo.BranchName != "") && (Convert.ToInt32(branchInfo.BranchCode) >= 0)
                       && (branchInfo.BranchLocation != "")) 
                    {
                        if (collection.AllKeys.Any())
                        {
                            try
                            {
                                int resultmsg = BranchUtils.CreateBranchInfo(branchInfo,bankCode);
                                if (resultmsg == 100)
                                {
                                    result = true;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                result = false;
                                errorMessage = ex.Message;
                            }
                        }

                    }
                    else
                    {
                        result = false;
                    }

                    this.TempData["branch_message"] = result
                                                  ? "Branch information is successfully added."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";

                    return this.RedirectToAction("CreateBranch");
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }

            }
            catch
            {
                return View();
            }
        }

        // GET: BranchSetup/BranchListView
        public ActionResult BranchListView()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];
            //Check Role link start          
            string methodlink = "BranchModification";//System.Reflection.MethodBase.GetCurrentMethod().Name;
            RoleChecker roleChecker = new RoleChecker();
            bool checkRole = roleChecker.checkRole(methodlink, clientCode);
            //Check Role link end


            TempData["userType"] = userType;

            if (TempData["userType"] != null && checkRole)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                if (this.TempData["branch_message"] != null)
                {
                    this.ViewData["branch_message"] = this.TempData["branch_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }
                
                List<MNBranchTable> branchList = new List<MNBranchTable>();
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                DataTable dtbranch = dsBank.Tables[0];

                var Result = from DataRow row in dtbranch.Rows
                             where (string)row["IsBlocked"] == "F" orderby row["BranchCode"]
                             select row;
                
                foreach (DataRow dr in Result)
                {
                    //999 is MNepal Branch Code So it should not be shown in BankAdmin
                    if (dr["BankCode"].ToString() == "0000")
                        continue;
                    MNBranchTable mbranchtable = new MNBranchTable
                    {
                       BranchCode = dr["BranchCode"].ToString(),
                       BranchName = dr["BranchName"].ToString(),
                       BranchLocation = dr["BranchLocation"].ToString(),
                       IsBlocked = dr["IsBlocked"].ToString()

                    };

                    branchList.Add(mbranchtable);
                }
                return View(branchList);
            }

          
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }



        
        // GET: BranchSetup/BranchModification
        public ActionResult BranchModification(string BranchCode)
        {
            if(BranchCode == null || BranchCode == "")
            {
              return RedirectToAction("BranchListView", "BranchSetup");
            }

            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];
            string bankCode = (string)Session["BankCode"];


            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;
                MNBranchTable MNBt = new MNBranchTable();

                if (this.TempData["branch_message"] != null)
                {
                    this.ViewData["branch_message"] = this.TempData["branch_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(BranchCode,bankCode);
                DataTable dtbranch = dsBank.Tables[0];
               
                string branchName = string.Empty;
                string branchCode = string.Empty;
                string branchLocation = string.Empty;
                foreach (DataRow dr in dtbranch.Rows)
                {
                    MNBt.BranchCode = @dr["BranchCode"].ToString();
                    MNBt.BranchName = @dr["BranchName"].ToString();
                    MNBt.BranchLocation = @dr["BranchLocation"].ToString();
                    MNBt.IsBlocked = @dr["IsBlocked"].ToString();
                }
             return View(MNBt);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // GET: BranchSetup/BranchStatus/5
        public ActionResult BranchStatus()
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

                if (this.TempData["branch_message"] != null)
                {
                    this.ViewData["branch_message"] = this.TempData["branch_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                //For Branch
                List<SelectListItem> item = new List<SelectListItem>();
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(bankCode);
                ViewBag.bank = dsBank.Tables[0];
                foreach (DataRow dr in ViewBag.bank.Rows)
                {
                    //999 is MNepal Branch Code So it should not be shown in BankAdmin
                    if (@dr["BankCode"].ToString() == "0000")
                        continue;
                    item.Add(new SelectListItem
                    {
                        Text = @dr["BranchName"].ToString(),
                        Value = @dr["BranchCode"].ToString()
                    });
                }
                ViewBag.BranchName = item;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpGet]
        public ActionResult BranchDetailSearch(String SearchValue)
        {
            string bankCode = (string)Session["BankCode"];
            MNBranchTable mNBranchTable = new MNBranchTable();
            if (SearchValue != "")
            {
                //For Branch                
                DataSet dsBank = BranchUtils.GetDataSetPopulateBranchName(SearchValue,bankCode);
                ViewBag.bank = dsBank.Tables[0];
                string branchName = string.Empty;
                string branchCode = string.Empty;
                string branchLocation = string.Empty;
                foreach (DataRow dr in ViewBag.bank.Rows)
                {
                    branchName = @dr["BranchName"].ToString();
                    branchCode = @dr["BranchCode"].ToString();
                    branchLocation = @dr["BranchLocation"].ToString();

                    mNBranchTable.BranchCode = @dr["BranchCode"].ToString();
                    mNBranchTable.BranchName = @dr["BranchName"].ToString();
                    mNBranchTable.BranchLocation = @dr["BranchLocation"].ToString();

                    mNBranchTable.IsBlocked = @dr["IsBlocked"].ToString().Trim();/*== "F"? "Active" : "Blocked";*/

                }
                ViewBag.BBranchName = branchName;
                ViewBag.BranchCode = branchCode;
                ViewBag.BranchLocation = branchLocation;
                ViewBag.IsBlocked = mNBranchTable.IsBlocked;

            }
            return Json(new { bn = mNBranchTable.BranchName, bc = mNBranchTable.BranchCode, bl = mNBranchTable.BranchLocation, bb = mNBranchTable.IsBlocked }, JsonRequestBehavior.AllowGet);


        }


        [HttpPost]
        public ActionResult EditBranch(MNBranchTable mbt)
        {
            try
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

                    //MNBranchTable branchInfo = new MNBranchTable
                    //{
                    //    BranchName = collection["txtBranchName"],
                    //    BranchCode =collection["txtBranchCode"],
                    //    BranchLocation = collection["txtBranchLocation"],
                    //    IsBlocked = collection["txtIsBlocked"]
                    //};

                    //if (string.IsNullOrEmpty(collection["txtBranchName"]))
                    //{
                    //    ModelState.AddModelError("BranchName", "*Please enter BranchName");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtBranchCode"]))
                    //{
                    //    ModelState.AddModelError("BranchCode", "*Please enter BranchCode");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtBranchLocation"]))
                    //{
                    //    ModelState.AddModelError("BranchLocation", "*Please enter BranchLocation");
                    //}

                    if (!ModelState.IsValid)
                    {
                        string errors = "";
                        foreach(var error in ModelState)
                        {
                            errors = errors + error + "\n";
                        }
                        ViewData["branch_message"] = " *Validation Error."+errors;
                        ViewData["message_class"] = "failed_info";
                        return View(mbt);
                    }

                    bool result = false;
                    string errorMessage = string.Empty;

                    if ((mbt.BranchName != "") && (Convert.ToInt32(mbt.BranchCode) != 0) 
                        && (mbt.BranchLocation != ""))
                    {
                        
                        
                            try
                            {
                                int resultmsg = BranchUtils.UpdateBranchInfo(mbt);
                                if (resultmsg == 100)
                                {
                                    result = true;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                result = false;
                                errorMessage = ex.Message;
                            }
                        

                    }
                    else
                    {
                        result = false;
                    }

                    TempData["branch_message"] = result
                                                  ? "Branch information is successfully updated."
                                                  : "Error while updating the information. ERROR :: "
                                                    + errorMessage;
                    TempData["message_class"] = result ? "success_info" : "failed_info";
                    if(result)
                     return RedirectToAction("BranchListView");

                    return View(mbt);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }

            }
            catch
            {
                return View();
            }
        }


        [HttpPost]
        public ActionResult EditBranchStatus(FormCollection collection)
        {
            try
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

                    MNBranchTable branchInfo = new MNBranchTable
                    {
                        BranchName = collection["txtBranchName"],
                        BranchCode = collection["txtBranchCode"],
                        BranchLocation = collection["txtBranchLocation"],
                        IsBlocked = collection["IsBlocked"]
                    };

                    if (string.IsNullOrEmpty(collection["txtBranchName"]))
                    {
                        ModelState.AddModelError("BranchName", "*Please enter BranchName");
                    }
                    if (string.IsNullOrEmpty(collection["txtBranchCode"]))
                    {
                        ModelState.AddModelError("BranchCode", "*Please enter BranchCode");
                    }
                    if (string.IsNullOrEmpty(collection["txtBranchLocation"]))
                    {
                        ModelState.AddModelError("BranchLocation", "*Please enter BranchLocation");
                    }

                    if (!ViewData.ModelState.IsValid)
                    {
                        ViewData["branch_message"] = " *Validation Error.";
                        ViewData["message_class"] = "failed_info";
                        return View();
                    }

                    bool result = false;
                    string errorMessage = string.Empty;

                    if ((branchInfo.BranchName != "") && (branchInfo.BranchLocation != "")
                        && (Convert.ToInt32(branchInfo.BranchCode) != 0))
                    {
                        if (collection.AllKeys.Any())
                        {
                            try
                            {
                                int resultmsg = BranchUtils.UpdateBranchInfo(branchInfo);
                                if (resultmsg == 100)
                                {
                                    result = true;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                result = false;
                                errorMessage = ex.Message;
                            }
                        }

                    }
                    else
                    {
                        result = false;
                    }

                    TempData["branch_message"] = result
                                                  ? "Branch information is successfully updated."
                                                  : "Error while updating the information. ERROR :: "
                                                    + errorMessage;
                    TempData["message_class"] = result ? "success_info" : "failed_info";

                    return RedirectToAction("BranchStatus");
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }

            }
            catch
            {
                return View();
            }
        }

    }
}