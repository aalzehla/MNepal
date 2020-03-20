using MNepalWCF.Models;
using MNepalWCF.UserModels;

namespace MNepalWCF.Utilities
{
    public class CustActivityUtils
    {
        public static int RegisterCustActivityInfo(CustActivityModel userSMSInfo)
        {
            var objCustActivityModel = new CustActivityUserModel();
            var objCustSMSInfo = new CustActivityModel
            {
                UserName = userSMSInfo.UserName,
                RequestMerchant = userSMSInfo.RequestMerchant,
                DestinationNo = userSMSInfo.DestinationNo,
                Amount = userSMSInfo.Amount,
                SMSStatus = userSMSInfo.SMSStatus,
                SMSSenderReply = userSMSInfo.SMSSenderReply,
                ErrorMessage = userSMSInfo.ErrorMessage,
                Mode = "SCA"
            };
            return objCustActivityModel.CustRegisterSMSInfo(objCustSMSInfo);
        }


        public static int InsertIPInfo(CustActivityModel userSMSInfo)
        {
            var objIPModel = new CustActivityUserModel();
            var objIPInfo = new CustActivityModel
            {
                RemoteIP = userSMSInfo.RemoteIP,
                ExternalIP = userSMSInfo.ExternalIP,
                LocalIP = userSMSInfo.LocalIP,
                Mode = "IP",
            };
            return objIPModel.InsertIPInfo(objIPInfo);
        }

    }
}