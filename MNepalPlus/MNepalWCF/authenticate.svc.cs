using MNepalProject.Controllers;
using MNepalProject.Models;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using MNepalWCF.Models;

namespace MNepalWCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "authenticate" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select authenticate.svc or authenticate.svc.cs at the Solution Explorer and start debugging.
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class authenticate //: Iauthenticate
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "authenticatelogin", Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        public string authenticatelogin(Stream input)
        {
            StreamReader sr = new StreamReader(input);
            string s = sr.ReadToEnd();
            sr.Dispose();
            NameValueCollection qs = HttpUtility.ParseQueryString(s);

            string UserName = qs["UserName"];
            string Password = qs["Password"];
            string userType = qs["userType"];

            //string UserName = uname; //qs["UserName"];
            //string Password = pwd; //qs["Password"];
            //string userType = utype;//qs["userType"];

            ReplyMessage replyMessage = new ReplyMessage();

            string result = "";
            UserValidate uservalidate = new UserValidate(UserName, Password, userType.ToLower());
            Message<UserValidate> uservalidaterequest = new Message<UserValidate>(uservalidate);
            MNComAndFocusOneLog comfocuslog = new MNComAndFocusOneLog(0, uservalidaterequest.message, "0", DateTime.Now);
            MNComAndFocusOneLogsController comfocuslogs = new MNComAndFocusOneLogsController();

            //
            result = comfocuslogs.InsertIntoComFocusOne(comfocuslog);
            if (result == "Success")
            {
                if (String.IsNullOrEmpty(uservalidate.UserName) || String.IsNullOrEmpty(uservalidate.Password))
                {
                    replyMessage.Response = "parameters missing";

                    if (String.IsNullOrEmpty(uservalidate.UserName))
                    {
                        replyMessage.Response = "parameters missing";
                    }

                    if (String.IsNullOrEmpty(uservalidate.Password))
                    {
                        replyMessage.Response = "parameters missing";
                    }
                    if (String.IsNullOrEmpty(uservalidate.userType.ToLower()))
                    {
                        replyMessage.Response = "parameters missing";
                    }

                    replyMessage.ResponseStatus(HttpStatusCode.BadRequest, replyMessage.Response);
                }
                else
                {
                    string ClientExtReply = "";
                    ClientExtReply = PassDataToMNClientExtController(uservalidate);
                    if (ClientExtReply == "true")
                    {
                        replyMessage.Response = "ok valid user";
                        replyMessage.ResponseStatus(HttpStatusCode.OK, replyMessage.Response);
                    }
                    else
                    {
                        replyMessage.Response = "Invalid User";
                        replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                    }
                }
            }
            else
            {
                replyMessage.Response = "request denied";
                replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, replyMessage.Response);
            }

            JObject jsonObject = new JObject();
            jsonObject["message"] = replyMessage.Response;
            return jsonObject["message"].ToString();
        }

        public string PassDataToMNClientExtController(UserValidate uv)
        {
            string reply = "";
            MNClientExt mnclientext = new MNClientExt();
            mnclientext.UserName = uv.UserName;
            mnclientext.Password = uv.Password;
            mnclientext.userType = uv.userType;


            MNClientExtsController mncontroller = new MNClientExtsController();
            reply = mncontroller.ValidateUser(mnclientext);

            return reply;
        }

    }
}
