using CustApp.Models;
using CustApp.Utilities;
using System.Data;
using System.Web.Mvc;
using System.Web.SessionState;
using System;

namespace CustApp.Controllers
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

                UserInfo userInfo = new UserInfo();

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
                    userInfo.HasBankKYC = dtableUserCheckLinkBankAcc.Rows[0]["HasBankKYC"].ToString();

                    ViewBag.HasBankKYC = userInfo.HasBankKYC;
                }
                //end milayako
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
                    //added//
                    userInfo.CHouseNo = dtableUser.Rows[0]["CHouseNo"].ToString();
                    userInfo.CStreet = dtableUser.Rows[0]["CStreet"].ToString();
                    userInfo.CWardNo = dtableUser.Rows[0]["CWardNo"].ToString();
                    userInfo.CDistrict = getDistrictName(dtableUser.Rows[0]["CDistrictID"].ToString()); //get district name from district id
                    userInfo.CVDC = dtableUser.Rows[0]["CMunicipalityVDC"].ToString();
                    userInfo.CProvince = dtableUser.Rows[0]["CProvince"].ToString();
                    userInfo.PHouseNo = dtableUser.Rows[0]["PHouseNo"].ToString();
                    userInfo.PStreet = dtableUser.Rows[0]["PStreet"].ToString();
                    userInfo.PWardNo = dtableUser.Rows[0]["PWardNo"].ToString();
                    userInfo.PDistrict = getDistrictName(dKYC.Rows[0]["PDistrict"].ToString()); //get district name from district id
                    userInfo.PVDC = dtableUser.Rows[0]["PMunicipalityVDC"].ToString();
                    userInfo.PProvince = dtableUser.Rows[0]["PProvince"].ToString();
                    userInfo.SelfRegistered = dtableUser.Rows[0]["SelfRegistered"].ToString();

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
                        if (ViewBag.hasKYC != "T")
                    {
                        ViewBag.Address = " ";
                        ViewBag.Status = " ";
                        ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                        ViewBag.ContactNumber2 = userInfo.ContactNumber2;
                        ViewBag.WalletNumber = " ";
                        //ViewBag.BankAccountNumber = " ";
                        ViewBag.BankNo = " ";
                        ViewBag.BranchCode = userInfo.BranchCode;
                        //added//
                        ViewBag.CAddress = " ";
                        ViewBag.PAddress = " ";

                        ViewBag.SelfReg = " ";
                    }
                    else
                    {
                        ViewBag.Address = userInfo.Address;
                        ViewBag.Status = userInfo.Status;
                        ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                        ViewBag.ContactNumber2 = userInfo.ContactNumber2;
                        ViewBag.WalletNumber = userInfo.WalletNumber;
                        //ViewBag.BankAccountNumber = userInfo.BankAccountNumber;
                        ViewBag.BankNo = userInfo.BankNo;
                        ViewBag.BranchCode = userInfo.BranchCode;
                        //added//
                        string CAddress = userInfo.CStreet + "," + " " + userInfo.CDistrict + "," + " " + "Province No." + userInfo.CProvince;
                        string PAddress = userInfo.PStreet + "," + " " + userInfo.PDistrict + "," + " " + "Province No. " + userInfo.PProvince;
                        ViewBag.CAddress = CAddress;
                        ViewBag.PAddress = PAddress;

                        ViewBag.SelfReg = userInfo.SelfRegistered;
                        //added//
                    }

                }

                 if (dKYC != null && dKYC.Rows.Count > 0)
                {
                    userInfo.DOB = dKYC.Rows[0]["DateOfBirth"].ToString().Split()[0];
                    userInfo.Gender = dKYC.Rows[0]["Gender"].ToString();
                    userInfo.FatherInlawName = dKYC.Rows[0]["FatherInlaw"].ToString();
                    userInfo.SpouseName = dKYC.Rows[0]["SpouseName"].ToString();
                    userInfo.GrandFatherName = dKYC.Rows[0]["GFathersName"].ToString();
                    userInfo.FatherName = dKYC.Rows[0]["FathersName"].ToString();
                    userInfo.CustStatus = dKYC.Rows[0]["CustStatus"].ToString();
                    if (ViewBag.hasKYC != "T")
                    {
                        ViewBag.DOB = " ";
                        ViewBag.Gender = " ";
                        ViewBag.FatherInlawName = " ";
                        ViewBag.SpouseName = " ";
                        ViewBag.GrandFatherName = " ";
                        ViewBag.FatherName = " ";
                        ViewBag.CustStatus = userInfo.CustStatus;
                    }
                    else
                    {
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

                        ViewBag.Gender = userInfo.Gender;
                        ViewBag.FatherInlawName = userInfo.FatherInlawName;
                        ViewBag.SpouseName = userInfo.SpouseName;
                        ViewBag.GrandFatherName = userInfo.GrandFatherName;
                        ViewBag.FatherName = userInfo.FatherName;
                        ViewBag.CustStatus = userInfo.CustStatus;
                    }
                }

                 if(dDoc != null && dDoc.Rows.Count > 0)
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