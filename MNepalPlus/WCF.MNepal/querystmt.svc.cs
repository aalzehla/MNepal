using MNepalProject.Controllers;
using MNepalProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Web.Helpers;
using WCF.MNepal.Models;
using WCF.MNepal.Utilities;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class querystmt
    {
        [OperationContract]
        [WebGet]
        public string Statement(string tid, string sc, string mobile, string sa, string pin, string src)
        {
            string result = "";
            //start:Com focus one log///
            BalanceQuery balanceQuery = new BalanceQuery(tid, sc, mobile, sa, pin, src);
            var comfocuslog = new MNComAndFocusOneLog(balanceQuery, DateTime.Now);
            var mncomfocuslog = new MNComAndFocusOneLogsController();
            mncomfocuslog.InsertIntoComFocusOne(comfocuslog);
            //end:Com focus one log//

            //NOTE:- may be need to validate before insert into reply typpe
            //start:insert into reply type as HTTP//
            var replyType = new MNReplyType(balanceQuery.tid, "HTTP");
            var mnreplyType = new MNReplyTypesController();
            mnreplyType.InsertIntoReplyType(replyType);
            //end:insert into reply type as HTTP//


            //start:insert into transaction master//
            if (balanceQuery.valid())
            {
                var transaction = new MNTransactionMaster(balanceQuery);
                var mntransaction = new MNTransactionsController();
                MNTransactionMaster validTransactionData = mntransaction.Validate(transaction, balanceQuery.pin);
                result = validTransactionData.Response;
            }
            else
            {
                balanceQuery.Response = "error";
                balanceQuery.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid");
                result = balanceQuery.Response;
            }
            //end:insert into transaction master//
            return result;
        }

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string StatementByDate(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            string mobile = qs["mobile"];
            string start = qs["startdate"];
            string end = qs["enddate"];

            ReplyMessage replyMessage = new ReplyMessage();
            string result = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;

            var mntransaction = new MNTransactionsController();
            DateTime enddate = DateTime.ParseExact(end, "yyyy-MM-dd",
                                       System.Globalization.CultureInfo.InvariantCulture);
            DateTime startdate = DateTime.ParseExact(start, "yyyy-MM-dd",
                                    System.Globalization.CultureInfo.InvariantCulture);


            string validTransactionData = mntransaction.StatementByDate(mobile, startdate, enddate);

            if (validTransactionData.Equals("null"))
            {
                statusCode = "400";
                //replyMessage.Response = validTransactionData;
                //replyMessage.ResponseCode = HttpStatusCode.BadRequest.ToString();
                //replyMessage.ResponseStatus(HttpStatusCode.BadRequest, "UnSuccess");
                message = replyMessage.Response;
            }
            else
            {
                statusCode = "200";
                //replyMessage.Response = validTransactionData;
                //replyMessage.ResponseCode = HttpStatusCode.OK.ToString();
                //replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                message = validTransactionData;

            }

            result = message;

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = message;
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                result = message;
            }

            if (!statusCode.Equals(""))
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = result
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string BankStatementByDate(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            string mobile = qs["mobile"];
            string start = qs["startdate"];
            string end = qs["enddate"];

            string result = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;

            var mntransaction = new MNTransactionsController();
            DateTime enddate = DateTime.ParseExact(end, "yyyy-MM-dd",
                                       System.Globalization.CultureInfo.InvariantCulture);
            DateTime startdate = DateTime.ParseExact(start, "yyyy-MM-dd",
                                    System.Globalization.CultureInfo.InvariantCulture);


            string validTransactionData = mntransaction.BankStatementByDate(mobile, startdate, enddate);

            if (validTransactionData.Equals("null"))
            {
                statusCode = "400";
                message = "No Data Found";
            }
            else
            {
                statusCode = "200";
                message = validTransactionData;

            }

            result = message;

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = message;
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                result = message;
            }

            if (!statusCode.Equals(""))
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = result
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }


        #region Customer Support
        public string PassDataToCustomerSupportController(CustomerSupport userInfo)
        {
            string results = "false";
            try
            {
                int result = CustActivityUtils.InsertCustSupportForm(userInfo);
                if (result > 0)
                {
                    results = "true";
                }
                else
                {
                    results = "false";
                }

            }
            catch (Exception ex)
            {
                results = "false";
            }


            return results;
        }

        [OperationContract]
        [WebInvoke(Method = "POST",
               ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string CustomerSupport(Stream input)
        {
            CustActivityModel custsmsInfo = new CustActivityModel();
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            string result = "";
            string statusCode = string.Empty;
            string message = string.Empty;
            string Name = qs["Name"];
            string MobileNumber = qs["MobileNumber"];
            string Email = qs["Email"];
            string Remarks = qs["Remarks"];
            string Category = qs["Category"];
            string passportImages = qs["Image"];
            string passportImagesName = "";
            string PassportImage = "";
            ReplyMessage replyMessage = new ReplyMessage();
            HttpPostedFileBase PassportPhoto = null;

            try
            {

                if (!string.IsNullOrEmpty(passportImages))
                {
                    //convert byte array to image
                    Image imgPP = Base64ToImage(passportImages);

                    // Convert Base64 String to byte[]
                    byte[] imageBytesPP = Convert.FromBase64String(passportImages);

                    string PhotoPathPP = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                    string PhotoNamePP = passportImagesName;

                    PassportPhoto = (HttpPostedFileBase)new MemoryPostedFile(imageBytesPP, PhotoPathPP, "Test");//filename "EcreditCardpayment.jpg"

                }

            }
            catch (Exception e)
            {
                statusCode = "400";
                message = "Image Missing/Invalid !" + e;
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }


            if ((PassportPhoto != null) && (PassportPhoto?.ContentLength != 0))
            {
                var pp = SaveAndReturnFileName(PassportPhoto, MobileNumber, "CustomerFeedback", "photo");
                var last4PP = pp.Substring(pp.Length - 4, 4);
                if (last4PP.Equals(".jpg"))
                {
                    PassportImage = string.Format(pp);
                }
                else
                {
                    PassportImage = string.Format(pp + ".jpg");
                }
            }


            if ((string.IsNullOrEmpty(MobileNumber)) || (string.IsNullOrEmpty(Remarks)) || (string.IsNullOrEmpty(Name)))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }
            else
            {

                if (!statusCode.Equals("400"))
                {

                    string customerNo = string.Empty;

                    CustomerSupport model = new CustomerSupport();

                    if ((string.IsNullOrEmpty(Name)) || (string.IsNullOrEmpty(MobileNumber)) || (Name == null) ||
                         (Email == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = " Parameters Missing/Invalid ";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                    }
                    else
                    {

                        string ClientExtReply = string.Empty;
                        CustomerSupport selfuservalidate = new CustomerSupport(Name, Email, MobileNumber, Remarks, Category, PassportImage);
                        ClientExtReply = PassDataToCustomerSupportController(selfuservalidate);
                        if (ClientExtReply == "true")
                        {
                            replyMessage.Response = "Successfully Requested!";
                            result = "Successfully Requested ! ";
                            replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                        }
                        else
                        {
                            replyMessage.Response = "User Not Requested !";
                            result = "User Not Requested ";
                            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                        }

                    }
                    string UserName = MobileNumber;
                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {

                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = MobileNumber,
                            RequestMerchant = "Customer Support Request",
                            DestinationNo = "",
                            Amount = "",
                            SMSStatus = "Success",
                            SMSSenderReply = replyMessage.Response,
                            ErrorMessage = "",
                        };
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var v = new
                        {
                            StatusCode = Convert.ToInt32(400),
                            StatusMessage = result
                        };
                        result = JsonConvert.SerializeObject(v);
                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = MobileNumber,
                            RequestMerchant = "Customer Support Request",
                            DestinationNo = "",
                            Amount = "",
                            SMSStatus = "Error",
                            SMSSenderReply = replyMessage.Response,
                            ErrorMessage = "",
                        };
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        var v = new
                        {
                            StatusCode = Convert.ToInt32(401),
                            StatusMessage = result
                        };
                        result = JsonConvert.SerializeObject(v);
                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = MobileNumber,
                            RequestMerchant = "Link Bank Account Request",
                            DestinationNo = "",
                            Amount = "",
                            SMSStatus = "Error",
                            SMSSenderReply = "Error",
                            ErrorMessage = "",
                        };
                    }

                }
                else
                {
                    // throw ex
                    statusCode = "400";
                    message = "Error";
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                    custsmsInfo = new CustActivityModel()
                    {
                        UserName = MobileNumber,
                        RequestMerchant = "Customer Support Request",
                        DestinationNo = "",
                        Amount = "",
                        SMSStatus = "Error",
                        SMSSenderReply = message,
                        ErrorMessage = "",
                    };
                }
            }

            if (statusCode == "")
            {
                result = result.ToString();
            }
            else if (statusCode != "200")
            {
                if (message == "")
                {
                    message = "";
                }
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }
            //Register For SMS
            try
            {
                int results = CustActivityUtils.RegisterCustActivityInfo(custsmsInfo);
                if (results > 0)
                {

                    message = result;
                    var v = new
                    {
                        StatusCode = Convert.ToInt32(200),
                        StatusMessage = result
                    };
                    result = JsonConvert.SerializeObject(v);
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

            /*END SMS Register*/
            return result;
        }

        #endregion
        
        public class MemoryPostedFile : HttpPostedFileBase
        {
            private readonly byte[] FileBytes;
            private string FilePath;

            public MemoryPostedFile(byte[] fileBytes, string path, string fileName = null)
            {
                this.FilePath = path;
                this.FileBytes = fileBytes;
                this._FileName = fileName;
                this._Stream = new MemoryStream(fileBytes);
            }

            public override int ContentLength { get { return FileBytes.Length; } }
            public override String FileName { get { return _FileName; } }
            private String _FileName;
            public override Stream InputStream
            {
                get
                {
                    if (_Stream == null)
                    {
                        _Stream = new FileStream(_FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                    return _Stream;
                }
            }
            private Stream _Stream;
            public override void SaveAs(string filename)
            {
                System.IO.File.WriteAllBytes(filename, System.IO.File.ReadAllBytes(FilePath));
            }
        }

        public Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        public string SaveAndReturnFileName(HttpPostedFileBase file, string mobile, string imagetype, string imageside)
        {

            if (file == null)
                return null;

            //if (IsImage(file) == false)
            //    return null;

            int iFileSize = file.ContentLength;
            if (iFileSize > 2097152) //1048576-1MB
                return null;

            WebImage img = new WebImage(file.InputStream);
            img.Resize(440, 300, true, true);
            var fileName = System.IO.Path.GetFileName(file.FileName);
            string randomFileName = mobile + "_" + imagetype + "_" + imageside +
                                "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".jpg";//Path.GetExtension(fileName);
            //var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Upload/"), randomFileName + ".jpg");

            //if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/Content/Upload")))
            //{
            //    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Content/Upload"));
            //}

            string _ImageSaveUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["ImageAddress"];

            var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Upload/"), randomFileName); ///MNepalAdmin.Com/Content/Upload/

            //if (!Directory.Exists(HttpContext.Current.Server.MapPath(_ImageSaveUrl + "~/Content/Upload")))
            //{
            //    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(_ImageSaveUrl + "~/Content/Upload"));
            //}
            img.Save(path);
            return randomFileName;
        }
    }
}
