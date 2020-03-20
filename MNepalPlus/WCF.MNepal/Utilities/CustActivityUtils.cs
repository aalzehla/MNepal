using WCF.MNepal.Models;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
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

        #region
        
        public static int InsertCustSupportForm(CustomerSupport userInfo)
        {
            var objCustSupportModel = new CustActivityUserModel();          
            return objCustSupportModel.InsertCustSupportForm(userInfo);
        }
        #endregion

        #region InsertMerchantTransaction

        public static int InsertMerchantTransaction(MerchantTransaction userInfo)
        {
            var objCustSupportModel = new CustActivityUserModel();
            return objCustSupportModel.InsertMerchantTransaction(userInfo);
        }
        #endregion
        #region UpdateStatusMerchantTransaction

        public static int UpdateStatusMerchantTransaction(MerchantTransaction userInfo)
        {
            var objCustSupportModel = new CustActivityUserModel();
            return objCustSupportModel.UpdateStatusMerchantTransaction(userInfo);
        }
        #endregion
		#region MerchantTransactionDetailsSchool
        public static int InsertMerchantTransactionDetails(FundTransfer fundtransfer)
        {
            var objMerchantTransactionDetailsModel = new CustActivityUserModel();
            return objMerchantTransactionDetailsModel.InsertMerchantTransactionDetail(fundtransfer);
        }
        #endregion
    }
}