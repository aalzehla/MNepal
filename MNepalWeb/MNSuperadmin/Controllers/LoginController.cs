using MNSuperadmin.Helper;
using MNSuperadmin.Models;
using MNSuperadmin.Settings;
using MNSuperadmin.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using System.Web.Security;

namespace MNSuperadmin.Controllers
{
    public class LoginController : Controller
    {
        string IPAddresses;
        string IPv4;
        // GET: Login
        [HttpGet]
        public ActionResult Index()
        {

            
            if (this.TempData["login_message"] != null)
            {
                this.ViewData["login_message"] = this.TempData["login_message"];
                this.ViewData["message_class"] = this.TempData["message_class"];
            }
            if(Session["LOGGED_USERNAME"]==null)
            {
                return View("Index");
            }
            else
            {
                TempData["userType"] = Session["LOGGED_USERTYPE"];

                //if (TempData["userType"].ToString() == "user")
                //{
                //    return RedirectToAction("Index", "UserDashboard");
                //}
                //else if (TempData["userType"].ToString() == "agent")
                //{
                //    return RedirectToAction("Index", "AgentDashboard");
                //}
                //else if (TempData["userType"].ToString() == "admin")
                //{
                //    return RedirectToAction("Index", "AdminDashboard");
                //}
                 if (TempData["userType"].ToString() == "superadmin")
                {
                    return RedirectToAction("Index", "SuperAdminDashboard");
                }
                //else if (TempData["userType"].ToString() == "merchant")
                //{
                //    return RedirectToAction("Index", "MerchantDashboard");
                //}
                else
                {
                    return View("Index");
                }
            }
            
        }

        //[ValidateAntiForgeryToken]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(FormCollection collection)
        {
            string userName = collection["txtUserName"] ?? string.Empty;
            string passWord = collection["txtPassword"] ?? string.Empty;
            passWord = HashAlgo.Hash(passWord);
            bool result = false;
            string displayMessage = null;
            string messageClass = string.Empty;
            bool ForceUserChangePW = true;
            //bool ForceUserChangePW = false; // change ForceUserChangePW = true; to apply OTP
            try
            {
                if ((string.IsNullOrEmpty(userName)) || (string.IsNullOrEmpty(passWord)))
                {
                    this.ViewData["login_message"] = "UserName/Password is Empty";
                    return this.View();
                }
                else if ((userName != string.Empty) && (passWord != string.Empty))
                {
                    this.ViewData["userName"] = userName;
                    if (Session["ID"] == null)
                    {
                        DataTable dtableMobileNo = RegisterUtils.GetCheckUserName(userName);//RegisterUtils.GetCheckMobileNo(userName);
                        if (dtableMobileNo.Rows.Count == 0)
                        {
                            result = false;
                            displayMessage = "User doesnot exist.";
                            messageClass = CssSetting.FailedMessageClass;
                        }
                        else
                        {
                            DataTable dtableUser = LoginUtils.GetLoginInfo(userName, passWord);//cipheredPassword);//passWord;cipheredPassword
                            if (dtableUser != null && dtableUser.Rows.Count > 0)
                            {
                                string clientCode = dtableUser.Rows[0]["ClientCode"].ToString();
                                string name = dtableUser.Rows[0]["Name"].ToString();
                                string userType = dtableUser.Rows[0]["UserType"].ToString();
                               //string UserName= dtableUser.Rows[0]["UserName"].ToString();
                                string UserBranch = dtableUser.Rows[0]["UserBranchCode"].ToString();
                                string BranchName = dtableUser.Rows[0]["UserBranchName"].ToString();
 
                             string COC = dtableUser.Rows[0]["COC"].ToString();
                                 
                                Session["LOGGED_USERNAME"] = userName;
                                Session["LOGGEDUSER_ID"] = clientCode;//LoggedUserID
                                Session["LOGGEDUSER_NAME"] = name;//LoggedUserFullname
                                Session["LOGGED_USERTYPE"] = userType;

                                //bool COC = false;

                                if ((userType.ToUpper() == "SUPERADMIN" || userType.ToUpper() == "ADMIN") && ForceUserChangePW)
                                {
                                    ViewModel.ResetVM RVM = new ViewModel.ResetVM();
                                    bool IsPassChanged =dtableUser.Rows[0]["PassChanged"].ToString()=="T"?true:false;
                                    bool IsPinChanged = dtableUser.Rows[0]["PinChanged"].ToString() == "T" ? true : false;
                                    bool IsFirstLogin = dtableUser.Rows[0]["IsFirstLogin"].ToString() == "T" ? true : false;
                                    RVM.ClientCode = clientCode;
                                    RVM.PhoneNumber = dtableUser.Rows[0]["ContactNumber1"].ToString();
                                    if (IsFirstLogin)
                                    {
                                        RVM.Message = "This is your first login you are required to change your Password and PIN";
                                        RVM.Mode = "FL"; //Short for First Login
                                        return View("Reset",RVM);
                                        
                                    }
                                    if (!IsPinChanged)
                                    {
                                        RVM.Message = "You are required to change your Pin";
                                        RVM.Mode = "PN"; //Short for Pin
                                        return View("Reset", RVM);
                                    }
                                    if (!IsPassChanged)
                                    {
                                        RVM.Message = "You are required to change your Password.";
                                        RVM.Mode = "PW"; //Short for Password
                                        return View("Reset", RVM);
                                    }
                                }
                                string agentId = string.Empty;
                                //if ((userType == "agent") || (userType == "admin"))
                                //{
                                //    COC = dtableUser.Rows[0]["COC"].ToString().Trim() == "T" ? true : false;
                                //    DataTable dtableAgent = AgentUtils.GetAgentId(clientCode);
                                //    if (dtableAgent != null && dtableAgent.Rows.Count > 0)
                                //    {
                                //        agentId = dtableAgent.Rows[0]["ID"].ToString();
                                //    }
                                //}
                                Session["LOGGED_USERNAME"] = userName;
                                Session["LOGGEDUSER_ID"] = clientCode;//LoggedUserID
                                Session["LOGGEDUSER_NAME"] = name;//LoggedUserFullname
                                Session["LOGGED_USERTYPE"] = userType;
                                Session["UserName"] = userName;
                                Session["UserBranch"] = UserBranch;
                                Session["BranchName"] = BranchName;
                                Session["COC"] = COC;
                                Session["AGENT_ID"] = agentId;
                                Session["UniqueId"] = string.Join("", DateTime.Now.Ticks.ToString().Select(c => ((int)c).ToString("X2")))+ string.Join("", userName.Select(c => ((int)c).ToString("X2")));
                                var featuredUserInfo = new UserInfo
                                {
                                    UserName = userName,
                                    ClientCode = clientCode,
                                    Name = name,
                                    UserType = userType,
                                    AgentId = agentId
                                };
                                FormsAuthentication.SetAuthCookie(userName, false);

                                ViewData["FeaturedUserInfo"] = featuredUserInfo;
                                ViewBag.Product = featuredUserInfo;
                                TempData["FeaturedUserInfo"] = featuredUserInfo;
                                TempData["userType"] = userType;

                                /*Stamp Login*/
                                MNAdminLog log = new MNAdminLog();
                                log.IPAddress = this.Request.UserHostAddress;
                                log.URL = this.Request.Url.PathAndQuery;
                                log.Message = " ";
                                log.Action = "LOGIN";
                                log.UniqueId =Session["UniqueId"].ToString() + "|" + HttpContext.Session.SessionID;
                                log.Branch = Session["UserBranch"].ToString();
                                log.UserId = Session["UserName"].ToString();
                                log.UserType = Session["LOGGED_USERTYPE"].ToString();
                                log.TimeStamp = DateTime.Now;

                                ///start  for ip address and clientdetails save in log

                                string macAdd = string.Empty;
                                ManagementObjectSearcher query = null;
                                ManagementObjectCollection queryCollection = null;
                                string sIPAddress = string.Empty;
                                try
                                {
                                    query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'");
                                    queryCollection = query.Get();
                                    foreach (ManagementObject mo in queryCollection)
                                    {
                                        if (mo["MacAddress"] != null)
                                        {
                                            macAdd = mo["MacAddress"].ToString();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                }

                                ViewBag.macAdd = macAdd;

                                string ipaddress; string localaddress; string allhttp; string useragent; string remoteaddr; string REMOTE_HOST; string REMOTE_PORT;
                                ipaddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                localaddress = Request.ServerVariables["LOCAL_ADDR"];
                                useragent = Request.ServerVariables["HTTP_USER_AGENT"];
                                remoteaddr = Request.ServerVariables["REMOTE_ADDR"];
                                REMOTE_HOST = Request.ServerVariables["REMOTE_HOST"];
                                REMOTE_PORT = Request.ServerVariables["REMOTE_PORT"];

                                if (ipaddress == "" || ipaddress == null)
                                {
                                    ipaddress = Request.ServerVariables["REMOTE_ADDR"];
                                }

                                ViewBag.ipaddress = ipaddress.ToString();
                                ViewBag.LOCAL = localaddress.ToString();
                                ViewBag.useragent = useragent.ToString();

                                ViewBag.remoteaddr = remoteaddr.ToString();
                                ViewBag.REMOTE_HOST = REMOTE_HOST.ToString();
                                ViewBag.REMOTE_PORT = REMOTE_PORT.ToString();

                                string HostName = Dns.GetHostName(); // Retrive the Name of HOST  
                                ViewBag.HostName = HostName; //"Host Name of machine ="
                                IPAddress[] ipaddres = Dns.GetHostAddresses(HostName);
                                ViewBag.allhttp = ipaddres[1].ToString(); //"IPv4 of Machine is "


                                IPHostEntry ipEntry = Dns.GetHostEntry(HostName);
                                IPAddress[] addr = ipEntry.AddressList;
                                string ips = addr[1].ToString();
                                ViewBag.myIPv6 = ips.ToString();

                                string macAddresses = string.Empty;
                                string gatewayip = string.Empty;
                                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                                {
                                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                                    {
                                        Console.WriteLine(ni.Name);
                                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                                        {
                                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                            {
                                                ViewBag.IPv4 = ip.Address.ToString();
                                            }
                                        }
                                    }
                                    if (ni.OperationalStatus == OperationalStatus.Up)
                                    {
                                        foreach (GatewayIPAddressInformation d in ni.GetIPProperties().GatewayAddresses)
                                        {
                                            gatewayip = d.Address.ToString();
                                        }
                                        macAddresses += ni.GetPhysicalAddress().ToString();
                                        break;
                                    }
                                }
                                ViewBag.macAddresses = macAddresses;
                                ViewBag.gatewayip = gatewayip;

                                IPAddresses = GetIPAddress();
                                ViewBag.IPAddresses = IPAddresses;
                                 
                                //for mac address 
                                //string privateip = collection["privateip"] ?? string.Empty;
                                string privateip = collection["txtprivateip"] ?? string.Empty;

                                //string getmacaddress = GetMacUsingARP(privateip);
                                Session["LOGGED_privateip"] = privateip;

                                log.PrivateIP = privateip;
                                log.ClientDetails = ViewBag.macAdd + ";" + ViewBag.ipaddress + ";" + ViewBag.LOCAL + ";" + ViewBag.useragent + ";" + ViewBag.remoteaddr + ";" + ViewBag.REMOTE_HOST + ";" + ViewBag.REMOTE_PORT + ";" + ViewBag.HostName + ";" + ViewBag.myIPv6 + ";" + ViewBag.gatewayip + ";" + ViewBag.IPAddresses;
                                 
                                ///end for ip address and clientdetails save in log

                                LoginUtils.LogAction(log);
                                
                                                              
                              
                                if (TempData["userType"].ToString() == "superadmin")
                                {
                                    return RedirectToAction("Index", "SuperAdminDashboard");
                                }
                                
                            }
                            else
                            {
                                result = false;
                                displayMessage = "Invalid Login! " + "\n Please make sure that the username and the password is Correct !!";
                                messageClass = CssSetting.FailedMessageClass;
                            }
                        }
                           
                    }
                }
            }
            catch (Exception ex)
            {
                displayMessage = ex.Message + "\n Please Contact to the administrator !!";
                messageClass = CssSetting.FailedMessageClass;
            }
            this.TempData["login_message"] = result
                                                  ? "" + displayMessage
                                                  : "Error:: Invalid Login " + displayMessage;
            this.TempData["message_class"] = result ? "success_info" : "failed_info";
            return RedirectToAction("Index");
        }


        public ActionResult CheckUserName(string UserName)
        {
            if ((UserName != "") || (UserName != null))
            {
                string userName = string.Empty;
                DataTable dtUserName = RegisterUtils.GetCheckUserName(UserName);
                if (dtUserName != null && dtUserName.Rows.Count > 0)
                {
                    userName = "Success";
                }
                else
                {
                    userName = "User Doesnot Exist !";
                }

                return Json(userName, JsonRequestBehavior.AllowGet);
            }
            else
                return View();

        }
        [HttpGet]
        public ActionResult Change()
        {
            return RedirectToAction("Index", "Login");
        }


        [HttpPost]
        public ActionResult Change(ViewModel.ResetVM model,string RetypePin,string RetypePassword)
        {
            
            try
            {
                string Message="";
               
                var OPin = string.Empty;
                var OPass = string.Empty;
                if (model.Mode != null)
                {
                    var Cred = LoginUtils.CheckCred(model);
                    if (Cred.Count > 0)
                    {
                        OPin = Cred["PIN"];
                        OPass = Cred["Password"];
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
                if (model.Mode.ToUpper() == "FL")
                {
                    PasswordValidator validate = new PasswordValidator();
                    if(!validate.IsValid(model.Password))
                    {
                        ModelState.AddModelError("Password", "New Password must be of 8-12 characters long which should include 1 uppercase & 1 number");
                    }
                    if(model.OldPin!=OPin)
                    {
                        ModelState.AddModelError("OldPin", "Incorrect Old Pin");
                    }
                    if (model.Pin!=RetypePin)
                    {
                        ModelState.AddModelError("Pin","Entered Pin Do not Match");
                    }

                    //if (model.OldPassword != OPass)
                    if (HashAlgo.Hash(model.OldPassword) != OPass)

                    {
                        ModelState.AddModelError("OldPassword", "Incorrect Old Password");
                    }
                    if (model.Password!=RetypePassword)
                    {
                        ModelState.AddModelError("Password", "Entered password Do not Match");
                    }
                    if(!ModelState.IsValid)
                    {
                        return View("Reset",model);
                    }

                    TempData["login_message"] = "Pin/Password Changed successfully";

                    /*Sms Message for  pin and Password Reset on first login */
                    Message = "Dear Customer," + "\n";
                    Message += " Your Pin and password has been changed"
                        + "." + "\n" + "Thank You";
                    Message += "-MNepal";


                }
                else if ( model.Mode.ToUpper() == "PN" ) //Pin Reset 
                {
                    if (model.OldPin != OPin)
                    {
                        ModelState.AddModelError("OldPin", "Incorrect Pin Do not Match");
                    }
                    if (model.Pin != RetypePin)
                    {
                        ModelState.AddModelError("Pin", "Entered Pin Do not Match");
                    }
                    if (!ModelState.IsValid)
                    {
                        return View("Reset", model);
                    }

                    TempData["login_message"] = "Pin Changed successfully";

                    /*Sms Message for Pin Reset */
                    Message = "Dear Customer," + "\n";
                    Message += " Your Pin has been changed"
                        + "." + "\n" + "Thank You";
                    Message += "-MNepal";
                }

                else if (model.Mode.ToUpper() == "PW") //Password Reset 
                {

                    PasswordValidator validate = new PasswordValidator();
                    if (!validate.IsValid(model.Password))
                    {
                        ModelState.AddModelError("Password", "New Password must be of minimum 8 characters long which should include 1 uppercase & 1 number");
                    }

                    //if (model.OldPassword != OPass)
                    if (HashAlgo.Hash(model.OldPassword) != OPass)
                    {
                        ModelState.AddModelError("OldPassword", "Incorrect Old Password");
                    }
                    if (model.Password != RetypePassword)
                    {
                        ModelState.AddModelError("Password", "Entered password Do not Match");
                    }
                    if (!ModelState.IsValid)
                    {
                        return View("Reset", model);
                    }

                    TempData["login_message"] = "Password Changed successfully";
                
                    /*Sms Message for Password Reset */
                    Message = "Dear Customer," + "\n";
                    Message += " Your Password has been changed"
                        + "." + "\n" + "Thank You";
                    Message += "-MNepal";
                }


                if (LoginUtils.ResetFLogin(model))
                {
                    /*Sms Start*/
                    /* SMSUtils SMS = new SMSUtils();
                     SMS.SendSMS(Message, model.PhoneNumber);*/
                    /*Sms End*/
                    TempData["message_class"] = CssSetting.SuccessMessageClass;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["messsage"] = "Error:: Unable to Process your request,Pleas refresh the page and try again";
                    TempData["message_class"] = CssSetting.FailedMessageClass;
                    return View("Reset", model);
                }

               
            }
            catch (Exception ex)
            {
                TempData["login_message"] = "Error:: " + ex.Message +". Please Try again.";
                TempData["message_class"] = CssSetting.FailedMessageClass;
                return RedirectToAction("Index","Login");
            }
          
        }

        protected string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        ///start for mac address

        private string GetMacUsingARP(string IPAddr)
        {
            IPAddress IP = IPAddress.Parse(IPAddr);
            byte[] macAddr = new byte[6];
            uint macAddrLen = (uint)macAddr.Length;
            if (SendARP((int)IP.Address, 0, macAddr, ref macAddrLen) != 0)
                throw new Exception("ARP command failed");
            string[] str = new string[(int)macAddrLen];
            for (int i = 0; i < macAddrLen; i++)
                str[i] = macAddr[i].ToString("x2");
            return string.Join(":", str);
        }

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr,
               ref uint PhyAddrLen);

        ///end for mac address
    }
}
 