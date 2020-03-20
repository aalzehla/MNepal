using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MNepalProject.Models.ViewModel;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Security;
using MNepalProject.Models;
using MNepalProject.Helper;
using MNepalProject.DAL;
namespace MNepalProject.Controllers.AccountController
{
    public class AccountController : Controller
    {

        #region Inilization

        TraceIdGenerator _tig = new TraceIdGenerator();
        dalUserManagement umDAL = new dalUserManagement();

        #endregion

        #region AccountMethod

        // GET: Account
        [AllowAnonymous]
       public ActionResult Login()
        {
            ViewData["msgLogin"] = "";
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel _loginData, string returnUrl)
        {
            try
           {
               HttpResponseMessage response = new HttpResponseMessage();

               //call the Authenticate webservice to verify the user
               if (ModelState.IsValid)
               {
                   //call the Authenticate webservice to verify the user
                   using (HttpClient client = new HttpClient())
                   {
                       var uri = "http://27.111.30.126/MNepal.WCF/query/authenticate";
                       var content = new FormUrlEncodedContent(new[] 
                    {
                        new KeyValuePair<string, string>("username", _loginData.username),
                        new KeyValuePair<string,string>("password",_loginData.password),
                        new KeyValuePair<string,string>("userType","user")
                    });
                       response = await client.PostAsync(uri, content);
                       var _status = response.IsSuccessStatusCode;

                       if (_status == true)
                       {
                           this.SignIn(_loginData);
                           return RedirectToLocal(returnUrl);
                       }
                       else
                       {
                           //return response.ReasonPhrase;
                           ViewData["msgLogin"] = "something is wrong";
                           return View();
                       }
                   }
               }
               ViewData["msgLogin"] = "something is wrong";
               return View();
           }
           catch(Exception ex)
           {
               throw;
           }
        }

        // Get
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewData["msg"] = "";
            return View();
        }

        [HttpPost]
        public async Task<string> Register(MNUsers user)
        {
            try
            {
                HttpResponseMessage response = new HttpResponseMessage();

                if (ModelState.IsValid)
                {
                     var tid = _tig.GenerateUniqueTraceID(); 
                    using (HttpClient client = new HttpClient())
                    {
                        var uri = "http://27.111.30.126/MNepal.WCF/query/registration";
                        var content = new FormUrlEncodedContent(new[] 
                    {
                        new KeyValuePair<string,string>("tid",tid),
                        new KeyValuePair<string,string>("sc",user.sc),
                        new KeyValuePair<string,string>("fname",user.firstname),
                        new KeyValuePair<string,string>("middlename",user.middlename),
                        new KeyValuePair<string,string>("lname",user.lastname),
                        new KeyValuePair<string,string>("amobile",User.Identity.Name),
                        new KeyValuePair<string,string>("umobile",user.umobile),
                        new KeyValuePair<string,string>("amount",user.amount),
                        new KeyValuePair<string,string>("dob",user.dob),
                        new KeyValuePair<string,string>("street",user.street),
                        new KeyValuePair<string,string>("ward",user.ward),
                        new KeyValuePair<string,string>("district",user.district),
                        new KeyValuePair<string,string>("zone",user.zone),
                        new KeyValuePair<string,string>("photoid",user.photoid),
                        new KeyValuePair<string,string>("ivrlang",user.ivrLang),
                        
                    });
                        response = await client.PostAsync(uri, content);
                        string responseBody = await response.Content.ReadAsStringAsync()+","+response.IsSuccessStatusCode;
                        return responseBody;
                    }
                }
                //return "model is invalid";
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        #region Profile Management

        [Authorize]
        public ActionResult Profile()
        {
            return View();
        }

        #endregion

        #endregion

        #region Helper

        private void SignIn(LoginViewModel _loginData)
        {
            try
            {
                FormsAuthentication.SignOut();
                //authentication ticket
                var isAgent = umDAL.isAgent(_loginData.username);
                var authTicket = new FormsAuthenticationTicket(
                        1,
                        _loginData.username,
                        DateTime.Now,
                        DateTime.Now.AddDays(1),
                        _loginData.rememberMe,
                        isAgent.ToString()
                        );
                var encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                HttpContext.Response.Cookies.Add(authCookie);

                //FormsAuthentication.SetAuthCookie(_loginData.username, _loginData.rememberMe);
               
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            try
            {
                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                   && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Board", "DashBoard");
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        #endregion
    }
}