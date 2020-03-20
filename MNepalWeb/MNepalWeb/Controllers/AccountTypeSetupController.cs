using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using MNepalWeb.Models;
using MNepalWeb.Utilities;
using System.Web.SessionState;
using System;
using System.Linq;
using MNepalWeb.Helper;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class AccountTypeSetupController : Controller
    {
        // GET: AccountTypeSetup
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

                List<AcTypeInfo> acTypeList = new List<AcTypeInfo>();
                DataTable dtblAcType = AcTypeUtils.GetAccountType();

                foreach (DataRow row in dtblAcType.Rows)
                {
                    AcTypeInfo acTypeobj = new AcTypeInfo
                    {
                        AcType = row["AcType"].ToString(),
                        AcTypeName = row["AcTypeDesc"].ToString(),
                        AllowEnquiry = row["AcAllowEnquiry"].ToString(),
                        AllowTransaction = row["AcAllowTransaction"].ToString(),
                        AllowAlert = row["AcAllowAlert"].ToString(),
                        Active = row["AcAllowActive"].ToString()
                    };

                    acTypeList.Add(acTypeobj);
                }
                return View(acTypeList);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        //GET : AccountTypeSetup/CreateAccountType
        [HttpGet]
        public ActionResult CreateAccountType()
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

                if (this.TempData["actype_message"] != null)
                {
                    this.ViewData["actype_message"] = this.TempData["actype_message"];
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
        
        public bool GetCheckingAcType(string AcType)
        {
            if ((AcType != "") || (AcType != null))
            {
                string result = string.Empty;
                DataTable dtableAcType = AcTypeUtils.GetCheckAcType(AcType);
                if (dtableAcType.Rows.Count == 0)
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





        //POST: AccountTypeSetup/CreateAccountType
        [HttpPost]
        public ActionResult CreateAccountType(FormCollection collection)
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
                    this.ViewData["userType"] = this.TempData["userType"];
                    ViewBag.UserType = this.TempData["userType"];
                    ViewBag.Name = name;

                    AcTypeInfo actypeInfo = new AcTypeInfo();
                    actypeInfo.AcType = collection["txtAcType"].ToString();
                    actypeInfo.AcTypeName = collection["txtAcTypeName"].ToString();
                    actypeInfo.AllowEnquiry = collection["txtAllowEnquiry"].ToString();
                    actypeInfo.AllowTransaction = collection["txtAllowTransaction"].ToString();
                    actypeInfo.AllowAlert = collection["txtAllowAlert"].ToString();
                    actypeInfo.Active = collection["txtActive"].ToString();

                    if (string.IsNullOrEmpty(collection["txtAcTypeName"]))
                    {
                        ModelState.AddModelError("AcTypeName", "*Please enter AcTypeName");
                    }
                    if (string.IsNullOrEmpty(collection["txtAllowEnquiry"]))
                    {
                        ModelState.AddModelError("AllowEnquiry", "*Please enter AllowEnquiry");
                    }
                    if (string.IsNullOrEmpty(collection["txtAllowTransaction"]))
                    {
                        ModelState.AddModelError("AllowTransaction", "*Please enter AllowTransaction");
                    }
                    if (string.IsNullOrEmpty(collection["txtAllowAlert"]))
                    {
                        ModelState.AddModelError("AllowAlert", "*Please enter AllowAlert");
                    }

                    if (string.IsNullOrEmpty(collection["txtActive"]))
                    {
                        ModelState.AddModelError("Active", "*Please enter Active");
                    }

                    bool result = false;
                    string errorMessage = string.Empty;

                    if ((actypeInfo.AcTypeName != "") && (actypeInfo.AllowEnquiry != "") && (actypeInfo.AllowTransaction != "") && (actypeInfo.AllowAlert != ""))
                    {
                        if (collection.AllKeys.Any())
                        {
                            try
                            {

                                int resultmsg = AcTypeUtils.CreateAcTypeInfo(actypeInfo);
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

                    this.TempData["actype_message"] = result
                                                  ? "Account Type information is successfully added."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";

                    return this.RedirectToAction("CreateAccountType");
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






        //GET: EditAccountType
        [HttpGet]
        public ActionResult EditAccountType(string AcTypeID)
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

                if (this.TempData["actype_message"] != null)
                {
                    this.ViewData["actype_message"] = this.TempData["actype_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }

                AcTypeInfo acInfo = new AcTypeInfo();
                DataTable dtblRegistration = AcTypeUtils.GetAcTypeDetails(AcTypeID);
                if (dtblRegistration.Rows.Count == 1)
                {

                    acInfo.AcType = dtblRegistration.Rows[0]["AcType"].ToString();
                    acInfo.AcTypeName = dtblRegistration.Rows[0]["AcTypeDesc"].ToString();
                    acInfo.AllowEnquiry = dtblRegistration.Rows[0]["AcAllowEnquiry"].ToString();
                    acInfo.AllowTransaction = dtblRegistration.Rows[0]["AcAllowTransaction"].ToString();
                    acInfo.AllowAlert = dtblRegistration.Rows[0]["AcAllowAlert"].ToString();
                    acInfo.Active = dtblRegistration.Rows[0]["AcAllowActive"].ToString();

                }
                //var allowEnq = new[] { "True", "False" };
                //ViewBag.Enquiry = allowEnq.ToSelectList(x => x, acInfo.AllowEnquiry /* selectedValue */);


                return View(acInfo);
               
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }



        //POST: EditAccountType
        [HttpPost]
        public ActionResult EditAccountType(FormCollection collection)
        {
            try
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                string clientCode = (string)Session["LOGGEDUSER_ID"];
                string name = (string)Session["LOGGEDUSER_NAME"];
                string userType = (string)Session["LOGGED_USERTYPE"];

                TempData["userType"] = userType;
                AcTypeInfo actypeInfo = new AcTypeInfo();

                if (TempData["userType"] != null)
                {
                    this.ViewData["userType"] = this.TempData["userType"];
                    ViewBag.UserType = this.TempData["userType"];
                    ViewBag.Name = name;

                    
                    //actypeInfo.AcType = collection["txtAcType"].ToString();
                    //actypeInfo.AcTypeName = collection["txtAcTypeName"].ToString();
                    //actypeInfo.AllowEnquiry = collection["txtAllowEnquiry"].ToString();
                    //actypeInfo.AllowTransaction = collection["txtAllowTransaction"].ToString();
                    //actypeInfo.AllowAlert = collection["txtAllowAlert"].ToString();
                    //actypeInfo.Active = collection["txtActive"].ToString();

                    actypeInfo.AcType = collection["AcType"].ToString();
                    actypeInfo.AcTypeName = collection["AcTypeName"].ToString();
                    actypeInfo.AllowEnquiry = collection["AllowEnquiry"].ToString();
                    actypeInfo.AllowTransaction = collection["AllowTransaction"].ToString();
                    actypeInfo.AllowAlert = collection["AllowAlert"].ToString();
                    actypeInfo.Active = collection["Active"].ToString();

                    //if (string.IsNullOrEmpty(collection["txtAcType"]))
                    //{
                    //    ModelState.AddModelError("AcType", "*Please enter AcType");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtAcTypeName"]))
                    //{
                    //    ModelState.AddModelError("AcTypeName", "*Please enter AcTypeName");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtAllowEnquiry"]))
                    //{
                    //    ModelState.AddModelError("AllowEnquiry", "*Please enter AllowEnquiry");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtAllowTransaction"]))
                    //{
                    //    ModelState.AddModelError("AllowTransaction", "*Please enter AllowTransaction");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtAllowAlert"]))
                    //{
                    //    ModelState.AddModelError("AllowAlert", "*Please enter AllowAlert");
                    //}
                    //if (string.IsNullOrEmpty(collection["txtActive"]))
                    //{
                    //    ModelState.AddModelError("Active", "*Please enter Active");
                    //}
                    if (!ViewData.ModelState.IsValid)
                    {
                        this.ViewData["actype_message"] = " *Validation Error.";
                        this.ViewData["message_class"] = "failed_info";
                        return View();
                    }
                    bool result = false;
                    string errorMessage = string.Empty;

                    if ((actypeInfo.AcType != "") && (actypeInfo.AcTypeName != "")
                            && (actypeInfo.Active != "") && (actypeInfo.AllowEnquiry != "") && (actypeInfo.AllowTransaction != "") && (actypeInfo.AllowAlert != ""))
                    {
                        if (collection.AllKeys.Any())
                        {
                            try
                            {
                                int resultmsg = AcTypeUtils.UpdateAccountType(actypeInfo);
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

                    this.TempData["actype_message"] = result
                                                  ? "Account information is successfully updated."
                                                  : "Error while updating the information. ERROR :: "
                                                    + errorMessage;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";
                    if (result)
                    {
                        return RedirectToAction("Index", "AccountTypeSetup");
                    }
                    else
                    {
                        return RedirectToAction("EditAccountType", new { AcTypeID = actypeInfo.AcType });
                    }
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