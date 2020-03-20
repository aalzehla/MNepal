using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;

namespace MNepalProject.DAL
{
    public class dalUserManagement
    {
        #region Initilization

        GlobalConnection gc = new GlobalConnection();

        #endregion

        #region Method

        private string GetClientCode(string username)
        {
            try
            {
                using(var db=gc.getConnection())
                {
                    var clientCode = db.Query<MNClientExt>("SELECT * FROM MNClientExt Where UserName=@0", username).SingleOrDefault();
                    if(clientCode!=null)
                    {
                        return clientCode.ClientCode;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool isAgent(string username)
        {
            var _OK = false;
            try
            {
                var clientCode = this.GetClientCode(username);
                if (clientCode != null)
                {
                    using (var db = gc.getConnection())
                    {
                        var agent = db.Query<MNAgent>("SELECT * FROM MNAgent WHERE ClientCode=@0", clientCode).SingleOrDefault();
                        if (agent != null)
                        {
                            return true;
                        }
                        else
                            return _OK;
                    }
                }
                return _OK;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }

        #endregion
}