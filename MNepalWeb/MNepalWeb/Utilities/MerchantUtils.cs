using MNepalWeb.Models;
using MNepalWeb.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MNepalWeb.Utilities
{
    public class MerchantUtils
    {
        #region "GET Merchant Detail Information"

        public static DataTable GetMerchantDetailInfo()
        {
            var objMerchantModel = new MerchantUserModels();
            var objMerchantInfo = new MNMerchants
            {
                Mode = "GMDI" // GET MERCHANT DETAIL INFORMATION
            };
            return objMerchantModel.GetMerchantDetailInformation(objMerchantInfo);
        }


        public static Dictionary<string,string> GetMerchantsType()
        {
            var objMerchantModel = new MerchantUserModels();
          
            return objMerchantModel.GetMerchantsType();
        }

        #endregion
    }
}