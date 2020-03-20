using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
{
    public class DematUtils
    {
        #region Response CP Demat payments
        public static int DematPaymentInfo(DematModel resCheckPaymentDematInfo)
        {
            var objresDematPaymentModel = new DematUserModel();
            var objresDematPaymentInfo = new DematModel
            {
                BoId = resCheckPaymentDematInfo.BoId,
                BankCode = resCheckPaymentDematInfo.BankCode,
                ClientCode = resCheckPaymentDematInfo.ClientCode,
                DematName = resCheckPaymentDematInfo.DematName,
                Fees = resCheckPaymentDematInfo.Fees,
                RetrievalRef = resCheckPaymentDematInfo.RetrievalRef,
                UserName = resCheckPaymentDematInfo.UserName,
                TotalAmount = resCheckPaymentDematInfo.TotalAmount,
                TimeStamp = resCheckPaymentDematInfo.TimeStamp,
                Mode = "RSDP"  //Response demat payment
            };
            return objresDematPaymentModel.ResponseDematPaymentInfo(objresDematPaymentInfo);
        }
        #endregion

        #region
        public static int DematExecutePaymentInfo(DematModel resExecutePaymentDematInfo)
        {
            var objresDematPaymentModel = new DematUserModel();
            var objresDematExecutePaymentInfo = new DematModel()
            {
                BoId = resExecutePaymentDematInfo.BoId,
                TotalAmount = resExecutePaymentDematInfo.TotalAmount,
                TimeStamp = resExecutePaymentDematInfo.TimeStamp,
                RetrievalRef = resExecutePaymentDematInfo.RetrievalRef,
                UserName = resExecutePaymentDematInfo.UserName,
                Status = resExecutePaymentDematInfo.Status,
                BankCode = resExecutePaymentDematInfo.BankCode,
                ClientCode = resExecutePaymentDematInfo.ClientCode,
                Mode = "RDEP"   // Response Demat Execute Payment

            };
            return objresDematPaymentModel.ResponseDematPaymentInfoPost(objresDematExecutePaymentInfo);
        }
        #endregion
    }
}