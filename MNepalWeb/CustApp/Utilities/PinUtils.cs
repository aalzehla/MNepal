using CustApp.Models;
using CustApp.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustApp.Utilities
{
    public static class PinUtils
    {
        #region "Member Change Pin Utilities"

        public static bool UpdateUserPinInfo(this UserInfo member)
        {
            var objMemberInfo = new UserInfo()
            {
				OPIN = member.OPIN,
                PIN = member.PIN,
                ClientCode = member.ClientCode,
                Mode = "CPIN" // Change Password
            };
            return new PinUserModel().UpdateUserPinInfo(objMemberInfo) > 0;
        }


        public static bool UpdateUserPin(UserInfo model,string status)
        {
           
            return new PinUserModel().UpdateUserPin(model,status)==1;
        }




     

        public static List<UserInfo> GetPinResetList(string BranchCode,bool COC,string MobileNo)
        {
            return new PinUserModel().GetPinApproveList(BranchCode, COC,MobileNo);
        }



        public static bool ApproveUserPinInfo(UserInfo member,string Mode)
        {
            var objMemberInfo = new UserInfo()
            {
                PIN = member.PIN,
                ClientCode = member.ClientCode,
                Password=member.Password
                

            };
            return new PinUserModel().ApproveUserPin(objMemberInfo,Mode) == 0;
        }

        public static bool RevertPinInfo( string ClientCode)
        {
            return new PinUserModel().RevertPasswordReset(ClientCode) == 1;
        }

        #endregion

        #region Admin Password Reset Approve

        public static bool UpdateUserPassword(UserInfo model, string status)
        {

            return new PinUserModel().UpdatePasswordReset(model, status) == 1;
        }


        public static List<UserInfo> GetPasswordResetList(string UserName)
        {
            return new PinUserModel().GetPasswordAAL(UserName);
        }



        #endregion

    }
}