using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Utilities;
using System.Data;
using System.Web.Mvc;
using System.Web.SessionState;
using System;

namespace ThailiMNepalAgent.Controllers
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

            TempData["userType"] = userType;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

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
                DataTable dKYC= DSet.Tables["dtKycDetail"];
                DataTable dDoc = DSet.Tables["dtKycDoc"];
                if (dtableUser != null && dtableUser.Rows.Count > 0)
                {
                    userInfo.ClientCode = dtableUser.Rows[0]["ClientCode"].ToString();
                    userInfo.Name = dtableUser.Rows[0]["Name"].ToString();
                    userInfo.UserType = dtableUser.Rows[0]["UserType"].ToString();

                    userInfo.Address = dtableUser.Rows[0]["Address"].ToString();
                    userInfo.Status = dtableUser.Rows[0]["Status"].ToString();
                    userInfo.ContactNumber1 = dtableUser.Rows[0]["ContactNumber1"].ToString();
                    userInfo.ContactNumber2 = dtableUser.Rows[0]["ContactNumber2"].ToString();
                    userInfo.WalletNumber = dtableUser.Rows[0]["WalletNumber"].ToString();
                    userInfo.BankAccountNumber = dtableUser.Rows[0]["BankAccountNumber"].ToString();
                    userInfo.BankNo = dtableUser.Rows[0]["BankNo"].ToString();
                    userInfo.BranchCode = dtableUser.Rows[0]["BranchCode"].ToString();
                    userInfo.SelfRegistered = dtableUser.Rows[0]["SelfRegistered"].ToString();


                    ViewBag.Address = userInfo.Address;
                    ViewBag.Status = userInfo.Status;
                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.ContactNumber2 = userInfo.ContactNumber2;
                    ViewBag.WalletNumber = userInfo.WalletNumber;
                    ViewBag.BankAccountNumber = userInfo.BankAccountNumber;
                    ViewBag.BankNo = userInfo.BankNo;
                    ViewBag.BranchCode = userInfo.BranchCode;
                    ViewBag.SelfReg = userInfo.SelfRegistered;
                }
                 if (dKYC != null && dKYC.Rows.Count > 0)
                {
                    //added//
                    userInfo.CHouseNo = dKYC.Rows[0]["CHouseNo"].ToString();
                    userInfo.CStreet = dKYC.Rows[0]["CStreet"].ToString();
                    userInfo.CWardNo = dKYC.Rows[0]["CWardNo"].ToString();
                    userInfo.CDistrict = getDistrictName(dKYC.Rows[0]["CDistrict"].ToString()); //get district name from district id
                    userInfo.CVDC = dKYC.Rows[0]["CMunicipalityVDC"].ToString();
                    userInfo.CProvince = dKYC.Rows[0]["CProvince"].ToString();
                    userInfo.PHouseNo = dKYC.Rows[0]["PHouseNo"].ToString();
                    userInfo.PStreet = dKYC.Rows[0]["PStreet"].ToString();
                    userInfo.PWardNo = dKYC.Rows[0]["PWardNo"].ToString();
                    userInfo.PDistrict = getDistrictName(dKYC.Rows[0]["PDistrict"].ToString()); //get district name from district id
                    userInfo.PVDC = dKYC.Rows[0]["PMunicipalityVDC"].ToString();
                    userInfo.PProvince = dKYC.Rows[0]["PProvince"].ToString();
                    
                    userInfo.DOB = dKYC.Rows[0]["DateOfBirth"].ToString().Split()[0];
                    userInfo.Gender = dKYC.Rows[0]["Gender"].ToString();
                    userInfo.FatherInlawName = dKYC.Rows[0]["FatherInlaw"].ToString();
                    userInfo.SpouseName = dKYC.Rows[0]["SpouseName"].ToString();
                    userInfo.GrandFatherName = dKYC.Rows[0]["GFathersName"].ToString();
                    userInfo.FatherName = dKYC.Rows[0]["FathersName"].ToString();
                    userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();

                    ViewBag.DOB = userInfo.DOB;
                    ViewBag.Gender = userInfo.Gender;
                    ViewBag.FatherInlawName = userInfo.FatherInlawName;
                    ViewBag.SpouseName = userInfo.SpouseName;
                    ViewBag.GrandFatherName = userInfo.GrandFatherName;
                    ViewBag.FatherName = userInfo.FatherName;
                    ViewBag.CustStatus = userInfo.CustStatus;

                    if (userInfo.DOB != "")
                    {
                        DateTime dt = Convert.ToDateTime(userInfo.DOB);
                        string x = dt.ToString("dd/MM/yyyy");
                        ViewBag.DOB = x;
                    }
                    else
                    {
                        ViewBag.DOB = userInfo.DOB;
                    }

                    //added//
                    string CAddress = userInfo.CStreet + "," + "" + userInfo.CDistrict + "," + "" + "Province No." + userInfo.CProvince;
                    string PAddress = userInfo.PStreet + "," + "" + userInfo.PDistrict + "," + "" + "Province No." + userInfo.PProvince;
                    ViewBag.CAddress = CAddress;
                    ViewBag.PAddress = PAddress;

                    
                    //added//
                }

                if (dDoc != null && dDoc.Rows.Count > 0)
                {
                    userInfo.Document = dDoc.Rows[0]["DocType"].ToString();
                    userInfo.FrontImage = dDoc.Rows[0]["FrontImage"].ToString();
                    userInfo.BackImage = dDoc.Rows[0]["BackImage"].ToString();
                    userInfo.PassportImage = dDoc.Rows[0]["PassportImage"].ToString();
                    
                    ViewBag.DocType = userInfo.Document;
                    ViewBag.FrontImage = userInfo.FrontImage;
                    ViewBag.BackImage = userInfo.BackImage;
                    ViewBag.PassportImage = userInfo.PassportImage;


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

        //edit start
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

        //edit end
    }
}