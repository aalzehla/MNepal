using MNepalAPI.Models;
using MNepalAPI.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MNepalAPI.Utilities
{
    public class GetQRCodeImageUrl
    {
        public static DataTable GetQRCodeImage(string merchantName, string merchantId)
        {
            var objUserModel = new GetQRCodeUrlModel();
            var objUserInfo = new QRCodeReader
            {
                merchantName = merchantName,
                merchantId = merchantId
            };
            return objUserModel.GetUserQRCodeUrl(objUserInfo);
        }
    }
}