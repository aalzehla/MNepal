using MNSuperadmin.Models;
using MNSuperadmin.UserModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Utilities
{
    public class AgentUtils
    {
        #region "GET Agent Profile Information By Searching Mobile Number"

        /// <summary>
        /// Get Customer Profile By Searching Mobile No
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <returns></returns>
        public static DataTable GetAgentProfileByMobileNo(string mobileNo)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                ContactNumber1 = mobileNo,
                Mode = "SABMN" // SEARCH AGENT BY MobileNo
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region GET All Agents Profile Information By Searching Mobile Number 
        //Active/InActive
        //Approved/Unapproved
        public static DataTable GetAllAgentPBMobileNo(string mobileNo)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                ContactNumber1 = mobileNo,
                Mode = "SABMA" // SEARCH ALL AGENT BY MobileNo
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region "GET Agent Profile Information By Searching Name"

        /// <summary>
        /// Get Customer Profile By Searching Name
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <returns></returns>
        public static DataTable GetAgentProfileByName(string name)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                Name = name,
                Mode = "SABN" // SEARCH AGENT BY Name
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region "GET Agent Profile Information By Searching Account Number"

        /// <summary>
        /// Get Customer Profile By Searching Account Number
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <returns></returns>
        public static DataTable GetAgentProfileByAC(string accountNo)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                WalletNumber = accountNo,
                Mode = "SABAC" // SEARCH AGENT BY Account Number
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region "GET Agent Profile Information By Searching Name & Account Number"

        /// <summary>
        /// Get Customer Profile By Searching Name N Account Number
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <param name="accountNo"></param>
        /// <returns></returns>
        public static DataTable GetAgentProfileByNameAC(string name, string accountNo)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                Name = name,
                WalletNumber = accountNo,
                Mode = "SABNAC" // SEARCH AGENT BY Name/AccountNumber
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion

        #region "GET Agent Profile Information By Searching Name & Account Number"

        /// <summary>
        /// Get Customer Profile By Searching Name N Account Number
        /// </summary>
        /// <param name="name"></param>
        /// <param name="accountNo"></param>
        /// <param name="mobileNo"></param>
        /// <returns></returns>
        public static DataTable GetAgentProfileByALL(string name, string accountNo, string mobileNo)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                Name = name,
                WalletNumber = accountNo,
                ContactNumber1 = mobileNo,
                Mode = "SABALL" // SEARCH AGENT BY ALL
            };
            return objUserModel.GetCustomerDetailInformation(objUserInfo);
        }

        #endregion


        #region "GET Agent ID"

        /// <summary>
        /// Get Agent ID By ClientCode
        /// </summary>
        /// <param name="clientCode"></param>
        /// <returns></returns>
        public static DataTable GetAgentId(string clientCode)
        {
            var objUserModel = new CustomerUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode,
                Mode = "GAID" // Get Agent ID By ClientCode
            };
            return objUserModel.GetAgentIdModel(objUserInfo);
        }

        #endregion

        #region Agent Registration Approval List
        public static List<UserInfo> GetAgentRegApprove(string userType, string UserName)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                UserType = userType


            };
            return objUserModel.GetAgentRegApprove(objUserInfo, UserName);
        }
        #endregion

        #region Agent Registration Approval List
        public static List<UserInfo> GetAgentRegReject(string userType, string UserName)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                UserType = userType


            };
            return objUserModel.GetAgentRegReject(objUserInfo, UserName);
        }
        #endregion

        #region Agent Registration Approval List
        public static List<UserInfo> GetAgentModReject(string userType, string UserName)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                UserType = userType


            };
            return objUserModel.GetAgentModReject(objUserInfo, UserName);
        }
        #endregion

        #region Approving Agent Registration
        internal static int AgentRegisterApprove(UserInfo member, string Approve)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,
                ApprovedBy = member.ApprovedBy
            };
            return objUserModel.AgentRegApprove(objUserInfo, Approve);
        }

        #endregion

        #region Approving Merchant Registration
        internal static int MerchantRegisterApprove(UserInfo member, string Approve)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,

            };
            return objUserModel.MerchantRegApprove(objUserInfo, Approve);
        }

        #endregion

        #region Rejecting Agent Registration
        internal static int AgentRegisterReject(UserInfo member, string Reject)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,
                Remarks = member.Remarks,
                AdminUserName = member.AdminUserName

            };
            return objUserModel.AgentRegApprove(objUserInfo, Reject);
        }

        #endregion

        #region GET Agent Modification Approval List
        public static List<UserInfo> GetAgentModApprove(string userType, string UserName)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                UserType = userType


            };
            return objUserModel.GetAgentModApprove(objUserInfo, UserName);
        }
        #endregion
        #region GET Agent Modification Approval List
        public static List<UserInfo> GetAgentCommissionApprove(string userType, string UserName)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                UserType = userType


            };
            return objUserModel.GetAgentCommissionApprove(objUserInfo, UserName);
        }
        #endregion
       
        #region Approving Agent Modification
        internal static bool AgentModApprove(UserInfo member)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,
                ApprovedBy = member.ApprovedBy,
            };
            return objUserModel.AgentModApprove(objUserInfo) == 100;
        }

        #endregion

        #region Rejecting Agent Modification
        internal static bool AgentModReject(UserInfo member)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,
                RejectedBy = member.RejectedBy,
                AdminUserName = member.AdminUserName,
                AdminBranch = member.AdminBranch,
                Remarks = member.Remarks
                
            };
            return objUserModel.AgentModReject(objUserInfo) == 100;
        }

        #endregion

        #region Rejecting Agent Registration

        public static DataSet GetAgentModifiedValue(string clientCode)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode

            };
            return objUserModel.GetAgentModifiedValue(objUserInfo);
        }




        internal static int AgentRegReject(UserInfo member, string Rejected)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = member.ClientCode,
                RejectedBy = member.RejectedBy,
                ApprovedDate = member.ApprovedDate

            };
            return objUserModel.AgentRegisterReject(objUserInfo, Rejected);
        }

        #endregion

        #region
        public static bool InsertAgentMakerChecker(string ClientCode, string ModifyingAdmin, string ModifyingBranch, string ModifiedField)
        {
            return new AgentUserModel().InsertIntoMakerCheckerAgent(ClientCode, ModifyingAdmin, ModifyingBranch, ModifiedField) == 100;
        }
        #endregion

        #region GET Agent Status Changed List

        internal static List<UserInfo> GetAgentStatus(string MobileNo)
        {
            var objUserModel = new AgentUserModel();
            return objUserModel.GetAgentStatusList(MobileNo);

        }

        #endregion

        #region Approve Agent Status Changed

        internal static bool AgentStatusApprove(string ClientCode)
        {
            var objUserModel = new AgentUserModel();

            return objUserModel.StatusApprove(ClientCode) == 1;
        }


        #endregion

        internal static bool AgentStatusReject(string ClientCode, string Status)
        {
            var objUserModel = new AgentUserModel();

            return objUserModel.StatusReject(ClientCode, Status) == 1;
        }

        public static int RejectModifiedAgentCommission(UserInfo model)
        {
            return new AgentUserModel().RejectModifiedAgentCommission(model);
        }
        public static int ApproveModifiedAgentCommission(UserInfo model)
        {
            return new AgentUserModel().ApproveModifiedAgentCommission(model);
        }

        #region "REJECTED LIST FOR APPROVE Agent Commission"

        public static List<UserInfo> GetRejectedAgentCommissionList(string IsModified)
        {
            var objUserModel = new AgentUserModel();
            var objUserInfo = new UserInfo();

            return objUserModel.GetRejectedAgentCommissionList(objUserInfo, IsModified);
        }

        #endregion

        #region
        public static bool InsertMakerAgentCommissionChecker(string ClientCode, string ModifyingAdmin, string ModifyingBranch, string ModifiedField)
        {
            return new AgentUserModel().InsertIntoMakerCheckerAgentCommission(ClientCode, ModifyingAdmin, ModifyingBranch, ModifiedField) == 100;
        }
        #endregion
    }
}