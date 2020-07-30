using MNepalProject.Controllers;
using MNepalProject.Helper;
using MNepalProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using System.Web.Helpers;
using WCF.MNepal.Helper;
using WCF.MNepal.Models;
using WCF.MNepal.Utilities;
using System.Collections.Generic;

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class selfReg
    {
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public string RegisterCheck(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string userName = qs["mobile"];
            string src = qs["src"];

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            ReplyMessage replyMessage = new ReplyMessage();
            string result = "";
            string code = string.Empty;
            string statusCode = string.Empty;
            string message = string.Empty;

            if ((userName == null) || (src == null))
            {
                statusCode = "400";
                message = "Parameters Missing/Invalid";
            }
            else
            {
                try
                {
                    if (!IsValidUserName(userName))
                    {
                        TraceIdGenerator otp = new TraceIdGenerator();
                        code = otp.GetUniqueOTPKey();

                        statusCode = "200";
                        message = code;
                        replyMessage.ResponseStatus(HttpStatusCode.OK, message);
                    }
                    else
                    {

                        //statusCode = "400";
                        //result = "The number is registered !!";
                        //replyMessage.Response = "The number is registered !!";
                        //replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        //message = result;

                        statusCode = "400";
                        result = "A OTP code will be sent to your phone if the account exists in our system.";
                        replyMessage.Response = "A OTP code will be sent to your phone if the account exists in our system.";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                        message = result;
                    }

                }
                catch (Exception ex)
                {
                    statusCode = "400";
                    result = "Please Contact to the administrator !!" + ex;
                    replyMessage.Response = result;
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                    message = result;
                }
            }

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    string messagereply = "Dear Customer," + "\n";
                    messagereply += " Your Verification Code is " + code
                        + "." + "\n" + "Close this message and enter code to verify account.";
                    messagereply += "Thank-you. -MNepal";

                    var client = new WebClient();

                    string mobile = "";
                    mobile = userName;

                    if ((mobile.Substring(0, 3) == "980") || (mobile.Substring(0, 3) == "981")) //FOR NCELL
                    {
                        //FOR NCELL
                        //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                        //+ "977" + mobile + "&Text=" + messagereply + "");

                        var contents = client.DownloadString(
                            SMSNCELL + "977" + mobile + "&Text=" + messagereply + "");

                        statusCode = "200";
                        message = code;
                    }
                    else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                        || (mobile.Substring(0, 3) == "986"))
                    {
                        //FOR NTC
                        //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                        //    + "977" + mobile + "&Text=" + messagereply + "");

                        var contents = client.DownloadString(
                            SMSNTC + "977" + mobile + "&Text=" + messagereply + "");

                        statusCode = "200";
                        message = code;
                    }
                    else
                    {
                        //FOR SMARTCELL
                        //var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                        //    + "977" + mobile + "&Text=" + messagereply + "");
                        statusCode = "400";
                        message = "Network Operator is not available !! ";
                    }

                    SMSLog log = new SMSLog();
                    log.SentBy = mobile;
                    log.Purpose = "Self Registration";
                    log.UserName = userName;
                    log.Message = messagereply;
                    CustomerUtils.LogSMS(log);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                //statusCode = "400";
                //result = "The number is registered !!";
                //replyMessage.Response = "The number is registered !!";
                //replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                //message = result;


                statusCode = "400";
                result = "A OTP code will be sent to your phone if the account exists in our system";
                replyMessage.Response = "A OTP code will be sent to your phone if the account exists in our system";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                message = result;
            }

            if (statusCode != "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }
            if (statusCode == "200")
            {
                var v = new
                {
                    StatusCode = Convert.ToInt32(statusCode),
                    StatusMessage = message
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }

        public bool IsValidUserName(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                DataTable dtCheckUserName = CustCheckUtils.GetCustUserInfo(username);
                if (dtCheckUserName.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }


        [OperationContract]
        [WebInvoke(Method = "POST",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string SelfNewRegAuthLogin(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            string PProvinceID = string.Empty;
            string CProvinceID = string.Empty;
            string PDistrictID = string.Empty;
            string CDistrictID = string.Empty;

            string result = "";
            string statusCode = string.Empty;
            string message = string.Empty;

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            string UserName = qs["UserName"];
            string FName = qs["fName"];
            string MName = qs["mName"];
            string LName = qs["lName"];

            string Gender = qs["gender"];
            string DateOfBirth = qs["dateOfBirth"];
            string CountryCode = qs["countryCode"];
            string Nationality = qs["nationality"];
            string FathersName = qs["fathersName"];
            string MothersName = qs["mothersName"];
            string MaritalStatus = qs["maritalStatus"];
            string SpouseName = qs["spouseName"];
            string GrandFathersName = qs["gFathersName"];
            string FatherInLaw = qs["FatherInLaw"];
            string Occupation = qs["occupation"];
            string EmailAddress = qs["EmailAddress"];

            string PProvince = qs["PZone"];
            string PDistrict = qs["PDistrict"];
            string PMunicipalityVDC = qs["PMunicipalityVDC"];
            string PHouseNo = qs["PHouseNo"];
            string PWardNo = qs["PWardNo"];
            string PStreet = qs["PStreet"];

            string CProvince = qs["CZone"];
            string CDistrict = qs["CDistrict"];
            string CMunicipalityVDC = qs["CMunicipalityVDC"];
            string CHouseNo = qs["CHouseNo"];
            string CWardNo = qs["CWardNo"];
            string CStreet = qs["CStreet"];

            string CitizenshipNo = qs["CitizenshipNo"];
            string CitizenPlaceOfIssue = qs["CitizenPlaceOfIssue"];
            string CitizenIssueDate = qs["CitizenIssueDate"];

            string PassportNo = qs["PassportNo"];
            string PassportPlaceOfIssue = qs["PassportPlaceOfIssue"];
            string PassportIssueDate = qs["PassportIssueDate"];
            string PassportExpireDate = qs["PassportExpiryDate"];

            string LicenseNo = qs["LicenseNo"];
            string LicensePlaceOfIssue = qs["LicensePlaceOfIssue"];
            string LicenseIssueDate = qs["LicenseIssueDate"];
            string LicenseExpiryDate = qs["LicenseExpiryDate"];

            string PANNumber = qs["PANNumber"];
            string BranchCode = qs["BranchCode"];
            string Source = qs["src"];
            string OTPCode = qs["OTPCode"];

            //DocType Image
            string DocumentType = qs["DocType"];

            string frontImages = qs["frontImage"];
            string backImages = qs["backImage"];
            string passportImages = qs["ppImage"];

            string frontImagesName = qs["frontImageName"];
            string backImagesName = qs["backImageName"];
            string passportImagesName = qs["ppImageName"];

            ReplyMessage replyMessage = new ReplyMessage();

            if ((string.IsNullOrEmpty(UserName)) || (string.IsNullOrEmpty(FName)) || (string.IsNullOrEmpty(LName)) ||
                (string.IsNullOrEmpty(Gender)) || (string.IsNullOrEmpty(DateOfBirth)) || (string.IsNullOrEmpty(CountryCode)) || (string.IsNullOrEmpty(Nationality)) ||
                (string.IsNullOrEmpty(FathersName)) 
                //|| (string.IsNullOrEmpty(MothersName)) 
                || (string.IsNullOrEmpty(MaritalStatus)) ||
                (string.IsNullOrEmpty(GrandFathersName)) || (string.IsNullOrEmpty(Occupation)) || (string.IsNullOrEmpty(EmailAddress)) ||
                (string.IsNullOrEmpty(PProvince)) || (string.IsNullOrEmpty(PDistrict)) || (string.IsNullOrEmpty(PMunicipalityVDC)) ||
                (string.IsNullOrEmpty(PWardNo)) ||
                (string.IsNullOrEmpty(CProvince)) || (string.IsNullOrEmpty(CDistrict)) || (string.IsNullOrEmpty(CMunicipalityVDC)) ||
                (string.IsNullOrEmpty(CWardNo)) ||
                (string.IsNullOrEmpty(BranchCode)) || (string.IsNullOrEmpty(Source)) ||
                (string.IsNullOrEmpty(frontImages)) || (string.IsNullOrEmpty(backImages)) || (string.IsNullOrEmpty(passportImages))) // (string.IsNullOrEmpty(PHouseNo)) || (string.IsNullOrEmpty(CHouseNo)) ||
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }
            else
            {
                if (MaritalStatus.Equals("Married"))
                {
                    if ((string.IsNullOrEmpty(SpouseName)) || (string.IsNullOrEmpty(FatherInLaw)))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Marital Status Parameters Missing/Invalid";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                    }
                }

                if (!statusCode.Equals("400"))
                {

                    if ((!string.IsNullOrEmpty(PProvince)) && (!string.IsNullOrEmpty(PDistrict))
                        && (!string.IsNullOrEmpty(CProvince)) && (!string.IsNullOrEmpty(CDistrict)))
                    {
                        PProvinceID = GetProvinceID(PProvince);
                        CProvinceID = GetProvinceID(CProvince);
                        PDistrictID = GetDistrictID(PDistrict);
                        CDistrictID = GetDistrictID(CDistrict);
                    }


                    if (!string.IsNullOrEmpty(DateOfBirth))
                    {
                        DateTime.ParseExact(DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        DateOfBirth = "";
                    }

                    if (!string.IsNullOrEmpty(CitizenIssueDate))
                    {
                        DateTime.ParseExact(CitizenIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        CitizenIssueDate = "01/01/1900";
                    }

                    if (!string.IsNullOrEmpty(PassportIssueDate))
                    {
                        DateTime.ParseExact(PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        PassportIssueDate = "01/01/1900";
                    }

                    if (!string.IsNullOrEmpty(PassportExpireDate))
                    {
                        DateTime.ParseExact(PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        PassportExpireDate = "01/01/1900";
                    }

                    if (!string.IsNullOrEmpty(LicenseIssueDate))
                    {
                        DateTime.ParseExact(LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        LicenseIssueDate = "01/01/1900";
                    }
                    if (!string.IsNullOrEmpty(LicenseExpiryDate))
                    {
                        DateTime.ParseExact(LicenseExpiryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        LicenseExpiryDate = "01/01/1900";
                    }

                    string customerNo = string.Empty;

                    UserValidate model = new UserValidate();

                    if ((PProvinceID == "") || (CProvinceID == "") ||
                        (PDistrictID == "") || (CDistrictID == ""))
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Province/District Parameters Missing/Invalid";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                    }
                    else
                    {
                        if (((!string.IsNullOrEmpty(CitizenshipNo)) && (!string.IsNullOrEmpty(CitizenPlaceOfIssue)) && (!string.IsNullOrEmpty(CitizenIssueDate))) ||
                                             ((!string.IsNullOrEmpty(PassportNo)) && (!string.IsNullOrEmpty(PassportPlaceOfIssue)) && (!string.IsNullOrEmpty(PassportIssueDate))
                                                && (!string.IsNullOrEmpty(PassportExpireDate))) ||
                                             ((!string.IsNullOrEmpty(LicenseNo)) && (!string.IsNullOrEmpty(LicensePlaceOfIssue)) && (!string.IsNullOrEmpty(LicenseIssueDate))
                                                && (!string.IsNullOrEmpty(LicenseExpiryDate))))
                        {
                            if (CitizenshipNo == null)
                            {
                                CitizenshipNo = "";
                            }
                            if (CitizenPlaceOfIssue == null)
                            {
                                CitizenPlaceOfIssue = "";
                            }

                            if (PassportNo == null)
                            {
                                PassportNo = "";
                            }
                            if (PassportPlaceOfIssue == null)
                            {
                                PassportPlaceOfIssue = "";
                            }

                            if (LicenseNo == null)
                            {
                                LicenseNo = "";
                            }
                            if (LicensePlaceOfIssue == null)
                            {
                                LicensePlaceOfIssue = "";
                            }

                            //string frontString = Base64String(frontImages);
                            //string backString = Base64String(backImages);
                            //string passportString = Base64String(passportImages);
                            try
                            {
                                if (!string.IsNullOrEmpty(frontImages))
                                {
                                    //convert byte array to image
                                    Image imgFront = Base64ToImage(frontImages);//(frontString);

                                    // Convert Base64 String to byte[]
                                    byte[] imageBytesFront = Convert.FromBase64String(frontImages);

                                    string PhotoPathFront = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                                    string PhotoNameFront = "Knitting Balls";

                                    model.FrontPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesFront, PhotoPathFront, PhotoNameFront);//filename "EcreditCardpayment.jpg"

                                    //var FrontPic = SaveAndReturnFileName(model.FrontPic, UserName, "Citizen", "front");
                                }
                                if (!string.IsNullOrEmpty(backImages))
                                {
                                    //convert byte array to image
                                    Image imgBack = Base64ToImage(backImages);//(backString);

                                    // Convert Base64 String to byte[]
                                    byte[] imageBytesBack = Convert.FromBase64String(backImages);

                                    string PhotoPathBack = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                                    string PhotoNameBack = backImagesName;

                                    model.BackPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesBack, PhotoPathBack, PhotoNameBack);//filename "EcreditCardpayment.jpg"

                                }
                                if (!string.IsNullOrEmpty(passportImages))
                                {
                                    //convert byte array to image
                                    Image imgPP = Base64ToImage(passportImages);

                                    // Convert Base64 String to byte[]
                                    byte[] imageBytesPP = Convert.FromBase64String(passportImages);

                                    string PhotoPathPP = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                                    string PhotoNamePP = passportImagesName;

                                    model.PassportPhoto = (HttpPostedFileBase)new MemoryPostedFile(imageBytesPP, PhotoPathPP, PhotoNamePP);//filename "EcreditCardpayment.jpg"

                                }

                            }
                            catch (Exception e)
                            {
                                statusCode = "400";
                                message = "Image Missing/Invalid !" + e;
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                            }


                            if (statusCode == "400")
                            {
                                // throw ex
                                statusCode = "400";
                                message = "Image Missing/Invalid";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                            }
                            else if (statusCode != "400")
                            {
                                DataTable dtableUserCheck = CustCheckUtils.GetCustUserInfo(UserName);

                                if (dtableUserCheck.Rows.Count == 0)
                                {
                                    statusCode = "400";
                                    message = "Not Registered !";
                                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                                }
                                else if (dtableUserCheck.Rows.Count == 1)
                                {
                                    if ((!model.PassportPhoto.Equals(""))
                                        || (!model.FrontPic.Equals(""))
                                            || (!model.BackPic.Equals("")))
                                    {
                                        var PP = SaveAndReturnFileName(model.PassportPhoto, UserName, "Passport", "photo");
                                        var front = SaveAndReturnFileName(model.FrontPic, UserName, DocumentType, "front");
                                        var back = SaveAndReturnFileName(model.BackPic, UserName, DocumentType, "back");

                                        var last4PP = PP.Substring(PP.Length - 4, 4);
                                        var last4Front = front.Substring(front.Length - 4, 4);
                                        var last4Back = back.Substring(back.Length - 4, 4);

                                        string PassportImage;
                                        string FrontImage;
                                        string BackImage;

                                        if (last4PP.Equals(".jpg"))
                                        {
                                            PassportImage = string.Format(PP);
                                        }
                                        else
                                        {
                                            PassportImage = string.Format(PP + ".jpg");
                                        }

                                        if (last4Front.Equals(".jpg"))
                                        {
                                            FrontImage = string.Format(front);
                                        }
                                        else
                                        {
                                            FrontImage = string.Format(front + ".jpg");
                                        }

                                        if (last4Back.Equals(".jpg"))
                                        {
                                            BackImage = string.Format(back);
                                        }
                                        else
                                        {
                                            BackImage = string.Format(back + ".jpg");
                                            // BackImage = string.Format("~/Content/Upload/{0}", back + ".jpg");
                                        }

                                        string userType = "user";
                                        string Passwords = SelfRegisterUtils.GeneratePassword();
                                        string Password = HashAlgo.Hash(Passwords);

                                        if ((string.IsNullOrEmpty(UserName)) || (string.IsNullOrEmpty(FName)) || (LName == null) ||
                                            (Gender == null) || (DateOfBirth == null) || (CountryCode == null) || (Nationality == null) ||
                                            (FathersName == null) 
                                            //|| (MothersName == null) 
                                            || (MaritalStatus == null) || (GrandFathersName == null) || (Occupation == null) ||
                                            (PProvince == null) || (PDistrict == null) || (PProvinceID == null) || (PDistrictID == null) ||
                                            (PMunicipalityVDC == null) || (PHouseNo == null) || (PWardNo == null) || (PStreet == null) ||
                                            (CProvinceID == null) || (CDistrictID == null) || (CMunicipalityVDC == null) || (CHouseNo == null) ||
                                            (CWardNo == null) || (CStreet == null) || (BranchCode == null) || (Source == null) ||
                                            (string.IsNullOrEmpty(FrontImage)) || (string.IsNullOrEmpty(BackImage)) || (string.IsNullOrEmpty(PassportImage)))
                                        {
                                            // throw ex
                                            statusCode = "400";
                                            message = " Parameters Missing/Invalid ";
                                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                                        }
                                        else
                                        {
                                            UserValidate uservalidate = new UserValidate(UserName, Password, userType.ToLower());
                                            Message<UserValidate> uservalidaterequest = new Message<UserValidate>(uservalidate);
                                            MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(0, uservalidaterequest.message, "0", DateTime.Now);
                                            MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

                                            result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
                                            if (result == "Success")
                                            {
                                                string ClientExtReply = "";
                                                if ((FrontImage.Length > 0) && (BackImage.Length > 0) && (PassportImage.Length > 0))
                                                {
                                                    string Pin = SelfRegisterUtils.GeneratePin();
                                                    UserValidate selfuservalidate = new UserValidate(UserName, Password, Pin, userType.ToLower(),
                                                    OTPCode, Source, FName, MName, LName, Gender, DateOfBirth, CountryCode, Nationality,
                                                    FathersName, MothersName, GrandFathersName, MaritalStatus, SpouseName, FatherInLaw,
                                                    Occupation,
                                                    PProvince, PDistrict, PDistrictID, PMunicipalityVDC, PHouseNo, PWardNo, PStreet, PProvinceID,
                                                    CProvince, CDistrict, CDistrictID, CMunicipalityVDC, CHouseNo, CWardNo, CStreet, CProvinceID,
                                                    CitizenshipNo, CitizenPlaceOfIssue, CitizenIssueDate,
                                                    PassportNo, PassportPlaceOfIssue, PassportIssueDate, PassportExpireDate,
                                                    LicenseNo, LicensePlaceOfIssue, LicenseIssueDate, LicenseExpiryDate,
                                                    PANNumber, DocumentType, "F", EmailAddress, BranchCode, FrontImage, BackImage, PassportImage);
                                                    ClientExtReply = PassDataToMNClientExtController(selfuservalidate);
                                                    if (ClientExtReply == "true")
                                                    {
                                                        replyMessage.Response = "Successfully Registered ! ";
                                                        result = "Successfully Registered ! ";
                                                        replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                                                    }
                                                    else
                                                    {
                                                        replyMessage.Response = "User Not Registered !";
                                                        result = "User Not Registered ";
                                                        replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                                    }
                                                }
                                                else
                                                {
                                                    replyMessage.Response = "Photo Not Uploded !";
                                                    result = "Photo Not Uploded !";
                                                    replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                                }
                                            }
                                            else
                                            {

                                                replyMessage.Response = "request denied";
                                                result = "Request Denied";
                                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                                                statusCode = "400";
                                                message = replyMessage.Response;
                                            }
                                        }


                                        OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                        if (response.StatusCode == HttpStatusCode.OK)
                                        {
                                            try
                                            {
                                                //Sender
                                                string messagereply = "Dear " + FName + "," + "\n";
                                                messagereply += " Your KYC request has been queued for the approval. You'll be notified shortly"
                                                                + "." + "\n";
                                                messagereply += "Thank you. MNepal";

                                                var client = new WebClient();

                                                if ((UserName.Substring(0, 3) == "980") || (UserName.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    //var content = client.DownloadString(
                                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    //    + "977" + UserName + "&Text=" + messagereply + "");
                                                    var contents = client.DownloadString(
                                                                    SMSNCELL + "977" + UserName + "&Text=" + messagereply + "");
                                                }
                                                else if ((UserName.Substring(0, 3) == "985") || (UserName.Substring(0, 3) == "984")
                                                            || (UserName.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    //var content = client.DownloadString(
                                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                    //    + "977" + UserName + "&Text=" + messagereply + "");

                                                    var contents = client.DownloadString(
                                                                    SMSNTC + "977" + UserName + "&Text=" + messagereply + "");
                                                }

                                                var v = new
                                                {
                                                    StatusCode = Convert.ToInt32(200),
                                                    StatusMessage = result
                                                };
                                                result = JsonConvert.SerializeObject(v);

                                            }
                                            catch (Exception ex)
                                            {
                                                throw ex;
                                            }
                                        }
                                        else if (response.StatusCode == HttpStatusCode.BadRequest)
                                        {
                                            var v = new
                                            {
                                                StatusCode = Convert.ToInt32(400),
                                                StatusMessage = result
                                            };
                                            result = JsonConvert.SerializeObject(v);
                                        }
                                        else if (response.StatusCode == HttpStatusCode.Unauthorized)
                                        {
                                            var v = new
                                            {
                                                StatusCode = Convert.ToInt32(401),
                                                StatusMessage = result
                                            };
                                            result = JsonConvert.SerializeObject(v);
                                        }

                                    }
                                    else
                                    {

                                        // throw ex
                                        statusCode = "400";
                                        message = "Image Missing/Invalid";
                                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);

                                    }
                                }

                            }

                        }
                        else
                        {
                            statusCode = "400";
                            message = "Missing/Invalid Required Data";
                            result = "Missing/Invalid Required Data";
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                        }
                    }
                }
                else
                {
                    statusCode = "400";
                    message = "Marital Status Missing/Invalid Required Data";
                    result = "Marital Status Missing/Invalid Required Data";
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
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

            return result;
        }


        public string PassDataToMNClientExtController(UserValidate userInfo)
        {
            string reply = "false";
            try
            {
                int results = SelfRegisterUtils.RegisterUsersInfo(userInfo);
                if (results > 0)
                {
                    reply = "true";
                }
                else
                {
                    reply = "false";
                }

            }
            catch (Exception ex)
            {
                reply = "false";
            }


            return reply;
        }



        public PassAndPin PassDataToQuickMNClientExtController(UserValidate userInfo)
        {
            PassAndPin results = new PassAndPin();
            results.result = "false";
            try
            {
                results = SelfRegisterUtils.QuickRegisterUsersInfo(userInfo);
                if (results.status > 0)
                {
                    results.result = "true";
                }
                else
                {
                    results.result = "false";
                }

            }
            catch (Exception ex)
            {
                results.result = "false";
            }


            return results;
        }


        public string GetProvinceID(string provinceName)
        {
            string PProvinceID = string.Empty;
            try
            {
                string results = SelfRegisterUtils.GetProvinceID(provinceName);
                if (!results.Equals("0"))
                {
                    PProvinceID = results.ToString();
                }
                else
                {
                    PProvinceID = "0";
                }

            }
            catch (Exception ex)
            {
                PProvinceID = "0";
            }
            return PProvinceID;
        }

        public string GetDistrictID(string districtName)
        {
            string DistrictID = string.Empty;
            SQLHelperQuery objDAL = new SQLHelperQuery();
            try
            {
                string districtString = "SELECT DistrictID FROM MNDistrict WHERE Name='" + districtName + "'";

                DataTable dt = new DataTable();
                dt = objDAL.AccessQueryMethod(districtString);
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        DistrictID = (row[0].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                DistrictID = "0";
            }
            return DistrictID;
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
            //string randomFileName = mobile + "_" + imagetype + "_" + imageside +
            //                    "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".jpg";//Path.GetExtension(fileName);

            TraceIdGenerator traceid = new TraceIdGenerator();
            string uchar = traceid.GetUniqueChar();

            string randomFileName = mobile + "_" + imagetype + "_" + imageside +
                                "_" + uchar + ".jpg";//Path.GetExtension(fileName);


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


        private bool IsImage(HttpPostedFileBase file)
        {
            //Checks for image type... you could also do filename extension checks and other things
            return ((file != null) && System.Text.RegularExpressions.Regex.IsMatch(file.ContentType, "image/\\S+") && (file.ContentLength > 0));
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

        private string Base64String(string Path)
        {
            using (Image image = Image.FromFile(Path))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }

        [OperationContract]
        [WebInvoke(Method = "POST",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string QuickRegister(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            //TraceIdGenerator _tig = new TraceIdGenerator();
            //var tid = _tig.GenerateTraceID();

            TraceIdGenerator traceid = new TraceIdGenerator();
            string tid = traceid.GenerateUniqueTraceID();

            string result = "";
            string statusCode = string.Empty;
            string message = string.Empty;
            string pin = string.Empty;
            string password = string.Empty;
            string retRef = tid;

            string UserName = qs["UserName"];
            string FName = qs["fName"];
            string MName = qs["mName"];
            string LName = qs["lName"];
            string Name = FName + " " + MName + " " + LName;
            string Source = qs["src"];
            string OTPCode = qs["OTPCode"];


            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];


            ReplyMessage replyMessage = new ReplyMessage();

            if ((string.IsNullOrEmpty(UserName)) || (string.IsNullOrEmpty(FName)) || (string.IsNullOrEmpty(LName)) ||
                (string.IsNullOrEmpty(Source)))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }
            else
            {
                if (IsValidUserName(UserName))
                {
                    statusCode = "400";
                    message = "Number Already Registered!!";
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                }
                else
                {
                    if (!statusCode.Equals("400"))
                    {

                        string customerNo = string.Empty;

                        UserValidate model = new UserValidate();

                        string userType = "user";
                        string Password = SelfRegisterUtils.GeneratePassword();

                        if ((string.IsNullOrEmpty(UserName)) || (string.IsNullOrEmpty(FName)) || (LName == null) ||
                             (Source == null))
                        {
                            // throw ex
                            statusCode = "400";
                            message = " Parameters Missing/Invalid ";
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                        }
                        else
                        {
                            UserValidate uservalidate = new UserValidate(UserName, Password, userType.ToLower());
                            Message<UserValidate> uservalidaterequest = new Message<UserValidate>(uservalidate);
                            MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(0, uservalidaterequest.message, "0", DateTime.Now);
                            MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

                            result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
                            if (result == "Success")
                            {
                                PassAndPin ClientExtReply = new PassAndPin();

                                string Pin = SelfRegisterUtils.GeneratePin();
                                UserValidate selfuservalidate = new UserValidate(UserName, HashAlgo.Hash(Password), Pin, userType.ToLower(),
                                OTPCode, Source, FName, MName, LName, retRef);
                                ClientExtReply = PassDataToQuickMNClientExtController(selfuservalidate);
                                if (ClientExtReply.result == "true")
                                {
                                    pin = ClientExtReply.pin;
                                    password = Password;
                                    replyMessage.Response = "Successfully Registered ! ";
                                    result = "Successfully Registered ! ";
                                    replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                                }
                                else
                                {
                                    replyMessage.Response = "User Not Registered !";
                                    result = "User Not Registered ";
                                    replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                }

                            }
                            else
                            {

                                replyMessage.Response = "request denied";
                                result = "Request Denied";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                                statusCode = "400";
                                message = replyMessage.Response;
                            }
                        }


                        OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            try
                            {
                                //Sender
                                string messagereply = "Dear " + FName + ", \n";
                                messagereply += "Your thaili wallet login Password is " + password +
                                " and transaction T-PIN is " + pin + "." + "\n";
                                messagereply += "Thank you. MNepal";

                                var client = new WebClient();

                                if ((UserName.Substring(0, 3) == "980") || (UserName.Substring(0, 3) == "981")) //FOR NCELL
                                {
                                    //FOR NCELL
                                    //var content = client.DownloadString(
                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                    //    + "977" + UserName + "&Text=" + messagereply + "");
                                    var contents = client.DownloadString(
                                              SMSNCELL + "977" + UserName + "&Text=" + messagereply + "");
                                }
                                else if ((UserName.Substring(0, 3) == "985") || (UserName.Substring(0, 3) == "984")
                                            || (UserName.Substring(0, 3) == "986"))
                                {
                                    //FOR NTC
                                    //var content = client.DownloadString(
                                    //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                    //    + "977" + UserName + "&Text=" + messagereply + "");
                                    var contents = client.DownloadString(
                                              SMSNTC + "977" + UserName + "&Text=" + messagereply + "");
                                }

                                var v = new
                                {
                                    StatusCode = Convert.ToInt32(200),
                                    StatusMessage = result
                                };

                                //insert into MNResponse
                                UserValidate selfuservalidateResponse = new UserValidate(UserName, retRef, "200", result);
                                int ret = SelfRegisterUtils.InsertResponseQuickSelfReg(selfuservalidateResponse);
                                
                                result = JsonConvert.SerializeObject(v);

                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            var v = new
                            {
                                StatusCode = Convert.ToInt32(400),
                                StatusMessage = result
                            };
                            result = JsonConvert.SerializeObject(v);
                        }
                        else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            var v = new
                            {
                                StatusCode = Convert.ToInt32(401),
                                StatusMessage = result
                            };
                            result = JsonConvert.SerializeObject(v);
                        }

                    }
                    else
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Error";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);

                    }
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

            return result;
        }

        [OperationContract]
        [WebGet]
        public string QuickRegisterView(string cmobileno)
        {
            string result = "";
            string message = string.Empty;
            string statusCode = string.Empty;

            string UserName = string.Empty;
            string EmailAddress = string.Empty;
            string FName = string.Empty;
            string MName = string.Empty;
            string LName = string.Empty;
            string Gender = string.Empty;
            string DateOfBirth = string.Empty;
            string CountryCode = string.Empty;
            string Nationality = string.Empty;
            string FathersName = string.Empty;
            string MothersName = string.Empty;
            string MaritalStatus = string.Empty;
            string SpouseName = string.Empty;
            string GFathersName = string.Empty;
            string FatherInLaw = string.Empty;
            string Occupation = string.Empty;

            string PProvince = string.Empty;
            string PDistrict = string.Empty;
            string PMunicipalityVDC = string.Empty;
            string PHouseNo = string.Empty;
            string PWardNo = string.Empty;
            string PStreet = string.Empty;
            string PProvinceName = string.Empty;
            string PDistrictName = string.Empty;

            string CProvince = string.Empty;
            string CDistrict = string.Empty;
            string CMunicipalityVDC = string.Empty;
            string CHouseNo = string.Empty;
            string CWardNo = string.Empty;
            string CStreet = string.Empty;
            string CProvinceName = string.Empty;
            string CDistrictName = string.Empty;

            string CitizenshipNo = string.Empty;
            string CitizenPlaceOfIssue = string.Empty;
            string CitizenIssueDate = string.Empty;

            string PassportNo = string.Empty;
            string PassportPlaceOfIssue = string.Empty;
            string PassportIssueDate = string.Empty;
            string PassportExpiryDate = string.Empty;

            string LicenseNo = string.Empty;
            string LicensePlaceOfIssue = string.Empty;
            string LicenseIssueDate = string.Empty;
            string LicenseExpiryDate = string.Empty;

            string DateADBS = string.Empty;
            string BSDateOfBirth = string.Empty;
            string CustStatus = string.Empty;
            string PANNumber = string.Empty;

            string DocType = string.Empty;
            string FrontImage = string.Empty;
            string BackImage = string.Empty;
            string PassportImage = string.Empty;

            ReplyMessage replyMessage = new ReplyMessage();

            if (string.IsNullOrEmpty(cmobileno))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }
            else
            {
                //start: check customer KYC detail
                var resultList = new List<ClientKYC>();
                DataTable dtableResult = CustCheckUtils.GetQRCustKYCInfo(cmobileno);
                if (dtableResult.Rows.Count == 1)
                {
                    foreach (DataRow dtableUser in dtableResult.Rows)
                    {
                        UserName = dtableUser["UserName"].ToString();
                        EmailAddress = dtableUser["EmailAddress"].ToString();
                        FName = dtableUser["FName"].ToString();
                        MName = dtableUser["MName"].ToString();
                        LName = dtableUser["LName"].ToString();
                        Gender = dtableUser["Gender"].ToString();
                        DateOfBirth = dtableUser["DateOfBirth"].ToString();
                        CountryCode = dtableUser["CountryCode"].ToString();
                        Nationality = dtableUser["Nationality"].ToString();
                        FathersName = dtableUser["FathersName"].ToString();
                        MothersName = dtableUser["MothersName"].ToString();
                        MaritalStatus = dtableUser["MaritalStatus"].ToString();
                        SpouseName = dtableUser["SpouseName"].ToString();
                        GFathersName = dtableUser["GFathersName"].ToString();
                        FatherInLaw = dtableUser["FatherInLaw"].ToString();
                        Occupation = dtableUser["Occupation"].ToString();

                        PProvince = dtableUser["PProvince"].ToString();
                        PProvinceName = dtableUser["PProvinceName"].ToString();
                        PDistrict = dtableUser["PDistrict"].ToString();
                        PDistrictName = dtableUser["PDistrictName"].ToString();
                        PMunicipalityVDC = dtableUser["PMunicipalityVDC"].ToString();
                        PHouseNo = dtableUser["PHouseNo"].ToString();
                        PWardNo = dtableUser["PWardNo"].ToString();
                        PStreet = dtableUser["PStreet"].ToString();

                        CProvince = dtableUser["CProvince"].ToString();
                        CProvinceName = dtableUser["CProvinceName"].ToString();
                        CDistrict = dtableUser["CDistrict"].ToString();
                        CDistrictName = dtableUser["CDistrictName"].ToString();
                        CMunicipalityVDC = dtableUser["CMunicipalityVDC"].ToString();
                        CHouseNo = dtableUser["CHouseNo"].ToString();
                        CWardNo = dtableUser["CWardNo"].ToString();
                        CStreet = dtableUser["CStreet"].ToString();

                        CitizenshipNo = dtableUser["CitizenshipNo"].ToString();
                        CitizenPlaceOfIssue = dtableUser["CitizenPlaceOfIssue"].ToString();
                        CitizenIssueDate = dtableUser["CitizenIssueDate"].ToString();

                        PassportNo = dtableUser["PassportNo"].ToString();
                        PassportPlaceOfIssue = dtableUser["PassportPlaceOfIssue"].ToString();
                        PassportIssueDate = dtableUser["PassportIssueDate"].ToString();
                        PassportExpiryDate = dtableUser["PassportExpiryDate"].ToString();

                        LicenseNo = dtableUser["LicenseNo"].ToString();
                        LicensePlaceOfIssue = dtableUser["LicensePlaceOfIssue"].ToString();
                        LicenseIssueDate = dtableUser["LicenseIssueDate"].ToString();
                        LicenseExpiryDate = dtableUser["LicenseExpiryDate"].ToString();

                        DateADBS = dtableUser["DateADBS"].ToString();
                        BSDateOfBirth = dtableUser["BSDateOfBirth"].ToString();
                        CustStatus = dtableUser["CustStatus"].ToString();
                        PANNumber = dtableUser["PANNumber"].ToString();

                        DocType = dtableUser["DocType"].ToString();
                        FrontImage = dtableUser["FrontImage"].ToString();
                        BackImage = dtableUser["BackImage"].ToString();
                        PassportImage = dtableUser["PassportImage"].ToString();

                        resultList.Add(new ClientKYC
                        {
                            UserName = UserName,
                            EmailAddress = EmailAddress,
                            FName = FName,
                            MName = MName,
                            LName = LName,
                            Gender = Gender,
                            DateOfBirth = DateOfBirth,
                            CountryCode = CountryCode,
                            Nationality = Nationality,
                            FathersName = FathersName,
                            MothersName = MothersName,
                            MaritalStatus = MaritalStatus,
                            SpouseName = SpouseName,
                            GFathersName = GFathersName,
                            FatherInLaw = FatherInLaw,
                            Occupation = Occupation,
                            PProvince = PProvince,
                            PDistrict = PDistrict,
                            PMunicipalityVDC = PMunicipalityVDC,
                            PHouseNo = PHouseNo,
                            PWardNo = PWardNo,
                            PStreet = PStreet,
                            PDistrictName = PDistrictName,
                            PProvinceName = PProvinceName,
                            CProvince = CProvince,
                            CDistrict = CDistrict,
                            CMunicipalityVDC = CMunicipalityVDC,
                            CHouseNo = CHouseNo,
                            CWardNo = CWardNo,
                            CStreet = CStreet,
                            CProvinceName = CProvinceName,
                            CDistrictName = CDistrictName,
                            CitizenshipNo = CitizenshipNo,
                            CitizenPlaceOfIssue = CitizenPlaceOfIssue,
                            CitizenIssueDate = CitizenIssueDate,
                            PassportNo = PassportNo,
                            PassportPlaceOfIssue = PassportPlaceOfIssue,
                            PassportIssueDate = PassportIssueDate,
                            PassportExpiryDate = PassportExpiryDate,
                            LicenseNo = LicenseNo,
                            LicensePlaceOfIssue = LicensePlaceOfIssue,
                            LicenseIssueDate = LicenseIssueDate,
                            LicenseExpiryDate = LicenseExpiryDate,
                            DateADBS = DateADBS,
                            BSDateOfBirth = BSDateOfBirth,
                            CustStatus = CustStatus,
                            PANNumber = PANNumber,
                            DocType = DocType,
                            FrontImage = FrontImage,
                            BackImage = BackImage,
                            PassportImage = PassportImage
                        });

                    }

                    string sJSONResponse = JsonConvert.SerializeObject(resultList);

                    result = sJSONResponse;

                    statusCode = "200";
                    replyMessage.Response = result;
                    message = replyMessage.Response;
                }
                else
                {
                    // throw ex
                    statusCode = "400";
                    replyMessage.Response = "Customer KYC Not Registred.";
                    result = "Customer KYC Not Registred.";
                    message = replyMessage.Response;
                }
                //end: check customer KYC detail
            }

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var v = new
                    {
                        StatusCode = Convert.ToInt32(statusCode),
                        StatusMessage = result
                    };
                    result = JsonConvert.SerializeObject(v);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
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

        #region Link Bank Account
        public string PassDataToLinkBankController(LinkBankAccount userInfo)
        {
            string results = "false";
            try
            {
                int result = SelfRegisterUtils.LinkBankAccount(userInfo);
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
        public string LinkBankAccount(Stream input)
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
            string AccountNumber = qs["AccountNumber"];
            string DateOfBirth = qs["DateOfBirth"];

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            ReplyMessage replyMessage = new ReplyMessage();

            if ((string.IsNullOrEmpty(Name)) || (string.IsNullOrEmpty(MobileNumber)) || (string.IsNullOrEmpty(AccountNumber) || (AccountNumber.Length != 14) || (string.IsNullOrEmpty(DateOfBirth))))
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

                    LinkBankAccount model = new LinkBankAccount();

                    if ((string.IsNullOrEmpty(Name)) || (string.IsNullOrEmpty(MobileNumber)) || (Name == null) ||
                         (AccountNumber == null))
                    {
                        // throw ex
                        statusCode = "400";
                        message = " Parameters Missing/Invalid ";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                    }
                    else
                    {
                        //UserValidate uservalidate = new UserValidate(UserName, Password, userType.ToLower());
                        //Message<UserValidate> uservalidaterequest = new Message<UserValidate>(uservalidate);
                        // MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(0, uservalidaterequest.message, "0", DateTime.Now);
                        //MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

                        result = CheckLinkBankNumber(MobileNumber);
                        string checkRequest = CheckAlreadyRegisteredNumber(MobileNumber);
                        if (checkRequest == "true")
                        {
                            if (result == "true")
                            {
                                string ClientExtReply = string.Empty;
                                DateTime DOB = DateTime.Parse(DateOfBirth);
                                LinkBankAccount selfuservalidate = new LinkBankAccount(Name, AccountNumber, MobileNumber, DOB);
                                ClientExtReply = PassDataToLinkBankController(selfuservalidate);
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
                            else
                            {
                                replyMessage.Response = "Already Linked with Bank !";
                                result = "Already Linked with Bank !";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                                statusCode = "400";
                                message = replyMessage.Response;
                            }
                        }
                        else
                        {
                            replyMessage.Response = "Already Requested with Linked with Bank !";
                            result = "Already Requested with Linked with Bank !";
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                            statusCode = "400";
                            message = replyMessage.Response;
                        }
                    }
                    string UserName = MobileNumber;
                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            //Sender

                            //string messagereply = "Dear " + Name.Substring(0, Name.IndexOf(" ")) + ", \n";
                            string messagereply = "Dear " + CustCheckUtils.GetName(UserName) + "," + "\n"; ;
                            //messagereply += "Your Link Bank request has been queued for the approval. You'll be notified shortly \n";
                            messagereply += "Your Bank Link request has been queued for approval. Please visit your nearest NIBL branch for approval with ID copy and photo \n";
                            messagereply += "Thank you. MNepal";

                            var client = new WebClient();

                            if ((UserName.Substring(0, 3) == "980") || (UserName.Substring(0, 3) == "981")) //FOR NCELL
                            {
                                //FOR NCELL
                                //var content = client.DownloadString(
                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                //    + "977" + UserName + "&Text=" + messagereply + "");
                                var content = client.DownloadString(
                                    SMSNCELL
                                    + "977" + UserName + "&Text=" + messagereply + "");
                            }
                            else if ((UserName.Substring(0, 3) == "985") || (UserName.Substring(0, 3) == "984")
                                        || (UserName.Substring(0, 3) == "986"))
                            {
                                //FOR NTC
                                //var content = client.DownloadString(
                                //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                //    + "977" + UserName + "&Text=" + messagereply + "");
                                var content = client.DownloadString(
                                    SMSNTC
                                    + "977" + UserName + "&Text=" + messagereply + "");
                            }

                            var v = new
                            {
                                StatusCode = Convert.ToInt32(200),
                                StatusMessage = result
                            };
                            result = JsonConvert.SerializeObject(v);

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        custsmsInfo = new CustActivityModel()
                        {
                            UserName = MobileNumber,
                            RequestMerchant = "Link Bank Account Request",
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
                            RequestMerchant = "Link Bank Account Request",
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
                        RequestMerchant = "Link Bank Account Request",
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
        public string CheckLinkBankNumber(string MobileNumber)
        {
            string result = "false";
            string HasBankKYC = "";
            string mobile = "";
            DataTable CheckLinkBankAccount = CustCheckUtils.GetCustInfo(MobileNumber);
            if (CheckLinkBankAccount.Rows.Count == 1)
            {
                foreach (DataRow dtableUser in CheckLinkBankAccount.Rows)
                {
                    mobile = dtableUser["UserName"].ToString();
                    HasBankKYC = dtableUser["HasBankKYC"].ToString();
                }
                if (HasBankKYC == "F")
                {
                    result = "true";
                }
            }

            return result;
        }
        public string CheckAlreadyRegisteredNumber(string MobileNumber)
        {
            string result = "false";
            string HasBankKYC = "";
            string mobile = "";
            result = CustCheckUtils.CheckAlreadyRequestedAccount(MobileNumber);
            return result;
        }
        #endregion


        #region SaveImage March26
        [OperationContract]
        [WebInvoke(Method = "POST",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string SaveImageAPI(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string result = "";
            string statusCode = string.Empty;
            string message = string.Empty;
            string UserName = qs["mobile"];
            string frontImages = qs["frontImage"];
            string backImages = qs["backImage"];
            string passportImages = qs["ppImage"];
            string frontImagesName = qs["frontImageName"];
            string backImagesName = qs["backImageName"];
            string passportImagesName = qs["ppImageName"];
            string DocumentType = qs["DocumentType"];
            string PassportImage = "";
            string FrontImage = "";
            string BackImage = "";
            ReplyMessage replyMessage = new ReplyMessage();

            UserValidate model = new UserValidate();

            try
            {
                if (!string.IsNullOrEmpty(frontImages))
                {
                    //convert byte array to image
                    Image imgFront = Base64ToImage(frontImages);//(frontString);

                    // Convert Base64 String to byte[]
                    byte[] imageBytesFront = Convert.FromBase64String(frontImages);

                    string PhotoPathFront = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                    string PhotoNameFront = "Knitting Balls";

                    model.FrontPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesFront, PhotoPathFront, PhotoNameFront);//filename "EcreditCardpayment.jpg"

                    //var FrontPic = SaveAndReturnFileName(model.FrontPic, UserName, "Citizen", "front");
                }
                if (!string.IsNullOrEmpty(backImages))
                {
                    //convert byte array to image
                    Image imgBack = Base64ToImage(backImages);//(backString);

                    // Convert Base64 String to byte[]
                    byte[] imageBytesBack = Convert.FromBase64String(backImages);

                    string PhotoPathBack = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                    string PhotoNameBack = backImagesName;

                    model.BackPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesBack, PhotoPathBack, PhotoNameBack);//filename "EcreditCardpayment.jpg"

                }
                if (!string.IsNullOrEmpty(passportImages))
                {
                    //convert byte array to image
                    Image imgPP = Base64ToImage(passportImages);

                    // Convert Base64 String to byte[]
                    byte[] imageBytesPP = Convert.FromBase64String(passportImages);

                    string PhotoPathPP = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                    string PhotoNamePP = passportImagesName;

                    model.PassportPhoto = (HttpPostedFileBase)new MemoryPostedFile(imageBytesPP, PhotoPathPP, PhotoNamePP);//filename "EcreditCardpayment.jpg"

                }

            }
            catch (Exception e)
            {
                statusCode = "400";
                message = "Image Missing/Invalid !" + e;
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }
            if (statusCode == "400")
            {
                // throw ex
                statusCode = "400";
                message = "Image Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }
            else if (statusCode != "400")
            {
                //if ((!model.PassportPhoto.Equals(""))
                //    || (!model.FrontPic.Equals(""))
                //        || (!model.BackPic.Equals("")))
                //{
                if ((model.PassportPhoto != null) && (model?.PassportPhoto?.ContentLength != 0))
                {
                    var pp = SaveAndReturnFileName(model.PassportPhoto, UserName, "Passport", "photo");
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
                if ((model.FrontPic != null) && (model?.FrontPic?.ContentLength != 0))
                {
                    var Front = SaveAndReturnFileName(model.FrontPic, UserName, DocumentType, "front");
                    var last4Front = Front.Substring(Front.Length - 4, 4);

                    if (last4Front.Equals(".jpg"))
                    {
                        FrontImage = string.Format(Front);
                    }
                    else
                    {
                        FrontImage = string.Format(Front + ".jpg");
                    }
                }
                if ((model.BackPic != null) && (model?.BackPic?.ContentLength != 0))
                {
                    var Back = SaveAndReturnFileName(model.BackPic, UserName, DocumentType, "back");
                    var last4Back = Back.Substring(Back.Length - 4, 4);

                    if (last4Back.Equals(".jpg"))
                    {
                        BackImage = string.Format(Back);
                    }
                    else
                    {
                        BackImage = string.Format(Back + ".jpg");
                        // BackImage = string.Format("~/Content/Upload/{0}", back + ".jpg");
                    }
                }
                string PP = PassportImage, front = FrontImage, back = BackImage;
                var v = new
                {
                    StatusCode = Convert.ToInt32(200),
                    StatusMessage = result,
                    PP,
                    front,
                    back
                };
                result = JsonConvert.SerializeObject(v);
                //}
                //else
                //{

                //    // throw ex
                //    statusCode = "400";
                //    message = "Image Missing/Invalid";
                //    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);

                //}


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

            return result;
        }
        #endregion


        #region SaveImageMerchantAPI
        [OperationContract]
        [WebInvoke(Method = "POST",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string SaveImageMerchantAPI(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string result = "";
            string statusCode = string.Empty;
            string message = string.Empty;
            string UserName = qs["mobile"];
            string frontImages = qs["frontImage"];
            string backImages = qs["backImage"];
            string passportImages = qs["ppImage"];

            string regCertiImages = qs["regCertiImage"];
            string taxClearFrontImages = qs["taxClearFrontImage"];
            string taxClearBackImages = qs["taxClearBackImage"];

            string frontImagesName = qs["frontImageName"];
            string backImagesName = qs["backImageName"];
            string passportImagesName = qs["ppImageName"];

            string regCertiImagesName = qs["regCertiImageName"];
            string taxClearFrontImagesName = qs["taxClearFrontImageName"];
            string taxClearBackImagesName = qs["taxClearBackImageName"];
            string DocumentType = qs["DocumentType"];
            string PassportImage = "";
            string FrontImage = "";
            string BackImage = "";

            string RegCertiImage = "";
            string TaxClearFrontImage = "";
            string TaxClearBackImage = "";
            ReplyMessage replyMessage = new ReplyMessage();

            UserValidate model = new UserValidate();

            try
            {
                //FOR FRONT IMAGE
                if (!string.IsNullOrEmpty(frontImages))
                {
                    //convert byte array to image
                    Image imgFront = Base64ToImage(frontImages);//(frontString);

                    // Convert Base64 String to byte[]
                    byte[] imageBytesFront = Convert.FromBase64String(frontImages);

                    string PhotoPathFront = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                    string PhotoNameFront = "Knitting Balls";

                    model.FrontPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesFront, PhotoPathFront, PhotoNameFront);//filename "EcreditCardpayment.jpg"

                    //var FrontPic = SaveAndReturnFileName(model.FrontPic, UserName, "Citizen", "front");
                }

                //FOR BACK IMAGE
                if (!string.IsNullOrEmpty(backImages))
                {
                    //convert byte array to image
                    Image imgBack = Base64ToImage(backImages);//(backString);

                    // Convert Base64 String to byte[]
                    byte[] imageBytesBack = Convert.FromBase64String(backImages);

                    string PhotoPathBack = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                    string PhotoNameBack = backImagesName;

                    model.BackPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesBack, PhotoPathBack, PhotoNameBack);//filename "EcreditCardpayment.jpg"

                }

                //FOR PP IMAGE
                if (!string.IsNullOrEmpty(passportImages))
                {
                    //convert byte array to image
                    Image imgPP = Base64ToImage(passportImages);

                    // Convert Base64 String to byte[]
                    byte[] imageBytesPP = Convert.FromBase64String(passportImages);

                    string PhotoPathPP = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                    string PhotoNamePP = passportImagesName;

                    model.PassportPhoto = (HttpPostedFileBase)new MemoryPostedFile(imageBytesPP, PhotoPathPP, PhotoNamePP);//filename "EcreditCardpayment.jpg"

                }

                //FOR REGISTRATION CERTIFICATE IMAGE
                if (!string.IsNullOrEmpty(regCertiImages))
                {
                    //convert byte array to image
                    Image imgRegCerti = Base64ToImage(regCertiImages);

                    // Convert Base64 String to byte[]
                    byte[] imageBytesRegCerti = Convert.FromBase64String(regCertiImages);

                    string PhotoPathRegCerti = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                    string PhotoNameRegCerti = regCertiImages;

                    model.RegCertiPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesRegCerti, PhotoPathRegCerti, PhotoNameRegCerti);//filename "EcreditCardpayment.jpg"

                }

                //FOR TAX CLEARANCE FRONT IMAGE
                if (!string.IsNullOrEmpty(taxClearFrontImages))
                {
                    //convert byte array to image
                    Image imgTaxClearFront = Base64ToImage(taxClearFrontImages);

                    // Convert Base64 String to byte[]
                    byte[] imageBytesTaxClearFront = Convert.FromBase64String(taxClearFrontImages);

                    string PhotoPathTaxClearFront = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                    string PhotoNameTaxClearFront = taxClearFrontImages;

                    model.TaxClearFrontPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesTaxClearFront, PhotoPathTaxClearFront, PhotoNameTaxClearFront);//filename "EcreditCardpayment.jpg"

                }

                //FOR TAX CLEARANCE BACK IMAGE
                if (!string.IsNullOrEmpty(taxClearBackImages))
                {
                    //convert byte array to image
                    Image imgTaxClearBack = Base64ToImage(taxClearBackImages);

                    // Convert Base64 String to byte[]
                    byte[] imageBytesTaxClearBack = Convert.FromBase64String(taxClearBackImages);

                    string PhotoPathTaxClearBack = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                    string PhotoNameTaxClearBack = taxClearBackImages;

                    model.TaxClearBackPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesTaxClearBack, PhotoPathTaxClearBack, PhotoNameTaxClearBack);//filename "EcreditCardpayment.jpg"

                }

            }
            catch (Exception e)
            {
                statusCode = "400";
                message = "Image Missing/Invalid !" + e;
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }
            if (statusCode == "400")
            {
                // throw ex
                statusCode = "400";
                message = "Image Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }
            else if (statusCode != "400")
            {
                //if ((!model.PassportPhoto.Equals(""))
                //    || (!model.FrontPic.Equals(""))
                //        || (!model.BackPic.Equals("")))
                //{
                if ((model.PassportPhoto != null) && (model?.PassportPhoto?.ContentLength != 0))
                {
                    var pp = SaveAndReturnFileName(model.PassportPhoto, UserName, "Passport", "photo");
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

                if ((model.FrontPic != null) && (model?.FrontPic?.ContentLength != 0))
                {
                    var Front = SaveAndReturnFileName(model.FrontPic, UserName, DocumentType, "front");
                    var last4Front = Front.Substring(Front.Length - 4, 4);

                    if (last4Front.Equals(".jpg"))
                    {
                        FrontImage = string.Format(Front);
                    }
                    else
                    {
                        FrontImage = string.Format(Front + ".jpg");
                    }
                }

                if ((model.BackPic != null) && (model?.BackPic?.ContentLength != 0))
                {
                    var Back = SaveAndReturnFileName(model.BackPic, UserName, DocumentType, "back");
                    var last4Back = Back.Substring(Back.Length - 4, 4);

                    if (last4Back.Equals(".jpg"))
                    {
                        BackImage = string.Format(Back);
                    }
                    else
                    {
                        BackImage = string.Format(Back + ".jpg");
                        // BackImage = string.Format("~/Content/Upload/{0}", back + ".jpg");
                    }
                }

                if ((model.RegCertiPic != null) && (model?.RegCertiPic?.ContentLength != 0))
                {
                    var RegCerti = SaveAndReturnFileName(model.RegCertiPic, UserName, DocumentType, "regcerti");
                    var last4RegCerti = RegCerti.Substring(RegCerti.Length - 4, 4);

                    if (last4RegCerti.Equals(".jpg"))
                    {
                        RegCertiImage = string.Format(RegCerti);
                    }
                    else
                    {
                        RegCertiImage = string.Format(RegCerti + ".jpg");
                        // BackImage = string.Format("~/Content/Upload/{0}", back + ".jpg");
                    }
                }

                if ((model.TaxClearFrontPic != null) && (model?.TaxClearFrontPic?.ContentLength != 0))
                {
                    var TaxClearFront = SaveAndReturnFileName(model.TaxClearFrontPic, UserName, DocumentType, "taxClearFront");
                    var last4TaxClearFront = TaxClearFront.Substring(TaxClearFront.Length - 4, 4);

                    if (last4TaxClearFront.Equals(".jpg"))
                    {
                        TaxClearFrontImage = string.Format(TaxClearFront);
                    }
                    else
                    {
                        TaxClearFrontImage = string.Format(TaxClearFront + ".jpg");
                        // BackImage = string.Format("~/Content/Upload/{0}", back + ".jpg");
                    }
                }

                if ((model.TaxClearBackPic != null) && (model?.TaxClearBackPic?.ContentLength != 0))
                {
                    var TaxClearBack = SaveAndReturnFileName(model.TaxClearBackPic, UserName, DocumentType, "taxClearBack");
                    var last4TaxClearBack = TaxClearBack.Substring(TaxClearBack.Length - 4, 4);

                    if (last4TaxClearBack.Equals(".jpg"))
                    {
                        TaxClearBackImage = string.Format(TaxClearBack);
                    }
                    else
                    {
                        TaxClearBackImage = string.Format(TaxClearBack + ".jpg");
                        // BackImage = string.Format("~/Content/Upload/{0}", back + ".jpg");
                    }
                }

                string PP = PassportImage,
                    front = FrontImage, back = BackImage,
                    regcerti = RegCertiImage,
                    taxClearFront = TaxClearFrontImage,
                    taxClearBack = TaxClearBackImage;

                var v = new
                {
                    StatusCode = Convert.ToInt32(200),
                    StatusMessage = result,
                    PP,
                    front,
                    back,
                    regcerti,
                    taxClearFront,
                    taxClearBack
                };
                result = JsonConvert.SerializeObject(v);
                //}
                //else
                //{

                //    // throw ex
                //    statusCode = "400";
                //    message = "Image Missing/Invalid";
                //    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);

                //}


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

            return result;
        }
        #endregion


        #region Registration BY AGENT

        [OperationContract]
        [WebInvoke(Method = "POST",
               ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string RegistrationByAgent(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();

            //SMS
            string SMSNTC = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalNTCSMSServerUrl"];
            string SMSNCELL = System.Web.Configuration.WebConfigurationManager.AppSettings["MNepalSMSServerUrl"];

            TraceIdGenerator traceid = new TraceIdGenerator();
            string tid = traceid.GenerateUniqueTraceID();

            NameValueCollection qs = HttpUtility.ParseQueryString(s);
            string PProvinceID = string.Empty;
            string CProvinceID = string.Empty;
            string PDistrictID = string.Empty;
            string CDistrictID = string.Empty;

            string result = "";
            string statusCode = string.Empty;
            string message = string.Empty;

            string UserName = qs["UserName"];
            string FName = qs["fName"];
            string MName = qs["mName"];
            string LName = qs["lName"];
            string Gender = qs["gender"];
            string DateOfBirth = qs["dateOfBirth"];
            string CountryCode = qs["countryCode"];
            string Nationality = qs["nationality"];
            string FathersName = qs["fathersName"];
            string MothersName = qs["mothersName"];
            string MaritalStatus = qs["maritalStatus"];
            string SpouseName = qs["spouseName"];
            string GrandFathersName = qs["gFathersName"];
            string FatherInLaw = qs["FatherInLaw"];
            string Occupation = qs["occupation"];
            string EmailAddress = qs["EmailAddress"];

            string PProvince = qs["PZone"];
            string PDistrict = qs["PDistrict"];
            string PMunicipalityVDC = qs["PMunicipalityVDC"];
            string PHouseNo = qs["PHouseNo"];
            string PWardNo = qs["PWardNo"];
            string PStreet = qs["PStreet"];

            string CProvince = qs["CZone"];
            string CDistrict = qs["CDistrict"];
            string CMunicipalityVDC = qs["CMunicipalityVDC"];
            string CHouseNo = qs["CHouseNo"];
            string CWardNo = qs["CWardNo"];
            string CStreet = qs["CStreet"];

            string CitizenshipNo = qs["CitizenshipNo"];
            string CitizenPlaceOfIssue = qs["CitizenPlaceOfIssue"];
            string CitizenIssueDate = qs["CitizenIssueDate"];

            string PassportNo = qs["PassportNo"];
            string PassportPlaceOfIssue = qs["PassportPlaceOfIssue"];
            string PassportIssueDate = qs["PassportIssueDate"];
            string PassportExpireDate = qs["PassportExpiryDate"];

            string LicenseNo = qs["LicenseNo"];
            string LicensePlaceOfIssue = qs["LicensePlaceOfIssue"];
            string LicenseIssueDate = qs["LicenseIssueDate"];
            string LicenseExpiryDate = qs["LicenseExpiryDate"];

            string PANNumber = qs["PANNumber"];
            string BranchCode = qs["BranchCode"];
            string Source = qs["src"];
            string OTPCode = qs["OTPCode"];
            string retRef = tid;

            //DocType Image
            string DocumentType = qs["DocType"];

            string frontImages = qs["frontImage"];
            string backImages = qs["backImage"];
            string passportImages = qs["ppImage"];

            string frontImagesName = qs["frontImageName"];
            string backImagesName = qs["backImageName"];
            string passportImagesName = qs["ppImageName"];

            ReplyMessage replyMessage = new ReplyMessage();
            if ((string.IsNullOrEmpty(UserName)))
            {
                // throw ex
                statusCode = "400";
                message = "Parameters Missing/Invalid";
                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
            }
            else
            {
                if (IsValidUserName(UserName))
                {
                    //statusCode = "400";
                    //message = "Number Already Registered!!";
                    //replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);

                    statusCode = "400";
                    message = "A OTP code will be sent to your phone if the account exists in our system.";
                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                }
                else
                {
                    //  (string.IsNullOrEmpty(PStreet)) || (string.IsNullOrEmpty(CStreet)) ||
                    if ((string.IsNullOrEmpty(UserName)) || (string.IsNullOrEmpty(FName)) || (string.IsNullOrEmpty(LName)) ||
                    (string.IsNullOrEmpty(Gender)) || (string.IsNullOrEmpty(DateOfBirth)) || (string.IsNullOrEmpty(CountryCode)) || (string.IsNullOrEmpty(Nationality)) ||
                    (string.IsNullOrEmpty(FathersName)) 
                    //|| (string.IsNullOrEmpty(MothersName)) 
                    || (string.IsNullOrEmpty(MaritalStatus)) ||
                    (string.IsNullOrEmpty(GrandFathersName)) || (string.IsNullOrEmpty(Occupation)) || (string.IsNullOrEmpty(EmailAddress)) ||
                    (string.IsNullOrEmpty(PProvince)) || (string.IsNullOrEmpty(PDistrict)) || (string.IsNullOrEmpty(PMunicipalityVDC)) ||
                    (string.IsNullOrEmpty(PWardNo)) ||
                    (string.IsNullOrEmpty(CProvince)) || (string.IsNullOrEmpty(CDistrict)) || (string.IsNullOrEmpty(CMunicipalityVDC)) ||
                    (string.IsNullOrEmpty(CWardNo)) ||
                    (string.IsNullOrEmpty(BranchCode)) || (string.IsNullOrEmpty(Source)) ||
                    (string.IsNullOrEmpty(frontImages)) || (string.IsNullOrEmpty(backImages)) || (string.IsNullOrEmpty(passportImages))) // (string.IsNullOrEmpty(PHouseNo)) || (string.IsNullOrEmpty(CHouseNo)) ||
                    {
                        // throw ex
                        statusCode = "400";
                        message = "Parameters Missing/Invalid";
                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                    }
                    else
                    {
                        if (MaritalStatus.Equals("Married"))
                        {
                            if ((string.IsNullOrEmpty(SpouseName)) || (string.IsNullOrEmpty(FatherInLaw)))
                            {
                                // throw ex
                                statusCode = "400";
                                message = "Marital Status Parameters Missing/Invalid";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                            }
                        }

                        if (!statusCode.Equals("400"))
                        {

                            if ((!string.IsNullOrEmpty(PProvince)) && (!string.IsNullOrEmpty(PDistrict))
                                && (!string.IsNullOrEmpty(CProvince)) && (!string.IsNullOrEmpty(CDistrict)))
                            {
                                PProvinceID = GetProvinceID(PProvince);
                                CProvinceID = GetProvinceID(CProvince);
                                PDistrictID = GetDistrictID(PDistrict);
                                CDistrictID = GetDistrictID(CDistrict);
                            }


                            if (!string.IsNullOrEmpty(DateOfBirth))
                            {
                                DateTime.ParseExact(DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                        .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                DateOfBirth = "";
                            }

                            if (!string.IsNullOrEmpty(CitizenIssueDate))
                            {
                                DateTime.ParseExact(CitizenIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                        .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                CitizenIssueDate = "01/01/1900";
                            }

                            if (!string.IsNullOrEmpty(PassportIssueDate))
                            {
                                DateTime.ParseExact(PassportIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                        .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                PassportIssueDate = "01/01/1900";
                            }

                            if (!string.IsNullOrEmpty(PassportExpireDate))
                            {
                                DateTime.ParseExact(PassportExpireDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                        .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                PassportExpireDate = "01/01/1900";
                            }

                            if (!string.IsNullOrEmpty(LicenseIssueDate))
                            {
                                DateTime.ParseExact(LicenseIssueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                        .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                LicenseIssueDate = "01/01/1900";
                            }
                            if (!string.IsNullOrEmpty(LicenseExpiryDate))
                            {
                                DateTime.ParseExact(LicenseExpiryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                                        .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                LicenseExpiryDate = "01/01/1900";
                            }

                            string customerNo = string.Empty;

                            UserValidate model = new UserValidate();

                            if ((PProvinceID == "") || (CProvinceID == "") ||
                                (PDistrictID == "") || (CDistrictID == ""))
                            {
                                // throw ex
                                statusCode = "400";
                                message = "Province/District Parameters Missing/Invalid";
                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                            }
                            else
                            {
                                if (((!string.IsNullOrEmpty(CitizenshipNo)) && (!string.IsNullOrEmpty(CitizenPlaceOfIssue)) && (!string.IsNullOrEmpty(CitizenIssueDate))) ||
                                                     ((!string.IsNullOrEmpty(PassportNo)) && (!string.IsNullOrEmpty(PassportPlaceOfIssue)) && (!string.IsNullOrEmpty(PassportIssueDate))
                                                        && (!string.IsNullOrEmpty(PassportExpireDate))) ||
                                                     ((!string.IsNullOrEmpty(LicenseNo)) && (!string.IsNullOrEmpty(LicensePlaceOfIssue)) && (!string.IsNullOrEmpty(LicenseIssueDate))
                                                        && (!string.IsNullOrEmpty(LicenseExpiryDate))))
                                {
                                    if (CitizenshipNo == null)
                                    {
                                        CitizenshipNo = "";
                                    }
                                    if (CitizenPlaceOfIssue == null)
                                    {
                                        CitizenPlaceOfIssue = "";
                                    }

                                    if (PassportNo == null)
                                    {
                                        PassportNo = "";
                                    }
                                    if (PassportPlaceOfIssue == null)
                                    {
                                        PassportPlaceOfIssue = "";
                                    }

                                    if (LicenseNo == null)
                                    {
                                        LicenseNo = "";
                                    }
                                    if (LicensePlaceOfIssue == null)
                                    {
                                        LicensePlaceOfIssue = "";
                                    }

                                    //string frontString = Base64String(frontImages);
                                    //string backString = Base64String(backImages);
                                    //string passportString = Base64String(passportImages);
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(frontImages))
                                        {
                                            //convert byte array to image
                                            Image imgFront = Base64ToImage(frontImages);//(frontString);

                                            // Convert Base64 String to byte[]
                                            byte[] imageBytesFront = Convert.FromBase64String(frontImages);

                                            string PhotoPathFront = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                                            string PhotoNameFront = "Knitting Balls";

                                            model.FrontPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesFront, PhotoPathFront, PhotoNameFront);//filename "EcreditCardpayment.jpg"

                                            //var FrontPic = SaveAndReturnFileName(model.FrontPic, UserName, "Citizen", "front");
                                        }
                                        if (!string.IsNullOrEmpty(backImages))
                                        {
                                            //convert byte array to image
                                            Image imgBack = Base64ToImage(backImages);//(backString);

                                            // Convert Base64 String to byte[]
                                            byte[] imageBytesBack = Convert.FromBase64String(backImages);

                                            string PhotoPathBack = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                                            string PhotoNameBack = backImagesName;

                                            model.BackPic = (HttpPostedFileBase)new MemoryPostedFile(imageBytesBack, PhotoPathBack, PhotoNameBack);//filename "EcreditCardpayment.jpg"

                                        }
                                        if (!string.IsNullOrEmpty(passportImages))
                                        {
                                            //convert byte array to image
                                            Image imgPP = Base64ToImage(passportImages);

                                            // Convert Base64 String to byte[]
                                            byte[] imageBytesPP = Convert.FromBase64String(passportImages);

                                            string PhotoPathPP = "/storage/emulated/0/Samsung/Image/2. Knitting Balls.jpg";
                                            string PhotoNamePP = passportImagesName;

                                            model.PassportPhoto = (HttpPostedFileBase)new MemoryPostedFile(imageBytesPP, PhotoPathPP, PhotoNamePP);//filename "EcreditCardpayment.jpg"

                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        statusCode = "400";
                                        message = "Image Missing/Invalid !" + e;
                                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                                    }


                                    if (statusCode == "400")
                                    {
                                        // throw ex
                                        statusCode = "400";
                                        message = "Image Missing/Invalid";
                                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                                    }
                                    else if (statusCode != "400")
                                    {
                                        DataTable dtableUserCheck = CustCheckUtils.GetCustUserInfo(UserName);

                                        if (dtableUserCheck.Rows.Count > 0)
                                        {
                                            statusCode = "400";
                                           // message = "Customer already Registered !";

                                            message = "A OTP code will be sent to your phone if the account exists in our system.";
                                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                                        }
                                        else if (dtableUserCheck.Rows.Count == 0)
                                        {
                                            if ((!model.PassportPhoto.Equals(""))
                                                || (!model.FrontPic.Equals(""))
                                                    || (!model.BackPic.Equals("")))
                                            {
                                                var PP = SaveAndReturnFileName(model.PassportPhoto, UserName, "Passport", "photo");
                                                var front = SaveAndReturnFileName(model.FrontPic, UserName, DocumentType, "front");
                                                var back = SaveAndReturnFileName(model.BackPic, UserName, DocumentType, "back");

                                                var last4PP = PP.Substring(PP.Length - 4, 4);
                                                var last4Front = front.Substring(front.Length - 4, 4);
                                                var last4Back = back.Substring(back.Length - 4, 4);

                                                string PassportImage;
                                                string FrontImage;
                                                string BackImage;

                                                if (last4PP.Equals(".jpg"))
                                                {
                                                    PassportImage = string.Format(PP);
                                                }
                                                else
                                                {
                                                    PassportImage = string.Format(PP + ".jpg");
                                                }

                                                if (last4Front.Equals(".jpg"))
                                                {
                                                    FrontImage = string.Format(front);
                                                }
                                                else
                                                {
                                                    FrontImage = string.Format(front + ".jpg");
                                                }

                                                if (last4Back.Equals(".jpg"))
                                                {
                                                    BackImage = string.Format(back);
                                                }
                                                else
                                                {
                                                    BackImage = string.Format(back + ".jpg");
                                                    // BackImage = string.Format("~/Content/Upload/{0}", back + ".jpg");
                                                }
                                                string Pin = "";
                                                string userType = "user";
                                                string Password = SelfRegisterUtils.GeneratePassword();

                                                if ((string.IsNullOrEmpty(UserName)) || (string.IsNullOrEmpty(FName)) || (LName == null) ||
                                                    (Gender == null) || (DateOfBirth == null) || (CountryCode == null) || (Nationality == null) ||
                                                    (FathersName == null) 
                                                    //|| (MothersName == null) 
                                                    || (MaritalStatus == null) || (GrandFathersName == null) || (Occupation == null) ||
                                                    (PProvince == null) || (PDistrict == null) || (PProvinceID == null) || (PDistrictID == null) ||
                                                    (PMunicipalityVDC == null) || (PHouseNo == null) || (PWardNo == null) || (PStreet == null) ||
                                                    (CProvinceID == null) || (CDistrictID == null) || (CMunicipalityVDC == null) || (CHouseNo == null) ||
                                                    (CWardNo == null) || (CStreet == null) || (BranchCode == null) || (Source == null) ||
                                                    (string.IsNullOrEmpty(FrontImage)) || (string.IsNullOrEmpty(BackImage)) || (string.IsNullOrEmpty(PassportImage)))
                                                {
                                                    // throw ex
                                                    statusCode = "400";
                                                    message = " Parameters Missing/Invalid ";
                                                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                                                }
                                                else
                                                {
                                                    UserValidate uservalidate = new UserValidate(UserName, Password, userType.ToLower());
                                                    Message<UserValidate> uservalidaterequest = new Message<UserValidate>(uservalidate);
                                                    MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(0, uservalidaterequest.message, "0", DateTime.Now);
                                                    MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

                                                    result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
                                                    if (result == "Success")
                                                    {
                                                        string ClientExtReply = "";
                                                        if ((FrontImage.Length > 0) && (BackImage.Length > 0) && (PassportImage.Length > 0))
                                                        {
                                                            Pin = SelfRegisterUtils.GeneratePin();
                                                            UserValidate selfuservalidate = new UserValidate(UserName, HashAlgo.Hash(Password), Pin, userType.ToLower(),
                                                            OTPCode, Source, FName, MName, LName, Gender, DateOfBirth, CountryCode, Nationality,
                                                            FathersName, MothersName, GrandFathersName, MaritalStatus, SpouseName, FatherInLaw,
                                                            Occupation,
                                                            PProvince, PDistrict, PDistrictID, PMunicipalityVDC, PHouseNo, PWardNo, PStreet, PProvinceID,
                                                            CProvince, CDistrict, CDistrictID, CMunicipalityVDC, CHouseNo, CWardNo, CStreet, CProvinceID,
                                                            CitizenshipNo, CitizenPlaceOfIssue, CitizenIssueDate,
                                                            PassportNo, PassportPlaceOfIssue, PassportIssueDate, PassportExpireDate,
                                                            LicenseNo, LicensePlaceOfIssue, LicenseIssueDate, LicenseExpiryDate,
                                                            PANNumber, DocumentType, "F", EmailAddress, BranchCode, FrontImage, BackImage, PassportImage);
                                                            ClientExtReply = PassDataToMNClientExtControllerAgent(selfuservalidate, retRef);
                                                            if (ClientExtReply == "true")
                                                            {
                                                                replyMessage.Response = "Successfully Registered ! ";
                                                                result = "Successfully Registered ! ";
                                                                replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);

                                                                //insert into MNResponse
                                                                UserValidate selfuservalidateResponse = new UserValidate(UserName, retRef, "200", result);
                                                                int ret = SelfRegisterUtils.InsertResponseQuickSelfReg(selfuservalidateResponse);
                                                            }
                                                            else
                                                            {
                                                                replyMessage.Response = "User Not Registered !";
                                                                result = "User Not Registered ";
                                                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            replyMessage.Response = "Photo Not Uploded !";
                                                            result = "Photo Not Uploded !";
                                                            replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                                                        }
                                                    }
                                                    else
                                                    {

                                                        replyMessage.Response = "request denied";
                                                        result = "Request Denied";
                                                        replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                                                        statusCode = "400";
                                                        message = replyMessage.Response;
                                                    }
                                                }


                                                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                                                if (response.StatusCode == HttpStatusCode.OK)
                                                {
                                                    //Send Pin and Password to user
                                                    try
                                                    {
                                                        //Sender
                                                        string messagereply = "Dear " + FName + ", \n";
                                                        messagereply += "Your new T- Pin is " + Pin +
                                                        " and Password is " + Password + "." + "\n";
                                                        messagereply += "Thank you. MNepal";

                                                        var client = new WebClient();

                                                        if ((UserName.Substring(0, 3) == "980") || (UserName.Substring(0, 3) == "981")) //FOR NCELL
                                                        {
                                                            //FOR NCELL
                                                            //var content = client.DownloadString(
                                                            //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                            //    + "977" + UserName + "&Text=" + messagereply + "");
                                                            var content = client.DownloadString(
                                                                SMSNCELL
                                                                + "977" + UserName + "&Text=" + messagereply + "");
                                                        }
                                                        else if ((UserName.Substring(0, 3) == "985") || (UserName.Substring(0, 3) == "984")
                                                                    || (UserName.Substring(0, 3) == "986"))
                                                        {
                                                            //FOR NTC
                                                            //var content = client.DownloadString(
                                                            //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                            //    + "977" + UserName + "&Text=" + messagereply + "");
                                                            var content = client.DownloadString(
                                                               SMSNTC
                                                               + "977" + UserName + "&Text=" + messagereply + "");
                                                        }

                                                        var v = new
                                                        {
                                                            StatusCode = Convert.ToInt32(200),
                                                            StatusMessage = result
                                                        };
                                                        result = JsonConvert.SerializeObject(v);

                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        throw ex;
                                                    }
                                                    try
                                                    {
                                                        //Sender
                                                        string messagereply = "Dear " + FName + ", \n";
                                                        messagereply += " Your KYC request has been queued for the approval. You'll be notified shortly"
                                                                    + "." + "\n";
                                                        messagereply += "Thank you. MNepal";

                                                        var client = new WebClient();

                                                        if ((UserName.Substring(0, 3) == "980") || (UserName.Substring(0, 3) == "981")) //FOR NCELL
                                                        {
                                                            //FOR NCELL
                                                            //var content = client.DownloadString(
                                                            //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                            //    + "977" + UserName + "&Text=" + messagereply + "");
                                                            var content = client.DownloadString(
                                                                SMSNCELL + "977" + UserName + "&Text=" + messagereply + "");
                                                        }
                                                        else if ((UserName.Substring(0, 3) == "985") || (UserName.Substring(0, 3) == "984")
                                                                    || (UserName.Substring(0, 3) == "986"))
                                                        {
                                                            //FOR NTC
                                                            //var content = client.DownloadString(
                                                            //    "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
                                                            //    + "977" + UserName + "&Text=" + messagereply + "");
                                                            var content = client.DownloadString(
                                                                SMSNTC + "977" + UserName + "&Text=" + messagereply + "");
                                                        }

                                                        var v = new
                                                        {
                                                            StatusCode = Convert.ToInt32(200),
                                                            StatusMessage = result
                                                        };
                                                        result = JsonConvert.SerializeObject(v);

                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        throw ex;
                                                    }
                                                }
                                                else if (response.StatusCode == HttpStatusCode.BadRequest)
                                                {
                                                    var v = new
                                                    {
                                                        StatusCode = Convert.ToInt32(400),
                                                        StatusMessage = result
                                                    };
                                                    result = JsonConvert.SerializeObject(v);
                                                }
                                                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                                                {
                                                    var v = new
                                                    {
                                                        StatusCode = Convert.ToInt32(401),
                                                        StatusMessage = result
                                                    };
                                                    result = JsonConvert.SerializeObject(v);
                                                }

                                            }
                                            else
                                            {

                                                // throw ex
                                                statusCode = "400";
                                                message = "Image Missing/Invalid";
                                                replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);

                                            }
                                        }

                                    }

                                }
                                else
                                {
                                    statusCode = "400";
                                    message = "Missing/Invalid Required Data";
                                    result = "Missing/Invalid Required Data";
                                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                                }
                            }
                        }
                        else
                        {
                            statusCode = "400";
                            message = "Marital Status Missing/Invalid Required Data";
                            result = "Marital Status Missing/Invalid Required Data";
                            replyMessage.ResponseStatus(HttpStatusCode.BadRequest, message);
                        }

                    }
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

            return result;
        }

        public string PassDataToMNClientExtControllerAgent(UserValidate userInfo, string retRef)
        {
            string reply = "false";
            try
            {
                int results = SelfRegisterUtils.RegistrationByAgent(userInfo, retRef);
                if (results > 0)
                {
                    reply = "true";
                }
                else
                {
                    reply = "false";
                }

            }
            catch (Exception ex)
            {
                reply = "false";
            }


            return reply;
        }
        #endregion
    }

}


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
