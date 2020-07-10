using MNepalAPI.Models;
using MNepalAPI.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalAPI.Utilities
{
    public class QRCodeGeneratorUtilities
    {
        public static int QRCode(QRCode qRCode)
        {
            var objresQRCodeGeneratorModel = new QRCodeGeneratorUserModel();
            var objresQRCodeGeneratorInfo = new QRCode
            {
                merchantName = qRCode.merchantName,
                merchantId = qRCode.merchantId,
                qrCodeImagePath = qRCode.qrCodeImagePath
            };
            return objresQRCodeGeneratorModel.ResponseQRCodeInfo(objresQRCodeGeneratorInfo);
        }
    }
}