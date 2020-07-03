using MNepalProject.Controllers;
using MNepalProject.Helper;
using MNepalProject.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using WCF.MNepal.ErrorMsg;
using WCF.MNepal.Helper;
using WCF.MNepal.Models;
using WCF.MNepal.Utilities;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class LoadFund
    {
        #region"Check Paypoint Khalti"
        [OperationContract]
        [WebInvoke(Method = "POST",
                  ResponseFormat = WebMessageFormat.Json)]
        public string KhaltiCheckPayment(Stream input)
        {
            string PaypointPwd = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointPwd"];
            string PaypointUserID = System.Web.Configuration.WebConfigurationManager.AppSettings["PaypointUserID"];
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; }; //to prevent from SSL error
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string tid = qs["tid"];
            string vid = qs["vid"]; //MerchantID
            string sc = "00";
            string mobile = qs["mobile"];
            string da = System.Web.Configuration.WebConfigurationManager.AppSettings["DestinationNoForTestServer"];
            string note = "utility payment for WebSurfer. Customer ID=" + qs["account"];//+ ". " + qs["note"];

            string src = qs["src"];
            string result = "";
            string sessionID = qs["tokenID"];
            string resultMessageResCP = "";

            string companyCode = qs["companyCode"];
            string serviceCode = qs["serviceCode"];

            string account = qs["account"];
            string special1 = qs["special1"];
            string special2 = "";
            string transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");//"2019-11-22T11:11:02";
            long millisecondstrandId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string transactionId = millisecondstrandId.ToString(); //"120163339819";
            string userId = PaypointUserID;
            //string userId = "MNepalLT";
            //string userPassword = "MNepalLT";
            string userPassword = PaypointPwd.Trim();
            string salePointType = "6";
            string ClientCode = qs["ClientCode"];
            string paypointType = qs["paypointType"];
            string transactionType = string.Empty;

            PaypointModel reqCPPaypointKhaltiInfo = new PaypointModel();//to store data request of CP which is also commmon in nea
            PaypointModel resCPPaypointKhaltiInfo = new PaypointModel();//to store data response of CP which is also commmon in nea

            //PaypointModel resPaypointPaymentInfo = new PaypointModel();
            PaypointModel resPaypointKhaltiPaymentInfo = new PaypointModel();//to store data of Response of CP only

            string totalAmount = string.Empty;
            string totalCount = string.Empty;
            string totalBAmount = string.Empty;
            string totalBCount = string.Empty;
            string balance = string.Empty;
            string message = string.Empty;
            string statusCode = string.Empty;
            string failedmessage = string.Empty;
            string GetMerchantName = string.Empty;
            string retrievalRef = string.Empty;
            string refStanCK = string.Empty;

            string customerNo = string.Empty;
            TraceIdGenerator traceid = new TraceIdGenerator();
            tid = traceid.GenerateUniqueTraceID();


            List<Packages> pkg = new List<Packages>();
            List<SmartCards> smartCards = new List<SmartCards>();
            List<User> users = new List<User>();

            //for CP transaction for vianet
            try
            {
                //for checkpayment link 
                //string URI = "https://test.paypoint.md:4445/PayPointWS/PayPointMSOperations.asmx/CheckPayment";

                //For checkpaypoint link in webconfig
                string URI = System.Web.Configuration.WebConfigurationManager.AppSettings["CPPaypointUrl"];

                string myParameters = "companyCode=" + companyCode + "&serviceCode=" + serviceCode +
                    "&account=" + account + "&special1=" + special1 + "&special2=" + special2 +
                    "&transactionDate=" + transactionDate + "&transactionId=" + transactionId +
                    "&userId=" + userId.Trim() + "&userPassword=" + userPassword.Trim() + "&salePointType=" + salePointType;

                //for checkpayment request insert in database
                reqCPPaypointKhaltiInfo = new PaypointModel()
                {
                    companyCodeReqCP = companyCode,
                    serviceCodeReqCP = serviceCode,
                    accountReqCP = account,
                    special1ReqCP = special1,
                    special2ReqCP = special2,

                    transactionDateReqCP = transactionDate,
                    transactionIdReqCP = transactionId,
                    userIdReqCP = userId,
                    userPasswordReqCP = userPassword,
                    salePointTypeReqCP = salePointType,

                    refStanReqCP = "",
                    amountReqCP = "",//amountInPaisa
                    billNumberReqCP = "",
                    //retrievalReferenceReqCP = fundtransfer.tid,
                    retrievalReferenceReqCP = tid,
                    remarkReqCP = "Check Payment",
                    UserName = mobile,
                    ClientCode = ClientCode,
                    paypointType = paypointType,

                };

                string billNumber = "";
                string amountpay = "";
                string refStan = "";
                string exectransactionId = ""; //Unique
                string exectransactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //Current DateTime
                string rltCheckPaymt = "";
                string customerName = "";
                string mask = "";
                string reserveInfo = "";

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var HtmlResult = wc.UploadString(URI, myParameters);// response from checkpayment

                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(HtmlResult);

                    XmlNodeList test = xmlDoc.GetElementsByTagName("*");
                    string results = test[0].InnerText;
                    string HtmlResult1 = results;

                    //for getting key value from check payment
                    var reader = new StringReader(HtmlResult1);
                    var xdoc = XDocument.Load(reader);

                    XDocument docParse = XDocument.Parse(xdoc.ToString());
                    IEnumerable<XElement> responses = docParse.Descendants();

                    var xElem = XElement.Parse(docParse.ToString());

                    rltCheckPaymt = xElem.Attribute("Result").Value;
                    string keyrlt = "";// response.Attribute("Key").Value;


                    message = resultMessageResCP;
                    if (xElem.Attribute("Result").Value == "000")
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        billNumber = xElem.Descendants().Elements("BillNumber").Where(x => x.Name == "BillNumber").SingleOrDefault().Value;
                        amountpay = xElem.Descendants().Elements("Amount").Where(x => x.Name == "Amount").SingleOrDefault().Value;
                        refStan = xElem.Descendants().Elements("RefStan").Where(x => x.Name == "RefStan").SingleOrDefault().Value;
                        exectransactionDate = xElem.Descendants().Elements("DueDate").Where(x => x.Name == "DueDate").SingleOrDefault().Value;
                        reserveInfo = xElem.Descendants().Elements("ReserveInfo").Where(x => x.Name == "ReserveInfo").SingleOrDefault().Value;
                        mask = xElem.Descendants().Elements("mask").Where(x => x.Name == "mask").SingleOrDefault().Value;



                        //Packages packages = new Packages();
                        //SmartCards cards = new SmartCards();
                        //User usrs = new User();

                        //string stringBuilderDescriptions = "";
                        //string stringBuilderAmounts = "";
                        //string stringBuilderPackageId = "";
                        //string stringBuilderUsers = "";

                        if (mask == "0" || mask == "6" || mask == "3")
                        {

                            //var package = xElem.Descendants("packages").SingleOrDefault();
                            //var ftthUser = xElem.Descendants("users").SingleOrDefault();
                            ////var packageList = package.Descendants("package").ToList();

                            //XmlDocument xmlDoc1 = new XmlDocument();
                            //xmlDoc1.LoadXml(package.ToString());

                            //if (ftthUser != null)
                            //{
                            //    XmlDocument xmlDoc3 = new XmlDocument();
                            //    xmlDoc3.LoadXml(ftthUser.ToString());
                            //    XmlNodeList usersxmlNodeList = xmlDoc3.SelectNodes("/users/user");
                            //    foreach (XmlNode xmlNode in usersxmlNodeList)
                            //    {
                            //        usrs.user = xmlNode.OuterXml;
                            //        users.Add(usrs);
                            //        stringBuilderUsers = stringBuilderUsers + usrs.user + Environment.NewLine;

                            //    }
                            //}


                            //XmlNodeList xmlNodeList = xmlDoc1.SelectNodes("/packages/package");



                            //foreach (XmlNode xmlNode in xmlNodeList)
                            //{
                            //    packages.Description = xmlNode.OuterXml; /*xmlNode.InnerText;*/
                            //    packages.Amount = xmlNode.Attributes["amount"].Value;
                            //    packages.PackageId = xmlNode.Attributes["id"].Value;
                            //    pkg.Add(packages);
                            //    stringBuilderDescriptions = stringBuilderDescriptions + packages.Description + Environment.NewLine;
                            //    stringBuilderAmounts = stringBuilderAmounts + packages.Amount + Environment.NewLine;
                            //    stringBuilderPackageId = stringBuilderPackageId + packages.PackageId + Environment.NewLine;
                            //}

                            resPaypointKhaltiPaymentInfo = new PaypointModel()
                            {
                                billNumber = billNumber,
                                reserveInfo = reserveInfo,
                                refStan = refStan,
                                amount = special1,
                                transactionDate = exectransactionDate,
                                customerName = account,
                                companyCode = companyCode,
                                UserName = mobile,
                                ClientCode = ClientCode
                            };


                        }
                        //end list of package

                        //for checkpayment payaments response insert in database for vianet                       

                        int resultsPayments = PaypointUtils.PaypointKhaltiPaymentInfo(resPaypointKhaltiPaymentInfo);
                    }
                    else
                    {
                        keyrlt = xElem.Attribute("Key").Value;
                        resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;
                    }
                    resultMessageResCP = xElem.Elements("ResultMessage").Where(x => x.Name == "ResultMessage").SingleOrDefault().Value;

                    //for checkpayment response insert in database in nepal water
                    resCPPaypointKhaltiInfo = new PaypointModel()
                    {
                        companyCodeResCP = companyCode,
                        serviceCodeResCP = serviceCode,
                        accountResCP = account,
                        special1ResCP = special1,
                        special2ResCP = special2,

                        transactionDateResCP = transactionDate,
                        transactionIdResCP = transactionId,
                        userIdResCP = userId,
                        userPasswordResCP = userPassword,
                        salePointTypeResCP = salePointType,

                        refStanResCP = refStan,
                        amountResCP = amountpay,
                        billNumberResCP = billNumber,
                        //retrievalReferenceResCP = fundtransfer.tid,
                        retrievalReferenceResCP = tid,
                        responseCodeResCP = rltCheckPaymt,
                        descriptionResCP = "Check Payment " + keyrlt,
                        customerNameCP = customerName,
                        UserName = mobile,
                        ClientCode = ClientCode,
                        paypointType = paypointType,
                        resultMessageResCP = resultMessageResCP,

                    };
                }


                if (!(rltCheckPaymt == "000"))//show error when CP response is not 000 
                {
                    statusCode = "400";
                    // message = result;
                    message = resultMessageResCP;
                    failedmessage = message;

                }
                else
                {
                    statusCode = "200";
                    message = resultMessageResCP;
                    failedmessage = message;
                }
            }
            catch (Exception ex)
            {
                message = result + ex + "Error Message ";
                //mnft.ResponseStatus(HttpStatusCode.BadRequest, result); //200 - OK
                statusCode = "400";
                // failedmessage = message;
                failedmessage = resultMessageResCP;

            }

            try
            {
                int resultsReqCP = PaypointUtils.RequestCPPaypointInfo(reqCPPaypointKhaltiInfo);
                int resultsResCP = PaypointUtils.ResponseCPPaypointInfo(resCPPaypointKhaltiInfo);


                if ((resultsReqCP > 0) && (resultsResCP > 0))

                {
                    message = result;
                }
                else
                {
                    message = result;
                }

            }
            catch (Exception ex)
            {
                string ss = ex.Message;
                message = result;
            }


            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode == "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = resCPPaypointKhaltiInfo.retrievalReferenceResCP,
                    refStanCK = resCPPaypointKhaltiInfo.refStanResCP,
                };
                result = JsonConvert.SerializeObject(v);
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = failedmessage; // "Could not process your request,Please Try again.";
                    }
                    message = result;
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = failedmessage,
                    retrievalRef = "",
                    refStanCK = ""
                };
                result = JsonConvert.SerializeObject(v);
            }
            return result;

        }

        #endregion
    }
}
