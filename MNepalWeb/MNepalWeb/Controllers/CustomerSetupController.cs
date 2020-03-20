using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MNepalWeb.Helper;
using MNepalWeb.Models;
using MNepalWeb.UserModels;
using MNepalWeb.Utilities;
using MNepalWeb.ViewModel;

namespace MNepalWeb.Controllers
{
    public class CustomerSetupController : Controller
    {
        // GET: CustomerSetup/CreateCusProfile
        [HttpGet]
        #region "Customer Profile Insert"
        public ActionResult CreateCusProfile()
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

                if (this.TempData["cusprofile_message"] != null)
                {
                    this.ViewData["cusprofile_message"] = this.TempData["cusprofile_message"];
                    this.ViewData["message_class"] = this.TempData["message_class"];
                }
                CusProfileViewModel Vm = new CusProfileViewModel();
                Vm.MNFeatures = CusProfileUserModel.GetMNFeature();


                return this.View(Vm);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion

        [HttpPost]
        #region "Customer Profile Insert"
        public ActionResult CreateCusProfile(FormCollection collection)
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

                    CusProfileViewModel cusProfile =
                        new CusProfileViewModel
                        {
                            m_ProfileCode = collection["txtProfileCode"].ToString().Trim(),
                            m_ProfileDesc = collection["txtProfileDesc"].ToString().Trim(),
                            m_ProfileStatus = collection["txtPStatus"].ToString(),
                            m_RenewPeriod = Convert.ToInt32(collection["txtRenewPeriod"].ToString().Trim() == ""? "0": collection["txtRenewPeriod"].ToString().Trim())
                         
                        };

                    //cusProfile.m_Registration = Convert.ToDecimal(collection["txtRegistration"].ToString().Trim() == "" ? "0" : collection["txtRegistration"].ToString().Trim());
                    //cusProfile.m_ReNew = Convert.ToDecimal(collection["txtRenewal"].ToString().Trim() == "" ? "0" : collection["txtRenewal"].ToString().Trim());
                    //cusProfile.m_PinReset = Convert.ToDecimal(collection["txtPIN"].ToString().Trim() == "" ? "0" : collection["txtPIN"].ToString().Trim());
                    //cusProfile.m_ChargeAccount = collection["txtCharge"].ToString();
                    /*
                    cusProfile.m_HasCharge = collection["txtHasCharge"].ToString();
                    cusProfile.m_IsDrAlert = collection["txtIsDrAlert"].ToString();
                    cusProfile.m_IsCrAlert = collection["txtIsCrAlert"].ToString();*/
                    //cusProfile.m_MinDrAlertAmt = Convert.ToDecimal(collection["txtMinDrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinDrAlertAmt"].ToString().Trim());
                    //cusProfile.m_MinCrAlertAmt = Convert.ToDecimal(collection["txtMinCrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinCrAlertAmt"].ToString().Trim());
                    if (collection.AllKeys.Contains("txtHasCharge"))
                    {
                        cusProfile.m_HasCharge = collection["txtHasCharge"].ToString();
                        cusProfile.m_Registration = collection["txtRegistration"].ToString();// Convert.ToDecimal(collection["txtRegistration"].ToString().Trim() == "" ? "0" : collection["txtRegistration"].ToString().Trim());
                        cusProfile.m_PinReset = collection["txtPIN"].ToString();// Convert.ToDecimal(collection["txtPIN"].ToString().Trim() == "" ? "0" : collection["txtPIN"].ToString().Trim());
                        cusProfile.m_ReNew = collection["txtRenewal"].ToString();// Convert.ToDecimal(collection["txtRenewal"].ToString().Trim() == "" ? "0" :collection["txtRenewal"].ToString().Trim());
                        cusProfile.ChargeAccount = collection["txtCharge"].ToString();
                    }
                    else
                    {
                        cusProfile.m_HasCharge = "F";
                        cusProfile.m_Registration = "0";

                        cusProfile.m_PinReset = "0";
                        cusProfile.m_ReNew = "0";
                        cusProfile.ChargeAccount = "";

                    }
                    if (collection.AllKeys.Contains("txtIsDrAlert"))
                    {
                        cusProfile.m_IsDrAlert = collection["txtIsDrAlert"].ToString();
                        cusProfile.m_MinDrAlertAmt = Convert.ToDecimal(collection["txtMinDrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinDrAlertAmt"].ToString().Trim());
                    }
                    else
                    {
                        cusProfile.m_IsDrAlert = "F";
                        cusProfile.m_MinDrAlertAmt = 0;
                    }
                    if (collection.AllKeys.Contains("txtIsCrAlert"))
                    {
                        cusProfile.m_IsCrAlert = collection["txtIsCrAlert"].ToString();
                        cusProfile.m_MinCrAlertAmt = Convert.ToDecimal(collection["txtMinCrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinCrAlertAmt"].ToString().Trim());
                    }
                    else
                    {
                        cusProfile.m_IsCrAlert = "F";
                        cusProfile.m_MinCrAlertAmt = 0;
                    }
                    if (collection.AllKeys.Contains("txtAutoRenew"))
                    {
                        cusProfile.m_AutoRenew = collection["txtAutoRenew"].ToString();
                    }
                    else
                    {
                        cusProfile.m_AutoRenew = "F";
                    }
                    /**/
                    string[] features;
                    List<MNFeatureMasterVM> MNFeature = new List<MNFeatureMasterVM>();

                    if (!String.IsNullOrEmpty(collection["hidFeatures"].ToString()))
                    {
                        features = collection["hidFeatures"].ToString().Split(',');
                        features = features.Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();

                        foreach (string feature in features)
                        {


                            MNFeature.Add(new MNFeatureMasterVM
                            {
                                FeatureCode = feature,
                                TxnCount = collection[String.Format("txtTxnCount{0}", feature)].ToString(),
                                PerDayTxnAmt = collection[String.Format("txtPerDayTxnAmt{0}", feature)].ToString(),
                                PerTxnAmt = collection[String.Format("txtPerTxnAmt{0}", feature)].ToString(),
                                TxnAmtM = collection[String.Format("txtTxnAmtM{0}", feature)].ToString()


                            });

                        }

                    }

                    if (string.IsNullOrEmpty(collection["txtProfileCode"]))
                    {
                        ModelState.AddModelError("m_ProfileCode", "*Please enter Code");
                    }
                    if (string.IsNullOrEmpty(collection["txtProfileDesc"]))
                    {
                        ModelState.AddModelError("m_ProfileDesc", "*Please enter Description");
                    }
                    if (string.IsNullOrEmpty(collection["txtPStatus"]))
                    {
                        ModelState.AddModelError("m_ProfileStatus", "*Please enter Profile Status");
                    }
                    if (string.IsNullOrEmpty(collection["txtRenewPeriod"]))
                    {
                        ModelState.AddModelError("m_RenewPeriod", "*Please enter Renew Period");
                    }

                    //if (string.IsNullOrEmpty(collection["txtAutoRenew"]))
                    //{
                    //    ModelState.AddModelError("m_AutoRenew ", "*Please enter Auto Renew");
                    //}
                    bool result = false;
                    string errorMessage = string.Empty;

                    if ((cusProfile.m_ProfileCode != "") && (cusProfile.m_ProfileDesc != "") && (cusProfile.m_ProfileStatus != ""))
                    {
                        if (collection.AllKeys.Any())
                        {
                            try
                            {

                                //int resultmsg = CusProfileUtils.GetChargesXMLStr(cusProfile);
                                CusProfileUtils cusutils = new CusProfileUtils();
                                List<MNProfileChargeClass> chargeList = new List<MNProfileChargeClass>();
                                chargeList.Add(new MNProfileChargeClass { ProfileCode = cusProfile.m_ProfileCode, Registration = cusProfile.m_Registration, ReNew = cusProfile.m_ReNew, PinReset = cusProfile.PinReset, ChargeAccount = cusProfile.m_ChargeAccount });

                                string chargeXML = cusutils.GetChargesXMLStr(chargeList);

                                List<MNTxnLimitClass> txnLimitList = new List<MNTxnLimitClass>();
                                foreach (var item in MNFeature)
                                {
                                    txnLimitList.Add(new MNTxnLimitClass
                                    {
                                        ProfileCode = cusProfile.m_ProfileCode.Trim(),
                                        FeatureCode = item.FeatureCode,
                                        TxnCount =item.TxnCount,
                                        PerTxnAmt =item.PerTxnAmt,
                                        PerDayAmt = item.PerDayTxnAmt,
                                        TxnAmtM = item.TxnAmtM
                                    });
                                }
                                //txnLimitList.Add(new MNTxnLimitClass { ProfileCode = cusProfile.m_ProfileCode, FeatureCode = "01", TxnCount = 10, PerTxnAmt = 500, PerDayAmt = 10000 });
                                //txnLimitList.Add(new MNTxnLimitClass { ProfileCode = cusProfile.m_ProfileCode, FeatureCode = "02", TxnCount = 10, PerTxnAmt = 600, PerDayAmt = 12000 });

                                string txnLimitXML = cusutils.GetTxnLimitXMLStr(txnLimitList);

                                //List<MNContactClass> contactList = new List<MNContactClass>();
                                //contactList.Add(new MNContactClass { ClientCode = "00112345", Address = "Kathmandu", ContactNum1 = "9841123456", ContactNum2 = "", EmailAddress = "email@gmail.com" });

                                //string contactXML = newMNUtil.GetContactXMLStr(contactList);



                                MNProfileClass newProfile = new MNProfileClass();
                                newProfile.ProfileCode = cusProfile.m_ProfileCode;
                                newProfile.ProfileDesc = cusProfile.m_ProfileDesc;
                                newProfile.AutoRenew = cusProfile.m_AutoRenew;
                                newProfile.RenewPeriod = cusProfile.m_RenewPeriod;
                                newProfile.ProfileStatus = cusProfile.m_ProfileStatus;
                                newProfile.Charge = chargeXML;
                                newProfile.TxnLimit = txnLimitXML;
                                newProfile.HasCharge = cusProfile.m_HasCharge;
                                newProfile.IsDrAlert = cusProfile.m_IsDrAlert;
                                newProfile.IsCrAlert = cusProfile.m_IsCrAlert;
                                newProfile.MinDrAlertAmt = cusProfile.m_MinDrAlertAmt;
                                newProfile.MinCrAlertAmt = cusProfile.m_MinCrAlertAmt;

                                string retMessage = string.Empty;

                                //int retValue = DAL.AddNewCustProfile(newProfile, ConnectionString, out retMessage);

                                //CusProfileUserModel.GetCustProfileDetails(cusProfile.m_ProfileCode, ConnectionString);
                                int retValue = CusProfileUserModel.AddNewCustProfile(newProfile, out retMessage);

                                if (retValue == 100)

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

                    this.TempData["cusprofile_message"] = result
                                                  ? "Customer Role is successfully added."
                                                  : "Error while inserting the information. ERROR :: "
                                                    + errorMessage;
                    this.TempData["message_class"] = result ? "success_info" : "failed_info";

                    return this.RedirectToAction("CreateCusProfile");
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }

            }
            catch (Exception ex)
            {
                CusProfileViewModel Vm = new CusProfileViewModel();
                Vm.MNFeatures = CusProfileUserModel.GetMNFeature();
                return View(Vm);
            }

        }

        [HttpGet]
        public bool CheckCustProfile(string ProfileCode) {

            if (!string.IsNullOrEmpty(ProfileCode))
            {
                var model = CusProfileUserModel.GetCustProfileDetails(ProfileCode.Trim(), false);
                if (model == null)
                {

                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;



        }


        #endregion


        #region "ManageCostumerProfile"

        public ActionResult ListCostumerProfile()
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
                return View(CusProfileUserModel.GetMNCustProfile());
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        #endregion


        #region "EditCostumerProfile"
        [HttpGet]
        public ActionResult EditCostumerProfile(string Id)
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

                var model = CusProfileUserModel.GetCustProfileDetails(Id, true);
                if (model == null)
                {
                    this.TempData["cusprofile_message"] = "Error Fetching Info For this Profile";

                    this.TempData["message_class"] = "failed_info";

                    return RedirectToAction("ListCostumerProfile", "CustomerSetup");
                }

                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpPost]
        public ActionResult EditCostumerProfile(FormCollection collection)
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

                CusProfileViewModel cusProfile = new CusProfileViewModel();
                cusProfile.m_ProfileCode = collection["txtProfileCode"].ToString().Trim();
                cusProfile.m_ProfileDesc = collection["txtProfileDesc"].ToString();
                cusProfile.m_ProfileStatus = collection["txtPStatus"].ToString();
                cusProfile.m_RenewPeriod = Convert.ToInt32(collection["txtRenewPeriod"].ToString().Trim() == "" ? "0" : collection["txtRenewPeriod"].ToString().Trim());
                //cusProfile.m_Registration = Convert.ToDecimal(collection["txtRegistration"].ToString().Trim() == "" ? "0" : collection["txtRegistration"].ToString().Trim());
                //cusProfile.m_ReNew = Convert.ToDecimal(collection["txtRenewal"].ToString().Trim() == "" ? "0" : collection["txtRenewal"].ToString().Trim());
                //cusProfile.m_PinReset = Convert.ToDecimal(collection["txtPIN"].ToString().Trim() == "" ? "0" : collection["txtPIN"].ToString().Trim());
                //cusProfile.m_ChargeAccount = collection["txtCharge"].ToString();
                //cusProfile.m_MinDrAlertAmt = Convert.ToDecimal(collection["txtMinDrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinDrAlertAmt"].ToString().Trim());
                //cusProfile.m_MinCrAlertAmt = Convert.ToDecimal(collection["txtMinCrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinCrAlertAmt"].ToString().Trim());
               
                if (collection.AllKeys.Contains("txtHasCharge"))
                {
                    cusProfile.m_HasCharge = collection["txtHasCharge"].ToString();
                    cusProfile.m_Registration = collection["txtRegistration"].ToString();// Convert.ToDecimal(collection["txtRegistration"].ToString().Trim() == "" ? "0" : collection["txtRegistration"].ToString().Trim());
                    cusProfile.m_PinReset = collection["txtPIN"].ToString();//Convert.ToDecimal(collection["txtPIN"].ToString().Trim() == "" ? "0" : collection["txtPIN"].ToString().Trim());
                    cusProfile.m_ReNew = collection["txtRenewal"].ToString();//Convert.ToDecimal(collection["txtRenewal"].ToString().Trim() == "" ? "0" : collection["txtRenewal"].ToString().Trim());
                    cusProfile.m_ChargeAccount = collection["txtCharge"].ToString();
                }
                else
                {
                    cusProfile.m_HasCharge = "F";
                    cusProfile.m_Registration = "0";
                    cusProfile.m_PinReset = "0";
                    cusProfile.m_ReNew = "0";
                    cusProfile.m_ChargeAccount = "";

                }
                if (collection.AllKeys.Contains("txtIsDrAlert"))
                {
                    cusProfile.m_IsDrAlert = collection["txtIsDrAlert"].ToString();
                    cusProfile.m_MinDrAlertAmt = Convert.ToDecimal(collection["txtMinDrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinDrAlertAmt"].ToString().Trim());
                }
                else
                {
                    cusProfile.m_IsDrAlert = "F";
                    cusProfile.m_MinDrAlertAmt = 0;
                }
                if (collection.AllKeys.Contains("txtIsCrAlert"))
                {
                    cusProfile.m_IsCrAlert = collection["txtIsCrAlert"].ToString();
                    cusProfile.m_MinCrAlertAmt = Convert.ToDecimal(collection["txtMinCrAlertAmt"].ToString().Trim() == "" ? "0" : collection["txtMinCrAlertAmt"].ToString().Trim());
                }
                else
                {

                    cusProfile.m_IsCrAlert = "F";
                    cusProfile.m_MinCrAlertAmt = 0;
                }
                if (collection.AllKeys.Contains("txtAutoRenew"))
                {
                    cusProfile.m_AutoRenew = collection["txtAutoRenew"].ToString();
                }
                else
                {
                    cusProfile.m_AutoRenew = "F";
                }
                /**/
                string[] features;
                List<MNFeatureMasterVM> MNFeature = new List<MNFeatureMasterVM>();

                if (!String.IsNullOrEmpty(collection["hidFeatures"].ToString()))
                {
                    features = collection["hidFeatures"].ToString().Split(',');
                    features = features.Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();

                    foreach (string feature in features)
                    {

                        MNFeature.Add(new MNFeatureMasterVM
                        {
                            FeatureCode = feature,
                            TxnCount = collection[String.Format("txtTxnCount{0}", feature)].ToString(),
                            PerDayTxnAmt = collection[String.Format("txtPerDayTxnAmt{0}", feature)].ToString(),
                            PerTxnAmt = collection[String.Format("txtPerTxnAmt{0}", feature)].ToString(),
                            TxnAmtM = collection[String.Format("txtTxnAmtM{0}", feature)].ToString()


                        });

                    }

                }

                if (string.IsNullOrEmpty(collection["txtProfileCode"]))
                {
                    ModelState.AddModelError("m_ProfileCode", "*Please enter Code");
                }
                if (string.IsNullOrEmpty(collection["txtProfileDesc"]))
                {
                    ModelState.AddModelError("m_ProfileDesc", "*Please enter Description");
                }
                if (string.IsNullOrEmpty(collection["txtPStatus"]))
                {
                    ModelState.AddModelError("m_ProfileStatus", "*Please enter Profile Status");
                }
                if (string.IsNullOrEmpty(collection["txtRenewPeriod"]))
                {
                    ModelState.AddModelError("m_RenewPeriod", "*Please enter Renew Period");
                }

                //if (string.IsNullOrEmpty(collection["txtAutoRenew"]))
                //{
                //    ModelState.AddModelError("m_AutoRenew ", "*Please enter Auto Renew");
                //}
                bool result = false;
                string errorMessage = string.Empty;

                if ((cusProfile.m_ProfileCode != "") && (cusProfile.m_ProfileDesc != "") && (cusProfile.m_ProfileStatus != ""))
                {
                    if (collection.AllKeys.Any())
                    {
                        try
                        {

                            //int resultmsg = CusProfileUtils.GetChargesXMLStr(cusProfile);
                            CusProfileUtils cusutils = new CusProfileUtils();
                            List<MNProfileChargeClass> chargeList = new List<MNProfileChargeClass>();
                            chargeList.Add(new MNProfileChargeClass { ProfileCode = cusProfile.m_ProfileCode, Registration = cusProfile.m_Registration, ReNew = cusProfile.m_ReNew, PinReset = cusProfile.PinReset, ChargeAccount = cusProfile.m_ChargeAccount });

                            string chargeXML = cusutils.GetChargesXMLStr(chargeList);

                            List<MNTxnLimitClass> txnLimitList = new List<MNTxnLimitClass>();
                            foreach (var item in MNFeature)
                            {
                                txnLimitList.Add(new MNTxnLimitClass
                                {
                                    ProfileCode = cusProfile.m_ProfileCode,
                                    FeatureCode = item.FeatureCode,
                                    TxnCount = item.TxnCount,
                                    PerTxnAmt = item.PerTxnAmt,
                                    PerDayAmt =  item.PerDayTxnAmt,
                                    TxnAmtM = item.TxnAmtM

                                });
                            }
                            //txnLimitList.Add(new MNTxnLimitClass { ProfileCode = cusProfile.m_ProfileCode, FeatureCode = "01", TxnCount = 10, PerTxnAmt = 500, PerDayAmt = 10000 });
                            //txnLimitList.Add(new MNTxnLimitClass { ProfileCode = cusProfile.m_ProfileCode, FeatureCode = "02", TxnCount = 10, PerTxnAmt = 600, PerDayAmt = 12000 });

                            string txnLimitXML = cusutils.GetTxnLimitXMLStr(txnLimitList);

                            //List<MNContactClass> contactList = new List<MNContactClass>();
                            //contactList.Add(new MNContactClass { ClientCode = "00112345", Address = "Kathmandu", ContactNum1 = "9841123456", ContactNum2 = "", EmailAddress = "email@gmail.com" });

                            //string contactXML = newMNUtil.GetContactXMLStr(contactList);



                            MNProfileClass newProfile = new MNProfileClass();
                            newProfile.ProfileCode = cusProfile.m_ProfileCode;
                            newProfile.ProfileDesc = cusProfile.m_ProfileDesc;
                            newProfile.AutoRenew = cusProfile.m_AutoRenew;
                            newProfile.RenewPeriod = cusProfile.m_RenewPeriod;
                            newProfile.ProfileStatus = cusProfile.m_ProfileStatus;
                            newProfile.Charge = chargeXML;
                            newProfile.TxnLimit = txnLimitXML;
                            newProfile.HasCharge = cusProfile.m_HasCharge;
                            newProfile.IsDrAlert = cusProfile.m_IsDrAlert;
                            newProfile.IsCrAlert = cusProfile.m_IsCrAlert;
                            newProfile.MinDrAlertAmt = cusProfile.m_MinDrAlertAmt;
                            newProfile.MinCrAlertAmt = cusProfile.m_MinCrAlertAmt;

                            string retMessage = string.Empty;

                            //int retValue = DAL.AddNewCustProfile(newProfile, ConnectionString, out retMessage);

                            //CusProfileUserModel.GetCustProfileDetails(cusProfile.m_ProfileCode, ConnectionString);
                            int retValue = CusProfileUserModel.UpdateCustProfile(newProfile, out retMessage);

                            if (retValue == 100)

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

                this.TempData["cusprofile_message"] = result
                                              ? "Customer Profile Successfully Updated"
                                              : "Error while updating the information. ERROR :: "
                                                + errorMessage;
                this.TempData["message_class"] = result ? "success_info" : "failed_info";
                if (result)
                    return this.RedirectToAction("ListCostumerProfile", "CustomerSetup");

                return this.RedirectToAction("EditCostumerProfile", new { Id = cusProfile.m_ProfileCode });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        #endregion


        #region View CustomerSetup Profile

        [HttpGet]
        public ActionResult ViewCostumerProfileDetail(string Id)
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

                var model = CusProfileUserModel.GetCustProfileDetails(Id, true);
                if (model == null)
                {
                    this.TempData["cusprofile_message"] = "Error Fetching Info For this Profile";

                    this.TempData["message_class"] = "failed_info";

                    return RedirectToAction("ListCostumerProfile", "CustomerSetup");
                }

                return View(model);

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        #endregion

    }
}