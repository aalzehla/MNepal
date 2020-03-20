using ThailiMerchantApp.Models;
using ThailiMerchantApp.Utilities;
using System.Data;
using System.Web.Mvc;
using System.Web.SessionState;
using System;
using System.Drawing;
using System.IO;
using ZXing;
using System.Drawing.Imaging;
using ThailiMerchantApp.Helper;

namespace ThailiMerchantApp.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult Index()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            ViewBag.userName = userName;
            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                //ViewBag.Name = name;

                ///start milayako
                MNBalance availBaln = new MNBalance();
                DataTable dtableUser1 = AvailBalnUtils.GetAvailBaln(clientCode);
                if (dtableUser1 != null && dtableUser1.Rows.Count > 0)
                {
                    availBaln.amount = dtableUser1.Rows[0]["AvailBaln"].ToString();

                    ViewBag.AvailBalnAmount = availBaln.amount;
                }

                //end milayako
                UserInfo userInfo = new UserInfo();
                DataSet DSet = ProfileUtils.GetCusDetailProfileInfoDS(clientCode);

                DataTable dtableUser = DSet.Tables["dtUserInfo"];
                DataTable dKYC = DSet.Tables["dtKycDetail"];
                DataTable dDoc = DSet.Tables["dtKycDoc"];
                DataTable dMerchants = DSet.Tables["dtMerchantInfo"];
                if (dtableUser != null && dtableUser.Rows.Count > 0)
                {
                    userInfo.ClientCode = dtableUser.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtableUser.Rows[0]["Name"].ToString();
                    userInfo.UserType = dtableUser.Rows[0]["UserType"].ToString();


                    userInfo.Status = dtableUser.Rows[0]["Status"].ToString();
                    userInfo.ContactNumber1 = dtableUser.Rows[0]["ContactNumber1"].ToString();
                    userInfo.ContactNumber2 = dtableUser.Rows[0]["ContactNumber2"].ToString();
                    userInfo.WalletNumber = dtableUser.Rows[0]["WalletNumber"].ToString();
                    userInfo.BankAccountNumber = dtableUser.Rows[0]["BankAccountNumber"].ToString();
                    userInfo.BankNo = dtableUser.Rows[0]["BankNo"].ToString();
                    userInfo.BranchCode = dtableUser.Rows[0]["BranchCode"].ToString();
                    //userInfo.SelfRegistered = dtableUser.Rows[0]["SelfRegistered"].ToString();

                    //ADDRESS 
                    userInfo.PStreet = dtableUser.Rows[0]["PStreet"].ToString();
                    userInfo.PDistrict = getDistrictName(dtableUser.Rows[0]["PDistrictID"].ToString()); //get district name from district id
                    userInfo.PProvince = dtableUser.Rows[0]["PProvince"].ToString();
                    string PStreet = userInfo.PStreet != "" ? (userInfo.PStreet + ",") : ("");
                    string Address = PStreet + " " + userInfo.PDistrict + "," + " " + "Province No." + userInfo.PProvince;
                    ViewBag.Address = Address;
                    //ADDRESS END

                    ViewBag.Name = userInfo.Name;
                    ViewBag.Status = userInfo.Status;
                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.ContactNumber2 = userInfo.ContactNumber2;
                    ViewBag.WalletNumber = userInfo.WalletNumber;
                    ViewBag.BankAccountNumber = userInfo.BankAccountNumber;
                    ViewBag.BankNo = userInfo.BankNo;
                    ViewBag.BranchCode = userInfo.BranchCode;
                    //ViewBag.SelfReg = userInfo.SelfRegistered;
                }
                if (dKYC != null && dKYC.Rows.Count > 0)
                {
                    //CITIZENSHIP AND PAN NUMBER DETAILS STARTS
                    userInfo.Citizenship = dKYC.Rows[0]["CitizenshipNo"].ToString();
                    userInfo.CitizenshipPlaceOfIssue = dKYC.Rows[0]["CitizenPlaceOfIssue"].ToString();
                    userInfo.CitizenshipIssueDate = dKYC.Rows[0]["CitizenIssueDate"].ToString();
                    userInfo.PanNo = dKYC.Rows[0]["PANNumber"].ToString();

                    ViewBag.Citizenship = userInfo.Citizenship;
                    ViewBag.CitizenshipPlaceOfIssue = userInfo.CitizenshipPlaceOfIssue;
                    ViewBag.CitizenshipIssueDate = userInfo.CitizenshipIssueDate;
                    ViewBag.PanNo = userInfo.PanNo;
                    //CITIZENSHIP AND PAN NUMBER DETAILS ENDS
                    //PROPRIETOR NAME STARTS
                    userInfo.FName = dKYC.Rows[0]["FName"].ToString();
                    userInfo.MName = dKYC.Rows[0]["MName"].ToString();
                    userInfo.LName = dKYC.Rows[0]["LName"].ToString();

                    ViewBag.ProprietorName = userInfo.FName + " " + userInfo.MName + " " + userInfo.LName;
                    //PROPRIETOR NAME ENDS
                }

                if (dDoc != null && dDoc.Rows.Count > 0)
                {

                    //IMAGE START
                    userInfo.Document = dDoc.Rows[0]["DocType"].ToString();
                    userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();
                    userInfo.FrontImage = dDoc.Rows[0]["FrontImage"].ToString();
                    userInfo.BackImage = dDoc.Rows[0]["BackImage"].ToString();
                    userInfo.RegCertificateImage = dDoc.Rows[0]["RegCertImage"].ToString();
                    userInfo.TaxClearFrontImage = dDoc.Rows[0]["TaxClearFrontImage"].ToString();
                    userInfo.TaxClearBackImage = dDoc.Rows[0]["TaxClearBackImage"].ToString();

                    ViewBag.DocType = userInfo.Document;
                    ViewBag.FrontImage = userInfo.FrontImage;
                    ViewBag.BackImage = userInfo.BackImage;
                    ViewBag.PassportImage = userInfo.PassportImage;
                    ViewBag.RegCertificateImage = userInfo.RegCertificateImage;
                    ViewBag.TaxClearFrontImage = userInfo.TaxClearFrontImage;
                    ViewBag.TaxClearBackImage = userInfo.TaxClearBackImage;
                    //IMAGE END

                }
                if (dMerchants != null && dMerchants.Rows.Count > 0)
                {
                    //userInfo.FName = dMerchants.Rows[0]["FirstName"].ToString();
                    //userInfo.MName = dMerchants.Rows[0]["MiddleName"].ToString();
                    //userInfo.LName = dMerchants.Rows[0]["LastName"].ToString();
                    //ViewBag.ProprietorName = userInfo.FName + " " + userInfo.MName + " " + userInfo.LName;

                    userInfo.MerchantCategory = getMerchantCategory(clientCode);
                    userInfo.MerchantId = dMerchants.Rows[0]["mid"].ToString();
                    userInfo.MName = dMerchants.Rows[0]["mname"].ToString();

                }
                //Check Link Bank Account
                string HasBankKYC = string.Empty;
                DataTable dtableUserCheckLinkBankAcc = ProfileUtils.CheckLinkBankAcc(userName);
                if (dtableUserCheckLinkBankAcc != null && dtableUserCheckLinkBankAcc.Rows.Count > 0)
                {
                    userInfo.HasBankKYC = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                    ViewBag.HasBankKYC = userInfo.HasBankKYC;

                }

                //for displaying bank account no. to hasBankKYC T
                if (ViewBag.HasBankKYC != "T")
                {
                    ViewBag.BankAccountNumber = " ";
                    ViewBag.BankName = " ";
                }
                else
                {
                    ViewBag.BankAccountNumber = userInfo.BankAccountNumber;
                    ViewBag.BankName = "NIBL";
                }

                
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        public ActionResult AdminProfile()
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

                UserInfo userInfo = new UserInfo();
                DataTable dtableUser = ProfileUtils.GetUserProfileInfo(clientCode);
                if (dtableUser != null && dtableUser.Rows.Count > 0)
                {

                    userInfo.Name = dtableUser.Rows[0]["Name"].ToString();
                    userInfo.UserName = dtableUser.Rows[0]["UserName"].ToString();
                    userInfo.UserBranchName = dtableUser.Rows[0]["UserBranchName"].ToString();
                    userInfo.EmailAddress = dtableUser.Rows[0]["EmailAddress"].ToString();
                    userInfo.AProfileName = dtableUser.Rows[0]["AProfileName"].ToString();
                    userInfo.COC = dtableUser.Rows[0]["COC"].ToString();
                    userInfo.Status = dtableUser.Rows[0]["Status"].ToString();
                    userInfo.BankNo = dtableUser.Rows[0]["BankNo"].ToString();

                    //userInfo.ClientCode = dtableUser.Rows[0]["ClientCode"].ToString();
                    //userInfo.UserType = dtableUser.Rows[0]["UserType"].ToString();
                    //userInfo.Address = dtableUser.Rows[0]["Address"].ToString();                   
                    //userInfo.ContactNumber1 = dtableUser.Rows[0]["ContactNumber1"].ToString();
                    //userInfo.ContactNumber2 = dtableUser.Rows[0]["ContactNumber2"].ToString();
                    //userInfo.WalletNumber = dtableUser.Rows[0]["WalletNumber"].ToString();
                    //userInfo.BankAccountNumber = dtableUser.Rows[0]["BankAccountNumber"].ToString();                    
                    //userInfo.BranchCode = dtableUser.Rows[0]["BranchCode"].ToString();

                    ///
                    ViewBag.UserName = userInfo.UserName;
                    ViewBag.UserBranchName = userInfo.UserBranchName;
                    ViewBag.EmailAddress = userInfo.EmailAddress;
                    ViewBag.AProfileName = userInfo.AProfileName;
                    ViewBag.COC = userInfo.COC;
                    ViewBag.Status = userInfo.Status;
                    ViewBag.BankNo = userInfo.BankNo;
                    ///

                    //ViewBag.Address = userInfo.Address;                    
                    //ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    //ViewBag.ContactNumber2 = userInfo.ContactNumber2;
                    //ViewBag.WalletNumber = userInfo.WalletNumber;
                    //ViewBag.BankAccountNumber = userInfo.BankAccountNumber;                   
                    //ViewBag.BranchCode = userInfo.BranchCode;

                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        //To get the District Name from District Name
        DAL objdal = new DAL();
        public string getDistrictName(string id)
        {
            string districtstring = "select Name from MNDistrict where DistrictID='" + id + "'";


            DataTable dt = new DataTable();
            dt = objdal.MyMethod(districtstring);
            string name = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                name = row["Name"].ToString();


            }
            return name;


        }

        public string getMerchantCategory(string id)
        {
            string catagoryName = "select Name from MNMerchant where Id = (select catid from MNMerchants where ClientCode='" + id + "')";
            DataTable dt = new DataTable();
            dt = objdal.MyMethod(catagoryName);
            string name = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                name = row["Name"].ToString();
            }
            return name;
        }

        
    }
}