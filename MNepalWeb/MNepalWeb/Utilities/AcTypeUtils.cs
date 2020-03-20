using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MNepalWeb.Models;
using MNepalWeb.UserModels;

namespace MNepalWeb.Utilities
{
    public class AcTypeUtils
    {
        #region "GET Account Type"

        /// <summary>
        /// Get AccountType
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAccountType()
        {
            var objAcTypeModel = new AcTypeUserModel();
            var objAcTypeInfo = new AcTypeInfo()
            {
                AcType = "",
                AcTypeName = "",
                AllowEnquiry = "",
                AllowTransaction = "",
                AllowAlert = "",
                Active = "",
                Mode = "GAA" // GET ALL ACCOUNTTYPE
            };
            return objAcTypeModel.GetDsAcTypeInfo(objAcTypeInfo);
        }

        #endregion

        #region "GET AccountType Information"

        public static DataTable GetAcTypeDetails(string actype)
        {
            var objAcTypeModel = new AcTypeUserModel();
            var objAcTypeInfo = new AcTypeInfo()
            {
                AcType = actype,
                //AcType = "",
                AcTypeName = "",
                AllowEnquiry = "",
                AllowTransaction = "",
                AllowAlert = "",
                Active = "",
                Mode = "GAD"   // GET ACCOUNTTYPE DETAILS
            };
            return objAcTypeModel.GetAccountInformation(objAcTypeInfo);
        }

        #endregion

        #region "UPDATE AccountType Information"
        public static int UpdateAccountType(AcTypeInfo acTypeInfo)
        {
            var objActypeModel = new AcTypeUserModel();
            var objAcTypeInfo = new AcTypeInfo()
            {

                AcType = acTypeInfo.AcType,
                AcTypeName = acTypeInfo.AcTypeName,
                AllowEnquiry = acTypeInfo.AllowEnquiry,
                AllowTransaction = acTypeInfo.AllowTransaction,
                AllowAlert = acTypeInfo.AllowAlert,
                Active = acTypeInfo.Active,
                Mode = "UAD" // Modify 
            };
            return objActypeModel.UpdateAccountType(objAcTypeInfo);
        }
        #endregion

        public static int CreateAcTypeInfo(AcTypeInfo objacInfo)
        {
            var objAcTypeModel = new AcTypeUserModel();
            var objAcTypeInfo = new AcTypeInfo()
            {
                AcType = objacInfo.AcType,
                AcTypeName = objacInfo.AcTypeName,
                AllowTransaction = objacInfo.AllowTransaction,
                AllowEnquiry = objacInfo.AllowEnquiry,
                AllowAlert = objacInfo.AllowAlert,
                Active = objacInfo.Active,
                Mode = "IAD"
            };
            return objAcTypeModel.AcTypeInfo(objAcTypeInfo);
        }


       #region "GET Checking AcType"
        public static DataTable GetCheckAcType(string actype)
        {
            var objAcTypeModel = new AcTypeUserModel();
            var objAcTypeInfo = new AcTypeInfo()
            {
                AcType = actype,
                AcTypeName = "",
                AllowEnquiry = "",
                AllowTransaction = "",
                AllowAlert = "",
                Active = "",
                Mode = "GCAT"
            };
            return objAcTypeModel.GetAcTypeInfo(objAcTypeInfo);

        }
      #endregion

    }
}