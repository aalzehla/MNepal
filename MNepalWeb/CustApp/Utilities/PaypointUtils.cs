using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CustApp.UserModels;
using CustApp.Models;
using System.Data;

namespace CustApp.Utilities
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

        #region Vianet Details
        public static DataSet GetVianetDetails(ISP KPObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new ISP
            {
                ClientCode = KPObj.ClientCode,
                UserName = KPObj.UserName,
                CustomerID = KPObj.CustomerID,
                refStan = KPObj.refStan,
                Mode = "Vianet" // GET Vianet Details
            };
            return objUserModel.GetVianetPaymentDetails(objUserInfo);
        }
        #endregion

        #region SIMTV Details
        public static DataSet GetSIMTVDetails(ISP KPObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new ISP
            {
                ClientCode = KPObj.ClientCode,
                UserName = KPObj.UserName,
                CustomerID = KPObj.CustomerID,
                refStan = KPObj.refStan,
                Mode = "SIMTV" // GET SIMTV Details
            };
            return objUserModel.GetVianetPaymentDetails(objUserInfo);
        }
        #endregion

        #region MeroTV Details
        public static DataSet GetMeroTVDetails(ISP KPObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new ISP
            {
                ClientCode = KPObj.ClientCode,
                UserName = KPObj.UserName,
                CustomerID = KPObj.CustomerID,
                refStan = KPObj.refStan,
                Mode = "MeroTV" // GET MeroTV Details
            };
            return objUserModel.GetVianetPaymentDetails(objUserInfo);
        }
        #endregion

        #region SkyTV Details
        public static DataSet GetSkyTVDetails(ISP KPObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new ISP
            {
                ClientCode = KPObj.ClientCode,
                UserName = KPObj.UserName,
                CustomerID = KPObj.CustomerID,
                refStan = KPObj.refStan,
                Mode = "SkyTV" // GET SkyTV Details
            };
            return objUserModel.GetVianetPaymentDetails(objUserInfo);
        }
        #endregion

        #region Dish Home Details
        public static DataSet GetDishHomeDetails(ISP iSP)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new ISP
            {
                ClientCode = iSP.ClientCode,
                UserName = iSP.UserName,
                CustomerID = iSP.CustomerID,
                refStan = iSP.refStan,
                Mode = "DishHome" // GET Dish Home Details
            };
            return objUserModel.GetDishHomePaymentDetails(objUserInfo);
        }
        #endregion

        #region Dish Home Details
        public static DataSet GetDishHomePinDetails(ISP iSP)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new ISP
            {
                ClientCode = iSP.ClientCode,
                UserName = iSP.UserName,
                CustomerID = iSP.CustomerID,
                refStan = iSP.refStan,
                Mode = "DHPin" // GET Dish Home Details
            };
            return objUserModel.GetDishHomePaymentDetails(objUserInfo);
        }
        #endregion

        #region
        public static Dictionary<string, string> GetDishHomeServiceCode()
        {
            var objKhanepaniModel = new PaypointUserModel();

            return objKhanepaniModel.GetDishHomeServiceCode();
        }

        #endregion

        #region
        public static Dictionary<string, string> GetSkyTvServiceCode()
        {
            var objKhanepaniModel = new PaypointUserModel();

            return objKhanepaniModel.GetSkyTvServiceCode();
        }

        #endregion

        #region
        public static Dictionary<string, string> GetNetTvVoucher()
        {
            var objKhanepaniModel = new PaypointUserModel();

            return objKhanepaniModel.GetNetTvVoucher();
        }

        #endregion

    }
}