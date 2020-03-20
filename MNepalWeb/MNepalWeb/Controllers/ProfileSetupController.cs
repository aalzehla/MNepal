using MNepalWeb.Connection;
using MNepalWeb.Models;
using MNepalWeb.Settings;
using MNepalWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.SessionState;

namespace MNepalWeb.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    public class ProfileSetupController : Controller
    {
        // GET: ProfileSetup
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

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #region "Create Admin Profile"


        // GET: ProfileSetup/CreateAdmin
        public ActionResult CreateAdmin()
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

                List<V_MNClientDetail> ClientDetail = new List<V_MNClientDetail>();
                DataTable dtblClientDetail = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM v_MNClientDetail WHERE UserType='admin'", sqlCon);
                    sqlDa.Fill(dtblClientDetail);
                    foreach (DataRow row in dtblClientDetail.Rows)
                    {
                        V_MNClientDetail cd = new V_MNClientDetail
                        {
                            ClientCode = row["ClientCode"].ToString(),
                            Name = row["Name"].ToString(),
                            Address = row["Address"].ToString(),
                            PIN = row["PIN"].ToString(),
                            Status = row["Status"].ToString(),
                            ContactNumber1 = row["ContactNumber1"].ToString(),
                            UserName = row["UserName"].ToString(),
                            UserType = row["UserType"].ToString(),
                            WalletNumber = row["WalletNumber"].ToString(),
                            BankAccountNumber = row["BankAccountNumber"].ToString(),
                            BankNo = row["BankNo"].ToString(),
                            BranchCode = row["BranchCode"].ToString()
                        };
                        ClientDetail.Add(cd);
                    }
                    sqlCon.Close();
                }
                return View(ClientDetail);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // GET: ProfileSetup/AdminDetail
        [HttpGet]
        public ActionResult AdminDetail(string SearchCol, string txtMobileNo, string txtName)
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

                List<V_MNClientDetail> List = new List<V_MNClientDetail>();
                if (SearchCol == "Mobile Number")
                {
                    DataTable dtblClientDetail = new DataTable();
                    using (SqlConnection sqlConn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                    {
                        sqlConn.Open();
                        SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM v_MNClientDetail Where UserType='admin' AND ContactNumber1 LIKE @ContactNumber1", sqlConn);
                        sqlDa.SelectCommand.Parameters.AddWithValue("@ContactNumber1", "%" + txtMobileNo + "%");
                        sqlDa.Fill(dtblClientDetail);

                        foreach (DataRow row in dtblClientDetail.Rows)
                        {

                            V_MNClientDetail cd = new V_MNClientDetail
                            {
                                ClientCode = row["ClientCode"].ToString(),
                                Name = row["Name"].ToString(),
                                Address = row["Address"].ToString(),
                                PIN = row["PIN"].ToString(),
                                Status = row["Status"].ToString(),
                                ContactNumber1 = row["ContactNumber1"].ToString(),
                                UserName = row["UserName"].ToString(),
                                UserType = row["UserType"].ToString(),
                                WalletNumber = row["WalletNumber"].ToString(),
                                BankAccountNumber = row["BankAccountNumber"].ToString()
                            };

                            List.Add(cd);
                            ViewData["dtableAdminDetail"] = row;
                        }
                    }
                    return View(List);
                }
                else if (SearchCol == "Name")
                {

                    DataTable dtblClientDetail = new DataTable();
                    using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                    {
                        sqlCon.Open();
                        SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM v_MNClientDetail Where UserType='admin' AND Name LIKE @Name", sqlCon);
                        sqlDa.SelectCommand.Parameters.AddWithValue("@Name", "%" + txtName + "%");
                        sqlDa.Fill(dtblClientDetail);

                        foreach (DataRow row in dtblClientDetail.Rows)
                        {

                            V_MNClientDetail cd = new V_MNClientDetail
                            {
                                ClientCode = row["ClientCode"].ToString(),
                                Name = row["Name"].ToString(),
                                Address = row["Address"].ToString(),
                                PIN = row["PIN"].ToString(),
                                Status = row["Status"].ToString(),
                                ContactNumber1 = row["ContactNumber1"].ToString(),
                                UserName = row["UserName"].ToString(),
                                UserType = row["UserType"].ToString(),
                                WalletNumber = row["WalletNumber"].ToString(),
                                BankAccountNumber = row["BankAccountNumber"].ToString()
                            };

                            List.Add(cd);
                            ViewData["dtableAdminDetail"] = row;
                        }

                    }
                    return View(List);
                }
                else
                    return View(List);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        // GET: ProfileSetup/CreateAdminProfile
        [HttpGet]
        public ActionResult CreateAdminProfile(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["admin_messsage"] != null)
            {
                this.ViewData["admin_messsage"] = this.TempData["admin_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                string displayMessage = string.Empty;
                string messageClass = string.Empty;

                if (id != "")
                {
                    DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(id);
                    if (dtblRegistration.Rows.Count > 0)
                    {
                        UserInfo userInfo =
                            new UserInfo
                            {
                                ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString(),
                                Name = dtblRegistration.Rows[0]["Name"].ToString(),
                                Address = dtblRegistration.Rows[0]["Address"].ToString(),
                                PIN = dtblRegistration.Rows[0]["PIN"].ToString(),
                                Status = dtblRegistration.Rows[0]["Status"].ToString(),
                                ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString(),
                                ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString(),
                                UserName = dtblRegistration.Rows[0]["UserName"].ToString(),
                                UserType = dtblRegistration.Rows[0]["UserType"].ToString(),
                                IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString(),
                                IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString(),
                                WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString(),
                                BankNo = dtblRegistration.Rows[0]["BankNo"].ToString(),
                                BranchCode = dtblRegistration.Rows[0]["BranchCode"].ToString(),
                                BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString()
                            };

                        return View(userInfo);
                    }
                }
                else
                {
                    displayMessage = "Please Check the details";
                    messageClass = CssSetting.FailedMessageClass;
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }


        // POST: ProfileSetup/CreateAdminProfile/1
        [HttpPost]
        public ActionResult CreateAdminProfile(string ClientCode, UserInfo userInfo)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            string displayMessage = null;
            string messageClass = null;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                try
                {
                    if ((ClientCode != "") || (ClientCode != null))
                    {
                        bool isUpdated = false;
                        isUpdated = CustomerUtils.UpdateCustomerUserInfo(userInfo);
                        displayMessage = isUpdated
                                                ? "The Admin User Information has successfully been updated."
                                                : "Error while updating Member information";
                        messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                    }
                }
                catch (Exception ex)
                {
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["admin_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;

                return RedirectToAction("CreateAdminProfile");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion


        #region "Modify Admin"

        // GET: ProfileSetup/ModifyAdmin
        [HttpGet]
        public ActionResult ModifyAdmin()
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

                List<UserInfo> RegistrationtList = new List<UserInfo>();
                DataTable dtblRegistration = ProfileUtils.GetAdminProfile();

                foreach (DataRow row in dtblRegistration.Rows)
                {
                    UserInfo regobj = new UserInfo
                    {
                        ClientCode = row["ClientCode"].ToString(),
                        Name = row["Name"].ToString(),
                        Address = row["Address"].ToString(),
                        PIN = row["PIN"].ToString(),
                        Status = row["Status"].ToString(),
                        ContactNumber1 = row["ContactNumber1"].ToString(),
                        ContactNumber2 = row["ContactNumber2"].ToString(),
                        UserName = row["UserName"].ToString(),
                        UserType = row["userType"].ToString()
                    };

                    RegistrationtList.Add(regobj);
                }
                return View(RegistrationtList);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // GET: ProfileSetup/ModifyStatusAdmin
        [HttpGet]
        public ActionResult ModifyStatusAdmin()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["adminmodify_messsage"] != null)
            {
                this.ViewData["adminmodify_messsage"] = this.TempData["adminmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCode);
                if (dtblRegistration.Rows.Count == 1)
                {
                    UserInfo userInfo = new UserInfo
                    {
                        ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString(),
                        Name = dtblRegistration.Rows[0]["Name"].ToString(),
                        Address = dtblRegistration.Rows[0]["Address"].ToString(),
                        PIN = dtblRegistration.Rows[0]["PIN"].ToString(),
                        Status = dtblRegistration.Rows[0]["Status"].ToString(),
                        ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString(),
                        ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString(),
                        UserName = dtblRegistration.Rows[0]["UserName"].ToString(),
                        UserType = dtblRegistration.Rows[0]["UserType"].ToString(),
                        IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString(),
                        IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString(),
                        WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString(),
                        BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString()
                    };

                    return View(userInfo);
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }


        // POST: ProfileSetup/ModifyStatusAdmin/1
        [HttpPost]
        public ActionResult ModifyStatusAdmin(string ClientCode, UserInfo userInfo)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            string displayMessage = null;
            string messageClass = null;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                try
                {
                    if ((ClientCode != "") || (ClientCode != null))
                    {
                        bool isUpdated = false;
                        isUpdated = CustomerUtils.UpdateCustomerUserInfo(userInfo);
                        displayMessage = isUpdated
                                                ? "The Admin Information has successfully been updated."
                                                : "Error while updating Admin information";
                        messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                    }
                }
                catch (Exception ex)
                {
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["adminmodify_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;

                return RedirectToAction("ModifyStatusAdmin");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion


        #region "Modify Agent"

        // GET: ProfileSetup/ModifyAgent
        [HttpGet]
        public ActionResult ModifyAgent()
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["agentmodify_messsage"] != null)
            {
                this.ViewData["agentmodify_messsage"] = this.TempData["agentmodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(clientCode);
                if (dtblRegistration.Rows.Count == 1)
                {
                    UserInfo userInfo = new UserInfo
                    {
                        ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString(),
                        Name = dtblRegistration.Rows[0]["Name"].ToString(),
                        Address = dtblRegistration.Rows[0]["Address"].ToString(),
                        PIN = dtblRegistration.Rows[0]["PIN"].ToString(),
                        Status = dtblRegistration.Rows[0]["Status"].ToString(),
                        ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString(),
                        ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString(),
                        UserName = dtblRegistration.Rows[0]["UserName"].ToString(),
                        UserType = dtblRegistration.Rows[0]["UserType"].ToString(),
                        IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString(),
                        IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString(),
                        WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString(),
                        BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString()
                    };

                    return View(userInfo);
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return View();

        }


        // POST: ProfileSetup/ModifyAgent/1
        [HttpPost]
        public ActionResult ModifyAgent(string ClientCode, UserInfo userInfo)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            string displayMessage = null;
            string messageClass = null;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                try
                {
                    if ((ClientCode != "") || (ClientCode != null))
                    {
                        bool isUpdated = false;
                        isUpdated = CustomerUtils.UpdateCustomerUserInfo(userInfo);
                        displayMessage = isUpdated
                                                ? "The Agent User Information has successfully been updated."
                                                : "Error while updating Member information";
                        messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                    }
                }
                catch (Exception ex)
                {
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["agentmodify_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;
                return RedirectToAction("ModifyAgent");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion


        #region "Create Customer"


        // GET: ProfileSetup/CreateCustomer
        public ActionResult CreateCustomer()
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

                List<V_MNClientDetail> ClientDetail = new List<V_MNClientDetail>();
                DataTable dtblClientDetail = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM v_MNClientDetail WHERE UserType='user'", sqlCon);
                    sqlDa.Fill(dtblClientDetail);
                    foreach (DataRow row in dtblClientDetail.Rows)
                    {
                        V_MNClientDetail cd = new V_MNClientDetail
                        {
                            ClientCode = row["ClientCode"].ToString(),
                            Name = row["Name"].ToString(),
                            Address = row["Address"].ToString(),
                            PIN = row["PIN"].ToString(),
                            Status = row["Status"].ToString(),
                            ContactNumber1 = row["ContactNumber1"].ToString(),
                            UserName = row["UserName"].ToString(),
                            UserType = row["UserType"].ToString(),
                            WalletNumber = row["WalletNumber"].ToString(),
                            BankAccountNumber = row["BankAccountNumber"].ToString(),
                            BankNo = row["BankNo"].ToString(),
                            BranchCode = row["BranchCode"].ToString()
                        };

                        ClientDetail.Add(cd);
                    }
                    sqlCon.Close();
                }
                return View(ClientDetail);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // GET: ProfileSetup/CustomerDetail
        [HttpGet]
        public ActionResult CustomerDetail(string SearchCol, string txtMobileNo, string txtName)
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

                List<V_MNClientDetail> List = new List<V_MNClientDetail>();
                if (SearchCol == "Mobile Number")
                {
                    DataTable dtblClientDetail = new DataTable();
                    using (SqlConnection sqlConn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                    {
                        sqlConn.Open();
                        SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM v_MNClientDetail Where UserType='user' AND ContactNumber1 LIKE @ContactNumber1", sqlConn);
                        sqlDa.SelectCommand.Parameters.AddWithValue("@ContactNumber1", "%" + txtMobileNo + "%");
                        sqlDa.Fill(dtblClientDetail);

                        foreach (DataRow row in dtblClientDetail.Rows)
                        {

                            V_MNClientDetail cd = new V_MNClientDetail
                            {
                                ClientCode = row["ClientCode"].ToString(),
                                Name = row["Name"].ToString(),
                                Address = row["Address"].ToString(),
                                PIN = row["PIN"].ToString(),
                                Status = row["Status"].ToString(),
                                ContactNumber1 = row["ContactNumber1"].ToString(),
                                UserName = row["UserName"].ToString(),
                                UserType = row["UserType"].ToString(),
                                WalletNumber = row["WalletNumber"].ToString(),
                                BankAccountNumber = row["BankAccountNumber"].ToString()
                            };

                            List.Add(cd);
                        }
                    }
                    return View(List);
                }
                else if (SearchCol == "Name")
                {

                    DataTable dtblClientDetail = new DataTable();
                    using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                    {
                        sqlCon.Open();
                        SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM v_MNClientDetail Where UserType='user' AND Name LIKE @Name", sqlCon);
                        sqlDa.SelectCommand.Parameters.AddWithValue("@Name", "%" + txtName + "%");
                        sqlDa.Fill(dtblClientDetail);

                        foreach (DataRow row in dtblClientDetail.Rows)
                        {
                            V_MNClientDetail cd = new V_MNClientDetail
                            {
                                ClientCode = row["ClientCode"].ToString(),
                                Name = row["Name"].ToString(),
                                Address = row["Address"].ToString(),
                                PIN = row["PIN"].ToString(),
                                Status = row["Status"].ToString(),
                                ContactNumber1 = row["ContactNumber1"].ToString(),
                                UserName = row["UserName"].ToString(),
                                UserType = row["UserType"].ToString(),
                                WalletNumber = row["WalletNumber"].ToString(),
                                BankAccountNumber = row["BankAccountNumber"].ToString()
                            };

                            List.Add(cd);
                        }

                    }
                    return View(List);
                }
                else
                    return View(List);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        // GET: ProfileSetup/CreateCustomerProfile
        [HttpGet]
        public ActionResult CreateCustomerProfile(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["customermodify_messsage"] != null)
            {
                this.ViewData["customermodify_messsage"] = this.TempData["customermodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                string displayMessage = string.Empty;
                string messageClass = string.Empty;

                if (id != "")
                {
                    DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(id);
                    if (dtblRegistration.Rows.Count > 0)
                    {
                        UserInfo userInfo =
                            new UserInfo
                            {
                                ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString(),
                                Name = dtblRegistration.Rows[0]["Name"].ToString(),
                                Address = dtblRegistration.Rows[0]["Address"].ToString(),
                                PIN = dtblRegistration.Rows[0]["PIN"].ToString(),
                                Status = dtblRegistration.Rows[0]["Status"].ToString(),
                                ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString(),
                                ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString(),
                                UserName = dtblRegistration.Rows[0]["UserName"].ToString(),
                                UserType = dtblRegistration.Rows[0]["UserType"].ToString(),
                                IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString(),
                                IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString(),
                                WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString(),
                                BankNo = dtblRegistration.Rows[0]["BankNo"].ToString(),
                                BranchCode = dtblRegistration.Rows[0]["BranchCode"].ToString(),
                                BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString()
                            };

                        return View(userInfo);
                    }
                }
                else
                {
                    displayMessage = "Please Check the details";
                    messageClass = CssSetting.FailedMessageClass;
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }


        // POST: ProfileSetup/CreateCustomerProfile/1
        [HttpPost]
        public ActionResult CreateCustomerProfile(string ClientCode, UserInfo userInfo)
        {
            try
            {
                string userName = (string)Session["LOGGED_USERNAME"];
                string clientCode = (string)Session["LOGGEDUSER_ID"];
                string name = (string)Session["LOGGEDUSER_NAME"];
                string userType = (string)Session["LOGGED_USERTYPE"];

                TempData["userType"] = userType;

                string displayMessage = null;
                string messageClass = null;

                if (TempData["userType"] != null)
                {
                    this.ViewData["userType"] = this.TempData["userType"];
                    ViewBag.UserType = this.TempData["userType"];
                    ViewBag.Name = name;

                    try
                    {
                        if ((ClientCode != "") || (ClientCode != null))
                        {
                            bool isUpdated = false;
                            isUpdated = CustomerUtils.UpdateCustomerUserInfo(userInfo);
                            displayMessage = isUpdated
                                                    ? "The User Information has successfully been updated."
                                                    : "Error while updating Member information";
                            messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                        }
                    }
                    catch (Exception ex)
                    {
                        displayMessage = ex.Message;
                        messageClass = CssSetting.FailedMessageClass;
                    }

                    this.TempData["customermodify_messsage"] = displayMessage;
                    this.TempData["message_class"] = messageClass;

                    return RedirectToAction("CreateCustomerProfile");
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


        #endregion


        #region "Modify Customer"

        // GET: ProfileSetup/ModifyCustomer
        [HttpGet]
        public ActionResult ModifyCustomer()
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

                List<V_MNClientDetail> ClientDetail = new List<V_MNClientDetail>();
                DataTable dtblClientDetail = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM v_MNClientDetail WHERE UserType='user'", sqlCon);
                    sqlDa.Fill(dtblClientDetail);
                    foreach (DataRow row in dtblClientDetail.Rows)
                    {
                        V_MNClientDetail cd = new V_MNClientDetail
                        {
                            ClientCode = row["ClientCode"].ToString(),
                            Name = row["Name"].ToString(),
                            Address = row["Address"].ToString(),
                            PIN = row["PIN"].ToString(),
                            Status = row["Status"].ToString(),
                            ContactNumber1 = row["ContactNumber1"].ToString(),
                            UserName = row["UserName"].ToString(),
                            UserType = row["UserType"].ToString(),
                            WalletNumber = row["WalletNumber"].ToString(),
                            BankAccountNumber = row["BankAccountNumber"].ToString(),
                            BankNo = row["BankNo"].ToString(),
                            BranchCode = row["BranchCode"].ToString()
                        };

                        ClientDetail.Add(cd);
                    }
                    sqlCon.Close();
                }
                return View(ClientDetail);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // GET: ProfileSetup/Detail
        [HttpGet]
        public ActionResult Detail(string SearchCol, string txtMobileNo, string txtName)
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

                List<V_MNClientDetail> List = new List<V_MNClientDetail>();
                if (SearchCol == "Mobile Number")
                {
                    DataTable dtblClientDetail = new DataTable();
                    using (SqlConnection sqlConn = new SqlConnection(DatabaseConnection.ConnectionStr()))
                    {
                        sqlConn.Open();
                        SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM v_MNClientDetail Where UserType='user' AND ContactNumber1 LIKE @ContactNumber1", sqlConn);
                        sqlDa.SelectCommand.Parameters.AddWithValue("@ContactNumber1", "%" + txtMobileNo + "%");
                        sqlDa.Fill(dtblClientDetail);

                        foreach (DataRow row in dtblClientDetail.Rows)
                        {

                            V_MNClientDetail cd = new V_MNClientDetail
                            {
                                ClientCode = row["ClientCode"].ToString(),
                                Name = row["Name"].ToString(),
                                Address = row["Address"].ToString(),
                                PIN = row["PIN"].ToString(),
                                Status = row["Status"].ToString(),
                                ContactNumber1 = row["ContactNumber1"].ToString(),
                                UserName = row["UserName"].ToString(),
                                UserType = row["UserType"].ToString(),
                                WalletNumber = row["WalletNumber"].ToString(),
                                BankAccountNumber = row["BankAccountNumber"].ToString()
                            };

                            List.Add(cd);
                        }
                    }
                    return View(List);
                }
                else if (SearchCol == "Name")
                {

                    DataTable dtblClientDetail = new DataTable();
                    using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionStr()))
                    {
                        sqlCon.Open();
                        SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM v_MNClientDetail Where UserType='user' AND Name LIKE @Name", sqlCon);
                        sqlDa.SelectCommand.Parameters.AddWithValue("@Name", "%" + txtName + "%");
                        sqlDa.Fill(dtblClientDetail);

                        foreach (DataRow row in dtblClientDetail.Rows)
                        {

                            V_MNClientDetail cd = new V_MNClientDetail
                            {
                                ClientCode = row["ClientCode"].ToString(),
                                Name = row["Name"].ToString(),
                                Address = row["Address"].ToString(),
                                PIN = row["PIN"].ToString(),
                                Status = row["Status"].ToString(),
                                ContactNumber1 = row["ContactNumber1"].ToString(),
                                UserName = row["UserName"].ToString(),
                                UserType = row["UserType"].ToString(),
                                WalletNumber = row["WalletNumber"].ToString(),
                                BankAccountNumber = row["BankAccountNumber"].ToString()
                            };

                            List.Add(cd);
                        }

                    }
                    return View(List);
                }
                else
                    return View(List);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        // GET: ProfileSetup/EditCustomerProfile
        [HttpGet]
        public ActionResult EditCustomerProfile(string id)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            if (this.TempData["customermodify_messsage"] != null)
            {
                this.ViewData["customermodify_messsage"] = this.TempData["customermodify_messsage"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                string displayMessage = string.Empty;
                string messageClass = string.Empty;

                if (id != "")
                {
                    DataTable dtblRegistration = ProfileUtils.GetUserProfileInfo(id);
                    if (dtblRegistration.Rows.Count > 0)
                    {
                        UserInfo userInfo =
                            new UserInfo
                            {
                                ClientCode = dtblRegistration.Rows[0]["ClientCode"].ToString(),
                                Name = dtblRegistration.Rows[0]["Name"].ToString(),
                                Address = dtblRegistration.Rows[0]["Address"].ToString(),
                                PIN = dtblRegistration.Rows[0]["PIN"].ToString(),
                                Status = dtblRegistration.Rows[0]["Status"].ToString(),
                                ContactNumber1 = dtblRegistration.Rows[0]["ContactNumber1"].ToString(),
                                ContactNumber2 = dtblRegistration.Rows[0]["ContactNumber2"].ToString(),
                                UserName = dtblRegistration.Rows[0]["UserName"].ToString(),
                                UserType = dtblRegistration.Rows[0]["UserType"].ToString(),
                                IsApproved = dtblRegistration.Rows[0]["IsApproved"].ToString(),
                                IsRejected = dtblRegistration.Rows[0]["IsRejected"].ToString(),
                                WalletNumber = dtblRegistration.Rows[0]["WalletNumber"].ToString(),
                                BankNo = dtblRegistration.Rows[0]["BankNo"].ToString(),
                                BranchCode = dtblRegistration.Rows[0]["BranchCode"].ToString(),
                                BankAccountNumber = dtblRegistration.Rows[0]["BankAccountNumber"].ToString()
                            };

                        return View(userInfo);
                    }
                }
                else
                {
                    displayMessage = "Please Check the details";
                    messageClass = CssSetting.FailedMessageClass;
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }


        // POST: ProfileSetup/EditCustomerProfile/1
        [HttpPost]
        public ActionResult EditCustomerProfile(string ClientCode, UserInfo userInfo)
        {
            string userName = (string)Session["LOGGED_USERNAME"];
            string clientCode = (string)Session["LOGGEDUSER_ID"];
            string name = (string)Session["LOGGEDUSER_NAME"];
            string userType = (string)Session["LOGGED_USERTYPE"];

            TempData["userType"] = userType;

            string displayMessage = null;
            string messageClass = null;

            if (TempData["userType"] != null)
            {
                this.ViewData["userType"] = this.TempData["userType"];
                ViewBag.UserType = this.TempData["userType"];
                ViewBag.Name = name;

                try
                {
                    if ((ClientCode != "") || (ClientCode != null))
                    {
                        bool isUpdated = false;
                        isUpdated = CustomerUtils.UpdateCustomerUserInfo(userInfo);
                        displayMessage = isUpdated
                                                ? "The User Information has successfully been updated."
                                                : "Error while updating Member information";
                        messageClass = isUpdated ? CssSetting.SuccessMessageClass : CssSetting.FailedMessageClass;
                    }
                }
                catch (Exception ex)
                {
                    displayMessage = ex.Message;
                    messageClass = CssSetting.FailedMessageClass;
                }

                this.TempData["customermodify_messsage"] = displayMessage;
                this.TempData["message_class"] = messageClass;

                return RedirectToAction("EditCustomerProfile");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        #endregion

    }
}