using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MNepalProject.Models;
using MNepalProject.IRepository;
using MNepalProject.Repository;
using MNepalProject.Services;
using MNepalProject.DAL;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MNepalProject.Helper;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Collections.Concurrent;
using System.Text;

using System.IO;
using System.Data.SqlClient;
using MNepalProject.Connection;
using System.Globalization;

namespace MNepalProject.Controllers
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class MNTransactionsController : Controller
    {

        const int STATUS_VERIFICATION_SUCCESS_AT_ECOMM = 1;//
        const int STATUS_VERIFICATION_FAILED_AT_ECOMM = 2;
        const int STATUS_WAITING_FOR_PUMORI_HTTPREPLY = 3;
        const int STATUS_PUMORI_HTTPREPLY_SUCCESS = 4;
        const int STATUS_PUMORI_HTTPREPLY_FAILED = 5;
        //const int STATUS_SUCCESSFUL_REPLY_SENT_TO_PTAS = 6;

        private IMNTransactionMasterRepository mnTransactionMasterRepository;
        MNTransactionLog transactionlog = new MNTransactionLog();
        MNTransactionLogController translog = new MNTransactionLogController();

        static Random rnd = new Random();
        static ConcurrentQueue<int> cq = new ConcurrentQueue<int>();
        static BlockingCollection<int> numbers = new BlockingCollection<int>(10);
        static ConcurrentDictionary<string, uint> wordCount = new ConcurrentDictionary<string, uint>();

        public MNTransactionsController()
        {
            this.mnTransactionMasterRepository = new MNTransactionMasterRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
        }

        // GET: Transactionsss
        public ActionResult Index()
        {
            return View();

        }

        private MNTransactionMaster LogReply(MNTransactionMaster mnTransMaster)
        {
            // save to databasee
            return mnTransMaster;
        }

        public MNTransactionMaster Validate(MNTransactionMaster transaction, string PIN)
        {
            var mnTransactionMaster = new MNTransactionMaster();//for returning
            string Remark1 = string.Empty;

            //string SourceBankName = string.Empty;
            //string DestinationBankName = string.Empty;

            //if (transaction.SourceBIN == "0004")
            //{
            //    SourceBankName = "Nepal Investment Bank LTD";
            //    DestinationBankName = "Nepal Investment Bank LTD";
            //}
            //if (transaction.SourceBIN == "0000")
            //{
            //    SourceBankName = "Pumori Bank";
            //    DestinationBankName = "Pumori Bank";
            //}

            //START FOR DESCRIPTION 1 :ISO Field 125
            string description1;
            const int MaxLength = 49;

            if ((transaction.merchantType == "school") || (transaction.merchantType == "college"))
            {
                description1 = "Bill#" + transaction.billNo + " Name:" + transaction.studName;
                if (description1.Length > MaxLength)
                    description1 = description1.Substring(0, MaxLength);
            }
            else if (transaction.merchantType == "insurance")
            {
                description1 = "PolicyNo#" + transaction.billNo + " InsuredName:" + transaction.studName;
                if (description1.Length > MaxLength)
                    description1 = description1.Substring(0, MaxLength);
            }
            else
            {
                description1 = "BILL PAYMENT"; //"Payment To " + getProductID[0]; //"Bank to Merchant's Bank a/c";
            }

            //FOR W2B
            if (transaction.FeatureCode == "01")
            {
                description1 = "Thaili to bank transfer";
            }
            //FOR B2W (Mobile Banking)
            if (transaction.FeatureCode == "10")
            {
                description1 = "Thaili cash load(" + transaction.DestinationMobile + ")";
            }
            //FOR CASH OUT
            if (transaction.FeatureCode == "51")
            {
                description1 = "Thaili cash load(" + transaction.DestinationMobile + ")";
            }
            //FOR TOPUP
            if (transaction.FeatureCode == "31" || transaction.FeatureCode == "34")
            {
                if (transaction.vid == "2") //NTTOPUP
                {
                    string des = transaction.Description;
                    string[] getProductID = des.Split('-');
                    description1 = "Thaili NT TopUp(" + getProductID[1] + ")";
                }
                if (transaction.vid == "10") //NCELL
                {
                    string des = transaction.Description;
                    string[] getProductID = des.Split('-');
                    description1 = "Thaili Ncell Topup(" + getProductID[1] + ")";
                }
                if (transaction.vid == "7") //NT LANDLINE
                {
                    string des = transaction.Description;
                    string[] getProductID = des.Split('-');
                    description1 = "Thaili NT LandLineADSL TopUp(" + getProductID[1] + ")";
                }
                if (transaction.vid == "1") //NT ADSL
                {
                    string des = transaction.Description;
                    string[] getProductID = des.Split('-');
                    description1 = "Thaili NT ADSL TopUp(" + getProductID[1] + ")";
                }

            }
            //FOR RECHARGE
            if (transaction.FeatureCode == "32" || transaction.FeatureCode == "35")
            {
                if (transaction.vid == "11") //NT GSM RECHARGE
                {
                    string des = transaction.Description;
                    string[] getProductID = des.Split('-');
                    description1 = "Thaili NT Recharge card";
                }
                if (transaction.vid == "12") //NT CDMA RECHARGE
                {
                    string des = transaction.Description;
                    string[] getProductID = des.Split('-');
                    description1 = "Thaili NT CDMA Recharge card";
                }
            }
            //END DESCRIPTION 1 :ISO Field 125


            //START FOR DETAIL IN STATEMENT (Remark column)

            //For Wallet to Wallet 
            if (transaction.FeatureCode == "00")
            {
                Remark1 = "FT (W to W) from " + transaction.SourceMobile + " to " + transaction.DestinationMobile;
            }
            ////For Bank to Wallet (Load Wallet)
            //if (transaction.FeatureCode == "10")
            //{
            //    Remark1 = "Load Wallet (Mobile Banking) from " + SourceBankName + " to " + transaction.DestinationMobile; //transaction.SourceMobile
            //}
            ////For Wallet to Bank (ank Transfer)
            //if (transaction.FeatureCode == "01")
            //{
            //Remark1 = "Bank Transfer from " + transaction.SourceMobile + " to " + mnTransactionMaster.DestinationAccount;
            //    Remark1 = "Bank Transfer to " + transaction.DestinationMobile + (DestinationBankName);
            //}
            ////For Bank to Bank
            //if (transaction.FeatureCode == "11")
            //{
            //    Remark1 = "FT (B to B) from " + transaction.SourceMobile + (SourceBankName) + " to " + transaction.DestinationMobile + (DestinationBankName);
            //}

            //For TOPUP 
            //Goto topup.svc Line 177 

            //For RECHARGE 
            //Goto coupon.svc Line 189 

            //For Merchant 
            //if (transaction.FeatureCode == )
            //{
            //    Remark1 = "Bill payment for" + transaction.M + " to " + transaction.DestinationMobile;
            //}

            //For Cash Out
            if (transaction.FeatureCode == "51")
            {
                Remark1 = "Cash Out through Agent - " + transaction.DestinationMobile;
            }

            //For Cash In
            if (transaction.FeatureCode == "50")
            {
                Remark1 = "Cash In to Wallet from " + transaction.SourceMobile + " to " + transaction.DestinationMobile;
            }

            //END FOR DETAIL IN STATEMENT (Remark column)




            //if (!mnTransactionMasterRepository.IsTraceIDUnique(transaction.TraceId))
            //{
            //    mnTransactionMaster.Response = "Trace ID Repeated";
            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid");
            //    return LogReply(mnTransactionMaster); //log our error reply/success reply before sending to focus one.
            //}
            //else
            //{

            mnTransactionMaster.TraceId = transaction.TraceId;
            mnTransactionMaster.StatusId = STATUS_VERIFICATION_SUCCESS_AT_ECOMM; //validating at EComm Side.
            mnTransactionMaster.RequestChannel = transaction.RequestChannel;
            mnTransactionMaster.Amount = transaction.Amount;
            if (mnTransactionMaster.Amount > 100000)
            {
                mnTransactionMaster.Response = "Limit Exceed";
                mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters invalid");
                return mnTransactionMaster;
            }
            mnTransactionMaster.CreatedDate = transaction.CreatedDate;
            mnTransactionMaster.Description = transaction.Description;
            //}

            Pin p = new Pin();
            if (PIN == "" && transaction.SourceMobile == "9841003066" && transaction.DestinationMobile == "9818713681")
            {
                //Start IVR Call if PIN is blank  ::
                DateTime dateTime = DateTime.Now;
                string isprocessed = "F";
                InsertDataForIVR insert = new InsertDataForIVR();
                bool getreply = insert.InsertData(transaction.TraceId, transaction.SourceMobile, isprocessed, dateTime);
                if (getreply == true)
                {
                    mnTransactionMaster.Response = "Transaction Successful :IVR";
                    mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                }
                else
                {

                }

            }
            else
            {
                if (transaction.FeatureCode == "41")
                {

                }
                else
                {
                    if (transaction.FeatureCode == "51")
                    {
                        if (!p.validPIN(transaction.DestinationMobile, PIN))
                        {
                            mnTransactionMaster.Response = "Invalid PIN";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters invalid");
                            return LogReply(mnTransactionMaster); //log our error reply/success reply before sending to focus one.
                        }
                    }
                    else
                    {
                        if (transaction.ReverseStatus == "T")
                        {
                            description1 = "Reverse (" + transaction.DestinationMobile + ")";
                            Remark1 = transaction.merchantType;// "Reverse for " + transaction.DestinationMobile;
                        }
                        else
                        {
                            if (!p.validPIN(transaction.SourceMobile, PIN))
                            {
                                mnTransactionMaster.Response = "Invalid PIN";
                                mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters invalid");
                                return LogReply(mnTransactionMaster); //log our error reply/success reply before sending to focus one.
                            }
                        }

                    }


                }
            }

            //Transaction Code
            switch (transaction.FeatureCode)
            {
                case "00":
                case "50":
                case "51":
                    //00: Wallet to Wallet Type
                    //50: Cash In
                    //51: Cash out
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transaction.SourceMobile))
                        {

                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("dw", ref mnTransactionMaster, transaction.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        //MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        //MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        //string OriginID = "123456789101112";
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = string.Empty;
                        string desc3 = string.Empty;
                        string Remark = string.Empty;
                        if (mnTransactionMaster.FeatureCode == "00")
                        {
                            Desc1 = "Transfer To Wallet "; //Wallet To Wallet
                            desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                            Remark = Remark1;
                        }
                        else if (mnTransactionMaster.FeatureCode == "50")
                        {
                            Desc1 = "Cash In ";
                            desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                            Remark = Remark1;
                        }
                        else if (mnTransactionMaster.FeatureCode == "51")
                        {
                            Desc1 = "Cash Out ";
                            //Desc1 = description1;
                            desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                            Remark = Remark1;
                        }

                        string desc2 = transaction.DestinationMobile;

                        string stringTokenNo = TraceIdGenerator.GetReqTokenCode() + 1; ;
                        //string Remark = mnTransactionMaster.Description; //"SecretTokenNo:" + stringTokenNo ; //
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transaction.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo, Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, desc2, desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                        /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest); //added c

                        //string test = "please try again test"; //added c
                        //    test = InsertIntoPumoriInTest(mnRequest); //added c
                        //if (test=="true")
                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    waitForReply = ConstructResponse(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                    if (waitForReply != "")
                                    {
                                        /*UnSuccessful*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/


                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Successful*/
                                        else
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = Convert.ToString(HttpStatusCode.OK);
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                            break;
                                        }
                                    }

                                }

                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/


                            //    mnTransactionMaster.Response = "Request will be responded separately";
                            //    mnTransactionMaster.ResponseCode = HttpStatusCode.NoContent.ToString();
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}
                        } //InsertIntoPumoriInTest(mnRequest) //testing
                        else
                        {
                            mnTransactionMaster.Response = "Please try again"; //Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;

                case "01":
                case "30":
                    //01: Wallet to bank
                    //30: Merchant Payment Wallet to Bank.
                    {
                        //01: Wallet to bank
                        //30: Merchant Payment Wallet to Bank.
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("db", ref mnTransactionMaster, transaction.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/


                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();

                        string descthree = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                        string desc1 = "";
                        string desc2 = transaction.DestinationMobile;
                        string desc3 = "";
                        string desc = transaction.Description;
                        string[] getProductID = desc.Split(':');
                        string Remark = "";

                        if (mnTransactionMaster.FeatureCode == "01")
                        {
                            string SourceBankName = string.Empty;
                            string DestinationBankName = string.Empty;

                            if (mnTransactionMaster.SourceBIN == "0004")
                            {
                                SourceBankName = "Nepal Investment Bank LTD";
                            }
                            if (mnTransactionMaster.SourceBIN == "0000")
                            {
                                SourceBankName = "Pumori Bank";
                            }

                            if (mnTransactionMaster.DestinationBIN == "0004")
                            {
                                DestinationBankName = "Nepal Investment Bank LTD";
                            }
                            if (mnTransactionMaster.DestinationBIN == "0000")
                            {
                                DestinationBankName = "Pumori Bank";
                            }

                            Remark1 = "Bank Transfer to " + transaction.DestinationMobile + " (" + DestinationBankName + ")";

                            //desc1 = "Transfer To Bank " + getProductID[0];//"Wallet To Bank";
                            desc1 = description1;
                            desc2 = desc2;
                            desc3 = descthree;
                            Remark = Remark1; //"Bank Transfer from " + transaction.SourceMobile + " to " + mnTransactionMaster.DestinationAccount;
                        }
                        else if (mnTransactionMaster.FeatureCode == "30")
                        {
                            desc1 = description1;
                            //desc1 = "BILL PAYMENT"; // "Payment To " + getProductID[0];//"Wallet To Merchant's Bank a/c";
                            desc2 = getProductID[1];
                            desc3 = descthree;
                            Remark = mnTransactionMaster.Description + " - " + desc1;
                        }
                        else if (mnTransactionMaster.FeatureCode == "31")
                        {
                            //Utility Payment

                            //Desc1 = getProductID[0];
                            //desc1 = "Payment To " + getProductID[0]; //"Wallet To Merchant's Bank a/c (Bill Payment)";
                            desc1 = description1;
                            desc2 = getProductID[1]; //getProductID[1];
                            desc3 = descthree; //getProductID[0];
                            Remark = mnTransactionMaster.Description + " - " + desc1;
                        }

                        //string Remark = mnTransactionMaster.Description + " - " + desc1;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transaction.ReverseStatus;
                        //TraceIdGenerator traceid = new TraceIdGenerator();
                        //RetrievalRef = traceid.GenerateUniqueTraceID();

                        //int len = RetrievalRef.Length;
                        //string lastPart = RetrievalRef.Substring(len - 6, 6);


                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, desc1, desc2, desc3, IsProcessed, Remark, ReverseStatus);
                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    //waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                    waitForReply = ConstructResponse(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                    if (waitForReply != "")
                                    {
                                        /*UnSuccessful*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; /*"Dear Sir/Mam, Request accepted. Will respond separately";*/
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, mnTransactionMaster.Response);
                                            break;

                                        }
                                        /*Successful*/
                                        else //if (waitForReply != "907" || waitForReply != "99" || waitForReply != "114" || waitForReply != "116" || waitForReply != "911")
                                        {
                                            if (waitForReply != "")
                                            {
                                                mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                                /*INSERT TRANSACTION LOG*/
                                                //Send Into TransactionLog
                                                transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                                transactionlog.UpdatedDate = DateTime.Now;
                                                transactionlog.StatusId = mnTransactionMaster.StatusId;
                                                transactionlog.Description = "Transaction Successful";

                                                translog.InsertDataIntoTransactionLog(transactionlog);
                                                /*END:TRANSACTION LOG*/

                                                /*UPDATE TRANSACTION*/
                                                mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                                /*END:TRANSACTION*/

                                                mnTransactionMaster.Response = waitForReply;
                                                mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                                mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                                break;
                                            }
                                        }
                                    }

                                }

                            } while (waitForReply == "");

                            //UnSuccessful
                            //if (waitForReply == "99" || waitForReply == "907" || waitForReply == "114" || waitForReply == "116" || waitForReply == "911")
                            //{
                            //    //if (waitForReply == "")
                            //    //{
                            //        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //        /*INSERT TRANSACTION LOG*/
                            //        //Send Into TransactionLog
                            //        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //        transactionlog.UpdatedDate = DateTime.Now;
                            //        transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //        transactionlog.Description = "Transaction Failed At Pumori";

                            //        translog.InsertDataIntoTransactionLog(transactionlog);
                            //        /*END:TRANSACTION LOG*/

                            //        /*UPDATE TRANSACTION*/
                            //        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //        mnTransactionMaster.Response = waitForReply; //"Dear Sir/Mam, Request accepted. Will respond separately";
                            //        mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, mnTransactionMaster.Response);
                            //    //}
                            //}

                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Request Could not be processed");
                        }
                    }
                    break;

                case "31":
                    //31: Utility Payment Wallet to Bank.
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("db", ref mnTransactionMaster, transaction.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/


                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();

                        string descthree = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                        string desc1 = "";
                        string desc2 = transaction.DestinationMobile;
                        string desc3 = "";
                        string desc = transaction.Description;
                        string[] getProductID = desc.Split('-');

                        if (mnTransactionMaster.FeatureCode == "01")
                        {
                            desc1 = "Transfer To Bank " + getProductID[0];//"Wallet To Bank";
                            desc2 = desc2;
                            desc3 = descthree;
                        }
                        else if (mnTransactionMaster.FeatureCode == "30")
                        {
                            desc1 = "Payment To " + getProductID[0];//"Wallet To Merchant's Bank a/c";
                            desc2 = getProductID[1];
                            desc3 = descthree;
                        }
                        else if (mnTransactionMaster.FeatureCode == "31")
                        {
                            //Utility Payment

                            //Desc1 = getProductID[0];
                            //desc1 = "Payment To " + getProductID[0]; //"Wallet To Merchant's Bank a/c (Bill Payment)";
                            desc1 = description1;
                            desc2 = getProductID[1]; //getProductID[1];
                            desc3 = descthree; //getProductID[0];
                        }
                        //desc1 = "MPOS TOP UP";
                        string Remark = mnTransactionMaster.Description;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transaction.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, desc1, desc2, desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest)) //added for testing -InsertIntoPumoriIn(mnRequest)-
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    //waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                    waitForReply = ConstructResponseForTopUp(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                    if (waitForReply != "")
                                    {
                                        /*UnSuccessful*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; /*"Dear Sir/Mam, Request accepted. Will respond separately";*/
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, mnTransactionMaster.Response);
                                            break;

                                        }
                                        /*Successful*/
                                        else //if (waitForReply != "907" || waitForReply != "99" || waitForReply != "114" || waitForReply != "116" || waitForReply != "911")
                                        {
                                            if (waitForReply != "")
                                            {
                                                mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                                mnTransactionMaster.createdTimeDate = GetTimeStampFromAccountName(mnRequest.RetrievalRef);
                                                /*INSERT TRANSACTION LOG*/
                                                //Send Into TransactionLog
                                                transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                                transactionlog.UpdatedDate = DateTime.Now;
                                                transactionlog.StatusId = mnTransactionMaster.StatusId;
                                                transactionlog.Description = "Transaction Successful";

                                                translog.InsertDataIntoTransactionLog(transactionlog);
                                                /*END:TRANSACTION LOG*/

                                                /*UPDATE TRANSACTION*/
                                                mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                                /*END:TRANSACTION*/

                                                mnTransactionMaster.Response = waitForReply;
                                                mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                                mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                                break;
                                            }
                                        }
                                    }

                                }

                            } while (waitForReply == "");

                            //UnSuccessful
                            //if (waitForReply == "99" || waitForReply == "907" || waitForReply == "114" || waitForReply == "116" || waitForReply == "911")
                            //{
                            //    //if (waitForReply == "")
                            //    //{
                            //        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //        /*INSERT TRANSACTION LOG*/
                            //        //Send Into TransactionLog
                            //        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //        transactionlog.UpdatedDate = DateTime.Now;
                            //        transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //        transactionlog.Description = "Transaction Failed At Pumori";

                            //        translog.InsertDataIntoTransactionLog(transactionlog);
                            //        /*END:TRANSACTION LOG*/

                            //        /*UPDATE TRANSACTION*/
                            //        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //        mnTransactionMaster.Response = waitForReply; //"Dear Sir/Mam, Request accepted. Will respond separately";
                            //        mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, mnTransactionMaster.Response);
                            //    //}
                            //}

                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again!!";//Request could not be processed Please try again!!
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Request Could not be processed");
                        }
                    }
                    break;

                case "32":
                    //32: Coupon Payment Wallet to Bank MOBILE RECHARGE
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        string separateString = transaction.DestinationMobile;
                        string[] getDestinationDetails = separateString.Split(',');

                        if (!LoadStationDetails("db", ref mnTransactionMaster, getDestinationDetails[0]))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;

                        float Amount = mnTransactionMaster.Amount;
                        //string FeeId = mnTransactionMaster.FeeId;
                        string FeeId = " ";
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "";

                        string desc = mnTransactionMaster.Description;
                        string[] getQtyAndDesc = desc.Split(':');
                        //Desc1 = getDestinationDetails[1] + getDestinationDetails[2];

                        //Desc1 = getDestinationDetails[1];
                        //Desc1 = getDestinationDetails[1];
                        Desc1 = ""; //"Payment To " + getQtyAndDesc[0];/
                        string merchantID = GetMerchantIDFromDestNumber(mnTransactionMaster.DestinationMobile);
                        if (merchantID == "13")
                        { //Check if Merchant is DishHome
                            Desc1 = "^^RcHg^^DISH HOME";
                        }
                        if (merchantID == "14")
                        { //Check if Merchant is DishHome
                            Desc1 = "^^RcHg^^BROADLINK";
                        }
                        if (merchantID == "11")
                        { //Check if Merchant is DishHome
                            Desc1 = "NTC MOBILE RECHARGE";
                            //Desc1 = description1;
                        }
                        if (merchantID == "12")
                        { //Check if Merchant is DishHome
                            Desc1 = "CDMA MOBILE RECHARGE";
                            //Desc1 = description1;
                        }
                        //string Remark = getQtyAndDesc[1];
                        //string Remark = getQtyAndDesc[0] + " " + getQtyAndDesc[1];
                        //string Desc2 = getQtyAndDesc[2];

                        string Remark = desc;
                        string Desc2 = desc;
                        string Desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile;//getQtyAndDesc[1];


                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transaction.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Message Validated and Sent to Pumori In";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    //waitForReply = LookINTOResponseTableForCoupon(mnRequest.RetrievalRef);
                                    waitForReply = ConstructResponseForCoupon(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1, mnRequest.Desc2, mnRequest.Desc3);
                                    if (waitForReply != "")
                                    {
                                        /*UnSuccess*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Success*/
                                        else
                                        {

                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                            break;
                                        }

                                    }
                                }

                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Dear Sir/Mam, Request accepted. Will respond separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, mnTransactionMaster.Response);
                            //}
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Request Could not be processed");
                        }
                    }
                    break;

                case "35":
                    //35: Coupon Payment Bank to Bank MOBILE RECHARGE
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        string separateString = transaction.DestinationMobile;
                        string[] getDestinationDetails = separateString.Split(',');

                        if (!LoadStationDetails("db", ref mnTransactionMaster, getDestinationDetails[0]))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        //string FeeId = mnTransactionMaster.FeeId;
                        string FeeId = " ";
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "";

                        string desc = mnTransactionMaster.Description;
                        string[] getQtyAndDesc = desc.Split(',');
                        //Desc1 = getDestinationDetails[1] + getDestinationDetails[2];
                        string[] getDesc = desc.Split(':');

                        //Desc1 = getDestinationDetails[1];
                        Desc1 = ""; //"Payment To " + getQtyAndDesc[0];/
                        string merchantID = GetMerchantIDFromDestNumber(mnTransactionMaster.DestinationMobile);
                        if (merchantID == "13")
                        { //Check if Merchant is DishHome
                            Desc1 = "^^RcHg^^DISH HOME";
                        }
                        if (merchantID == "14")
                        { //Check if Merchant is DishHome
                            Desc1 = "^^RcHg^^BROADLINK";
                        }
                        if (merchantID == "11")
                        { //Check if Merchant is DishHome
                            Desc1 = "NTC MOBILE RECHARGE";
                            //Desc1 = description1;
                        }
                        if (merchantID == "12")
                        { //Check if Merchant is DishHome
                            Desc1 = "CDMA MOBILE RECHARGE";
                            //Desc1 = description1;
                        }
                        /*string Desc2 = getDesc[2];*/ //getQtyAndDesc[0];
                        string Desc2 = desc;
                        string Desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                        //string Remark = getDesc[1];
                        string Remark = desc;

                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transaction.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Message Validated and Sent to Pumori In";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    //waitForReply = LookINTOResponseTableForCoupon(mnRequest.RetrievalRef);
                                    waitForReply = ConstructResponseForCoupon(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1, mnRequest.Desc2, mnRequest.Desc3);
                                    if (waitForReply != "")
                                    {
                                        //UnSuccess
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        //Success
                                        else
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                            break;
                                        }

                                    }
                                }
                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Dear Sir/Mam, Request accepted. Will respond separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, mnTransactionMaster.Response);
                            //}

                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Request Could not be processed");
                        }
                    }
                    break;


                case "10":
                    //10: Bank to Wallet
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("dw", ref mnTransactionMaster, transaction.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;

                        string SourceBankName = string.Empty;
                        string DestinationBankName = string.Empty;

                        if (mnTransactionMaster.SourceBIN == "0004")
                        {
                            SourceBankName = "Nepal Investment Bank LTD";
                        }
                        if (mnTransactionMaster.SourceBIN == "0000")
                        {
                            SourceBankName = "Pumori Bank";
                        }

                        if (mnTransactionMaster.DestinationBIN == "0004")
                        {
                            DestinationBankName = "Nepal Investment Bank LTD";
                        }
                        if (mnTransactionMaster.DestinationBIN == "0000")
                        {
                            DestinationBankName = "Pumori Bank";
                        }

                        if (transaction.ReverseStatus == "T")
                        {
                            Remark1 = Remark1;
                        }
                        else
                        {
                            Remark1 = "Load Wallet(Mobile Banking) from " + SourceBankName + " to " + transaction.DestinationMobile;
                        }

                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        //string Desc1 = "Transfer To Wallet";//Bank To Wallet
                        string Desc1 = description1;
                        string Desc2 = mnTransactionMaster.Description;
                        string Desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                        //string Remark = "";
                        string Remark = Remark1;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transaction.ReverseStatus;

                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    //waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                    waitForReply = ConstructResponse(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);
                                    if (waitForReply != "")
                                    {
                                        /*UnSuccess*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Success*/
                                        else
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                            break;
                                        }
                                    }

                                }

                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Request will be respond separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}

                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again"; //Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;

                case "11":
                case "33":
                    //11: Bank to Bank
                    //33: Merchant Payment Bank to Bank.
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("db", ref mnTransactionMaster, transaction.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        //string Desc1 = "Bank To Bank";
                        //string Desc2 = mnTransactionMaster.Description;

                        string Desc1 = "";
                        string Desc2 = "";
                        string Desc3 = "";
                        string Remark = "";
                        string desc = mnTransactionMaster.Description;
                        string[] getProductID = desc.Split(':');

                        if (mnTransactionMaster.FeatureCode == "11")
                        {
                            string SourceBankName = string.Empty;
                            string DestinationBankName = string.Empty;

                            if (mnTransactionMaster.SourceBIN == "0004")
                            {
                                SourceBankName = "Nepal Investment Bank LTD";
                            }
                            if (mnTransactionMaster.SourceBIN == "0000")
                            {
                                SourceBankName = "Pumori Bank";
                            }

                            if (mnTransactionMaster.DestinationBIN == "0004")
                            {
                                DestinationBankName = "Nepal Investment Bank LTD";
                            }
                            if (mnTransactionMaster.DestinationBIN == "0000")
                            {
                                DestinationBankName = "Pumori Bank";
                            }

                            if (transaction.ReverseStatus == "T")
                            {
                                Remark1 = Remark1;
                            }
                            else
                            {
                                Remark1 = "FT (B to B) from " + transaction.SourceMobile + " (" + SourceBankName + ")" + " to " + transaction.DestinationMobile + " (" + DestinationBankName + ")";
                            }
                            //Remark1 = "FT (B to B) from " + transaction.SourceMobile + " (" + SourceBankName + ")" + " to " + transaction.DestinationMobile + " (" + DestinationBankName + ")";

                            Desc1 = "Transfer To Bank "; //Bank To Bank
                            Desc2 = transaction.DestinationMobile; //desc;
                            Desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                            Remark = Remark1;
                        }

                        else if (mnTransactionMaster.FeatureCode == "33")
                        {
                            Desc1 = description1;
                            // Merchant Payment Bank to Bank
                            //const int MaxLength = 49;
                            //var name = "456546/Rupendra kaji Bahadur Budhacharya";
                            //if (name.Length > MaxLength)
                            //    name = name.Substring(0, MaxLength);
                            //Desc1 = name;//"BILL PAYMENT"; //"Payment To " + getProductID[0]; //"Bank to Merchant's Bank a/c";
                            Desc2 = transaction.DestinationMobile; //desc;
                            Desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                            Remark = desc + description1;
                        }

                        else if (mnTransactionMaster.FeatureCode == "34")
                        {
                            //Utility Payment Bank to Bank

                            //Desc1 = getProductID[0];
                            //Desc1 = "BILL PAYMENT";//"Payment To " + getProductID[0]; //"Bank to Merchant's Bank a/c (Bill Payment)";
                            Desc1 = description1;
                            Desc2 = getProductID[1]; //transaction.DestinationMobile; //desc;
                            Desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile; //getProductID[0];
                            Remark = desc + " - Bill#" + transaction.billNo + " Name:" + transaction.studName;

                        }
                        //string Remark = desc + " - Bill#" + transaction.billNo + " Name:" + transaction.studName;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transaction.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            //do
                            //{
                            for (int i = 0; i < 99999; i++)
                            {
                                //waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                waitForReply = ConstructResponse(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                if (waitForReply != "")
                                {
                                    /*UnSuccess*/
                                    if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                        (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                        (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                        (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                        (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                        (waitForReply == "98") || (waitForReply == "99") ||
                                        (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                        (waitForReply == "911") || (waitForReply == "913"))
                                    {
                                        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                        /*INSERT TRANSACTION LOG*/
                                        //Send Into TransactionLog
                                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                        transactionlog.UpdatedDate = DateTime.Now;
                                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                                        transactionlog.Description = "Transaction Failed At Pumori";

                                        translog.InsertDataIntoTransactionLog(transactionlog);
                                        /*END:TRANSACTION LOG*/

                                        /*UPDATE TRANSACTION*/
                                        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                        /*END:TRANSACTION*/

                                        mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                        mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                        mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                        break;
                                    }
                                    /*Success*/
                                    else
                                    {

                                        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                        /*INSERT TRANSACTION LOG*/
                                        //Send Into TransactionLog
                                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                        transactionlog.UpdatedDate = DateTime.Now;
                                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                                        transactionlog.Description = "Transaction Successful";

                                        translog.InsertDataIntoTransactionLog(transactionlog);
                                        /*END:TRANSACTION LOG*/

                                        /*UPDATE TRANSACTION*/
                                        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                        /*END:TRANSACTION*/

                                        mnTransactionMaster.Response = waitForReply;
                                        mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                        mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                        break;
                                    }
                                }

                            }

                            //} while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Request will be responded separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;

                case "34":
                    //34: Utility Payment Bank to Bank.
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("db", ref mnTransactionMaster, transaction.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        //string Desc1 = "Bank To Bank";
                        //string Desc2 = mnTransactionMaster.Description;

                        string Desc1 = "";
                        string Desc2 = "";
                        string Desc3 = "";
                        string desc = mnTransactionMaster.Description;
                        string[] getProductID = desc.Split('-');

                        if (mnTransactionMaster.FeatureCode == "11")
                        {
                            Desc1 = "Transfer To Bank "; //Bank To Bank
                            Desc2 = transaction.DestinationMobile; //desc;
                            Desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                        }

                        else if (mnTransactionMaster.FeatureCode == "33")
                        {
                            // Merchant Payment Bank to Bank
                            Desc1 = "Payment To " + getProductID[0]; //"Bank to Merchant's Bank a/c";
                            Desc2 = transaction.DestinationMobile; //desc;
                            Desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile;
                        }

                        else if (mnTransactionMaster.FeatureCode == "34")
                        {
                            //Utility Payment Bank to Bank

                            //Desc1 = getProductID[0];
                            //Desc1 = "Payment To " + getProductID[0]; //"Bank to Merchant's Bank a/c (Bill Payment)";
                            Desc1 = description1;
                            Desc2 = getProductID[1]; //transaction.DestinationMobile; //desc;
                            Desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile; //getProductID[0];

                        }
                        string Remark = desc;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transaction.ReverseStatus;

                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            //do
                            //{
                            for (int i = 0; i < 99999; i++)
                            {
                                //waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                waitForReply = ConstructResponseForTopUp(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                if (waitForReply != "")
                                {
                                    /*UnSuccess*/
                                    if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                        (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                        (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                        (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                        (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                        (waitForReply == "98") || (waitForReply == "99") ||
                                        (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                        (waitForReply == "911") || (waitForReply == "913"))
                                    {
                                        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                        /*INSERT TRANSACTION LOG*/
                                        //Send Into TransactionLog
                                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                        transactionlog.UpdatedDate = DateTime.Now;
                                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                                        transactionlog.Description = "Transaction Failed At Pumori";

                                        translog.InsertDataIntoTransactionLog(transactionlog);
                                        /*END:TRANSACTION LOG*/

                                        /*UPDATE TRANSACTION*/
                                        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                        /*END:TRANSACTION*/

                                        mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                        mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                        mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                        break;
                                    }
                                    /*Success*/
                                    else
                                    {

                                        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                        mnTransactionMaster.createdTimeDate = GetTimeStampFromAccountName(mnRequest.RetrievalRef);
                                        /*INSERT TRANSACTION LOG*/
                                        //Send Into TransactionLog
                                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                        transactionlog.UpdatedDate = DateTime.Now;
                                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                                        transactionlog.Description = "Transaction Successful";

                                        translog.InsertDataIntoTransactionLog(transactionlog);
                                        /*END:TRANSACTION LOG*/

                                        /*UPDATE TRANSACTION*/
                                        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                        /*END:TRANSACTION*/

                                        mnTransactionMaster.Response = waitForReply;
                                        mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                        mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                        break;
                                    }
                                }

                            }

                            //} while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Request will be responded separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;
                case "20":
                    //20: Balance Query Wallet
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        //source bin and destination bin same for bq
                        mnTransactionMaster.DestinationBIN = mnTransactionMaster.SourceBIN;
                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "Balance Inquery For Wallet Account";
                        string Desc2 = mnTransactionMaster.Description;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";

                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, IsProcessed);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/


                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                    if (waitForReply != "")
                                    {
                                        /*UnSuccess*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.NoContent.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Success*/
                                        else
                                        {

                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, mnTransactionMaster.Response);
                                            break;
                                        }
                                    }
                                }
                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Request will be responded separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;
                case "22":
                    //balance query bank
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        //source bin and destination bin same for bq
                        mnTransactionMaster.DestinationBIN = mnTransactionMaster.SourceBIN;
                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "Balance Inquery For Bank Account";
                        string Desc2 = mnTransactionMaster.Description;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, IsProcessed);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                    if (waitForReply != "")
                                    {
                                        /*UnSuccess*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Success*/
                                        else
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, mnTransactionMaster.Response);
                                            break;
                                        }
                                    }
                                }

                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Request will be responded separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}

                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;
                case "21":
                    //ministatement for wallet
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        //source bin and destination bin same for bq
                        mnTransactionMaster.DestinationBIN = mnTransactionMaster.SourceBIN;
                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "MiniStatement for Wallet";
                        string Desc2 = mnTransactionMaster.Description;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo, Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, IsProcessed);
                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            //do
                            //{
                            for (int i = 0; i < 99999; i++)
                            {
                                waitForReply = LookINTOResponseTableForMiniStatement(mnRequest.RetrievalRef);
                                if (waitForReply != "[]")
                                {
                                    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                    /*INSERT TRANSACTION LOG*/
                                    //Send Into TransactionLog
                                    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                    transactionlog.UpdatedDate = DateTime.Now;
                                    transactionlog.StatusId = mnTransactionMaster.StatusId;
                                    transactionlog.Description = "Transaction Successful";

                                    translog.InsertDataIntoTransactionLog(transactionlog);
                                    /*END:TRANSACTION LOG*/

                                    /*UPDATE TRANSACTION*/
                                    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                    /*END:TRANSACTION*/

                                    mnTransactionMaster.Response = waitForReply;
                                    mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                    mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                    break;
                                }
                            }

                            if (waitForReply == "")
                            {
                                mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                /*INSERT TRANSACTION LOG*/
                                //Send Into TransactionLog
                                transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                transactionlog.UpdatedDate = DateTime.Now;
                                transactionlog.StatusId = mnTransactionMaster.StatusId;
                                transactionlog.Description = "Transaction Failed At Pumori";

                                translog.InsertDataIntoTransactionLog(transactionlog);
                                /*END:TRANSACTION LOG*/

                                /*UPDATE TRANSACTION*/
                                mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                /*END:TRANSACTION*/

                                mnTransactionMaster.Response = "Request will be responded separately";
                                mnTransactionMaster.ResponseCode = HttpStatusCode.NoContent.ToString();
                                mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            }
                            //} while (waitForReply == "");
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again"; //Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;
                case "23":
                    //ministatement for bank
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        //source bin and destination bin same for bq
                        mnTransactionMaster.DestinationBIN = mnTransactionMaster.SourceBIN;
                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "MiniStatement for Bank";
                        string Desc2 = mnTransactionMaster.Description;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo, Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, IsProcessed);
                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            for (int i = 0; i < 99999; i++)
                            {
                                waitForReply = LookINTOResponseTableForMiniStatement(mnRequest.RetrievalRef);
                                if (waitForReply != "[]")
                                {
                                    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                    /*INSERT TRANSACTION LOG*/
                                    //Send Into TransactionLog
                                    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                    transactionlog.UpdatedDate = DateTime.Now;
                                    transactionlog.StatusId = mnTransactionMaster.StatusId;
                                    transactionlog.Description = "Transaction Successful";

                                    translog.InsertDataIntoTransactionLog(transactionlog);
                                    /*END:TRANSACTION LOG*/

                                    /*UPDATE TRANSACTION*/
                                    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                    /*END:TRANSACTION*/

                                    mnTransactionMaster.Response = waitForReply;
                                    mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                    mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                    break;
                                }
                            }

                            if (waitForReply == "")
                            {
                                mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                /*INSERT TRANSACTION LOG*/
                                //Send Into TransactionLog
                                transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                transactionlog.UpdatedDate = DateTime.Now;
                                transactionlog.StatusId = mnTransactionMaster.StatusId;
                                transactionlog.Description = "Transaction Failed At Pumori";

                                translog.InsertDataIntoTransactionLog(transactionlog);
                                /*END:TRANSACTION LOG*/

                                /*UPDATE TRANSACTION*/
                                mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                /*END:TRANSACTION*/

                                mnTransactionMaster.Response = "Request will be responded separately";
                                mnTransactionMaster.ResponseCode = HttpStatusCode.NoContent.ToString();
                                mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            }
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;

                case "40":
                    //Remittance Token
                    {
                        //Remittance Token
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        string destinationMobile = transaction.DestinationMobile;
                        if (destinationMobile.Length.ToString() == "10")
                        {
                            mnTransactionMaster.DestinationMobile = transaction.DestinationMobile;
                            mnTransactionMaster.DestinationAccount = null;
                            mnTransactionMaster.DestinationBIN = null;
                            mnTransactionMaster.DestinationBranchCode = null;
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Destination Mobile Length Insufficient";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Destination Mobile Length Insufficient");
                            break;
                        }

                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetailsWithOutDestBIN(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/

                        GenerateToken getToken = new GenerateToken();
                        string token = getToken.GetUniqueKey();
                        mnTransactionMaster.DestinationAccount = token;
                        mnTransactionMaster.Description = transaction.Description;    //secret code

                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        //Generate token and reply

                        string msgConstruct = "Token requested by " + transaction.SourceMobile + " for transferring amount NPR " + transaction.Amount + " to " + transaction.DestinationMobile;
                        var v = new { AmounttransferredBalance = Convert.ToDecimal(transaction.Amount).ToString("#,##0.00"), RequestedToken = token, message = msgConstruct };
                        string json = JsonConvert.SerializeObject(v);

                        mnTransactionMaster.Response = json;
                        mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString(); //200
                        mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Reply Token Number");

                        /*END:TRANSACTION LOG*/

                    }
                    break;
                case "41":
                    //Remittance Redeem
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;

                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transaction.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("dw", ref mnTransactionMaster, transaction.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "";
                        string[] splitData = mnTransactionMaster.Description.Split(',');

                        if (mnTransactionMaster.FeatureCode == "41")
                        {
                            Desc1 = "Remittance Redeem of User:" + splitData[3];
                        }

                        string Desc2 = "SecretCode:" + splitData[0] + "," + "Token:" + splitData[1];
                        string Desc3 = transaction.SourceMobile + " - " + transaction.DestinationMobile; //splitData[2];
                        string Remark = ""; // "SecretCode:" + splitData[0] + "," + "Token:" + splitData[1]; 

                        string IsProcessed = "F";
                        string ReverseStatus = transaction.ReverseStatus;

                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);

                                    if (waitForReply != "")
                                    {
                                        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                        /*INSERT TRANSACTION LOG*/
                                        //Send Into TransactionLog
                                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                        transactionlog.UpdatedDate = DateTime.Now;
                                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                                        transactionlog.Description = "Transaction Successful";

                                        translog.InsertDataIntoTransactionLog(transactionlog);
                                        /*END:TRANSACTION LOG*/

                                        /*UPDATE TRANSACTION*/
                                        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                        /*END:TRANSACTION*/

                                        string msgConstruct = "Token Redeemed Successfully of amount NPR" + transaction.Amount;
                                        var v = new { AmounttransferredBalance = Convert.ToDecimal(transaction.Amount).ToString("#,##0.00"), message = msgConstruct };
                                        string json = JsonConvert.SerializeObject(v);

                                        mnTransactionMaster.Response = json;
                                        mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                        mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                        break;
                                    }
                                }
                                if (waitForReply == "")
                                {
                                    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                    /*INSERT TRANSACTION LOG*/
                                    //Send Into TransactionLog
                                    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                    transactionlog.UpdatedDate = DateTime.Now;
                                    transactionlog.StatusId = mnTransactionMaster.StatusId;
                                    transactionlog.Description = "Transaction Failed At Pumori";

                                    translog.InsertDataIntoTransactionLog(transactionlog);
                                    /*END:TRANSACTION LOG*/

                                    /*UPDATE TRANSACTION*/
                                    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                    /*END:TRANSACTION*/

                                    mnTransactionMaster.Response = "Dear Sir/Mam, Request accepted. Will respond separately";
                                    mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                    mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Dear Sir/Mam, Request accepted. Will respond separately");
                                }

                            } while (waitForReply == "");
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Request could not be processed");
                        }
                    }
                    break;

                default:
                    {
                        mnTransactionMaster.Response = "";
                        mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString(); //500
                        mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Invalid Parameter");
                    }
                    break;

            }

            return mnTransactionMaster;
        }


        #region Load Wallet Validation
        public MNTransactionMaster LoadWalletValidate(MNTransactionMaster transaction, string PIN)
        {

            var mnTransactionMaster = new MNTransactionMaster();//for returning


            mnTransactionMaster.TraceId = transaction.TraceId;
            mnTransactionMaster.StatusId = STATUS_VERIFICATION_SUCCESS_AT_ECOMM; //validating at EComm Side.
            mnTransactionMaster.RequestChannel = transaction.RequestChannel;
            mnTransactionMaster.Amount = transaction.Amount;
            if (mnTransactionMaster.Amount > 100000)
            {
                mnTransactionMaster.Response = "Limit Exceed";
                mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters invalid");
                return mnTransactionMaster;
            }
            mnTransactionMaster.CreatedDate = transaction.CreatedDate;
            mnTransactionMaster.Description = transaction.Description;


            //Pin p = new Pin();
            //if (PIN == "" && transaction.SourceMobile == "9841003066" && transaction.DestinationMobile == "9818713681")
            //{
            //    //Start IVR Call if PIN is blank  ::
            //    DateTime dateTime = DateTime.Now;
            //    string isprocessed = "F";
            //    InsertDataForIVR insert = new InsertDataForIVR();
            //    bool getreply = insert.InsertData(transaction.TraceId, transaction.SourceMobile, isprocessed, dateTime);
            //    if (getreply == true)
            //    {
            //        mnTransactionMaster.Response = "Transaction Successful :IVR";
            //        mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
            //    }
            //    else
            //    {

            //    }
            //}
            //else
            //{
            //    if (transaction.FeatureCode == "41")
            //    {

            //    }
            //    else
            //    {
            //        if (!p.validPIN(transaction.SourceMobile, PIN))
            //        {
            //            mnTransactionMaster.Response = "Invalid PIN";
            //            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
            //            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters invalid");
            //            return LogReply(mnTransactionMaster); //log our error reply/success reply before sending to focus one.
            //        }
            //    }
            //}

            //Transaction Code
            switch (transaction.FeatureCode)
            {
                case "00":
                    {
                        mnTransactionMaster.FeatureCode = transaction.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        //if (!LoadStationDetails("sw", ref mnTransactionMaster, transaction.SourceMobile))
                        //{

                        //    mnTransactionMaster.Response = "Invalid Source User";
                        //    mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                        //    mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                        //    break;
                        //}

                        mnTransactionMaster.SourceBIN = "0000"; //control bank account sourceBin
                        mnTransactionMaster.SourceBranchCode = "001";
                        mnTransactionMaster.SourceAccount = getControlBankAccount("0000");
                        mnTransactionMaster.ProductId = 1;
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("dw", ref mnTransactionMaster, transaction.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/

                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        //MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        //MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/

                        /*MNREQUEST*/
                        string OriginID = "123456789101112";
                        // string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = "001";
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;
                        //  string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = string.Empty;
                        string desc3 = string.Empty;
                        string SourceBankName = string.Empty;
                        string DestinationBankName = string.Empty;
                        string Remark1 = "";

                        if (mnTransactionMaster.SourceBIN == "0004")
                        {
                            SourceBankName = "Nepal Investment Bank LTD";
                        }
                        if (mnTransactionMaster.SourceBIN == "0000")
                        {
                            SourceBankName = "Pumori Bank";
                        }

                        if (mnTransactionMaster.DestinationBIN == "0004")
                        {
                            DestinationBankName = "Nepal Investment Bank LTD";
                        }
                        if (mnTransactionMaster.DestinationBIN == "0000")
                        {
                            DestinationBankName = "Pumori Bank";
                        }

                        Remark1 = "Load Wallet(E-Banking) from Nepal Investment Bank LTD to " + transaction.DestinationMobile;

                        if (mnTransactionMaster.FeatureCode == "00")
                        {
                            Desc1 = "Transfer To Wallet "; //Wallet To Wallet
                            desc3 = transaction.DestinationMobile;
                        }

                        string desc2 = transaction.DestinationMobile;
                        string stringTokenNo = TraceIdGenerator.GetReqTokenCode() + 1; ;
                        //string Remark = mnTransactionMaster.Description; //"SecretTokenNo:" + stringTokenNo ; //
                        string Remark = Remark1;

                        string IsProcessed = "F";
                        string ReverseStatus = transaction.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, desc2, desc3, IsProcessed, Remark, ReverseStatus);

                        /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest); //added c

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    waitForReply = ConstructResponse(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                    if (waitForReply != "")
                                    {
                                        /*UnSuccessful*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Successful*/
                                        else
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = "Success";//waitForReply;
                                            mnTransactionMaster.ResponseCode = Convert.ToString(HttpStatusCode.OK);
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                            break;
                                        }
                                    }
                                }

                            } while (waitForReply == "");
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again"; //Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;
                default:
                    {
                        mnTransactionMaster.Response = "";
                        mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString(); //500
                        mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Invalid Parameter");
                    }
                    break;
            }
            return mnTransactionMaster;
        }
        #endregion

        #region Control Bank Retreive
        public string getControlBankAccount(string BankCode)
        {
            string result = "";
            string formattedResult = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    string sql = "select * from MNBankTable where BankCode='" + BankCode + "'";
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    {

                        using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {

                                result = rdr["ControlAc"].ToString();

                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }
        #endregion


        public string LookINTOResponseTableForMiniStatement(string RetrievalRef)
        {
            int MINIStatement_Len = 95;//38
            const int MiniStatement_row_len = 19;//19
            string miniStatementReply = "";
            JArray final_ministatement = new JArray();

            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM MNResponse where RetrievalRef = '" + RetrievalRef + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            miniStatementReply = rdr["MiniStmntRec"].ToString();
                            if (miniStatementReply != "")
                            {
                                for (int i = 0; i < MINIStatement_Len; i = i + MiniStatement_row_len)
                                {
                                    final_ministatement.Add(rearrange(miniStatementReply.Substring(i, MiniStatement_row_len)));
                                }
                            }
                        }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                miniStatementReply = ex.ToString();
            }
            //return final_ministatement.ToString();
            return JsonConvert.SerializeObject(final_ministatement, Formatting.Indented);
        }



        public JObject rearrange(string ministatement_row)
        {
            string yy = ministatement_row.Substring(0, 2);
            string mm = ministatement_row.Substring(2, 2);
            string dd = ministatement_row.Substring(4, 2);
            string formateddate = dd + "/" + mm + "/" + yy;
            string amount = ministatement_row.Substring(6, 12).Trim();
            amount = amount.TrimStart('0');
            string drcr = ministatement_row.Substring(18, 1).Trim();
            MiniStatement m = new MiniStatement(formateddate, amount, drcr);
            //var v = new { date = formateddate, amount = amount, transactionType = drcr };
            JObject o = new JObject();
            o["date"] = formateddate;
            o["amount"] = amount;
            o["transactionType"] = drcr;
            //string json = JsonConvert.SerializeObject(v);
            return o;
        }

        private bool LoadFeatureDetails(ref MNTransactionMaster mnTransactionMaster)
        {
            bool result = false;
            FeatureDetails featureDetail = new FeatureDetails(mnTransactionMaster.ProductId, mnTransactionMaster.FeatureCode, mnTransactionMaster.SourceBIN, mnTransactionMaster.DestinationBIN);

            if (featureDetail.feature != null)
            {
                mnTransactionMaster.FeeId = featureDetail.feature.FeeId.ToString();
                // Limit
                var limit = featureDetail.feature.LimitId;
                //var agent = featureDetail.feature.ag
                result = true;
            }
            return result;
        }

        private bool LoadFeatureDetailsWithOutDestBIN(ref MNTransactionMaster mnTransactionMaster)
        {
            bool result = false;
            FeatureDetails featureDetail = new FeatureDetails(mnTransactionMaster.ProductId, mnTransactionMaster.FeatureCode, mnTransactionMaster.SourceBIN);

            if (featureDetail.feature != null)
            {
                mnTransactionMaster.FeeId = featureDetail.feature.FeeId.ToString();
                // Limit
                var limit = featureDetail.feature.LimitId;
                //var agent = featureDetail.feature.ag
                result = true;
            }
            return result;
        }


        private bool LoadStationDetails(string stationType, ref MNTransactionMaster mnTransactionMaster, string mobile)
        {
            bool result = true;
            MNClientContact mnClientContact = new MNClientContact(null, mobile, null);
            ClientsDetails ClientDetails = new ClientsDetails(mnClientContact);
            switch (stationType)
            {
                case "sw":
                    {

                        if (ClientDetails.accountInfo == null)
                        {
                            mnTransactionMaster.Response = "Source Default Wallet Not Found.";
                            result = false;
                            break;
                        }
                        else
                        {
                            mnTransactionMaster.SourceMobile = ClientDetails.clientContact.ContactNumber1;
                            mnTransactionMaster.SourceAccount = ClientDetails.accountInfo.WalletNumber;
                            mnTransactionMaster.SourceBIN = ClientDetails.accountInfo.BIN;
                            mnTransactionMaster.SourceBranchCode = ClientDetails.accountInfo.BranchCode;
                        }


                        if (ClientDetails.subscribedActiveProduct == null)
                        {
                            mnTransactionMaster.Response = "Product is inactive or not default";
                            result = false;
                            break;
                        }
                        else
                        {
                            mnTransactionMaster.ProductId = ClientDetails.subscribedActiveProduct.ProductId;
                        }
                    }
                    break;
                case "sb":
                    {

                        if (ClientDetails.bankAccountMap == null)
                        {
                            mnTransactionMaster.Response = "Source Default Bank Account Not Found.";
                            result = false;
                            break;
                        }
                        else
                        {
                            mnTransactionMaster.SourceMobile = ClientDetails.clientContact.ContactNumber1;
                            mnTransactionMaster.SourceAccount = ClientDetails.bankAccountMap.BankAccountNumber;
                            mnTransactionMaster.SourceBIN = ClientDetails.bankAccountMap.BIN;
                            mnTransactionMaster.SourceBranchCode = ClientDetails.bankAccountMap.BranchCode;
                        }

                        if (ClientDetails.subscribedActiveProduct == null)
                        {
                            mnTransactionMaster.Response = "Product is inactive or not default";
                            result = false;
                            break;
                        }
                        else
                        {
                            mnTransactionMaster.ProductId = ClientDetails.subscribedActiveProduct.ProductId;
                        }
                    }
                    break;


                case "dw":
                    {
                        /*
                            int TraceId
                            sourcechannel
                            [Column(TypeName = "VARCHAR")]
                            [StringLength(2)]
                            public string FeatureCode { get; set; }
                            [Column(TypeName = "VARCHAR")]
                            [StringLength(10)]
                            public string SourceMobile { get; set; }
                            [Column(TypeName = "VARCHAR")]
                            [StringLength(20)]
                            public string SourceAccount { get; set; }
                            [Column(TypeName = "VARCHAR")]
                            [StringLength(4)]
                            public string SourceBIN { get; set; }
                     
                            [StringLength(10)]
                            public string DestinationMobile { get; set; }
                            [StringLength(20)]
                            public string DestinationAccount { get; set; }
                            [StringLength(4)]
                            public string DestinationBIN { get; set; }
                             */


                        if (ClientDetails.accountInfo == null)
                        {
                            mnTransactionMaster.Response = "Destiny Default Wallet Account Not Found.";
                            result = false;
                            break;
                        }
                        else
                        {
                            mnTransactionMaster.DestinationMobile = ClientDetails.clientContact.ContactNumber1;
                            mnTransactionMaster.DestinationAccount = ClientDetails.accountInfo.WalletNumber;
                            mnTransactionMaster.DestinationBIN = ClientDetails.accountInfo.BIN;
                            mnTransactionMaster.DestinationBranchCode = ClientDetails.accountInfo.BranchCode;
                        }
                    }
                    break;
                case "db":
                    {

                        if (ClientDetails.bankAccountMap == null)
                        {
                            mnTransactionMaster.Response = "Destiny Default Bank Account Not Found.";
                            result = false;
                            break;
                        }
                        else
                        {
                            mnTransactionMaster.DestinationMobile = ClientDetails.clientContact.ContactNumber1;
                            mnTransactionMaster.DestinationAccount = ClientDetails.bankAccountMap.BankAccountNumber;
                            mnTransactionMaster.DestinationBIN = ClientDetails.bankAccountMap.BIN;
                            mnTransactionMaster.DestinationBranchCode = ClientDetails.bankAccountMap.BranchCode;
                        }
                    }
                    break;
                default:
                    break;
            }
            return result;
        }


        // To be removed
        private bool InsertIntoPumoriIn(MNRequest request)
        {
            bool result = true;
            //var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            try
            {
                ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, request);

                //    dataContext.Insert(request);

                int ret;

                request.Mode = "IREQ";
                using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
                {
                    try
                    {
                        sqlCon.Open();
                        using (SqlCommand sqlCmd = new SqlCommand("[s_MNRequestInsert]", sqlCon))
                        {
                            sqlCmd.CommandType = CommandType.StoredProcedure;
                            sqlCmd.Parameters.Add(new SqlParameter("@OriginID", request.OriginID));
                            sqlCmd.Parameters.Add("@OriginType", SqlDbType.VarChar, 20).Value = request.OriginType;
                            sqlCmd.Parameters.Add("@ServiceCode", SqlDbType.VarChar).Value = request.ServiceCode;
                            sqlCmd.Parameters.Add("@SourceBankCode", SqlDbType.VarChar).Value = request.SourceBankCode;
                            sqlCmd.Parameters.Add("@SourceBranchCode", SqlDbType.VarChar, 24).Value = request.SourceBranchCode;
                            sqlCmd.Parameters.Add("@SourceAccountNo", SqlDbType.VarChar).Value = request.SourceAccountNo;
                            sqlCmd.Parameters.Add("@DestBankCode", SqlDbType.VarChar).Value = request.DestBankCode;
                            sqlCmd.Parameters.Add("@DestBranchCode", SqlDbType.VarChar, 24).Value = request.DestBranchCode;
                            sqlCmd.Parameters.Add("@DestAccountNo", SqlDbType.VarChar).Value = request.DestAccountNo;
                            sqlCmd.Parameters.Add("@Amount", SqlDbType.Money).Value = request.Amount;
                            sqlCmd.Parameters.Add("@FeeId", SqlDbType.VarChar, 8).Value = request.FeeId;
                            sqlCmd.Parameters.Add("@TraceNo", SqlDbType.VarChar).Value = request.TraceNo;
                            sqlCmd.Parameters.Add("@TranDate", SqlDbType.Date, 30).Value = request.TranDate;
                            sqlCmd.Parameters.Add("@RetrievalRef", SqlDbType.VarChar).Value = request.RetrievalRef;
                            sqlCmd.Parameters.Add("@Desc1", SqlDbType.VarChar).Value = request.Desc1;
                            sqlCmd.Parameters.Add("@Desc2", SqlDbType.VarChar, 120).Value = request.Desc2;
                            sqlCmd.Parameters.Add("@Desc3", SqlDbType.VarChar).Value = request.Desc3;
                            sqlCmd.Parameters.Add("@ReversalStatus", SqlDbType.VarChar).Value = request.ReversalStatus;
                            sqlCmd.Parameters.Add("@OTraceNo", SqlDbType.VarChar, 20).Value = "";// request.OTraceNo;
                            sqlCmd.Parameters.Add("@OTranDateTime", SqlDbType.VarChar).Value = "";//request.OTranDateTime;
                            //sqlCmd.Parameters.Add("@IsProcessed", SqlDbType.VarChar).Value = request.IsProcessed;
                            sqlCmd.Parameters.Add("@Status", SqlDbType.VarChar, 20).Value = "";// request.Status;
                            sqlCmd.Parameters.Add("@FromSMS", SqlDbType.VarChar).Value = "";//request.FromSMS;
                            sqlCmd.Parameters.Add("@Remark", SqlDbType.VarChar).Value = request.Remark;
                            sqlCmd.Parameters.Add("@mode", SqlDbType.VarChar).Value = request.Mode;
                            sqlCmd.Parameters.Add("@RegIDOut", SqlDbType.Int);
                            sqlCmd.Parameters["@RegIDOut"].Direction = ParameterDirection.Output;
                            ret = sqlCmd.ExecuteNonQuery();
                            if (request.Mode.Equals("IREQ", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ret = Convert.ToInt32(sqlCmd.Parameters["@RegIDOut"].Value);
                                result = true;

                            }

                        }
                        sqlCon.Close();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;

            }
            return result;
        }

        private string InsertIntoPumoriInTest(MNRequest request)
        {
            var result = "true";
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            try
            {
                //ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, request);
                var stage1 = Task.Run(() =>
                {
                    dataContext.Insert(request);
                });
                Task.WaitAll(stage1);

            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }


        private string InsertIntoPumoriInDirect(MNRequest request)
        {
            string result = "ok";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;
            MNRequest pumoriIn = new MNRequest();
            //pumoriIn.OriginID = "123456789101112";
            pumoriIn.OriginID = request.OriginID;
            pumoriIn.OriginType = "HTTP";
            pumoriIn.ServiceCode = request.ServiceCode;
            pumoriIn.SourceAccountNo = request.SourceAccountNo;
            pumoriIn.SourceBankCode = request.SourceBankCode;
            pumoriIn.DestBankCode = request.DestBankCode;
            pumoriIn.DestAccountNo = request.DestAccountNo;
            pumoriIn.Amount = request.Amount;
            pumoriIn.FeeId = request.FeeId;
            pumoriIn.TraceNo = request.TraceNo;
            pumoriIn.TranDate = DateTime.Now;


            string sql = "Insert into MNRequest(OriginID,OriginType,ServiceCode,SourceAccountNo,SourceBankCode,DestBankCode,DestAccountNo,Amount,FeeId,TraceNo,TranDate,RetrievalRef) values ('";
            sql += pumoriIn.OriginID + "','";
            sql += pumoriIn.OriginType + "','";
            sql += pumoriIn.ServiceCode + "','";
            sql += pumoriIn.SourceAccountNo + "','";
            sql += pumoriIn.SourceBankCode + "','";
            sql += pumoriIn.DestBankCode + "','";
            sql += pumoriIn.DestAccountNo + "','";
            sql += pumoriIn.Amount + "','";
            sql += pumoriIn.FeeId + "','";
            sql += pumoriIn.TraceNo + "','";
            sql += pumoriIn.TranDate + "','";
            sql += pumoriIn.TraceNo;
            sql += "')";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                        command.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }

        public string ConstructResponse(string RetrievalRef, float tamount, string dest, string desc)
        {
            string Balance = "";
            string json = "";
            for (int i = 0; i < 1; i++)
            {
                Balance = LookINTOResponseTable(RetrievalRef);

                if (Balance == "0.00")
                {

                    string msgConstruct = "Amount was successfully transferred from " + desc + " 977" + dest + " with NPR" + Convert.ToDecimal(tamount).ToString("#,##0.00") + ".";
                    var v = new { AmounttransferredBalance = Convert.ToDecimal(tamount).ToString("#,##0.00"), availableBalance = Balance, message = msgConstruct };
                    json = JsonConvert.SerializeObject(v);
                    break;
                }
                else if (Balance != "")
                {
                    if ((Balance == "90") || (Balance == "91") || (Balance == "92") || (Balance == "94") ||
                        (Balance == "95") || (Balance == "98") || (Balance == "99"))
                    {
                        var v = new { errorcode = Balance };
                        json = Balance; //JsonConvert.SerializeObject(v);
                        break;
                    }
                    if ((Balance == "111") || (Balance == "114") || (Balance == "115") || (Balance == "116") ||
                        (Balance == "119") || (Balance == "121") || (Balance == "163"))
                    {
                        var v = new { errorcode = Balance };
                        json = Balance; //JsonConvert.SerializeObject(v);
                        break;
                    }
                    if ((Balance == "180") || (Balance == "181") || (Balance == "182") || (Balance == "183") ||
                         (Balance == "184") || (Balance == "185") || (Balance == "186") || (Balance == "187") ||
                         (Balance == "188") || (Balance == "189") || (Balance == "190") || (Balance == "800"))
                    {
                        var v = new { errorcode = Balance };
                        json = Balance; // JsonConvert.SerializeObject(v);
                        break;
                    }
                    if ((Balance == "902") || (Balance == "904") || (Balance == "906") || (Balance == "907"))
                    {
                        var v = new { errorcode = Balance };
                        json = Balance; //JsonConvert.SerializeObject(v);
                        break;
                    }
                    if ((Balance == "909") || (Balance == "911") || (Balance == "913"))
                    {
                        var v = new { errorcode = Balance };
                        json = Balance; // JsonConvert.SerializeObject(v);
                        break;
                    }
                    if ((Balance != "99") || (Balance != "114") || (Balance != "116") ||
                        (Balance != "902") || (Balance != "904") || (Balance != "906") || (Balance != "907") || (Balance != "909") || (Balance != "911") || (Balance != "913"))
                    {
                        string msgConstruct = "Amount was successfully transferred from " + desc + " 977" + dest + " with NPR" + Convert.ToDecimal(tamount).ToString("#,##0.00") + ".";
                        var v = new { AmounttransferredBalance = Convert.ToDecimal(tamount).ToString("#,##0.00"), availableBalance = Balance, message = msgConstruct };
                        json = JsonConvert.SerializeObject(v);
                        break;
                    }
                }
            }
            return json;
        }
        public string ConstructResponseForTopUp(string RetrievalRef, float tamount, string dest, string desc)
        {
            string Balance = "";
            string json = "";
            for (int i = 0; i < 1; i++)
            {
                Balance = LookINTOResponseTable(RetrievalRef);

                if (Balance == "0.00")
                {

                    string msgConstruct = "Amount was successfully transferred from " + desc + " with NPR" + Convert.ToDecimal(tamount).ToString("#,##0.00") + "."; //+ " 977" + dest
                    var v = new { AmounttransferredBalance = Convert.ToDecimal(tamount).ToString("#,##0.00"), availableBalance = Balance, message = msgConstruct };
                    json = JsonConvert.SerializeObject(v);
                    break;
                }
                else if (Balance != "")
                {
                    if ((Balance == "90") || (Balance == "91") || (Balance == "92") || (Balance == "94") ||
                        (Balance == "95") || (Balance == "98") || (Balance == "99"))
                    {
                        var v = new { errorcode = Balance };
                        json = Balance; //JsonConvert.SerializeObject(v);
                        break;
                    }
                    if ((Balance == "111") || (Balance == "114") || (Balance == "115") || (Balance == "116") ||
                        (Balance == "119") || (Balance == "121") || (Balance == "163"))
                    {
                        var v = new { errorcode = Balance };
                        json = Balance; //JsonConvert.SerializeObject(v);
                        break;
                    }
                    if ((Balance == "180") || (Balance == "181") || (Balance == "182") || (Balance == "183") ||
                         (Balance == "184") || (Balance == "185") || (Balance == "186") || (Balance == "187") ||
                         (Balance == "188") || (Balance == "189") || (Balance == "190") || (Balance == "800"))
                    {
                        var v = new { errorcode = Balance };
                        json = Balance; // JsonConvert.SerializeObject(v);
                        break;
                    }
                    if ((Balance == "902") || (Balance == "904") || (Balance == "906") || (Balance == "907"))
                    {
                        var v = new { errorcode = Balance };
                        json = Balance; //JsonConvert.SerializeObject(v);
                        break;
                    }
                    if ((Balance == "909") || (Balance == "911") || (Balance == "913"))
                    {
                        var v = new { errorcode = Balance };
                        json = Balance; // JsonConvert.SerializeObject(v);
                        break;
                    }
                    if ((Balance != "99") || (Balance != "114") || (Balance != "116") ||
                        (Balance != "902") || (Balance != "904") || (Balance != "906") || (Balance != "907") || (Balance != "909") || (Balance != "911") || (Balance != "913"))
                    {
                        string msgConstruct = "Amount was successfully transferred from " + desc + " with NPR" + Convert.ToDecimal(tamount).ToString("#,##0.00") + ".";
                        var v = new { AmounttransferredBalance = Convert.ToDecimal(tamount).ToString("#,##0.00"), availableBalance = Balance, message = msgConstruct };
                        json = JsonConvert.SerializeObject(v);
                        break;
                    }
                }
            }
            return json;
        }
        public string ConstructResponseForCoupon(string RetrievalRef, float tamount, string dest, string desc, string desc2, string desc3)
        {
            string coupon = "";
            string json = "";
            for (int i = 0; i < 1; i++)
            {
                coupon = LookINTOResponseTableForCoupon(RetrievalRef);
                if ((coupon == "90") || (coupon == "91") || (coupon == "92") || (coupon == "94") ||
                    (coupon == "95") || (coupon == "98") || (coupon == "99"))
                {
                    var v = new { errorcode = coupon };
                    json = coupon; //JsonConvert.SerializeObject(v);
                    break;
                }
                if ((coupon == "111") || (coupon == "114") || (coupon == "115") || (coupon == "116") ||
                    (coupon == "119") || (coupon == "121") || (coupon == "163"))
                {
                    var v = new { errorcode = coupon };
                    json = coupon; //JsonConvert.SerializeObject(v);
                    break;
                }
                if ((coupon == "180") || (coupon == "181") || (coupon == "182") || (coupon == "183") ||
                     (coupon == "184") || (coupon == "185") || (coupon == "186") || (coupon == "187") ||
                     (coupon == "188") || (coupon == "189") || (coupon == "190") || (coupon == "800"))
                {
                    var v = new { errorcode = coupon };
                    json = coupon; // JsonConvert.SerializeObject(v);
                    break;
                }
                if ((coupon == "902") || (coupon == "904") || (coupon == "906") || (coupon == "907"))
                {
                    var v = new { errorcode = coupon };
                    json = coupon; //JsonConvert.SerializeObject(v);
                    break;
                }
                if ((coupon == "909") || (coupon == "911") || (coupon == "913"))
                {
                    var v = new { errorcode = coupon };
                    json = coupon; // JsonConvert.SerializeObject(v);
                    break;
                }
                if ((coupon != "99") || (coupon != "114") || (coupon != "116") ||
                    (coupon != "902") || (coupon != "904") || (coupon != "906") || (coupon != "907") || (coupon != "909") || (coupon != "911") || (coupon != "913"))
                {
                    if (coupon != "")
                    {
                        string msgConstruct = "Coupon purchased successfully of amount NPR " + Convert.ToDecimal(tamount).ToString("#,##0.00") + ".";
                        //var v = new { couponType = coupon, couponnumber = "010868732861", purchasedAmount = Convert.ToDecimal(tamount).ToString("#,##0.00"), qty = desc2, message = msgConstruct };
                        //if (coupon.Contains("|"))
                        //{
                        // coupon = coupon.Replace('|', ',');
                        //}
                        var v = new { couponType = coupon, couponnumber = coupon, purchasedAmount = Convert.ToDecimal(tamount).ToString("#,##0.00"), qty = desc2, message = msgConstruct };
                        json = JsonConvert.SerializeObject(v);
                        break;
                    }
                }
            }
            return json;
        }


        public string LookINTOResponseTable(string RetrievalRef)
        {
            string result = "";
            string formattedResult = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open(); //RetrievalRef = "091018145216";
                    string sql = "SELECT * FROM MNResponse where RetrievalRef = '" + RetrievalRef + "'";
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    {
                        // Setting command timeout to 90 second
                        //command.CommandTimeout = 90;
                        using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {

                                result = rdr["Balance"].ToString();
                                string RechargePIN = rdr["MiniStmntRec"].ToString();
                                int responseCode = int.Parse(rdr["ResponseCode"].ToString());
                                int flag = 0;
                                if (responseCode != 0)
                                {
                                    formattedResult = responseCode.ToString();
                                    //formattedResult = "Error in ResponeCode:Data Not Available " + responseCode;
                                    flag = 1;
                                }
                                if ((RechargePIN != "") || (!RechargePIN.Equals(".")))
                                {
                                    RechargePIN = "" + RechargePIN;
                                }
                                if (result != "" && flag == 0)
                                {
                                    string[] balReplyArr = result.Split('|');
                                    result = balReplyArr[1];
                                    if (result.Contains("+"))
                                    {
                                        decimal number = decimal.Parse(result.Replace("+", " "));
                                        //string whatYouWant = String.Format("{0:n}", number);
                                        string whatYouWant = Convert.ToDecimal(number).ToString("#,##0.00");

                                        formattedResult += "" + whatYouWant + RechargePIN; // result.Replace("+", " ");
                                    }
                                    else
                                    {
                                        float number = float.Parse(result);
                                        string whatYouWant = String.Format("{0:n}", number);
                                        formattedResult += "" + whatYouWant + RechargePIN;
                                    }

                                }


                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return formattedResult;
        }
        public string GetTimeStampFromAccountName(string RetrievalRef)
        {
            string result = "";
            string formattedResult = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open(); //RetrievalRef = "091018145216";
                    string sql = "SELECT AcHolderName FROM MNResponse where RetrievalRef = '" + RetrievalRef + "'";
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    {
                        // Setting command timeout to 90 second
                        //command.CommandTimeout = 90;
                        using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {

                                result = rdr["AcHolderName"].ToString();

                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }
        public string GetMerchantIDFromDestNumber(string destNumber)
        {
            string result = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open(); //RetrievalRef = "091018145216";
                    string sql = "select mid from MNMerchants as m join MNClientExt as c on m.ClientCode=c.ClientCode where c.userType='merchant' and UserName='" + destNumber + "'";
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    {
                        // Setting command timeout to 90 second
                        //command.CommandTimeout = 90;
                        using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {

                                result = rdr["mid"].ToString();
                                break;
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }
        private string LookINTOResponseTableForCoupon(string RetrievalRef)
        {
            string result = "";
            string formattedResult = "";
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            string sql = "SELECT * FROM MNResponse where RetrievalRef = '" + RetrievalRef + "'";
            try
            {
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conStr))
                {
                    con.Open();
                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, con))
                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                        while (rdr.Read())
                        {
                            result = rdr["Balance"].ToString();
                            //result = rdr["MiniStmntRec"].ToString();
                            string CouponPIN = rdr["MiniStmntRec"].ToString();
                            int responseCode = int.Parse(rdr["ResponseCode"].ToString());
                            int flag = 0;
                            if (responseCode != 0)
                            {
                                formattedResult = responseCode.ToString();
                                //formattedResult = "Error in ResponeCode:Data Not Available " + responseCode;
                                flag = 1;
                            }

                            if (result != "" && flag == 0)
                            {
                                //string[] balReplyArr = result.Split('|');
                                //result = balReplyArr[1];
                                //if (result.Contains("+"))
                                //{
                                //    float number = float.Parse(result.Replace("+", " "));
                                //    //string whatYouWant = String.Format("{0:n}", number);
                                //    string whatYouWant = Convert.ToDecimal(number).ToString("#,##0.00");

                                //    formattedResult += "Available Balance NPR " + whatYouWant + "." + RechargePIN; // result.Replace("+", " ");
                                //}
                                //else
                                //{
                                //float number = float.Parse(result);
                                //string whatYouWant = String.Format("{0:n}", number);

                                //formattedResult += "" + CouponPIN;

                                if (CouponPIN != "")
                                {
                                    formattedResult += "" + CouponPIN;
                                }
                                else
                                {
                                    formattedResult += "0000000000000";
                                }

                                // }

                            }
                        }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return formattedResult;
        }

        public int InsertNewUserInfoIntoTransDB(MNTransactionMaster mn)
        {
            int transId = 0;
            try
            {
                transId = mnTransactionMasterRepository.InsertintoTransactionMaster(mn);
                return transId;
            }
            catch (Exception ex)
            {

            }
            return transId;
        }

        static void BackgroundTaskWithObject(Object stateInfo)
        {
            MNRequest data = (MNRequest)stateInfo;
            Console.WriteLine($"Hi {data.RetrievalRef} from ThreadPool.");
            Thread.Sleep(TimeSpan.FromSeconds(1)); //1000 -1min
        }

        public string StatementByDate(string mobile, DateTime startdate, DateTime enddate)
        {
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            DataTable dtableResult = null;


            using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
            {
                try
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCustomerMiniStmtTest]", sqlCon))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.Add(new SqlParameter("@ContactNumber1", mobile));
                        sqlCmd.Parameters.Add(new SqlParameter("@StartDate", startdate));
                        sqlCmd.Parameters.Add(new SqlParameter("@EndDate", enddate));
                        using (var dataset = new DataSet())
                        {
                            da.Fill(dataset);

                            if (dataset.Tables.Count > 0)
                            {
                                dtableResult = dataset.Tables[0];
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    sqlCon.Close();
                }
            }
            return JsonConvert.SerializeObject(dtableResult);
        }

        public string BankStatementByDate(string mobile, DateTime startdate, DateTime enddate)
        {
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[MNepalDBConnectionStringProvider.GetConnection()].ConnectionString;

            DataTable dtableResult = null;


            using (SqlConnection sqlCon = new SqlConnection(DatabaseConnection.ConnectionString()))
            {
                try
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("[s_MNCustomerBankStmtTest]", sqlCon))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.Add(new SqlParameter("@ContactNumber1", mobile));
                        sqlCmd.Parameters.Add(new SqlParameter("@StartDate", startdate));
                        sqlCmd.Parameters.Add(new SqlParameter("@EndDate", enddate));
                        using (var dataset = new DataSet())
                        {
                            da.Fill(dataset);

                            if (dataset.Tables.Count > 0)
                            {
                                dtableResult = dataset.Tables[0];
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    sqlCon.Close();
                }
            }
            return JsonConvert.SerializeObject(dtableResult, Formatting.Indented);
        }




        #region paypoint

        public MNTransactionMaster Validatepaypoint(MNTransactionMaster transactionpaypoint, string PIN)
        {
            var mnTransactionMaster = new MNTransactionMaster();//for returning
            string Remark1 = string.Empty;

            //string SourceBankName = string.Empty;
            //string DestinationBankName = string.Empty;

            //if (transaction.SourceBIN == "0004")
            //{
            //    SourceBankName = "Nepal Investment Bank LTD";
            //    DestinationBankName = "Nepal Investment Bank LTD";
            //}
            //if (transaction.SourceBIN == "0000")
            //{
            //    SourceBankName = "Pumori Bank";
            //    DestinationBankName = "Pumori Bank";
            //}

            //START FOR DESCRIPTION 1 :ISO Field 125
            string description1;
            const int MaxLength = 49;

            if ((transactionpaypoint.merchantType == "school") || (transactionpaypoint.merchantType == "college"))
            {
                description1 = "Bill#" + transactionpaypoint.billNo + " Name:" + transactionpaypoint.studName;
                if (description1.Length > MaxLength)
                    description1 = description1.Substring(0, MaxLength);
            }
            else if (transactionpaypoint.merchantType == "insurance")
            {
                description1 = "PolicyNo#" + transactionpaypoint.billNo + " InsuredName:" + transactionpaypoint.studName;
                if (description1.Length > MaxLength)
                    description1 = description1.Substring(0, MaxLength);
            }
            else
            {
                description1 = "BILL PAYMENT"; //"Payment To " + getProductID[0]; //"Bank to Merchant's Bank a/c";
            }
            //END DESCRIPTION 1 :ISO Field 125


            //START FOR DETAIL IN STATEMENT (Remark column)

            //For Wallet to Wallet 
            if (transactionpaypoint.FeatureCode == "00")
            {
                //Remark1 = "FT (W to W) from " + transactionpaypoint.SourceMobile + " to " + transactionpaypoint.DestinationMobile;
                Remark1 = transactionpaypoint.Description;

            }


            ////For Bank to Wallet (Load Wallet)
            //if (transaction.FeatureCode == "10")
            //{
            //    Remark1 = "Load Wallet (Mobile Banking) from " + SourceBankName + " to " + transaction.DestinationMobile; //transaction.SourceMobile
            //}
            ////For Wallet to Bank (ank Transfer)
            //if (transaction.FeatureCode == "01")
            //{
            //Remark1 = "Bank Transfer from " + transaction.SourceMobile + " to " + mnTransactionMaster.DestinationAccount;
            //    Remark1 = "Bank Transfer to " + transaction.DestinationMobile + (DestinationBankName);
            //}
            ////For Bank to Bank
            //if (transaction.FeatureCode == "11")
            //{
            //    Remark1 = "FT (B to B) from " + transaction.SourceMobile + (SourceBankName) + " to " + transaction.DestinationMobile + (DestinationBankName);
            //}

            //For TOPUP 
            //Goto topup.svc Line 177 

            //For RECHARGE 
            //Goto coupon.svc Line 189 

            //For Merchant 
            //if (transaction.FeatureCode == )
            //{
            //    Remark1 = "Bill payment for" + transaction.M + " to " + transaction.DestinationMobile;
            //}

            //For Cash Out
            if (transactionpaypoint.FeatureCode == "51")
            {
                Remark1 = "Cash Out through Agent - " + transactionpaypoint.DestinationMobile;
            }

            //For Cash In
            if (transactionpaypoint.FeatureCode == "50")
            {
                Remark1 = "Cash In to Wallet from " + transactionpaypoint.SourceMobile + " to " + transactionpaypoint.DestinationMobile;
            }

            //END FOR DETAIL IN STATEMENT (Remark column)




            //if (!mnTransactionMasterRepository.IsTraceIDUnique(transaction.TraceId))
            //{
            //    mnTransactionMaster.Response = "Trace ID Repeated";
            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters missing/invalid");
            //    return LogReply(mnTransactionMaster); //log our error reply/success reply before sending to focus one.
            //}
            //else
            //{

            mnTransactionMaster.TraceId = transactionpaypoint.TraceId;
            mnTransactionMaster.StatusId = STATUS_VERIFICATION_SUCCESS_AT_ECOMM; //validating at EComm Side.
            mnTransactionMaster.RequestChannel = transactionpaypoint.RequestChannel;
            mnTransactionMaster.Amount = transactionpaypoint.Amount;
            if (mnTransactionMaster.Amount > 100000)
            {
                mnTransactionMaster.Response = "Limit Exceed";
                mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters invalid");
                return mnTransactionMaster;
            }
            mnTransactionMaster.CreatedDate = transactionpaypoint.CreatedDate;
            mnTransactionMaster.Description = transactionpaypoint.Description;
            //}

            Pin p = new Pin();
            if (PIN == "" && transactionpaypoint.SourceMobile == "9841003066" && transactionpaypoint.DestinationMobile == "9818713681")
            {
                //Start IVR Call if PIN is blank  ::
                DateTime dateTime = DateTime.Now;
                string isprocessed = "F";
                InsertDataForIVR insert = new InsertDataForIVR();
                bool getreply = insert.InsertData(transactionpaypoint.TraceId, transactionpaypoint.SourceMobile, isprocessed, dateTime);
                if (getreply == true)
                {
                    mnTransactionMaster.Response = "Transaction Successful :IVR";
                    mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                }
                else
                {

                }

            }
            else
            {
                if (transactionpaypoint.FeatureCode == "41")
                {

                }
                else
                {
                    if (transactionpaypoint.FeatureCode == "51")
                    {
                        if (!p.validPIN(transactionpaypoint.DestinationMobile, PIN))
                        {
                            mnTransactionMaster.Response = "Invalid PIN";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters invalid");
                            return LogReply(mnTransactionMaster); //log our error reply/success reply before sending to focus one.
                        }
                    }
                    else
                    {
                        if (transactionpaypoint.ReverseStatus == "T")
                        {

                        }
                        else
                        {
                            if (!p.validPIN(transactionpaypoint.SourceMobile, PIN))
                            {
                                mnTransactionMaster.Response = "Invalid PIN";
                                mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters invalid");
                                return LogReply(mnTransactionMaster); //log our error reply/success reply before sending to focus one.
                            }
                        }

                    }




                    //if (!p.validPIN(transaction.SourceMobile, PIN))
                    //{
                    //    mnTransactionMaster.Response = "Invalid PIN";
                    //    mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                    //    mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "parameters invalid");
                    //    return LogReply(mnTransactionMaster); //log our error reply/success reply before sending to focus one.
                    //}
                }
            }

            //Transaction Code
            switch (transactionpaypoint.FeatureCode)
            {
                case "00":
                case "50":
                case "51":
                    //00: Wallet to Wallet Type
                    //50: Cash In
                    //51: Cash out
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {

                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("dw", ref mnTransactionMaster, transactionpaypoint.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        //MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        //MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        //string OriginID = "123456789101112";
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = string.Empty;
                        string desc3 = string.Empty;
                        string Remark = string.Empty;
                        if (mnTransactionMaster.FeatureCode == "00")
                        {
                            Desc1 = "Transfer To Wallet "; //Wallet To Wallet
                            desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile; 
                          
						    Remark = Remark1;
                        }
                        else if (mnTransactionMaster.FeatureCode == "50")
                        {
                            Desc1 = "Cash In ";
                            desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;
                            Remark = Remark1;
                        }
                        else if (mnTransactionMaster.FeatureCode == "51")
                        {
                            //Desc1 = "Cash Out ";
                            Desc1 = description1;
                            desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;
                            Remark = Remark1;
                        }

                        string desc2 = transactionpaypoint.DestinationMobile;

                        string stringTokenNo = TraceIdGenerator.GetReqTokenCode() + 1; ;
                        //string Remark = mnTransactionMaster.Description; //"SecretTokenNo:" + stringTokenNo ; //
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transactionpaypoint.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo, Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, desc2, desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                        /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest); //added c

                        //string test = "please try again test"; //added c
                        //    test = InsertIntoPumoriInTest(mnRequest); //added c
                        //if (test=="true")
                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    waitForReply = ConstructResponse(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                    if (waitForReply != "")
                                    {
                                        /*UnSuccessful*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/


                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Successful*/
                                        else
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = Convert.ToString(HttpStatusCode.OK);
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                            break;
                                        }
                                    }

                                }

                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/


                            //    mnTransactionMaster.Response = "Request will be responded separately";
                            //    mnTransactionMaster.ResponseCode = HttpStatusCode.NoContent.ToString();
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}
                        } //InsertIntoPumoriInTest(mnRequest) //testing
                        else
                        {
                            mnTransactionMaster.Response = "Please try again"; //Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;

                case "01":
                case "30":
                    //01: Wallet to bank
                    //30: Merchant Payment Wallet to Bank.
                    {
                        //01: Wallet to bank
                        //30: Merchant Payment Wallet to Bank.
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("db", ref mnTransactionMaster, transactionpaypoint.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/


                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();

                        string descthree = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;
                        string desc1 = "";
                        string desc2 = transactionpaypoint.DestinationMobile;
                        string desc3 = "";
                        string desc = transactionpaypoint.Description;
                        string[] getProductID = desc.Split(':');
                        string Remark = "";

                        if (mnTransactionMaster.FeatureCode == "01")
                        {
                            string SourceBankName = string.Empty;
                            string DestinationBankName = string.Empty;

                            if (mnTransactionMaster.SourceBIN == "0004")
                            {
                                SourceBankName = "Nepal Investment Bank LTD";
                            }
                            if (mnTransactionMaster.SourceBIN == "0000")
                            {
                                SourceBankName = "Pumori Bank";
                            }

                            if (mnTransactionMaster.DestinationBIN == "0004")
                            {
                                DestinationBankName = "Nepal Investment Bank LTD";
                            }
                            if (mnTransactionMaster.DestinationBIN == "0000")
                            {
                                DestinationBankName = "Pumori Bank";
                            }

                            //Remark1 = "Bank Transfer to " + transactionpaypoint.DestinationMobile + " (" + DestinationBankName + ")";
                            Remark1 = transactionpaypoint.Description;

                            desc1 = "Transfer To Bank " + getProductID[0];//"Wallet To Bank";
                            desc2 = desc2;
                            desc3 = descthree;
                            Remark = Remark1; //"Bank Transfer from " + transaction.SourceMobile + " to " + mnTransactionMaster.DestinationAccount;
                        }
                        else if (mnTransactionMaster.FeatureCode == "30")
                        {
                            desc1 = description1;
                            //desc1 = "BILL PAYMENT"; // "Payment To " + getProductID[0];//"Wallet To Merchant's Bank a/c";
                            desc2 = getProductID[1];
                            desc3 = descthree;
                            Remark = mnTransactionMaster.Description + " - " + desc1;
                        }
                        else if (mnTransactionMaster.FeatureCode == "31")
                        {
                            //Utility Payment

                            //Desc1 = getProductID[0];
                            //desc1 = "Payment To " + getProductID[0]; //"Wallet To Merchant's Bank a/c (Bill Payment)";
                            desc1 = description1;
                            desc2 = getProductID[1]; //getProductID[1];
                            desc3 = descthree; //getProductID[0];
                            Remark = mnTransactionMaster.Description + " - " + desc1;
                        }

                        //string Remark = mnTransactionMaster.Description + " - " + desc1;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transactionpaypoint.ReverseStatus;
                        //TraceIdGenerator traceid = new TraceIdGenerator();
                        //RetrievalRef = traceid.GenerateUniqueTraceID();

                        //int len = RetrievalRef.Length;
                        //string lastPart = RetrievalRef.Substring(len - 6, 6);


                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, desc1, desc2, desc3, IsProcessed, Remark, ReverseStatus);
                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    //waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                    waitForReply = ConstructResponse(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                    if (waitForReply != "")
                                    {
                                        /*UnSuccessful*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; /*"Dear Sir/Mam, Request accepted. Will respond separately";*/
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, mnTransactionMaster.Response);
                                            break;

                                        }
                                        /*Successful*/
                                        else //if (waitForReply != "907" || waitForReply != "99" || waitForReply != "114" || waitForReply != "116" || waitForReply != "911")
                                        {
                                            if (waitForReply != "")
                                            {
                                                mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                                /*INSERT TRANSACTION LOG*/
                                                //Send Into TransactionLog
                                                transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                                transactionlog.UpdatedDate = DateTime.Now;
                                                transactionlog.StatusId = mnTransactionMaster.StatusId;
                                                transactionlog.Description = "Transaction Successful";

                                                translog.InsertDataIntoTransactionLog(transactionlog);
                                                /*END:TRANSACTION LOG*/

                                                /*UPDATE TRANSACTION*/
                                                mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                                /*END:TRANSACTION*/

                                                mnTransactionMaster.Response = waitForReply;
                                                mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                                mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                                break;
                                            }
                                        }
                                    }

                                }

                            } while (waitForReply == "");

                            //UnSuccessful
                            //if (waitForReply == "99" || waitForReply == "907" || waitForReply == "114" || waitForReply == "116" || waitForReply == "911")
                            //{
                            //    //if (waitForReply == "")
                            //    //{
                            //        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //        /*INSERT TRANSACTION LOG*/
                            //        //Send Into TransactionLog
                            //        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //        transactionlog.UpdatedDate = DateTime.Now;
                            //        transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //        transactionlog.Description = "Transaction Failed At Pumori";

                            //        translog.InsertDataIntoTransactionLog(transactionlog);
                            //        /*END:TRANSACTION LOG*/

                            //        /*UPDATE TRANSACTION*/
                            //        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //        mnTransactionMaster.Response = waitForReply; //"Dear Sir/Mam, Request accepted. Will respond separately";
                            //        mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, mnTransactionMaster.Response);
                            //    //}
                            //}

                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Request Could not be processed");
                        }
                    }
                    break;

                case "31":
                    //31: Utility Payment Wallet to Bank.
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("db", ref mnTransactionMaster, transactionpaypoint.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/


                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();

                        string descthree = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;
                        string desc1 = "";
                        string desc2 = transactionpaypoint.DestinationMobile;
                        string desc3 = "";
                        string desc = transactionpaypoint.Description;
                        string[] getProductID = desc.Split('-');

                        if (mnTransactionMaster.FeatureCode == "01")
                        {
                            desc1 = "Transfer To Bank " + getProductID[0];//"Wallet To Bank";
                            desc2 = desc2;
                            desc3 = descthree;
                        }
                        else if (mnTransactionMaster.FeatureCode == "30")
                        {
                            desc1 = "Payment To " + getProductID[0];//"Wallet To Merchant's Bank a/c";
                            desc2 = getProductID[1];
                            desc3 = descthree;
                        }
                        else if (mnTransactionMaster.FeatureCode == "31")
                        {
                            //Utility Payment

                            //Desc1 = getProductID[0];
                            //desc1 = "Payment To " + getProductID[0]; //"Wallet To Merchant's Bank a/c (Bill Payment)";
                            desc1 = description1;
                            desc2 = getProductID[1]; //getProductID[1];
                            desc3 = descthree; //getProductID[0];
                        }
                        //desc1 = "MPOS TOP UP";
                        string Remark = mnTransactionMaster.Description;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transactionpaypoint.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, desc1, desc2, desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest)) //added for testing -InsertIntoPumoriIn(mnRequest)-
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    //waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                    waitForReply = ConstructResponseForTopUp(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                    if (waitForReply != "")
                                    {
                                        /*UnSuccessful*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; /*"Dear Sir/Mam, Request accepted. Will respond separately";*/
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, mnTransactionMaster.Response);
                                            break;

                                        }
                                        /*Successful*/
                                        else //if (waitForReply != "907" || waitForReply != "99" || waitForReply != "114" || waitForReply != "116" || waitForReply != "911")
                                        {
                                            if (waitForReply != "")
                                            {
                                                mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                                mnTransactionMaster.createdTimeDate = GetTimeStampFromAccountName(mnRequest.RetrievalRef);
                                                /*INSERT TRANSACTION LOG*/
                                                //Send Into TransactionLog
                                                transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                                transactionlog.UpdatedDate = DateTime.Now;
                                                transactionlog.StatusId = mnTransactionMaster.StatusId;
                                                transactionlog.Description = "Transaction Successful";

                                                translog.InsertDataIntoTransactionLog(transactionlog);
                                                /*END:TRANSACTION LOG*/

                                                /*UPDATE TRANSACTION*/
                                                mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                                /*END:TRANSACTION*/

                                                mnTransactionMaster.Response = waitForReply;
                                                mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                                mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                                break;
                                            }
                                        }
                                    }

                                }

                            } while (waitForReply == "");

                            //UnSuccessful
                            //if (waitForReply == "99" || waitForReply == "907" || waitForReply == "114" || waitForReply == "116" || waitForReply == "911")
                            //{
                            //    //if (waitForReply == "")
                            //    //{
                            //        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //        /*INSERT TRANSACTION LOG*/
                            //        //Send Into TransactionLog
                            //        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //        transactionlog.UpdatedDate = DateTime.Now;
                            //        transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //        transactionlog.Description = "Transaction Failed At Pumori";

                            //        translog.InsertDataIntoTransactionLog(transactionlog);
                            //        /*END:TRANSACTION LOG*/

                            //        /*UPDATE TRANSACTION*/
                            //        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //        mnTransactionMaster.Response = waitForReply; //"Dear Sir/Mam, Request accepted. Will respond separately";
                            //        mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, mnTransactionMaster.Response);
                            //    //}
                            //}

                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again!!";//Request could not be processed Please try again!!
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Request Could not be processed");
                        }
                    }
                    break;

                case "32":
                    //32: Coupon Payment Wallet to Bank MOBILE RECHARGE
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        string separateString = transactionpaypoint.DestinationMobile;
                        string[] getDestinationDetails = separateString.Split(',');

                        if (!LoadStationDetails("db", ref mnTransactionMaster, getDestinationDetails[0]))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;

                        float Amount = mnTransactionMaster.Amount;
                        //string FeeId = mnTransactionMaster.FeeId;
                        string FeeId = " ";
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "";

                        string desc = mnTransactionMaster.Description;
                        string[] getQtyAndDesc = desc.Split(':');
                        //Desc1 = getDestinationDetails[1] + getDestinationDetails[2];

                        //Desc1 = getDestinationDetails[1];
                        //Desc1 = getDestinationDetails[1];
                        Desc1 = ""; //"Payment To " + getQtyAndDesc[0];/
                        string merchantID = GetMerchantIDFromDestNumber(mnTransactionMaster.DestinationMobile);
                        if (merchantID == "13")
                        { //Check if Merchant is DishHome
                            Desc1 = "^^RcHg^^DISH HOME";
                        }
                        if (merchantID == "14")
                        { //Check if Merchant is DishHome
                            Desc1 = "^^RcHg^^BROADLINK";
                        }
                        if (merchantID == "11")
                        { //Check if Merchant is DishHome
                            Desc1 = "NTC MOBILE RECHARGE";
                            //Desc1 = description1;
                        }
                        if (merchantID == "12")
                        { //Check if Merchant is DishHome
                            Desc1 = "CDMA MOBILE RECHARGE";
                            //Desc1 = description1;
                        }
                        //string Remark = getQtyAndDesc[1];
                        //string Remark = getQtyAndDesc[0] + " " + getQtyAndDesc[1];
                        //string Desc2 = getQtyAndDesc[2];

                        string Remark = desc;
                        string Desc2 = desc;
                        string Desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;//getQtyAndDesc[1];


                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transactionpaypoint.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Message Validated and Sent to Pumori In";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    //waitForReply = LookINTOResponseTableForCoupon(mnRequest.RetrievalRef);
                                    waitForReply = ConstructResponseForCoupon(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1, mnRequest.Desc2, mnRequest.Desc3);
                                    if (waitForReply != "")
                                    {
                                        /*UnSuccess*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Success*/
                                        else
                                        {

                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                            break;
                                        }

                                    }
                                }

                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Dear Sir/Mam, Request accepted. Will respond separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, mnTransactionMaster.Response);
                            //}
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Request Could not be processed");
                        }
                    }
                    break;

                case "35":
                    //35: Coupon Payment Bank to Bank MOBILE RECHARGE
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        string separateString = transactionpaypoint.DestinationMobile;
                        string[] getDestinationDetails = separateString.Split(',');

                        if (!LoadStationDetails("db", ref mnTransactionMaster, getDestinationDetails[0]))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        //string FeeId = mnTransactionMaster.FeeId;
                        string FeeId = " ";
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "";

                        string desc = mnTransactionMaster.Description;
                        string[] getQtyAndDesc = desc.Split(',');
                        //Desc1 = getDestinationDetails[1] + getDestinationDetails[2];
                        string[] getDesc = desc.Split(':');

                        //Desc1 = getDestinationDetails[1];
                        Desc1 = ""; //"Payment To " + getQtyAndDesc[0];/
                        string merchantID = GetMerchantIDFromDestNumber(mnTransactionMaster.DestinationMobile);
                        if (merchantID == "13")
                        { //Check if Merchant is DishHome
                            Desc1 = "^^RcHg^^DISH HOME";
                        }
                        if (merchantID == "14")
                        { //Check if Merchant is DishHome
                            Desc1 = "^^RcHg^^BROADLINK";
                        }
                        if (merchantID == "11")
                        { //Check if Merchant is DishHome
                            Desc1 = "NTC MOBILE RECHARGE";
                            //Desc1 = description1;
                        }
                        if (merchantID == "12")
                        { //Check if Merchant is DishHome
                            Desc1 = "CDMA MOBILE RECHARGE";
                            //Desc1 = description1;
                        }
                        /*string Desc2 = getDesc[2];*/ //getQtyAndDesc[0];
                        string Desc2 = desc;
                        string Desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;
                        //string Remark = getDesc[1];
                        string Remark = desc;

                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transactionpaypoint.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Message Validated and Sent to Pumori In";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    //waitForReply = LookINTOResponseTableForCoupon(mnRequest.RetrievalRef);
                                    waitForReply = ConstructResponseForCoupon(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1, mnRequest.Desc2, mnRequest.Desc3);
                                    if (waitForReply != "")
                                    {
                                        //UnSuccess
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        //Success
                                        else
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                            break;
                                        }

                                    }
                                }
                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Dear Sir/Mam, Request accepted. Will respond separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, mnTransactionMaster.Response);
                            //}

                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Request Could not be processed");
                        }
                    }
                    break;


                case "10":
                    //10: Bank to Wallet
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("dw", ref mnTransactionMaster, transactionpaypoint.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;

                        string SourceBankName = string.Empty;
                        string DestinationBankName = string.Empty;

                        if (mnTransactionMaster.SourceBIN == "0004")
                        {
                            SourceBankName = "Nepal Investment Bank LTD";
                        }
                        if (mnTransactionMaster.SourceBIN == "0000")
                        {
                            SourceBankName = "Pumori Bank";
                        }

                        if (mnTransactionMaster.DestinationBIN == "0004")
                        {
                            DestinationBankName = "Nepal Investment Bank LTD";
                        }
                        if (mnTransactionMaster.DestinationBIN == "0000")
                        {
                            DestinationBankName = "Pumori Bank";
                        }

                        //Remark1 = "Load Wallet(Mobile Banking) from " + SourceBankName + " to " + transactionpaypoint.DestinationMobile;
                        Remark1 = transactionpaypoint.Description;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        //string Desc1 = "Transfer To Wallet";//Bank To Wallet
                        string Desc1 = description1;
                        string Desc2 = mnTransactionMaster.Description;
                        string Desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;
                        //string Remark = "";
                        string Remark = Remark1;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transactionpaypoint.ReverseStatus;

                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    //waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                    waitForReply = ConstructResponse(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);
                                    if (waitForReply != "")
                                    {
                                        /*UnSuccess*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Success*/
                                        else
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                            break;
                                        }
                                    }

                                }

                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Request will be respond separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}

                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again"; //Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;

                case "11":
                case "33":
                    //11: Bank to Bank
                    //33: Merchant Payment Bank to Bank.
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("db", ref mnTransactionMaster, transactionpaypoint.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        //string Desc1 = "Bank To Bank";
                        //string Desc2 = mnTransactionMaster.Description;

                        string Desc1 = "";
                        string Desc2 = "";
                        string Desc3 = "";
                        string Remark = "";
                        string desc = mnTransactionMaster.Description;
                        string[] getProductID = desc.Split(':');

                        if (mnTransactionMaster.FeatureCode == "11")
                        {
                            string SourceBankName = string.Empty;
                            string DestinationBankName = string.Empty;

                            if (mnTransactionMaster.SourceBIN == "0004")
                            {
                                SourceBankName = "Nepal Investment Bank LTD";
                            }
                            if (mnTransactionMaster.SourceBIN == "0000")
                            {
                                SourceBankName = "Pumori Bank";
                            }

                            if (mnTransactionMaster.DestinationBIN == "0004")
                            {
                                DestinationBankName = "Nepal Investment Bank LTD";
                            }
                            if (mnTransactionMaster.DestinationBIN == "0000")
                            {
                                DestinationBankName = "Pumori Bank";
                            }

                            //Remark1 = "FT (B to B) from " + transactionpaypoint.SourceMobile + " (" + SourceBankName + ")" + " to " + transactionpaypoint.DestinationMobile + " (" + DestinationBankName + ")";
                            Remark1 = transactionpaypoint.Description;

                            Desc1 = "Transfer To Bank "; //Bank To Bank
                            Desc2 = transactionpaypoint.DestinationMobile; //desc;
                            Desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;
                            Remark = Remark1;
                        }

                        else if (mnTransactionMaster.FeatureCode == "33")
                        {
                            Desc1 = description1;
                            // Merchant Payment Bank to Bank
                            //const int MaxLength = 49;
                            //var name = "456546/Rupendra kaji Bahadur Budhacharya";
                            //if (name.Length > MaxLength)
                            //    name = name.Substring(0, MaxLength);
                            //Desc1 = name;//"BILL PAYMENT"; //"Payment To " + getProductID[0]; //"Bank to Merchant's Bank a/c";
                            Desc2 = transactionpaypoint.DestinationMobile; //desc;
                            Desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;
                            Remark = desc + description1;
                        }

                        else if (mnTransactionMaster.FeatureCode == "34")
                        {
                            //Utility Payment Bank to Bank

                            //Desc1 = getProductID[0];
                            //Desc1 = "BILL PAYMENT";//"Payment To " + getProductID[0]; //"Bank to Merchant's Bank a/c (Bill Payment)";
                            Desc1 = description1;
                            Desc2 = getProductID[1]; //transaction.DestinationMobile; //desc;
                            Desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile; //getProductID[0];
                            Remark = desc + " - Bill#" + transactionpaypoint.billNo + " Name:" + transactionpaypoint.studName;

                        }
                        //string Remark = desc + " - Bill#" + transaction.billNo + " Name:" + transaction.studName;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transactionpaypoint.ReverseStatus;
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            //do
                            //{
                            for (int i = 0; i < 99999; i++)
                            {
                                //waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                waitForReply = ConstructResponse(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                if (waitForReply != "")
                                {
                                    /*UnSuccess*/
                                    if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                        (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                        (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                        (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                        (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                        (waitForReply == "98") || (waitForReply == "99") ||
                                        (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                        (waitForReply == "911") || (waitForReply == "913"))
                                    {
                                        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                        /*INSERT TRANSACTION LOG*/
                                        //Send Into TransactionLog
                                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                        transactionlog.UpdatedDate = DateTime.Now;
                                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                                        transactionlog.Description = "Transaction Failed At Pumori";

                                        translog.InsertDataIntoTransactionLog(transactionlog);
                                        /*END:TRANSACTION LOG*/

                                        /*UPDATE TRANSACTION*/
                                        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                        /*END:TRANSACTION*/

                                        mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                        mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                        mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                        break;
                                    }
                                    /*Success*/
                                    else
                                    {

                                        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                        /*INSERT TRANSACTION LOG*/
                                        //Send Into TransactionLog
                                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                        transactionlog.UpdatedDate = DateTime.Now;
                                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                                        transactionlog.Description = "Transaction Successful";

                                        translog.InsertDataIntoTransactionLog(transactionlog);
                                        /*END:TRANSACTION LOG*/

                                        /*UPDATE TRANSACTION*/
                                        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                        /*END:TRANSACTION*/

                                        mnTransactionMaster.Response = waitForReply;
                                        mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                        mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                        break;
                                    }
                                }

                            }

                            //} while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Request will be responded separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;

                case "34":
                    //34: Utility Payment Bank to Bank.
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("db", ref mnTransactionMaster, transactionpaypoint.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        //string Desc1 = "Bank To Bank";
                        //string Desc2 = mnTransactionMaster.Description;

                        string Desc1 = "";
                        string Desc2 = "";
                        string Desc3 = "";
                        string desc = mnTransactionMaster.Description;
                        string[] getProductID = desc.Split('-');

                        if (mnTransactionMaster.FeatureCode == "11")
                        {
                            Desc1 = "Transfer To Bank "; //Bank To Bank
                            Desc2 = transactionpaypoint.DestinationMobile; //desc;
                            Desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;
                        }

                        else if (mnTransactionMaster.FeatureCode == "33")
                        {
                            // Merchant Payment Bank to Bank
                            Desc1 = "Payment To " + getProductID[0]; //"Bank to Merchant's Bank a/c";
                            Desc2 = transactionpaypoint.DestinationMobile; //desc;
                            Desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile;
                        }

                        else if (mnTransactionMaster.FeatureCode == "34")
                        {
                            //Utility Payment Bank to Bank

                            //Desc1 = getProductID[0];
                            //Desc1 = "Payment To " + getProductID[0]; //"Bank to Merchant's Bank a/c (Bill Payment)";
                            Desc1 = description1;
                            Desc2 = getProductID[1]; //transaction.DestinationMobile; //desc;
                            Desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile; //getProductID[0];

                        }
                        string Remark = desc;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        string ReverseStatus = transactionpaypoint.ReverseStatus;

                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            //do
                            //{
                            for (int i = 0; i < 99999; i++)
                            {
                                //waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                waitForReply = ConstructResponseForTopUp(mnRequest.RetrievalRef, mnRequest.Amount, mnTransactionMaster.DestinationMobile, mnRequest.Desc1);

                                if (waitForReply != "")
                                {
                                    /*UnSuccess*/
                                    if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                        (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                        (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                        (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                        (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                        (waitForReply == "98") || (waitForReply == "99") ||
                                        (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                        (waitForReply == "911") || (waitForReply == "913"))
                                    {
                                        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                        /*INSERT TRANSACTION LOG*/
                                        //Send Into TransactionLog
                                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                        transactionlog.UpdatedDate = DateTime.Now;
                                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                                        transactionlog.Description = "Transaction Failed At Pumori";

                                        translog.InsertDataIntoTransactionLog(transactionlog);
                                        /*END:TRANSACTION LOG*/

                                        /*UPDATE TRANSACTION*/
                                        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                        /*END:TRANSACTION*/

                                        mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                        mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                        mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                        break;
                                    }
                                    /*Success*/
                                    else
                                    {

                                        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                        mnTransactionMaster.createdTimeDate = GetTimeStampFromAccountName(mnRequest.RetrievalRef);
                                        /*INSERT TRANSACTION LOG*/
                                        //Send Into TransactionLog
                                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                        transactionlog.UpdatedDate = DateTime.Now;
                                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                                        transactionlog.Description = "Transaction Successful";

                                        translog.InsertDataIntoTransactionLog(transactionlog);
                                        /*END:TRANSACTION LOG*/

                                        /*UPDATE TRANSACTION*/
                                        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                        /*END:TRANSACTION*/

                                        mnTransactionMaster.Response = waitForReply;
                                        mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                        mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                        break;
                                    }
                                }

                            }

                            //} while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Request will be responded separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;
                case "20":
                    //20: Balance Query Wallet
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        //source bin and destination bin same for bq
                        mnTransactionMaster.DestinationBIN = mnTransactionMaster.SourceBIN;
                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "Balance Inquery For Wallet Account";
                        string Desc2 = mnTransactionMaster.Description;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";

                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, IsProcessed);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/


                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                    if (waitForReply != "")
                                    {
                                        /*UnSuccess*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.NoContent.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Success*/
                                        else
                                        {

                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, mnTransactionMaster.Response);
                                            break;
                                        }
                                    }
                                }
                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Request will be responded separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;
                case "22":
                    //balance query bank
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        //source bin and destination bin same for bq
                        mnTransactionMaster.DestinationBIN = mnTransactionMaster.SourceBIN;
                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "Balance Inquery For Bank Account";
                        string Desc2 = mnTransactionMaster.Description;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, IsProcessed);

                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);
                                    if (waitForReply != "")
                                    {
                                        /*UnSuccess*/
                                        if ((waitForReply == "111") || (waitForReply == "114") || (waitForReply == "115") || (waitForReply == "116") || (waitForReply == "119") ||
                                            (waitForReply == "121") || (waitForReply == "163") || (waitForReply == "180") || (waitForReply == "181") || (waitForReply == "182") ||
                                            (waitForReply == "183") || (waitForReply == "184") || (waitForReply == "185") || (waitForReply == "186") || (waitForReply == "187") ||
                                            (waitForReply == "188") || (waitForReply == "189") || (waitForReply == "190") || (waitForReply == "800") ||
                                            (waitForReply == "90") || (waitForReply == "91") || (waitForReply == "92") || (waitForReply == "94") || (waitForReply == "95") ||
                                            (waitForReply == "98") || (waitForReply == "99") ||
                                            (waitForReply == "902") || (waitForReply == "904") || (waitForReply == "906") || (waitForReply == "907") || (waitForReply == "909") ||
                                            (waitForReply == "911") || (waitForReply == "913"))
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Failed At Pumori";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply; //"Request will be responded separately";
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "accepted. will respond separately");
                                            break;
                                        }
                                        /*Success*/
                                        else
                                        {
                                            mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                            /*INSERT TRANSACTION LOG*/
                                            //Send Into TransactionLog
                                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                            transactionlog.UpdatedDate = DateTime.Now;
                                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                                            transactionlog.Description = "Transaction Successful";

                                            translog.InsertDataIntoTransactionLog(transactionlog);
                                            /*END:TRANSACTION LOG*/

                                            /*UPDATE TRANSACTION*/
                                            mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                            /*END:TRANSACTION*/

                                            mnTransactionMaster.Response = waitForReply;
                                            mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                            mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, mnTransactionMaster.Response);
                                            break;
                                        }
                                    }
                                }

                            } while (waitForReply == "");

                            //if (waitForReply == "")
                            //{
                            //    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                            //    /*INSERT TRANSACTION LOG*/
                            //    //Send Into TransactionLog
                            //    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            //    transactionlog.UpdatedDate = DateTime.Now;
                            //    transactionlog.StatusId = mnTransactionMaster.StatusId;
                            //    transactionlog.Description = "Transaction Failed At Pumori";

                            //    translog.InsertDataIntoTransactionLog(transactionlog);
                            //    /*END:TRANSACTION LOG*/

                            //    /*UPDATE TRANSACTION*/
                            //    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                            //    /*END:TRANSACTION*/

                            //    mnTransactionMaster.Response = "Request will be responded separately";
                            //    mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            //}

                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;
                case "21":
                    //ministatement for wallet
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        //source bin and destination bin same for bq
                        mnTransactionMaster.DestinationBIN = mnTransactionMaster.SourceBIN;
                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);
                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "MiniStatement for Wallet";
                        string Desc2 = mnTransactionMaster.Description;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo, Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, IsProcessed);
                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            //do
                            //{
                            for (int i = 0; i < 99999; i++)
                            {
                                waitForReply = LookINTOResponseTableForMiniStatement(mnRequest.RetrievalRef);
                                if (waitForReply != "[]")
                                {
                                    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                    /*INSERT TRANSACTION LOG*/
                                    //Send Into TransactionLog
                                    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                    transactionlog.UpdatedDate = DateTime.Now;
                                    transactionlog.StatusId = mnTransactionMaster.StatusId;
                                    transactionlog.Description = "Transaction Successful";

                                    translog.InsertDataIntoTransactionLog(transactionlog);
                                    /*END:TRANSACTION LOG*/

                                    /*UPDATE TRANSACTION*/
                                    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                    /*END:TRANSACTION*/

                                    mnTransactionMaster.Response = waitForReply;
                                    mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                    mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                    break;
                                }
                            }

                            if (waitForReply == "")
                            {
                                mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                /*INSERT TRANSACTION LOG*/
                                //Send Into TransactionLog
                                transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                transactionlog.UpdatedDate = DateTime.Now;
                                transactionlog.StatusId = mnTransactionMaster.StatusId;
                                transactionlog.Description = "Transaction Failed At Pumori";

                                translog.InsertDataIntoTransactionLog(transactionlog);
                                /*END:TRANSACTION LOG*/

                                /*UPDATE TRANSACTION*/
                                mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                /*END:TRANSACTION*/

                                mnTransactionMaster.Response = "Request will be responded separately";
                                mnTransactionMaster.ResponseCode = HttpStatusCode.NoContent.ToString();
                                mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            }
                            //} while (waitForReply == "");
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again"; //Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;
                case "23":
                    //ministatement for bank
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sb", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        //source bin and destination bin same for bq
                        mnTransactionMaster.DestinationBIN = mnTransactionMaster.SourceBIN;
                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "MiniStatement for Bank";
                        string Desc2 = mnTransactionMaster.Description;
                        //string Desc3;
                        //string OTraceNo;
                        //string OTranDateTime;
                        string IsProcessed = "F";
                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo, Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, IsProcessed);
                        //OriginID[varchar(16)], OriginType[varchar(4)], ServiceCode[varchar(2)],
                        //SourceBankCode[varchar(4)], SourceBranchCode[varchar(5)], SourceAccountNo[varchar(20)],
                        //DestBankCode[varchar(4)], DestBranchCode[varchar(5)], DestAccountNo[varchar(20)],
                        //Amount[float], FeeId[int],TraceNo[string], RetrivalRef[string], Desc1[string],Desc2[string],
                        //OTranDateTime(string), IsProcessed(string)
                        /*
                            /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            for (int i = 0; i < 99999; i++)
                            {
                                waitForReply = LookINTOResponseTableForMiniStatement(mnRequest.RetrievalRef);
                                if (waitForReply != "[]")
                                {
                                    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                    /*INSERT TRANSACTION LOG*/
                                    //Send Into TransactionLog
                                    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                    transactionlog.UpdatedDate = DateTime.Now;
                                    transactionlog.StatusId = mnTransactionMaster.StatusId;
                                    transactionlog.Description = "Transaction Successful";

                                    translog.InsertDataIntoTransactionLog(transactionlog);
                                    /*END:TRANSACTION LOG*/

                                    /*UPDATE TRANSACTION*/
                                    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                    /*END:TRANSACTION*/

                                    mnTransactionMaster.Response = waitForReply;
                                    mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                    mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                    break;
                                }
                            }

                            if (waitForReply == "")
                            {
                                mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                /*INSERT TRANSACTION LOG*/
                                //Send Into TransactionLog
                                transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                transactionlog.UpdatedDate = DateTime.Now;
                                transactionlog.StatusId = mnTransactionMaster.StatusId;
                                transactionlog.Description = "Transaction Failed At Pumori";

                                translog.InsertDataIntoTransactionLog(transactionlog);
                                /*END:TRANSACTION LOG*/

                                /*UPDATE TRANSACTION*/
                                mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                /*END:TRANSACTION*/

                                mnTransactionMaster.Response = "Request will be responded separately";
                                mnTransactionMaster.ResponseCode = HttpStatusCode.NoContent.ToString();
                                mnTransactionMaster.ResponseStatus(HttpStatusCode.NoContent, "accepted. will respond separately");
                            }
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Could not be processed");
                        }
                    }
                    break;

                case "40":
                    //Remittance Token
                    {
                        //Remittance Token
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;
                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        string destinationMobile = transactionpaypoint.DestinationMobile;
                        if (destinationMobile.Length.ToString() == "10")
                        {
                            mnTransactionMaster.DestinationMobile = transactionpaypoint.DestinationMobile;
                            mnTransactionMaster.DestinationAccount = null;
                            mnTransactionMaster.DestinationBIN = null;
                            mnTransactionMaster.DestinationBranchCode = null;
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Destination Mobile Length Insufficient";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Destination Mobile Length Insufficient");
                            break;
                        }

                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetailsWithOutDestBIN(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/

                        GenerateToken getToken = new GenerateToken();
                        string token = getToken.GetUniqueKey();
                        mnTransactionMaster.DestinationAccount = token;
                        mnTransactionMaster.Description = transactionpaypoint.Description;    //secret code

                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        //Generate token and reply

                        string msgConstruct = "Token requested by " + transactionpaypoint.SourceMobile + " for transferring amount NPR " + transactionpaypoint.Amount + " to " + transactionpaypoint.DestinationMobile;
                        var v = new { AmounttransferredBalance = Convert.ToDecimal(transactionpaypoint.Amount).ToString("#,##0.00"), RequestedToken = token, message = msgConstruct };
                        string json = JsonConvert.SerializeObject(v);

                        mnTransactionMaster.Response = json;
                        mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString(); //200
                        mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Reply Token Number");

                        /*END:TRANSACTION LOG*/

                    }
                    break;
                case "41":
                    //Remittance Redeem
                    {
                        mnTransactionMaster.FeatureCode = transactionpaypoint.FeatureCode;

                        //services for ww
                        /*SOURCE*/
                        if (!LoadStationDetails("sw", ref mnTransactionMaster, transactionpaypoint.SourceMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Source User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Source User");
                            break;
                        }
                        /*END:SOURCE*/

                        /*DESTINATION*/
                        if (!LoadStationDetails("dw", ref mnTransactionMaster, transactionpaypoint.DestinationMobile))
                        {
                            mnTransactionMaster.Response = "Invalid Destination User";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Destination User");
                            break;
                        }
                        /*END:DESTINATION*/


                        /*FEATURE DETAILS*/
                        if (!LoadFeatureDetails(ref mnTransactionMaster))
                        {
                            mnTransactionMaster.Response = "Invalid Product Request";
                            mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Invalid Product Request");
                            break;
                        }
                        /*END:FEATURE DETAILS*/



                        /*TRANSACTION*/
                        int TransactionId = mnTransactionMasterRepository.InsertintoTransactionMaster(mnTransactionMaster);
                        /*END:TRANSACTION*/

                        /*TRANSACTION LOG*/

                        MNTransactionLog transactionlog = new MNTransactionLog();
                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                        transactionlog.UpdatedDate = DateTime.Now;
                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                        transactionlog.Description = "Message Validated At Ecomm";

                        MNTransactionLogController translog = new MNTransactionLogController();
                        translog.InsertDataIntoTransactionLog(transactionlog);

                        /*END:TRANSACTION LOG*/



                        /*MNREQUEST*/
                        string OriginID = mnTransactionMaster.SourceMobile;
                        string OriginType = "6011";

                        string ServiceCode = mnTransactionMaster.FeatureCode;

                        //source details
                        string SourceBankCode = mnTransactionMaster.SourceBIN;
                        string SourceBranchCode = mnTransactionMaster.SourceBranchCode;
                        string SourceAccountNo = mnTransactionMaster.SourceAccount;

                        //destination details
                        string DestBankCode = mnTransactionMaster.DestinationBIN;
                        string DestBranchCode = mnTransactionMaster.DestinationBranchCode;
                        string DestAccountNo = mnTransactionMaster.DestinationAccount;


                        float Amount = mnTransactionMaster.Amount;
                        string FeeId = mnTransactionMaster.FeeId;
                        string TraceNo = mnTransactionMaster.TraceId.ToString();
                        DateTime TranDate = DateTime.Now;
                        string RetrievalRef = mnTransactionMaster.TraceId.ToString();
                        string Desc1 = "";
                        string[] splitData = mnTransactionMaster.Description.Split(',');

                        if (mnTransactionMaster.FeatureCode == "41")
                        {
                            Desc1 = "Remittance Redeem of User:" + splitData[3];
                        }

                        string Desc2 = "SecretCode:" + splitData[0] + "," + "Token:" + splitData[1];
                        string Desc3 = transactionpaypoint.SourceMobile + " - " + transactionpaypoint.DestinationMobile; //splitData[2];
                        string Remark = ""; // "SecretCode:" + splitData[0] + "," + "Token:" + splitData[1]; 

                        string IsProcessed = "F";
                        string ReverseStatus = transactionpaypoint.ReverseStatus;

                        MNRequest mnRequest = new MNRequest(OriginID, OriginType, ServiceCode, SourceBankCode, SourceBranchCode, SourceAccountNo, DestBankCode, DestBranchCode, DestAccountNo,
                            Amount, FeeId, TraceNo, TranDate, RetrievalRef, Desc1, Desc2, Desc3, IsProcessed, Remark, ReverseStatus);

                        /*END:MNREQUEST*/

                        ThreadPool.QueueUserWorkItem(BackgroundTaskWithObject, mnRequest);

                        if (InsertIntoPumoriIn(mnRequest))
                        {
                            mnTransactionMaster.StatusId = STATUS_WAITING_FOR_PUMORI_HTTPREPLY;

                            /*INSERT TRANSACTION LOG*/
                            //Send Into TransactionLog
                            transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                            transactionlog.UpdatedDate = DateTime.Now;
                            transactionlog.StatusId = mnTransactionMaster.StatusId;
                            transactionlog.Description = "Validated Message is Sent to Pumori Request";

                            translog.InsertDataIntoTransactionLog(transactionlog);
                            /*END:TRANSACTION LOG*/

                            string waitForReply = "";
                            do
                            {
                                for (int i = 0; i < 99999; i++)
                                {
                                    waitForReply = LookINTOResponseTable(mnRequest.RetrievalRef);

                                    if (waitForReply != "")
                                    {
                                        mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_SUCCESS;
                                        /*INSERT TRANSACTION LOG*/
                                        //Send Into TransactionLog
                                        transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                        transactionlog.UpdatedDate = DateTime.Now;
                                        transactionlog.StatusId = mnTransactionMaster.StatusId;
                                        transactionlog.Description = "Transaction Successful";

                                        translog.InsertDataIntoTransactionLog(transactionlog);
                                        /*END:TRANSACTION LOG*/

                                        /*UPDATE TRANSACTION*/
                                        mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                        /*END:TRANSACTION*/

                                        string msgConstruct = "Token Redeemed Successfully of amount NPR" + transactionpaypoint.Amount;
                                        var v = new { AmounttransferredBalance = Convert.ToDecimal(transactionpaypoint.Amount).ToString("#,##0.00"), message = msgConstruct };
                                        string json = JsonConvert.SerializeObject(v);

                                        mnTransactionMaster.Response = json;
                                        mnTransactionMaster.ResponseCode = HttpStatusCode.OK.ToString();
                                        mnTransactionMaster.ResponseStatus(HttpStatusCode.OK, "Success");
                                        break;
                                    }
                                }
                                if (waitForReply == "")
                                {
                                    mnTransactionMaster.StatusId = STATUS_PUMORI_HTTPREPLY_FAILED;

                                    /*INSERT TRANSACTION LOG*/
                                    //Send Into TransactionLog
                                    transactionlog.TransactionId = TransactionId;    /*To retrive the latest Inserted Data into TrasncationMaster*/
                                    transactionlog.UpdatedDate = DateTime.Now;
                                    transactionlog.StatusId = mnTransactionMaster.StatusId;
                                    transactionlog.Description = "Transaction Failed At Pumori";

                                    translog.InsertDataIntoTransactionLog(transactionlog);
                                    /*END:TRANSACTION LOG*/

                                    /*UPDATE TRANSACTION*/
                                    mnTransactionMasterRepository.UpdateintoTransactionMaster(mnTransactionMaster);
                                    /*END:TRANSACTION*/

                                    mnTransactionMaster.Response = "Dear Sir/Mam, Request accepted. Will respond separately";
                                    mnTransactionMaster.ResponseCode = HttpStatusCode.BadRequest.ToString();
                                    mnTransactionMaster.ResponseStatus(HttpStatusCode.BadRequest, "Dear Sir/Mam, Request accepted. Will respond separately");
                                }

                            } while (waitForReply == "");
                        }
                        else
                        {
                            mnTransactionMaster.Response = "Please try again";//Request could not be processed
                            mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString();
                            mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Request could not be processed");
                        }
                    }
                    break;

                default:
                    {
                        mnTransactionMaster.Response = "";
                        mnTransactionMaster.ResponseCode = HttpStatusCode.InternalServerError.ToString(); //500
                        mnTransactionMaster.ResponseStatus(HttpStatusCode.InternalServerError, "Invalid Parameter");
                    }
                    break;

            }

            return mnTransactionMaster;
        }


        #endregion


    }
}