using ThailiMNepalAgent.Models;
using ThailiMNepalAgent.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ThailiMNepalAgent.Utilities
{
    public class AvailBalnUtils
    {
        #region "AvailBaln Utilities"

        /// <summary>
        /// Get the availBaln information of given clientCode
        /// </summary>
        /// <param name="clientCode">Pass clientCode as string</param>
        /// <returns>Returns the datatable of availBaln information</returns>
        public static DataTable GetAvailBaln(string clientCode)
        {
            var objUserModel = new AvailBalnUserModels();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode
            };
            return objUserModel.GetUserAvailBaln(objUserInfo);
        }

        #endregion
    }
}