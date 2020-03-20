using MNepalWeb.Models;
using MNepalWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MNepalWeb.Helper
{
    public class RoleChecker
    {
        //
        public DataSet GetParentMenu(string clientCode)
        {
            List<UserProfilesInfo> userProfilesList = new List<UserProfilesInfo>();
            DataSet dtUserProfile = UserProfileUtils.GetAdminUserProfileInfo1(clientCode);
            return dtUserProfile;
        }

        public bool checkRole(string link,string clientCode) {            
            DataSet ds = GetParentMenu(clientCode);
            DataTable dt = ds.Tables[0];
            
            foreach (DataRow dr in dt.Rows)
            {

                string LinkUrl = dr["LinkUrl"].ToString();
                if (LinkUrl == link)
                {
                   return true;
                }
            }
            return false;
        }

        public bool checkIndexRole(string link, string clientCode,string ControllerName)
        {
            DataSet ds = GetParentMenu(clientCode);
            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                string controllerNamefromDb = dr["MenuModuleName"].ToString();
                string LinkUrl = dr["LinkUrl"].ToString();
                if (LinkUrl == link&& controllerNamefromDb== ControllerName)
                {
                    return true;
                }
            }
            return false;
        }

    }
}