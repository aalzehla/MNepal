//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Data.Entity;
//using MNepalProject.Models;
//namespace MNepalProject.DAL
//{
//    public class MNepalInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<MNepalDBContext>
//    {
        
//        protected override void Seed(MNepalDBContext context)
//        {
//            Database.SetInitializer<MNepalDBContext>(new MNepalInitializer());
//            var banks = new List<MNBank>
//            {
//            new MNBank{BIN="0000",Name="MNepal",CreatedDate=DateTime.Now},
//            new MNBank{BIN="0001",Name="Nepal Investment Bank",CreatedDate=DateTime.Now},
//            new MNBank{BIN="0002",Name="Laxmi Bank",CreatedDate=DateTime.Now},
//            new MNBank{BIN="0003",Name="KIST Bank",CreatedDate=DateTime.Now}
            
//            //new Bank{Name="KIST Bank",BIN="KIST",CreatedDate=DateTime.Parse("2014-02-04 23:08:36")}
            
//            };

//            banks.ForEach(s => context.MNBanks.Add(s));
//            //banks.ForEach(ss => context.Banks.AddOrUpdate(p => p.BIN, ss));

//            context.SaveChanges();

            
//            /*ProductMaster*/
//            var products = new List<MNProductMaster>
//            {
//                new MNProductMaster{ProductName="Corporate",BankId=1,BIN="0001"},
//                new MNProductMaster{ProductName="Consumer",BankId=1,BIN="0001"}
       
//            };

//            products.ForEach(s => context.MNProductMasters.Add(s));
//            context.SaveChanges();

//            /*LimitMaster*/
//            var limits = new List<MNLimitMaster>
//            {
//                new MNLimitMaster{Span=1,LimitStart=DateTime.Now,Amount=5000,NoOfTransaction=5},
//                new MNLimitMaster{Span=7,LimitStart=DateTime.Now,Amount=10000,NoOfTransaction=5},
//                new MNLimitMaster{Span=12,LimitStart=DateTime.Now,Amount=12000,NoOfTransaction=5}
             
//            };

//            limits.ForEach(s => context.MNLimitMasters.Add(s));
//            context.SaveChanges();

//            /*FeeMaster*/
//            var fees = new List<MNFeeMaster>
//            {
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{},
//                new MNFeeMaster{}

//            };
//            fees.ForEach(s => context.MNFeeMasters.Add(s));
//            context.SaveChanges();


//            /*FeatureMaster*/

//            var featuremasters = new List<MNFeatureMaster> 
//            { 
//                new MNFeatureMaster{FeatureCode="01",FeatureWord="P2P",FeatureGroup="Financial",FeatureName="Person to Person",CanHaveMultiple="False"},
//                new MNFeatureMaster{FeatureCode="02",FeatureWord="LW",FeatureGroup="Financial",FeatureName="Load Wallet",CanHaveMultiple="False"},
//                new MNFeatureMaster{FeatureCode="03",FeatureWord="UWL",FeatureGroup="Financial",FeatureName="Unload Wallet",CanHaveMultiple="False"},
//                new MNFeatureMaster{FeatureCode="04",FeatureWord="ATNT",FeatureGroup="Financial",FeatureName="Air Time NT Top Up",CanHaveMultiple="False"},
//                new MNFeatureMaster{FeatureCode="05",FeatureWord="ATNcell",FeatureGroup="Financial",FeatureName="Air Time NCELL TopUp",CanHaveMultiple="False"},
//                new MNFeatureMaster{FeatureCode="06",FeatureWord="WBP",FeatureGroup="Financial",FeatureName="Water Bill Payment ",CanHaveMultiple="False"},
//                new MNFeatureMaster{FeatureCode="07",FeatureWord="CP",FeatureGroup="Non Financial",FeatureName="Change Pin",CanHaveMultiple="False"},
//                new MNFeatureMaster{FeatureCode="08",FeatureWord="BI",FeatureGroup="Non Financial",FeatureName="Balance Inquiry",CanHaveMultiple="False"},
//                new MNFeatureMaster{FeatureCode="09",FeatureWord="MS",FeatureGroup="Non Financial",FeatureName="Mini Statement",CanHaveMultiple="False"},
//                new MNFeatureMaster{FeatureCode="10",FeatureWord="AI",FeatureGroup="Non Financial",FeatureName="Send Acc Info",CanHaveMultiple="False"}

//            }; 
//            featuremasters.ForEach(s => context.MNFeatureMasters.Add(s));
//            context.SaveChanges();

            


//            /*CustomerMaster*/
//            var customer = new List<MNClient>
//            {
//                new MNClient{ClientCode="C000001",Name="Kamal Manandhar", Address="Kathmandu"},
//                new MNClient{ClientCode="C000002",Name="Prashiddha Joshi", Address="Kathmandu"},
//                new MNClient{ClientCode="C000003",Name="Kopila Shrestha", Address="Kathmandu"},
//                new MNClient{ClientCode="C000004",Name="Sameer Mahat", Address="Kathmandu"},
//                new MNClient{ClientCode="C000005",Name="Krishna Karki", Address="Kathmandu"},
//                new MNClient{ClientCode="C000006",Name="Anu Joshi", Address="Kathmandu"},
//                new MNClient{ClientCode="C000007",Name="Nisha Karki", Address="Kathmandu"},
//                new MNClient{ClientCode="C000008",Name="Pooja Aryal", Address="Kathmandu"}
//            };
//            customer.ForEach(s => context.MNClients.Add(s));
//            context.SaveChanges();


//            /*MNClientConatct*/
//            var clientcontact = new List<MNClientContact>
//            {
//                new MNClientContact{ClientCode="C000001",ContactNumber1="9851235680",ContactNumber2="9801235680"},
//                new MNClientContact{ClientCode="C000002",ContactNumber1="9841003066",ContactNumber2="9801003066"},
//                new MNClientContact{ClientCode="C000003",ContactNumber1="9841731382",ContactNumber2="9801731382"},
//                new MNClientContact{ClientCode="C000004",ContactNumber1="9851235682",ContactNumber2="9801235682"},
//                new MNClientContact{ClientCode="C000005",ContactNumber1="9851235683",ContactNumber2="9801235683"},
//                new MNClientContact{ClientCode="C000006",ContactNumber1="9851235684",ContactNumber2="9801235684"},
//                new MNClientContact{ClientCode="C000007",ContactNumber1="9851235685",ContactNumber2="9801235685"},
//                new MNClientContact{ClientCode="C000008",ContactNumber1="9851235686",ContactNumber2="981235686"}
//            };
//            clientcontact.ForEach(s => context.MNClientContacts.Add(s));
//            context.SaveChanges();


//            /*Agent Masters*/
//            var agentmasters = new List<MNANMMaster>
//            {
//                new MNANMMaster{Name="Ajar Manandhar"}
//            };

//            agentmasters.ForEach(s => context.MNANMMasters.Add(s));
//            context.SaveChanges();


//            /*Status*/
//            var status = new List<MNStatus>
//            {
//                new MNStatus{StatusName="Message Validate", Description="New Request Received From FocusOne and message is in process of validation"},
//                new MNStatus{StatusName="PIN Request",Description="PIN Request In Progress"},
//                new MNStatus{StatusName="PIN Validate",Description="PIN validation In Progress"},
//                new MNStatus{StatusName="Message Response",Description="Message Response to FocusOne"},
//                new MNStatus{StatusName="Generate Token",Description="For Domestic Remittance"}
//            };

//            status.ForEach(s => context.MNStatuses.Add(s));
//            context.SaveChanges();


           
//            /*BankAccountMap*/
//            var bankaccountmap = new List<MNBankAccountMap>
//            {
//                new MNBankAccountMap{ClientCode="C000001",BankAccountNumber="A000001", BIN="0001",IsDefault=true},
//                new MNBankAccountMap{ClientCode="C000001",BankAccountNumber="B000001", BIN="0002",IsDefault=false},
//                new MNBankAccountMap{ClientCode="C000001",BankAccountNumber="C000001", BIN="0003",IsDefault=false},

//                new MNBankAccountMap{ClientCode="C000002",BankAccountNumber="A000002", BIN="0001",IsDefault=true},
//                new MNBankAccountMap{ClientCode="C000002",BankAccountNumber="B000002", BIN="0002",IsDefault=false},

//                new MNBankAccountMap{ClientCode="C000003",BankAccountNumber="C000002", BIN="0003",IsDefault=true}
               
//            };
//            bankaccountmap.ForEach(s => context.MNBankAccountMaps.Add(s));
//            context.SaveChanges();

//            /*AccountInfo*/
//            var accountinfo = new List<MNAccountInfo>
//            {
//                new MNAccountInfo{ClientCode="C000001",WalletNumber="W001", BIN="0001",IsDefault=true},
//                new MNAccountInfo{ClientCode="C000001",WalletNumber="W002", BIN="0002",IsDefault=false},
//                new MNAccountInfo{ClientCode="C000001",WalletNumber="W003", BIN="0003",IsDefault=false},

//                new MNAccountInfo{ClientCode="C000002",WalletNumber="W004", BIN="0001",IsDefault=true},
//                new MNAccountInfo{ClientCode="C000002",WalletNumber="W005", BIN="0002",IsDefault=false},
//                new MNAccountInfo{ClientCode="C000003",WalletNumber="W006", BIN="0003",IsDefault=true}
                
//            };
//            accountinfo.ForEach(s => context.MNAccountInfos.Add(s));
//            context.SaveChanges();

            
//            var agent = new List<MNAgent>
//            {
//                new MNAgent{ANMId=1,Name="Nil Kamal"}

//            };
//            agent.ForEach(s => context.MNAgents.Add(s));
//            context.SaveChanges();

//            var agentfees = new List<MNAgentFeesMaster>
//            {
               
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{},
//                new MNAgentFeesMaster{}
               

//            };
//            agentfees.ForEach(s => context.MNAgentFeesMasters.Add(s));
//            context.SaveChanges();


//            /*SubscribedProduct*/
//            var subscribedproduct = new List<MNSubscribedProduct>
//            {
//                new MNSubscribedProduct{ClientCode="C000001",ProductId=1,IsDefault=true,ProductStatus="Active"}, 

//                new MNSubscribedProduct{ClientCode="C000002",ProductId=1,IsDefault=false,ProductStatus="Active"},
//                new MNSubscribedProduct{ClientCode="C000002",ProductId=2,IsDefault=true,ProductStatus="Active"},
              
//                new MNSubscribedProduct{ClientCode="C000003",ProductId=1,IsDefault=true,ProductStatus="Active"},
//                new MNSubscribedProduct{ClientCode="C000003",ProductId=2,IsDefault=false,ProductStatus="InActive"},

//                new MNSubscribedProduct{ClientCode="C000004",ProductId=1,IsDefault=true,ProductStatus="Active"},
//                new MNSubscribedProduct{ClientCode="C000005",ProductId=1,IsDefault=true,ProductStatus="Active"},
//                new MNSubscribedProduct{ClientCode="C000006",ProductId=1,IsDefault=true,ProductStatus="Active"},
//                new MNSubscribedProduct{ClientCode="C000007",ProductId=1,IsDefault=true,ProductStatus="Active"},
//                new MNSubscribedProduct{ClientCode="C000008",ProductId=1,IsDefault=true,ProductStatus="Active"}
                  
//            };
//            subscribedproduct.ForEach(s => context.MNSubscribedProducts.Add(s));
//            context.SaveChanges();


//            /*Feature*/
//            var feature = new List<MNFeature>
//            { 
                
//                new MNFeature{ProductId=1,FeatureCode="01",SourceBIN="0001",DestinationBIN="0001",LimitId=1, FeeId=1 },  /*Product Id=1 mean Coporate Product for BankId 1*/
//                new MNFeature{ProductId=1,FeatureCode="002",SourceBIN="0001",DestinationBIN="0001",LimitId=2, FeeId=2 }
               
//            };
//            feature.ForEach(s => context.MNFeatures.Add(s));
//            context.SaveChanges();
//        }
//    }
//}