using MNepalProject.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MNepalProject.Models;
using System.Threading.Tasks;
using System.Net.Http;
using MNepalProject.DAL;
using System.Web.Http.Description;
using System.Net;
using Newtonsoft.Json;

namespace MNepalProject.Controllers.Transaction
{
    [Authorize]
    public class TransactionController : Controller
    {
        #region Inilization

        TraceIdGenerator _tig = new TraceIdGenerator();
        dalTransactionManagement tmDAl = new dalTransactionManagement();

        #endregion

        #region FinancialTransaction

        #region FundTransfer
        // GET: Transaction
        public ActionResult FundTransfer()
        {
            return View();
        }

        //post fund transfer
        [HttpPost]
        public async Task<string> FundTransfer(MNFundTransfer _ft)
        {
            //HttpRequestMessage request=new HttpRequestMessage();
            HttpResponseMessage _res=new HttpResponseMessage();
            var tid = _tig.GenerateTraceID();
            var mobile = User.Identity.Name;//mobile is username

            using (HttpClient client = new HttpClient())
            {
                var uri = "http://27.111.30.126/MNepal.WCF/ft/request";
                var content = new FormUrlEncodedContent(new[] 
                        {
                            new KeyValuePair<string, string>("tid", tid),
                            new KeyValuePair<string,string>("sc",_ft.sc),
                            new KeyValuePair<string, string>("mobile",mobile),
                            new KeyValuePair<string,string>("da",_ft.da),
                            new KeyValuePair<string, string>("amount", _ft.amount),
                            new KeyValuePair<string,string>("pin",_ft.pin),
                            new KeyValuePair<string, string>("note", _ft.note),
                            new KeyValuePair<string,string>("src",_ft.sourcechannel)
                        });
                _res = await client.PostAsync(new Uri(uri), content);

                
               // _res.EnsureSuccessStatusCode();
                string responseBody = _res.StatusCode.ToString() + " ," + await _res.Content.ReadAsStringAsync();
                _res.ReasonPhrase = responseBody;
                
                //return responseBody;
                return responseBody;
            }

            //_res.StatusCode = HttpStatusCode.OK;
            //_res.ReasonPhrase = "testeststet";
        }

        #endregion

        public ActionResult Coupon()
        {
            return View();
        }

        public ActionResult UtilityPayment()
        {
            return View();
        }

        [HttpPost]
        public async Task<HttpResponseMessage> MNPayment(MNPayment pMN)
        {
            try
            {
                var tid = _tig.GenerateTraceID();
                var mobile = User.Identity.Name;//mobile is username
                pMN.mobile = mobile;
                pMN.tid = tid;

                var data = JsonConvert.SerializeObject(pMN);
                var content=data;
                using (HttpClient client = new HttpClient())
                {
                    var uri = "http://27.111.30.126/MNepal.WCF/merchant/payment";
                    //var content = new FormUrlEncodedContent(new[] 
                    //    {                          
                    //        new KeyValuePair<string,string>("sc",pMN.sc),
                    //        new KeyValuePair<string,string>("vid",pMN.prod),
                    //        new KeyValuePair<string, string>("mobile",mobile),
                    //        new KeyValuePair<string,string>("sa",""),
                    //        new KeyValuePair<string,string>("prod",""),
                    //        new KeyValuePair<string, string>("amount",pMN.amount),
                    //        new KeyValuePair<string, string>("tid", tid),
                    //        new KeyValuePair<string,string>("pin",pMN.pin),
                    //        new KeyValuePair<string, string>("note",pMN.note),
                    //        new KeyValuePair<string,string>("src","http")
                    //    });
                   // var _res = await client.PostAsync(new Uri(uri),content);
                   // string responseBody = await _res.Content.ReadAsStringAsync();
                    return null;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult MerchantPayment()
        {
            return View();
        }

        #endregion

        #region Non-financialTransaction

        public ActionResult BalanceQuery()
        {
            return View();
        }

        //GET:Balance
        public async Task<HttpResponseMessage> GetBalance(string PIN,string uType)
        {
            HttpResponseMessage _res = new HttpResponseMessage();
            var tid = _tig.GenerateTraceID();
            var mobile = User.Identity.Name;//mobile is username

            using (HttpClient client = new HttpClient())
            {
                var uri = "http://27.111.30.126/MNepal.WCF/query/balance?tid="+tid+"&sc="+uType+"&mobile="+mobile+"&sa=&pin="+PIN+"&src=http";
               
                _res = await client.GetAsync(new Uri(uri));
                _res.EnsureSuccessStatusCode();
                string responseBody = await _res.Content.ReadAsStringAsync();
                return _res;
            }
        }

        //Get:Balance from database
        public string GetBalanceFromDatabase()
        {
            try
            {
                 var userName = User.Identity.Name;
                 var clientCode = tmDAl.GetClientCode(userName);
                 var balance = tmDAl.GetBalanceFromDatabase(clientCode);

                 return balance;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        //Get: MiniStatementView
        public ActionResult MiniStatement()
        {
            return View();
        }

        public async Task<string> GetMiniStatement(string PIN, string uType)
        {
            HttpResponseMessage _res = new HttpResponseMessage();
            var tid = _tig.GenerateTraceID();
            var mobile = User.Identity.Name;//mobile is username

            using (HttpClient client = new HttpClient())
            {
                var uri = "http://27.111.30.126/MNepal.WCF/query/balance?tid=" + tid + "&sc=" + uType + "&mobile=" + mobile + "&sa=&pin=" + PIN + "&src=http";

                _res = await client.GetAsync(new Uri(uri));
                _res.EnsureSuccessStatusCode();
                string responseBody = await _res.Content.ReadAsStringAsync();
                return responseBody;
            }
        }

        public ActionResult ChangePin()
        {
            return View();
        }

        #endregion

        #region Helper

        public string GetPin(string da)
        {
            var clientCode = "";
            var PIN = "";
            if (tmDAl.GetClientCode(da) != null)
            {
               clientCode= tmDAl.GetClientCode(da);
               PIN = tmDAl.GetPIN(clientCode);

               return PIN;
            }
            else
            return null;
        }

        #endregion
    }
}