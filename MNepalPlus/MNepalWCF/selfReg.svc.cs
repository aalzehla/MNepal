using MNepalProject.Controllers;
using MNepalProject.Helper;
using MNepalProject.Models;
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

namespace WCF.MNepal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
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

                        statusCode = "400";
                        result = "The number is registered !!";
                        replyMessage.Response = "The number is registered !!";
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
                        var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                        + "977" + mobile + "&Text=" + messagereply + "");

                        statusCode = "200";
                        message = code;
                    }
                    else if ((mobile.Substring(0, 3) == "985") || (mobile.Substring(0, 3) == "984")
                        || (mobile.Substring(0, 3) == "986"))
                    {
                        //FOR NTC
                        var contents = client.DownloadString("http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Parts=1&Password=mnepal120&From=37878&To="
                            + "977" + mobile + "&Text=" + messagereply + "");

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
                statusCode = "400";
                result = "The number is registered !!";
                replyMessage.Response = "The number is registered !!";
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
                if (dtCheckUserName.Rows.Count == 1)
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
                (string.IsNullOrEmpty(FathersName)) || (string.IsNullOrEmpty(MothersName)) || (string.IsNullOrEmpty(MaritalStatus)) ||
                (string.IsNullOrEmpty(GrandFathersName)) || (string.IsNullOrEmpty(Occupation)) || (string.IsNullOrEmpty(EmailAddress)) ||
                (string.IsNullOrEmpty(PProvince)) || (string.IsNullOrEmpty(PDistrict)) || (string.IsNullOrEmpty(PMunicipalityVDC)) || 
                (string.IsNullOrEmpty(PHouseNo)) || (string.IsNullOrEmpty(PWardNo)) || (string.IsNullOrEmpty(PStreet)) ||
                (string.IsNullOrEmpty(CProvince)) || (string.IsNullOrEmpty(CDistrict)) || (string.IsNullOrEmpty(CMunicipalityVDC)) || 
                (string.IsNullOrEmpty(CHouseNo)) || (string.IsNullOrEmpty(CWardNo)) || (string.IsNullOrEmpty(CStreet)) || 
                (string.IsNullOrEmpty(BranchCode)) || (string.IsNullOrEmpty(Source)) || 
                (string.IsNullOrEmpty(frontImages)) || (string.IsNullOrEmpty(backImages)) || (string.IsNullOrEmpty(passportImages)))
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
                                    message = "Already Registered !";
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
                                            PassportImage = string.Format("~/Content/Upload/{0}", PP);
                                        }
                                        else {
                                            PassportImage = string.Format("~/Content/Upload/{0}", PP + ".jpg");
                                        }

                                        if (last4Front.Equals(".jpg"))
                                        {
                                            FrontImage = string.Format("~/Content/Upload/{0}", front);
                                        }
                                        else {
                                            FrontImage = string.Format("~/Content/Upload/{0}", front + ".jpg");
                                        }

                                        if (last4Back.Equals(".jpg"))
                                        {
                                            BackImage = string.Format("~/Content/Upload/{0}", back);
                                        }
                                        else {
                                            BackImage = string.Format("~/Content/Upload/{0}", back + ".jpg");
                                        }

                                        string userType = "user";
                                        string Password = SelfRegisterUtils.GeneratePassword();

                                        if ( (string.IsNullOrEmpty(UserName)) || (string.IsNullOrEmpty(FName)) || (LName == null) ||
                                            (Gender == null) || (DateOfBirth == null) || (CountryCode == null) || (Nationality == null) ||
                                            (FathersName == null) || (MothersName == null) || (MaritalStatus == null) || (GrandFathersName == null) || (Occupation == null) ||
                                            (PProvince == null) || (PDistrict == null) || (PProvinceID == null) || (PDistrictID == null) ||
                                            (PMunicipalityVDC == null) || (PHouseNo == null) || (PWardNo == null) || (PStreet == null) ||
                                            (CProvinceID == null) || (CDistrictID == null) || (CMunicipalityVDC == null) || (CHouseNo == null) ||
                                            (CWardNo == null) || (CStreet == null) || (BranchCode == null) || (Source == null) ||
                                            (string.IsNullOrEmpty(FrontImage)) || (string.IsNullOrEmpty(BackImage)) || (string.IsNullOrEmpty(PassportImage)) )
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
                                                    FathersName, MothersName, GrandFathersName, MaritalStatus, SpouseName,  FatherInLaw,
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
                                                string messagereply = "Dear Customer," + "\n";
                                                messagereply += " Please Wait for your Approval " + UserName
                                                                + "." + "\n";
                                                messagereply += "Thank you. MNepal";

                                                var client = new WebClient();

                                                if ((UserName.Substring(0, 3) == "980") || (UserName.Substring(0, 3) == "981")) //FOR NCELL
                                                {
                                                    //FOR NCELL
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=2&KeywordId=3&Password=mnepal120&From=37878&To="
                                                        + "977" + UserName + "&Text=" + messagereply + "");
                                                }
                                                else if ((UserName.Substring(0, 3) == "985") || (UserName.Substring(0, 3) == "984")
                                                            || (UserName.Substring(0, 3) == "986"))
                                                {
                                                    //FOR NTC
                                                    var content = client.DownloadString(
                                                        "http://smsvas.mos.com.np/PostSMS.ashx?QueueId=&TelecomId=1&KeywordId=3&Password=mnepal120&From=37878&To="
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

        public string GetProvinceID(string provinceName) {
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
            string randomFileName = mobile + "_" + imagetype + "_" + imageside +
                                "_" + Guid.NewGuid().ToString().Substring(0, 4) + Path.GetExtension(fileName);
            //var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Upload/"), randomFileName + ".jpg");

            //if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/Content/Upload")))
            //{
            //    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Content/Upload"));
            //}

            string _ImageSaveUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["ImageAddress"];

            var path = Path.Combine(HttpContext.Current.Server.MapPath(_ImageSaveUrl + "Content/Upload/"), randomFileName); ///MNepalAdmin.Com/Content/Upload/

            if (!Directory.Exists(HttpContext.Current.Server.MapPath(_ImageSaveUrl + "Content/Upload")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(_ImageSaveUrl + "Content/Upload"));
            }
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
