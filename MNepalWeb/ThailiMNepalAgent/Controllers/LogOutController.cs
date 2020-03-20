using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Net;
using System.Net.Sockets;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace ThailiMNepalAgent.Controllers
{
    public class LogOutController : Controller
    {
        string IPAddresses;
        // GET: LogOut
        public ActionResult Index()
        {
            if (Session["LOGGED_USERNAME"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                try
                {
                    /*Stamp Logout*/
                    MNAdminLog log = new MNAdminLog();
                    log.IPAddress = this.Request.UserHostAddress;
                    log.URL = this.Request.Url.PathAndQuery;
                    log.Message = " ";
                    log.Action = "LOGOUT";
                    if (Session["UniqueId"] != null)
                        log.UniqueId = Session["UniqueId"].ToString() + "|" + HttpContext.Session.SessionID;
                    else
                        log.UniqueId = HttpContext.Session.SessionID;

                    if (Session["UserBranch"] != null)
                        log.Branch = Session["UserBranch"].ToString();
                    else
                        log.Branch = "000";

                    if (Session["UserName"] != null)
                        log.UserId = Session["UserName"].ToString();
                    else
                        log.UserId = "IIS";

                    if (Session["LOGGED_USERTYPE"] != null)
                        log.UserType = Session["LOGGED_USERTYPE"].ToString();
                    else
                        log.UserType = "SERVER";

                    log.TimeStamp = DateTime.Now;

                    //For User IP address etc
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
                    //End User IP address etc
                    string privateip = "";
                    //End User IP address etc
                    if ((Session["privateip"]) != null)
                    {
                        privateip = Session["privateip"].ToString();
                    }
                    else
                    {
                        privateip = "";
                    }
                    //string getmacaddress = GetMacUsingARP(privateip);

                    log.ClientDetails = log.ClientDetails = ViewBag.macAdd + ";" + ViewBag.ipaddress + ";" + ViewBag.LOCAL + ";" + ViewBag.useragent + ";" + ViewBag.remoteaddr + ";" + ViewBag.REMOTE_HOST + ";" + ViewBag.REMOTE_PORT + ";" + ViewBag.HostName + ";" + ViewBag.allhttp + ";" + ViewBag.myIPv6 + ";" + ViewBag.macAddresses + ";" + ViewBag.gatewayip + ";" + ViewBag.IPAddresses;

                    log.PrivateIP = ViewBag.IPv4;

                    LoginUtils.LogAction(log);

                    if (Session["UserName"] != null)
                        LoginUtils.LogOutUser(Session["UserName"].ToString());


                    //Clear Cookie
                    FormsAuthentication.SignOut();
                    Session.Abandon();//Clear the session
                    return RedirectToAction("Index", "Login");
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Index", "Login");
                }
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
    }
}