using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.DAL;
using MNepalProject.Models;
using System.Threading.Tasks;


namespace MNepalProject.DAL
{
    internal class dalTransactionManagement
    {
        #region Inilization

        GlobalConnection gc = new GlobalConnection();

        #endregion

        #region Method

        internal string GetClientCode(string username)
        {
            try
            {
                using(var db=gc.getConnection())
                {
                    var _result = db.Query<MNClientExt>("SELECT * FROM MNClientExt WHERE UserName =@0", username).SingleOrDefault();
                    if (_result!=null)
                    {
                        return _result.ClientCode;
                    }
                    else
                        return null;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
         }

        internal string GetBalanceFromDatabase(string clientCode)
        {
            try
            {
                using(var db=gc.getConnection())
                {
                    var query = db.Query<MNBalance>("SELECT GoodBaln as amount FROM Master Where ClientCode=@0", clientCode).SingleOrDefault();
                    if(query!=null)
                    {
                        return query.amount;
                    }
                    return null;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        internal string GetPIN(string clientCode)
        {
            try
            {
                using(var db=gc.getConnection())
                {
                    var _result = db.Query<MNClient>("SELECT * FROM MNClient WHERE ClientCode=@0", clientCode).SingleOrDefault();
                    return _result.PIN;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        internal  List<MNTransactionMaster> GetMyTransaction(string sourcemobile)//source mobile=username=user mobile no
        {
            try
            {
                List<MNTransactionMaster> tm = new List<MNTransactionMaster>();
                using(var db=gc.getConnection())
                {
                    //tm= db.Query<MNTransactionMaster>("SELECT * FROM MNTransactionMaster WHERE SourceMobile=@0 ORDER BY CreatedDate Desc", sourcemobile).ToList();
                    tm = db.Query<MNTransactionMaster>(@"SELECT a.*,b.FeatureName as FeatureName,c.StatusName as StatusName FROM MNTransactionMaster a 
                                                         INNER  JOIN MNFeatureMaster b ON a.FeatureCode=b.FeatureCode INNER JOIN 
                                                            MNStatus c ON a.StatusId=c.ID WHERE a.SourceMobile=@0 OR a.DestinationMobile=@1 ORDER BY CreatedDate desc", sourcemobile,sourcemobile).ToList();
                    return tm;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        internal string GetFeatureName(string _featureCode)
        {
            try
            {
                using (var db = gc.getConnection())
                {
                    var result= db.Query<MNFeatureMaster>("SELECT * FROM MNFeatureMaster WHERE FeatureCode=@0", _featureCode).SingleOrDefault();
                    if (result != null)
                    {
                        return result.FeatureName;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        internal string GetStatusName(int _statusCode)
        {
            try
            {
                using(var db=gc.getConnection())
                {
                    var result = db.Query<MNStatus>("SELECT * FROM MNStatus WHERE ID=@0", _statusCode).SingleOrDefault();
                    if(result!=null)
                    {
                        return result.StatusName;
                    }
                    return null;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        internal string GetBankName(string BankCode)
        {
            try
            {
                using (var db = gc.getConnection())
                {
                   var result= db.Query<MNBankTable>("SELECT * FROM MNBankTable WHERE BankCode=@0", BankCode).SingleOrDefault();
                    if(result!=null)
                    {
                        return result.BankName;
                    }
                    return null;

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion
    }
}