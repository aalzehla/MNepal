using System.Data;
using MNepalProject.Models;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
{
    public class AgentCheckUtils
    {
        #region Agent Checker

        public static DataTable GetAgentInfo(string amobile)
        {
            var objModel = new AgentCheckerUserModel();
            var objAgentInfo = new MNClientExt
            {
                UserName = amobile
            };
            return objModel.GetAgentCheckInfo(objAgentInfo);
        }

        #endregion
    }
}