using MNepalWeb.Models;
using MNepalWeb.Utilities;
using System.Data;
using System.Web.Mvc;
using System.Web.SessionState;

namespace MNepalWeb.Controllers
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

                UserInfo userInfo = new UserInfo();
                DataTable dtableUser = ProfileUtils.GetUserProfileInfo(clientCode);
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

                    ViewBag.Address = userInfo.Address;
                    ViewBag.Status = userInfo.Status;
                    ViewBag.ContactNumber1 = userInfo.ContactNumber1;
                    ViewBag.ContactNumber2 = userInfo.ContactNumber2;
                    ViewBag.WalletNumber = userInfo.WalletNumber;
                    ViewBag.BankAccountNumber = userInfo.BankAccountNumber;
                    ViewBag.BankNo = userInfo.BankNo;
                    ViewBag.BranchCode = userInfo.BranchCode;

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

        //edit end
    }
}