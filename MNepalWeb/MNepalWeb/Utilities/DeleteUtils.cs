using MNepalWeb.Models;
using MNepalWeb.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Utilities
{
    public static class DeleteUtils
    {
        #region "Member Change Delete Status Utilities"

        public static bool UpdateDeleteUserInfo(this UserInfo member)
        {
            var objMemberInfo = new UserInfo()
            {
                Status = member.Status,
                ClientCode = member.ClientCode,
                Mode = "DUSST" // DELETE USER STATUS
            };
            return new DeleteUserModel().DeleteUserStatusInfo(objMemberInfo) > 0;
        }

        public static bool ChangeUserInfo(string ClientCode,string Status, string AdminBranchCode,string AdminUserName,string BlockRemarks)
        {
         
            return new DeleteUserModel().ChangeUserStatus(ClientCode,Status,AdminBranchCode,AdminUserName,BlockRemarks) ==1;
        }

        #endregion
    }
}