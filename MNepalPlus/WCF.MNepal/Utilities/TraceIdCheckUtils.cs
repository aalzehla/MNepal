using MNepalProject.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
{
    public class TraceIdCheckUtils
    {
        #region TraceID Checker

        public static DataTable GetTraceIDInfo(string tid)
        {
            var objModel = new TraceIdCheckerUserModel();
            var objTraceIDInfo = new MNTraceID
            {
                TraceID = tid
            };
            return objModel.GetTraceIDCheckInfo(objTraceIDInfo);
        }

        #endregion
    }
}