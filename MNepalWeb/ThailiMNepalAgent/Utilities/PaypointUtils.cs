using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThailiMNepalAgent.UserModels;
using ThailiMNepalAgent.Models;
using System.Data;

namespace ThailiMNepalAgent.Utilities
{
    public class PaypointUtils
    {
        #region
        public static Dictionary<string, string> GetNEAName()
        {
            var objNEAModel = new PaypointUserModel();

            return objNEAModel.GetNEAName();
        }

        #endregion

        #region
        public static Dictionary<string, string> GetKhanepaniName()
        {
            var objKhanepaniModel = new PaypointUserModel();

            return objKhanepaniModel.GetKhanepaniName();
        }

        #endregion

        #region
        public static Dictionary<string, string> GetNepalWaterName()
        {
            var objKhanepaniModel = new PaypointUserModel();

            return objKhanepaniModel.GetNepalWaterName();
        }

        #endregion

        #region NEA Details
        public static DataSet GetNEADetails(NEAFundTransfer NEAObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new NEAFundTransfer
            {
                ClientCode = NEAObj.ClientCode,
                UserName = NEAObj.UserName,
                SCNo = NEAObj.SCNo,
                NEABranchCode = NEAObj.NEABranchCode,
                CustomerID = NEAObj.CustomerID,
                refStan=NEAObj.refStan,
                Mode = "NEA" // GET NEA Details
            };
            return objUserModel.GetNEAPaymentDetails(objUserInfo);
        }
        #endregion

        #region Khanepani Details
        public static DataSet GetKPDetails(Khanepani KPObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new Khanepani
            {
                ClientCode = KPObj.ClientCode,
                UserName = KPObj.UserName,
                KhanepaniCounter = KPObj.KhanepaniCounter,
                CustomerID = KPObj.CustomerID,
                refStan=KPObj.refStan,
                Mode = "KP" // GET NEA Details
            };
            return objUserModel.GetKPPaymentDetails(objUserInfo);
        }
        #endregion

        #region Nepal Water Details
        public static DataSet GetNWDetails(NepalWater KPObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new NepalWater
            {
                ClientCode = KPObj.ClientCode,
                UserName = KPObj.UserName,
                NWCounter = KPObj.NWCounter,
                CustomerID = KPObj.CustomerID,
                refStan = KPObj.refStan,
                Mode = "NW" // GET NEA Details
            };
            return objUserModel.GetNWPaymentDetails(objUserInfo);
        }
        #endregion


        #region Wlink Details
        public static DataSet GetWlinkDetails(ISP KPObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new ISP
            {
                ClientCode = KPObj.ClientCode,
                UserName = KPObj.UserName,
                CustomerID = KPObj.CustomerName,
                refStan = KPObj.refStan,
                Mode = "Wlink" // GET Wlink Details
            };
            return objUserModel.GetWlinkPaymentDetails(objUserInfo);
        }
        #endregion

        #region Subisu Details
        public static DataSet GetSubisuDetails(ISP KPObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new ISP
            {
                ClientCode = KPObj.ClientCode,
                UserName = KPObj.UserName,
                CustomerID = KPObj.CustomerID,
                refStan = KPObj.refStan,
                Mode = "Subisu" // GET Subisu Details
            };
            return objUserModel.GetSubisuPaymentDetails(objUserInfo);
        }
        #endregion

    }
}