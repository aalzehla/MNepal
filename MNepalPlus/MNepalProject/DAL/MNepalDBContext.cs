using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Configuration; 



namespace MNepalProject.DAL
{
    public class MNepalDBContext :DbContext
    {
        public MNepalDBContext(): base("name=DbConnectionString")
        {
            
            
        }
   
        //public DbSet<MNClient> MNClients { get; set; }
        //public DbSet<MNClientContact> MNClientContacts { get; set; }
        //public DbSet<MNClientExt> MNClientExts { get; set; }

        //public DbSet<MNAccountInfo> MNAccountInfos { get; set; }
        //public DbSet<MNAgent> MNAgents { get; set; }
        //public DbSet<MNAgentFeesMaster> MNAgentFeesMasters { get; set; }
        //public DbSet<MNANMMaster> MNANMMasters { get; set; }

        //public DbSet<MNBank> MNBanks { get; set; }
        //public DbSet<MNBankAccountMap> MNBankAccountMaps { get; set; }
       
        //public DbSet<MNComAndFocusOneLog> MNComAndFocusOneLogs { get; set; }
        //public DbSet<MNFeature> MNFeatures { get; set; }
        //public DbSet<MNFeatureMaster> MNFeatureMasters { get; set; }

        //public DbSet<MNFeeMaster> MNFeeMasters { get; set; }
        //public DbSet<MNLimitMaster> MNLimitMasters { get; set; }

        //public DbSet<MNProductMaster> MNProductMasters { get; set; }
        ////public DbSet<MNRequest> MNRequests { get; set; }
        ////public DbSet<MNResponse> MNResponses { get; set; }
        //public DbSet<MNReplyType> MNReplyTypes { get; set; }

        //public DbSet<MNStatus> MNStatuses { get; set; }
        //public DbSet<MNSubscribedProduct> MNSubscribedProducts { get; set; }
        //public DbSet<MNTransactionMaster> MNTransactionMasters { get; set; }
        //public DbSet<MNTransactionLog> MNTransactionLogs { get; set; }
       
        //private SqlConnection _dapercon;
        //public SqlConnection dapercon
        //{
        //    get { return _dapercon; }
        //    set
        //    {
        //        _dapercon = new SqlConnection(ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ToString());
        //    }
        //}
        

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        //    base.OnModelCreating(modelBuilder);
        //}
         
    }

   
}