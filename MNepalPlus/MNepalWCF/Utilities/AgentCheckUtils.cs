using System.Data;
using MNepalProject.Models;
using MNepalWCF.UserModels;

namespace MNepalWCF.Utilities
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