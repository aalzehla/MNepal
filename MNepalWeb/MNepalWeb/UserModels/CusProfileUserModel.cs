using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

using MNepalWeb.Utilities.SqlHelper;
using MNepalWeb.Models;
using MNepalWeb.ViewModel;
using MNepalWeb.Connection;
using MNepalWeb.Utilities;

namespace MNepalWeb.UserModels
{
    public class CusProfileUserModel
    {
        public static object SqlHelper { get; private set; }
        
        public static int GetCustProfileDetails(string ProfileCode, string ConnectionString)
        {

            int retValue = 0;
            string retMsgStr = string.Empty;

            SqlParameter[] Params =
                                {
                                new SqlParameter("@ProfileCode", SqlDbType.VarChar,50)  //0
                                };


            Params[0].Value = ProfileCode;

            try
            {
                
                DataSet ds = Utilities.SqlHelper.SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, "s_GetProfileDetails", Params);
                DataTable dtProfile = ds.Tables[0];
                DataTable dtTxnLimit = ds.Tables[1];
                ds.Dispose();
            }

            catch (SqlException ex)
            {
                throw ex;
            }

            return retValue;
        }
        CusProfileUtils CPU = new CusProfileUtils();

        public static int AddNewCustProfile(MNProfileClass CustProfile, string ConnectionString, out string Message)
        {

            int retValue = 0;
            string retMsgStr = string.Empty;

            SqlParameter[] Params =
                                {
                                new SqlParameter("@RETURN_VALUE",SqlDbType.Int),            //0
                                new SqlParameter("@MsgStr", SqlDbType.VarChar,35),          //1

                                new SqlParameter("@ProfileCode",SqlDbType.VarChar,50),      //2
                                new SqlParameter("@ProfileDesc", SqlDbType.VarChar,100),    //3
                                new SqlParameter("@ProfileStatus", SqlDbType.Char,1),       //4

                                new SqlParameter("@RenewPeriod", SqlDbType.Int),            //5
                                new SqlParameter("@AutoRenew", SqlDbType.Char,1),           //6
                                new SqlParameter("@Charges", SqlDbType.VarChar,2000),       //7
                                new SqlParameter("@TxnLimit", SqlDbType.VarChar,2000),      //8

                                new SqlParameter("@HasCharge", SqlDbType.Char,1),           //9
                                new SqlParameter("@IsDrAlert", SqlDbType.Char,1),           //10
                                new SqlParameter("@IsCrAlert", SqlDbType.Char,1),           //11
                                new SqlParameter("@MinDrAlertAmt", SqlDbType.Decimal),      //12
                                new SqlParameter("@MinCrAlertAmt", SqlDbType.Decimal),      //13
                                };


            Params[0].Value = "100";
            Params[0].Direction = ParameterDirection.ReturnValue;

            Params[1].Value = string.Empty;
            Params[1].Direction = ParameterDirection.Output;

            Params[2].Value = CustProfile.ProfileCode;
            Params[3].Value = CustProfile.ProfileDesc;
            Params[4].Value = CustProfile.ProfileStatus;
            Params[5].Value = CustProfile.RenewPeriod;
            Params[6].Value = CustProfile.AutoRenew;
            Params[7].Value = CustProfile.Charge;
            Params[8].Value = CustProfile.TxnLimit;

            Params[9].Value = CustProfile.HasCharge;
            Params[10].Value = CustProfile.IsDrAlert;
            Params[11].Value = CustProfile.IsCrAlert;
            Params[12].Value = CustProfile.MinDrAlertAmt;
            Params[13].Value = CustProfile.MinCrAlertAmt;


            try
            {
                SqlDataReader rdr = Utilities.SqlHelper.SqlHelper.ExecuteReader(ConnectionString, CommandType.StoredProcedure, "s_MNProfileInsert", Params);
                retValue = Convert.ToInt32(Params[0].Value);
                retMsgStr = Convert.ToString(Params[1].Value);
            }

            catch (SqlException ex)
            {
                throw ex;
            }

            Message = retMsgStr;
            return retValue;
        }

        //Add New Profile
        public static int AddNewCustProfile(MNProfileClass CustProfile,out string Message)
        {

            int retValue = 0;
            string retMsgStr = string.Empty;

            SqlParameter[] Params =
                                {
                                new SqlParameter("@RETURN_VALUE",SqlDbType.Int),            //0
                                new SqlParameter("@MsgStr", SqlDbType.VarChar,35),          //1

                                new SqlParameter("@ProfileCode",SqlDbType.VarChar,50),      //2
                                new SqlParameter("@ProfileDesc", SqlDbType.VarChar,100),    //3
                                new SqlParameter("@ProfileStatus", SqlDbType.Char,1),       //4

                                new SqlParameter("@RenewPeriod", SqlDbType.Int),            //5
                                new SqlParameter("@AutoRenew", SqlDbType.Char,1),           //6
                                new SqlParameter("@Charges", SqlDbType.VarChar,2000),       //7
                                new SqlParameter("@TxnLimit", SqlDbType.VarChar,2000),      //8

                                new SqlParameter("@HasCharge", SqlDbType.Char,1),           //9
                                new SqlParameter("@IsDrAlert", SqlDbType.Char,1),           //10
                                new SqlParameter("@IsCrAlert", SqlDbType.Char,1),           //11
                                new SqlParameter("@MinDrAlertAmt", SqlDbType.Decimal),      //12
                                new SqlParameter("@MinCrAlertAmt", SqlDbType.Decimal),      //13
                                };


            Params[0].Value = "100";
            Params[0].Direction = ParameterDirection.ReturnValue;

            Params[1].Value = string.Empty;
            Params[1].Direction = ParameterDirection.Output;

            Params[2].Value = CustProfile.ProfileCode;
            Params[3].Value = CustProfile.ProfileDesc;
            Params[4].Value = CustProfile.ProfileStatus;
            Params[5].Value = CustProfile.RenewPeriod;
            Params[6].Value = CustProfile.AutoRenew;
            Params[7].Value = CustProfile.Charge;
            Params[8].Value = CustProfile.TxnLimit;

            Params[9].Value = CustProfile.HasCharge;
            Params[10].Value = CustProfile.IsDrAlert;
            Params[11].Value = CustProfile.IsCrAlert;
            Params[12].Value = CustProfile.MinDrAlertAmt;
            Params[13].Value = CustProfile.MinCrAlertAmt;

            string ConnectionString = DatabaseConnection.ConnectionStr().ToString();
            try
            {
                SqlDataReader rdr = Utilities.SqlHelper.SqlHelper.ExecuteReader(ConnectionString, CommandType.StoredProcedure, "s_MNProfileInsert", Params);
                retValue = Convert.ToInt32(Params[0].Value);
                retMsgStr = Convert.ToString(Params[1].Value);
            }

            catch (SqlException ex)
            {
                throw ex;
            }

            Message = retMsgStr;
            return retValue;
        }

        //Update New Profile
        public static int UpdateCustProfile(MNProfileClass CustProfile, out string Message)
        {

            int retValue = 0;
            string retMsgStr = string.Empty;

            SqlParameter[] Params =
                                {
                                new SqlParameter("@RETURN_VALUE",SqlDbType.Int),            //0
                                new SqlParameter("@MsgStr", SqlDbType.VarChar,35),          //1

                                new SqlParameter("@ProfileCode",SqlDbType.VarChar,50),      //2
                                new SqlParameter("@ProfileDesc", SqlDbType.VarChar,100),    //3
                                new SqlParameter("@ProfileStatus", SqlDbType.Char,1),       //4

                                new SqlParameter("@RenewPeriod", SqlDbType.Int),            //5
                                new SqlParameter("@AutoRenew", SqlDbType.Char,1),           //6
                                new SqlParameter("@Charges", SqlDbType.VarChar,2000),       //7
                                new SqlParameter("@TxnLimit", SqlDbType.VarChar,2000),      //8

                                new SqlParameter("@HasCharge", SqlDbType.Char,1),           //9
                                new SqlParameter("@IsDrAlert", SqlDbType.Char,1),           //10
                                new SqlParameter("@IsCrAlert", SqlDbType.Char,1),           //11
                                new SqlParameter("@MinDrAlertAmt", SqlDbType.Decimal),      //12
                                new SqlParameter("@MinCrAlertAmt", SqlDbType.Decimal),      //13
                                };


            Params[0].Value = "100";
            Params[0].Direction = ParameterDirection.ReturnValue;

            Params[1].Value = string.Empty;
            Params[1].Direction = ParameterDirection.Output;

            Params[2].Value = CustProfile.ProfileCode;
            Params[3].Value = CustProfile.ProfileDesc;
            Params[4].Value = CustProfile.ProfileStatus;
            Params[5].Value = CustProfile.RenewPeriod;
            Params[6].Value = CustProfile.AutoRenew;
            Params[7].Value = CustProfile.Charge;
            Params[8].Value = CustProfile.TxnLimit;

            Params[9].Value = CustProfile.HasCharge;
            Params[10].Value = CustProfile.IsDrAlert;
            Params[11].Value = CustProfile.IsCrAlert;
            Params[12].Value = CustProfile.MinDrAlertAmt;
            Params[13].Value = CustProfile.MinCrAlertAmt;

            string ConnectionString = DatabaseConnection.ConnectionStr().ToString();
            try
            {
                SqlDataReader rdr = Utilities.SqlHelper.SqlHelper.ExecuteReader(ConnectionString, CommandType.StoredProcedure, "s_MNProfileUpdate", Params);
                retValue = Convert.ToInt32(Params[0].Value);
                retMsgStr = Convert.ToString(Params[1].Value);
            }

            catch (SqlException ex)
            {
                throw ex;
            }

            Message = retMsgStr;
            return retValue;
        }

        
        //Get Profile
        public static CusProfileViewModel  GetCustProfileDetails(string ProfileCode,bool GetAll)
        { 
            string retMsgStr = string.Empty;
            CusProfileViewModel CustProfile = new CusProfileViewModel();
         
            SqlParameter[] Params =
                                {
                                new SqlParameter("@ProfileCode", SqlDbType.VarChar,50)  //0
                                };


            Params[0].Value = ProfileCode;

            try
            {
                string ConnectionString = DatabaseConnection.ConnectionStr().ToString();
                DataSet ds = Utilities.SqlHelper.SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, "s_GetProfileDetails", Params);
                DataTable dtProfile = ds.Tables[0];
                DataTable dtTxnLimit = ds.Tables[1];
                if(dtProfile.Rows.Count<=0)
                {
                    return null;
                }
                foreach(DataRow row in dtProfile.Rows)
                {
                    CustProfile.ProfileCode = row["CustProfileCode"].ToString();
                    CustProfile.ProfileDesc = row["ProfileDesc"].ToString();
                    CustProfile.ProfileStatus = row["ProfileStatus"].ToString();
                    CustProfile.RenewPeriod = Convert.ToInt32(row["RenewPeriod"].ToString() == "" ? "0" : row["RenewPeriod"].ToString());
                    CustProfile.AutoRenew= row["AutoRenew"].ToString();
                    CustProfile.Registration = decimal.Parse(row["Registration"].ToString()).ToString("G29");// Math.Round(Convert.ToDecimal(row["Registration"].ToString() == "" ? "0" : row["Registration"].ToString()),2);
                    CustProfile.ReNew= decimal.Parse(row["ReNew"].ToString()).ToString("G29");// Math.Round(Convert.ToDecimal(row["ReNew"].ToString() == "" ? "0" : row["ReNew"].ToString()),2);
                    CustProfile.PinReset = decimal.Parse(row["PinReset"].ToString()).ToString("G29"); //Math.Round(Convert.ToDecimal(row["PinReset"].ToString() == "" ? "0" : row["PinReset"].ToString()),2);
                    CustProfile.ChargeAccount= row["ChargeAccount"].ToString();
                    CustProfile.HasCharge =  row["HasCharge"].ToString();
                    CustProfile.IsDrAlert = row["IsDrAlert"].ToString();
                    CustProfile.IsCrAlert = row["IsCrAlert"].ToString();
                    CustProfile.MinCrAlertAmt =Math.Round(Convert.ToDecimal(row["MinCrAlertAmt"].ToString() == "" ? "0" : row["MinCrAlertAmt"].ToString()),2);
                    CustProfile.MinDrAlertAmt = Math.Round(Convert.ToDecimal(row["MinDrAlertAmt"].ToString() == "" ? "0" : row["MinDrAlertAmt"].ToString()),2);
                    
                }
                List<MNFeatureMasterVM> MNFeatures = new List<MNFeatureMasterVM>();
                List<MNFeatureMasterVM> MNFeaturesAll = new List<MNFeatureMasterVM>();
          
                foreach (DataRow row in dtTxnLimit.Rows)
                {
                    MNFeatures.Add(
                      new MNFeatureMasterVM {
                          FeatureCode = row["FeatureCode"].ToString(),
                          TxnCount = row["TxnCount"].ToString(),
                          PerTxnAmt = decimal.Parse(row["PerTxnAmt"].ToString()).ToString("G29"),
                          PerDayTxnAmt = decimal.Parse(row["PerDayAmt"].ToString()).ToString("G29"),
                          TxnAmtM = row["TxnAmtM"].ToString(),
                          FeatureWord = row["FeatureWord"].ToString(),
                          FeatureGroup = row["FeatureGroup"].ToString(),
                          FeatureName = row["FeatureName"].ToString(),
                          CanHaveMultiple = row["CanHaveMultiple"].ToString(),
                          IsSelected = "T"
                      });
                }

                if (GetAll)
                {
                    MNFeaturesAll = GetMNFeature().ToList();

                    HashSet<string> SelectedFeatures = new HashSet<string>(MNFeatures.Select(x => x.FeatureName));
                    MNFeaturesAll = MNFeaturesAll.Where(x => !SelectedFeatures.Contains(x.FeatureName)).ToList();
                    MNFeatures.AddRange(MNFeaturesAll);
                }
               
                CustProfile.MNFeatures = MNFeatures;
                
                    ds.Dispose();

            }

            catch (SqlException ex)
            {
                throw ex;
            }

            return CustProfile;
        }

        //Get Features from FeatureMaster
        public static IEnumerable<MNFeatureMasterVM> GetMNFeature()
        {


            string ConnectionString = DatabaseConnection.ConnectionStr().ToString();

            List<MNFeatureMasterVM> MNFeature = new List<MNFeatureMasterVM>();
            try
            {

                string Sql = "Select * from MNFeatureMaster WHERE CanFeatureFI = 'T' ";
                DataTable dtbl = Utilities.SqlHelper.SqlHelper.ExecuteDataTable(ConnectionString, CommandType.Text, Sql);
                if (dtbl.Rows.Count > 0)
                {
                    for (int i = 0; i < dtbl.Rows.Count; i++)
                    {
                        MNFeature.Add(
                            new MNFeatureMasterVM
                            {
                                FeatureCode = dtbl.Rows[i]["FeatureCode"].ToString(),
                                FeatureGroup = dtbl.Rows[i]["FeatureGroup"].ToString(),
                                FeatureName = dtbl.Rows[i]["FeatureName"].ToString(),
                                FeatureWord = dtbl.Rows[i]["FeatureWord"].ToString(),
                                CanHaveMultiple = dtbl.Rows[i]["CanHaveMultiple"].ToString(),
                                IsSelected="F"                                
                            });
                    }
                }

            }

            catch (SqlException ex)
            {
                throw ex;
            }

            return MNFeature;
        }

        //List Profiles
        public static IEnumerable<MNCustProfile> GetMNCustProfile()
        {


            string ConnectionString = DatabaseConnection.ConnectionStr().ToString();

            List<MNCustProfile> MNCustProfile = new List<MNCustProfile>();
            try
            {

                string Sql = @"SELECT  MP.CustProfileCode, MP.ProfileDesc, MP.ProfileStatus, MP.RenewPeriod, MP.AutoRenew,
                               MPC.Registration, MPC.ReNew, MPC.PinReset, MPC.ChargeAccount, MP.HasCharge, MP.IsDrAlert, MP.IsCrAlert, MP.MinDrAlertAmt,
                               MP.MinCrAlertAmt
                               FROM    dbo.MNCustProfile AS MP ( NOLOCK )
                               INNER JOIN dbo.MNProfileCharge AS MPC ( NOLOCK ) ON MPC.ProfileCode = MP.CustProfileCode";

                DataTable dtbl = Utilities.SqlHelper.SqlHelper.ExecuteDataTable(ConnectionString, CommandType.Text, Sql);
                if (dtbl.Rows.Count > 0)
                {
                    for (int i = 0; i < dtbl.Rows.Count; i++)
                    {
                        MNCustProfile.Add(
                            new MNCustProfile
                            {
                                ProfileCode = dtbl.Rows[i]["CustProfileCode"].ToString(),
                                ProfileDesc = dtbl.Rows[i]["ProfileDesc"].ToString(),
                                ProfileStatus =Convert.ToChar(dtbl.Rows[i]["ProfileStatus"].ToString())
                            });
                    }
                }

            }

            catch (SqlException ex)
            {
                throw ex;
            }

            return MNCustProfile;
        }

        public static IEnumerable<MNCustProfile> GetMNCustProfile(string ProfileCode)
        {


            string ConnectionString = DatabaseConnection.ConnectionStr().ToString();

            List<MNCustProfile> MNCustProfile = new List<MNCustProfile>();
            try
            {

                string Sql = @"SELECT * FROM dbo.MNCustProfile WHERE CustProfileCode=@CustProfileCode";

                DataTable dtbl = Utilities.SqlHelper.SqlHelper.ExecuteDataTable(ConnectionString, CommandType.Text, Sql);
                if (dtbl.Rows.Count > 0)
                {
                    for (int i = 0; i < dtbl.Rows.Count; i++)
                    {
                        MNCustProfile.Add(
                            new MNCustProfile
                            {
                                ProfileCode = dtbl.Rows[i]["CustProfileCode"].ToString(),
                                ProfileDesc = dtbl.Rows[i]["ProfileDesc"].ToString(),
                                ProfileStatus = Convert.ToChar(dtbl.Rows[i]["ProfileStatus"].ToString())

                            });
                    }
                }

            }

            catch (SqlException ex)
            {
                throw ex;
            }

            return MNCustProfile;
        }







    

    }
}