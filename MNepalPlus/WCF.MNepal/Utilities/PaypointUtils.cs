using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WCF.MNepal.Models;
using WCF.MNepal.UserModels;

namespace WCF.MNepal.Utilities
{
    public class PaypointUtils
    {
        #region Request check payment
        public static int RequestCPPaypointInfo(PaypointModel reqCPPaypointInfo)
        {
            var objreqCPaypointModel = new PaypointUserModel();
            var objreqCPaypointInfo = new PaypointModel
            {
                companyCode = reqCPPaypointInfo.companyCodeReqCP,
                serviceCode = reqCPPaypointInfo.serviceCodeReqCP,
                account = reqCPPaypointInfo.accountReqCP,
                special1 = reqCPPaypointInfo.special1ReqCP,
                special2 = reqCPPaypointInfo.special2ReqCP,

                transactionDate = reqCPPaypointInfo.transactionDateReqCP,
                transactionId = reqCPPaypointInfo.transactionIdReqCP,
                refStan = reqCPPaypointInfo.refStanReqCP,
                amount = reqCPPaypointInfo.amountReqCP,
                billNumber = reqCPPaypointInfo.billNumberReqCP,

                userId = reqCPPaypointInfo.userIdReqCP,
                userPassword = reqCPPaypointInfo.userPasswordReqCP,
                salePointType = reqCPPaypointInfo.salePointTypeReqCP,
                retrievalReference = reqCPPaypointInfo.retrievalReferenceReqCP,
                remarks = reqCPPaypointInfo.remarkReqCP,
                UserName = reqCPPaypointInfo.UserName,
                ClientCode = reqCPPaypointInfo.ClientCode,
                paypointType = reqCPPaypointInfo.paypointType,
                //ErrorMessage = reqPaypointInfo.ErrorMessage,
                Mode = "RQCP"




            };
            return objreqCPaypointModel.RequestPaypointInfo(objreqCPaypointInfo);
        }

        #endregion

        #region Response check payment

        public static int ResponseCPPaypointInfo(PaypointModel resCPPaypointInfo)
        {
            var objresCPaypointModel = new PaypointUserModel();
            var objresCPaypointInfo = new PaypointModel
            {
                companyCode = resCPPaypointInfo.companyCodeResCP,
                serviceCode = resCPPaypointInfo.serviceCodeResCP,
                account = resCPPaypointInfo.accountResCP,
                special1 = resCPPaypointInfo.special1ResCP,
                special2 = resCPPaypointInfo.special2ResCP,

                transactionDate = resCPPaypointInfo.transactionDateResCP,
                transactionId = resCPPaypointInfo.transactionIdResCP,
                refStan = resCPPaypointInfo.refStanResCP,
                amount = resCPPaypointInfo.amountResCP,
                billNumber = resCPPaypointInfo.billNumberResCP,

                userId = resCPPaypointInfo.userIdResCP,
                userPassword = resCPPaypointInfo.userPasswordResCP,
                salePointType = resCPPaypointInfo.salePointTypeResCP,
                retrievalReference = resCPPaypointInfo.retrievalReferenceResCP,
                responseCode = resCPPaypointInfo.responseCodeResCP,
                description = resCPPaypointInfo.descriptionResCP,
                customerName = resCPPaypointInfo.customerNameCP,
                UserName = resCPPaypointInfo.UserName,
                ClientCode = resCPPaypointInfo.ClientCode,
                paypointType = resCPPaypointInfo.paypointType,
                resultMessage = resCPPaypointInfo.resultMessageResCP,
                //ErrorMessage = reqPaypointInfo.ErrorMessage,
                Mode = "RSCP"




            };
            return objresCPaypointModel.ResponsePaypointInfo(objresCPaypointInfo);
        }
        #endregion

        #region Request excute payment
        public static int RequestEPPaypointInfo(PaypointModel reqEPPaypointInfo)
        {
            var objreqEPaypointModel = new PaypointUserModel();
            var objreqEPaypointInfo = new PaypointModel
            {
                companyCode = reqEPPaypointInfo.companyCodeReqEP,
                serviceCode = reqEPPaypointInfo.serviceCodeReqEP,
                account = reqEPPaypointInfo.accountReqEP,
                special1 = reqEPPaypointInfo.special1ReqEP,
                special2 = reqEPPaypointInfo.special2ReqEP,

                transactionDate = reqEPPaypointInfo.transactionDateReqEP,
                transactionId = reqEPPaypointInfo.transactionIdReqEP,
                refStan = reqEPPaypointInfo.refStanReqEP,
                amount = reqEPPaypointInfo.amountReqEP,
                billNumber = reqEPPaypointInfo.billNumberReqEP,

                userId = reqEPPaypointInfo.userIdReqEP,
                userPassword = reqEPPaypointInfo.userPasswordReqEP,
                salePointType = reqEPPaypointInfo.salePointTypeReqEP,
                retrievalReference = reqEPPaypointInfo.retrievalReferenceReqEP,
                remarks = reqEPPaypointInfo.remarkReqEP,
                UserName = reqEPPaypointInfo.UserName,
                ClientCode = reqEPPaypointInfo.ClientCode,
                paypointType = reqEPPaypointInfo.paypointType,
                //ErrorMessage = reqPaypointInfo.ErrorMessage,
                Mode = "RQEP"

            };
            return objreqEPaypointModel.RequestPaypointInfo(objreqEPaypointInfo);
        }

        #endregion

        #region Response excute payment

        public static int ResponseEPPaypointInfo(PaypointModel resEPPaypointInfo)
        {
            var objresEPaypointModel = new PaypointUserModel();
            var objresEPaypointInfo = new PaypointModel
            {
                companyCode = resEPPaypointInfo.companyCodeResEP,
                serviceCode = resEPPaypointInfo.serviceCodeResEP,
                account = resEPPaypointInfo.accountResEP,
                special1 = resEPPaypointInfo.special1ResEP,
                special2 = resEPPaypointInfo.special2ResEP,

                transactionDate = resEPPaypointInfo.transactionDateResEP,
                transactionId = resEPPaypointInfo.transactionIdResEP,
                refStan = resEPPaypointInfo.refStanResEP,
                amount = resEPPaypointInfo.amountResEP,
                billNumber = resEPPaypointInfo.billNumberResEP,

                userId = resEPPaypointInfo.userIdResEP,
                userPassword = resEPPaypointInfo.userPasswordResEP,
                salePointType = resEPPaypointInfo.salePointTypeResEP,
                retrievalReference = resEPPaypointInfo.retrievalReferenceResEP,
                responseCode = resEPPaypointInfo.responseCodeResEP,
                description = resEPPaypointInfo.descriptionResEP,
                customerName = resEPPaypointInfo.customerNameResEP,
                UserName = resEPPaypointInfo.UserName,
                ClientCode = resEPPaypointInfo.ClientCode,
                paypointType = resEPPaypointInfo.paypointType,
                resultMessage = resEPPaypointInfo.resultMessageResEP,
                //ErrorMessage = reqPaypointInfo.ErrorMessage,
                Mode = "RSEP"




            };
            return objresEPaypointModel.ResponsePaypointInfo(objresEPaypointInfo);
        }
        #endregion

        #region Request get transaction payment
        public static int RequestGTPaypointInfo(PaypointModel reqGTPaypointInfo)
        {
            var objreqGTPaypointModel = new PaypointUserModel();
            var objreqGTPaypointInfo = new PaypointModel
            {
                companyCode = reqGTPaypointInfo.companyCodeReqGTP,
                serviceCode = reqGTPaypointInfo.serviceCodeReqGTP,
                account = reqGTPaypointInfo.accountReqGTP,
                special1 = reqGTPaypointInfo.special1ReqGTP,
                special2 = reqGTPaypointInfo.special2ReqGTP,

                transactionDate = reqGTPaypointInfo.transactionDateReqGTP,
                transactionId = reqGTPaypointInfo.transactionIdReqGTP,
                refStan = reqGTPaypointInfo.refStanReqGTP,
                amount = reqGTPaypointInfo.amountReqGTP,
                billNumber = reqGTPaypointInfo.billNumberReqGTP,

                userId = reqGTPaypointInfo.userIdReqGTP,
                userPassword = reqGTPaypointInfo.userPasswordReqGTP,
                salePointType = reqGTPaypointInfo.salePointTypeReqGTP,
                retrievalReference = reqGTPaypointInfo.retrievalReferenceReqGTP,
                remarks = reqGTPaypointInfo.remarkReqGTP,
                UserName = reqGTPaypointInfo.UserName,
                ClientCode = reqGTPaypointInfo.ClientCode,
                paypointType = reqGTPaypointInfo.paypointType,
                //ErrorMessage = reqPaypointInfo.ErrorMessage,

                Mode = "RQGTP"

            };
            return objreqGTPaypointModel.RequestPaypointInfo(objreqGTPaypointInfo);
        }

        #endregion

        #region Response Get Transaction payment

        public static int ResponseGTPaypointInfo(PaypointModel resGTPaypointInfo)
        {
            var objresGTPaypointModel = new PaypointUserModel();
            var objresGTPaypointInfo = new PaypointModel
            {
                companyCode = resGTPaypointInfo.companyCodeResGTP,
                serviceCode = resGTPaypointInfo.serviceCodeResGTP,
                account = resGTPaypointInfo.accountResGTP,
                special1 = resGTPaypointInfo.special1ResGTP,
                special2 = resGTPaypointInfo.special2ResGTP,

                transactionDate = resGTPaypointInfo.transactionDateResGTP,
                transactionId = resGTPaypointInfo.transactionIdResGTP,
                refStan = resGTPaypointInfo.refStanResGTP,
                amount = resGTPaypointInfo.amountResGTP,
                billNumber = resGTPaypointInfo.billNumberResGTP,

                userId = resGTPaypointInfo.userIdResGTP,
                userPassword = resGTPaypointInfo.userPasswordResGTP,
                salePointType = resGTPaypointInfo.salePointTypeResGTP,
                retrievalReference = resGTPaypointInfo.retrievalReferenceResGTP,
                responseCode = resGTPaypointInfo.responseCodeResGTP,
                description = resGTPaypointInfo.descriptionResGTP,
                customerName = resGTPaypointInfo.customerNameResGTP,
                UserName = resGTPaypointInfo.UserName,
                ClientCode = resGTPaypointInfo.ClientCode,
                paypointType = resGTPaypointInfo.paypointType,
                resultMessage = resGTPaypointInfo.resultMessageResGTP,
                //ErrorMessage = reqPaypointInfo.ErrorMessage,
                Mode = "RSGTP"

            };
            return objresGTPaypointModel.ResponsePaypointInfo(objresGTPaypointInfo);
        }
        #endregion




        #region"Response CP NEA payments"
        public static int PaypointPaymentInfo(PaypointModel resPaypointPaymentInfo)
        {
            var objresPaypointPaymentModel = new PaypointUserModel();
            var objresPaypointPaymentInfo = new PaypointModel
            {


                descriptionP = resPaypointPaymentInfo.descriptionP,
                billDateP = resPaypointPaymentInfo.billDateP,
                billAmountP = resPaypointPaymentInfo.billAmountP,
                amountP = resPaypointPaymentInfo.amountP,
                totalAmountP = resPaypointPaymentInfo.totalAmountP,

                statusP = resPaypointPaymentInfo.statusP,
                amountfactP = resPaypointPaymentInfo.amountfactP,
                amountmaskP = resPaypointPaymentInfo.amountmaskP,
                amountmaxP = resPaypointPaymentInfo.amountmaxP,
                amountminP = resPaypointPaymentInfo.amountminP,
                amountstepP = resPaypointPaymentInfo.amountstepP,

                codservP = resPaypointPaymentInfo.codservP,
                commissionP = resPaypointPaymentInfo.commissionP,
                commisvalueP = resPaypointPaymentInfo.commisvalueP,
                destinationP = resPaypointPaymentInfo.destinationP,
                fioP = resPaypointPaymentInfo.fioP,
                iP = resPaypointPaymentInfo.iP,
                idP = resPaypointPaymentInfo.idP,
                jP = resPaypointPaymentInfo.jP,
                requestIdP = resPaypointPaymentInfo.requestIdP,
                show_counterP = resPaypointPaymentInfo.show_counterP,
                i_countP = resPaypointPaymentInfo.i_countP,
                UserName = resPaypointPaymentInfo.UserName,
                ClientCode = resPaypointPaymentInfo.ClientCode,
                Mode = "RSP"




            };
            return objresPaypointPaymentModel.ResponsePaypointPaymentInfo(objresPaypointPaymentInfo);
        }
        #endregion


        #region"Response CP Nepal Water payments"
        public static int PaypointNepalWaterPaymentInfo(PaypointModel resPaypointNepalWaterPaymentInfo)
        {
            var objresPaypointNepalWaterPaymentModel = new PaypointUserModel();
            var objresPaypointNepalWaterPaymentInfo = new PaypointModel
            {


                descriptionP = resPaypointNepalWaterPaymentInfo.descriptionP,
                billDateP = resPaypointNepalWaterPaymentInfo.billDateP,
                billAmountP = resPaypointNepalWaterPaymentInfo.billAmountP,
                amountP = resPaypointNepalWaterPaymentInfo.amountP,
                totalAmountP = resPaypointNepalWaterPaymentInfo.totalAmountP,

                statusP = resPaypointNepalWaterPaymentInfo.statusP,
                amountfactP = resPaypointNepalWaterPaymentInfo.amountfactP,
                amountmaskP = resPaypointNepalWaterPaymentInfo.amountmaskP,
                amountmaxP = resPaypointNepalWaterPaymentInfo.amountmaxP,
                amountminP = resPaypointNepalWaterPaymentInfo.amountminP,

                amountstepP = resPaypointNepalWaterPaymentInfo.amountstepP,
                codservP = resPaypointNepalWaterPaymentInfo.codservP,
                commissionP = resPaypointNepalWaterPaymentInfo.commissionP,
                commisvalueP = resPaypointNepalWaterPaymentInfo.commisvalueP,
                destinationP = resPaypointNepalWaterPaymentInfo.destinationP,



                fioP = resPaypointNepalWaterPaymentInfo.fioP,
                iP = resPaypointNepalWaterPaymentInfo.iP,
                idP = resPaypointNepalWaterPaymentInfo.idP,
                jP = resPaypointNepalWaterPaymentInfo.jP,
                requestIdP = resPaypointNepalWaterPaymentInfo.requestIdP,

                show_counterP = resPaypointNepalWaterPaymentInfo.show_counterP,
                i_countP = resPaypointNepalWaterPaymentInfo.i_countP,
                legatNumberP = resPaypointNepalWaterPaymentInfo.legatNumberP,
                discountAmountP = resPaypointNepalWaterPaymentInfo.discountAmountP,
                counterRentP = resPaypointNepalWaterPaymentInfo.counterRentP,

                fineAmountP = resPaypointNepalWaterPaymentInfo.fineAmountP,
                billDateFromP = resPaypointNepalWaterPaymentInfo.billDateFromP,
                billDateToP = resPaypointNepalWaterPaymentInfo.billDateToP,

                UserName = resPaypointNepalWaterPaymentInfo.UserName,
                ClientCode = resPaypointNepalWaterPaymentInfo.ClientCode,


                Mode = "RSNWP" //Response nepal water payment from Checkpayment




            };
            return objresPaypointNepalWaterPaymentModel.ResponsePaypointNepalWaterPaymentInfo(objresPaypointNepalWaterPaymentInfo);
        }
        #endregion


        #region"Response Checkpayment for khanepani invoice"
        public static int PaypointKhanepaniInvoiceInfo(PaypointModel resPaypointKhanepaniInvoiceInfo)
        {
            var objresPaypointKhanepaniInvoiceModel = new PaypointUserModel();
            var objresPaypointKhanepaniInvoiceInfo = new PaypointModel
            {
                statusKI = resPaypointKhanepaniInvoiceInfo.statusKI,
                total_advance_amountKI = resPaypointKhanepaniInvoiceInfo.total_advance_amountKI,
                customer_codeKI = resPaypointKhanepaniInvoiceInfo.customer_codeKI,
                addressKI = resPaypointKhanepaniInvoiceInfo.addressKI,
                total_credit_sales_amountKI = resPaypointKhanepaniInvoiceInfo.total_credit_sales_amountKI,

                customer_nameKI = resPaypointKhanepaniInvoiceInfo.customer_nameKI,
                current_month_duesKI = resPaypointKhanepaniInvoiceInfo.current_month_duesKI,
                mobile_numberKI = resPaypointKhanepaniInvoiceInfo.mobile_numberKI,
                total_duesKI = resPaypointKhanepaniInvoiceInfo.total_duesKI,
                previous_duesKI = resPaypointKhanepaniInvoiceInfo.previous_duesKI,

                current_month_discountKI = resPaypointKhanepaniInvoiceInfo.current_month_discountKI,
                current_month_fineKI = resPaypointKhanepaniInvoiceInfo.current_month_fineKI,
                refStan = resPaypointKhanepaniInvoiceInfo.refStan,
                UserName = resPaypointKhanepaniInvoiceInfo.UserName,
                ClientCode = resPaypointKhanepaniInvoiceInfo.ClientCode,

                Mode = "RSKI"



            };
            return objresPaypointKhanepaniInvoiceModel.ResponsePaypointKhanepaniInvoiceInfo(objresPaypointKhanepaniInvoiceInfo);
        }
        #endregion


        #region NEA Details
        public static DataSet GetNEADetails(NEAFundTransfer NEAObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new NEAFundTransfer
            {
                ClientCode = NEAObj.ClientCode,
                UserName = NEAObj.UserName,
                SCNo = NEAObj.SCNo,
                NEABranchCode = NEAObj.NEABranchCode,
                CustomerID = NEAObj.CustomerID,
                refStan = NEAObj.refStan,
                Mode = "NEA" // GET NEA Details
            };
            return objUserModel.GetNEAPaymentDetails(objUserInfo);
        }

        public static DataSet GetNEADetailsPay(NEAFundTransfer NEAObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new NEAFundTransfer
            {
                ClientCode = NEAObj.ClientCode,
                UserName = NEAObj.UserName,
                SCNo = NEAObj.SCNo,
                NEABranchCode = NEAObj.NEABranchCode,
                CustomerID = NEAObj.CustomerID,
                refStan = NEAObj.refStan,
                Mode = "NEA" // GET NEA Details
            };
            return objUserModel.GetNEAPaymentDetailsPay(objUserInfo);
        }
        #endregion

        #region "AvailBaln Utilities"

        /// <summary>
        /// Get the availBaln information of given clientCode
        /// </summary>
        /// <param name="clientCode">Pass clientCode as string</param>
        /// <returns>Returns the datatable of availBaln information</returns>
        public static DataTable GetAvailBaln(string clientCode)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new UserInfo
            {
                ClientCode = clientCode
            };
            return objUserModel.GetUserAvailBaln(objUserInfo);
        }

        #endregion

        #region Khanepani Details
        public static DataSet GetKPDetails(Khanepani KPObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new Khanepani
            {
                ClientCode = KPObj.ClientCode,
                UserName = KPObj.UserName,
                KhanepaniCounter = KPObj.KhanepaniCounter,
                CustomerID = KPObj.CustomerID,
                refStan = KPObj.refStan,
                retrievalReference = KPObj.retrievalReference,
                Mode = "KP" // GET KP Details
            };
            return objUserModel.GetKPPaymentDetails(objUserInfo);
        }

        public static DataSet GetKPDetailsPay(Khanepani KPObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new Khanepani
            {
                ClientCode = KPObj.ClientCode,
                UserName = KPObj.UserName,
                KhanepaniCounter = KPObj.KhanepaniCounter,
                CustomerID = KPObj.CustomerID,
                refStan = KPObj.refStan,
                Mode = "KP" // GET KP Details
            };
            return objUserModel.GetKPPaymentDetailsPay(objUserInfo);
        }
        #endregion

        #region Nepal Water Details
        public static DataSet GetNWDetails(NepalWater NWObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new NepalWater
            {
                ClientCode = NWObj.ClientCode,
                UserName = NWObj.UserName,
                NWCounter = NWObj.NWCounter,
                CustomerID = NWObj.CustomerID,
                refStan = NWObj.refStan,
                retrievalReference = NWObj.retrievalReference,
                Mode = "NW" // GET NW Details
            };
            return objUserModel.GetNWPaymentDetails(objUserInfo);
        }

        public static DataSet GetNWDetailsPay(NepalWater NWObj)
        {
            var objUserModel = new PaypointUserModel();
            var objUserInfo = new NepalWater
            {
                ClientCode = NWObj.ClientCode,
                UserName = NWObj.UserName,
                NWCounter = NWObj.NWCounter,
                CustomerID = NWObj.CustomerID,
                refStan = NWObj.refStan,
                Mode = "NW" // GET NW Details
            };
            return objUserModel.GetNWPaymentDetailsPay(objUserInfo);
        }
        #endregion

        #region"Response CP Wlink payments"
        public static int PaypointWlinkInfo(PaypointModel resPaypointWlinkPaymentInfo)
        {
            var objresPaypointWlinkModel = new PaypointUserModel();
            var objresPaypointWlinkPaymentInfo = new PaypointModel
            {
                description = resPaypointWlinkPaymentInfo.description,
                amountP = resPaypointWlinkPaymentInfo.amountP,
                PackageId = resPaypointWlinkPaymentInfo.PackageId,
                billDateP = resPaypointWlinkPaymentInfo.transactionDate,
                billAmountP = resPaypointWlinkPaymentInfo.amount,
                billNumberCP = resPaypointWlinkPaymentInfo.billNumber,
                refStanCP = resPaypointWlinkPaymentInfo.refStan,
                customerNameCP = resPaypointWlinkPaymentInfo.customerName,
                companyCodeCP = resPaypointWlinkPaymentInfo.companyCode,
                userId = resPaypointWlinkPaymentInfo.UserName,
                customer_codeKI = resPaypointWlinkPaymentInfo.ClientCode,
                RemainingDays = resPaypointWlinkPaymentInfo.RemainingDays,
                Mode = "RSWlinkP" //Response wlink payment from Checkpayment
            };
            return objresPaypointWlinkModel.ResponsePaypointWlinkPaymentInfo(objresPaypointWlinkPaymentInfo);
        }
        #endregion

        #region Request excute payment
        public static int RequestEPPaypointWlinkInfo(PaypointModel reqEPPaypointInfo)
        {
            var objreqEPaypointModel = new PaypointUserModel();
            var objreqEPaypointInfo = new PaypointModel
            {
                companyCode = reqEPPaypointInfo.companyCodeReqEP,
                serviceCode = reqEPPaypointInfo.serviceCodeReqEP,
                account = reqEPPaypointInfo.accountReqEP,
                special1 = reqEPPaypointInfo.special1ReqEP,
                special2 = reqEPPaypointInfo.special2ReqEP,

                transactionDate = reqEPPaypointInfo.transactionDateReqEP,
                transactionId = reqEPPaypointInfo.transactionIdReqEP,
                refStan = reqEPPaypointInfo.refStanReqEP,
                amount = reqEPPaypointInfo.amountReqEP,
                billNumber = reqEPPaypointInfo.billNumberReqEP,

                userId = reqEPPaypointInfo.userIdReqEP,
                userPassword = reqEPPaypointInfo.userPasswordReqEP,
                salePointType = reqEPPaypointInfo.salePointTypeReqEP,
                retrievalReference = reqEPPaypointInfo.retrievalReferenceReqEP,
                remarks = reqEPPaypointInfo.remarkReqEP,
                UserName = reqEPPaypointInfo.UserName,
                ClientCode = reqEPPaypointInfo.ClientCode,
                paypointType = reqEPPaypointInfo.paypointType,
                //ErrorMessage = reqPaypointInfo.ErrorMessage,
                Mode = "RQEP"

            };
            return objreqEPaypointModel.RequestPaypointInfo(objreqEPaypointInfo);
        }

        #endregion

        #region"Response CP Wlink payments"
        public static int PaypointSubisuInfo(PaypointModel resPaypointSubisuPaymentInfo)
        {
            var objresPaypointWlinkModel = new PaypointUserModel();
            var objresPaypointWlinkPaymentInfo = new PaypointModel
            {
                billDateP = resPaypointSubisuPaymentInfo.transactionDateReqCP,
                billAmountP = resPaypointSubisuPaymentInfo.amountReqCP,
                billNumberCP = resPaypointSubisuPaymentInfo.billNumberReqCP,
                refStanCP = resPaypointSubisuPaymentInfo.refStanReqCP,
                customerNameCP = resPaypointSubisuPaymentInfo.customerNameCP,
                companyCodeCP = resPaypointSubisuPaymentInfo.companyCodeCP,
                userId = resPaypointSubisuPaymentInfo.UserName,
                customer_codeKI = resPaypointSubisuPaymentInfo.ClientCode,
                Mode = "RSSubisu" //Response subisu payment from Checkpayment
            };
            return objresPaypointWlinkModel.ResponsePaypointSubisuPaymentInfo(objresPaypointWlinkPaymentInfo);
        }
        #endregion

        #region"Response CP Vianet payments"
        public static int PaypointVianetPaymentInfo(PaypointModel resPaypointVianetPaymentInfo)
        {
            var objresPaypointVianetPaymentModel = new PaypointUserModel();
            var objresPaypointVianetPaymentInfo = new PaypointModel
            {


                description = resPaypointVianetPaymentInfo.description,
                amountP = resPaypointVianetPaymentInfo.amountP,
                PackageId = resPaypointVianetPaymentInfo.PackageId,
                smartCards="",
                ftthUser = "",
                reserveInfo = "",
                billNumber = resPaypointVianetPaymentInfo.billNumber,
                refStan = resPaypointVianetPaymentInfo.refStan,
                amount = resPaypointVianetPaymentInfo.amount,
                transactionDate = resPaypointVianetPaymentInfo.transactionDate,
                customerName = resPaypointVianetPaymentInfo.customerName,
                companyCode = resPaypointVianetPaymentInfo.companyCode,
                UserName = resPaypointVianetPaymentInfo.UserName,
                ClientCode = resPaypointVianetPaymentInfo.ClientCode,
                Mode = "RSVianetP" //Response vianet payment from Checkpayment

            };
            return objresPaypointVianetPaymentModel.ResponsePaypointVianetPaymentInfo(objresPaypointVianetPaymentInfo);
        }
        #endregion

        #region"Response CP SIMTV payments"
        public static int PaypointSIMTVPaymentInfo(PaypointModel resPaypointVianetPaymentInfo)
        {
            var objresPaypointSIMTVPaymentModel = new PaypointUserModel();
            var objresPaypointSIMTVPaymentInfo = new PaypointModel
            {


                description = resPaypointVianetPaymentInfo.description,
                amountP = resPaypointVianetPaymentInfo.amountP,
                PackageId = "",
                smartCards="",
                ftthUser = "",
                reserveInfo ="",
                billNumber = resPaypointVianetPaymentInfo.billNumber,
                refStan = resPaypointVianetPaymentInfo.refStan,
                amount = resPaypointVianetPaymentInfo.amount,
                transactionDate = resPaypointVianetPaymentInfo.transactionDate,
                customerName = resPaypointVianetPaymentInfo.customerName,
                companyCode = resPaypointVianetPaymentInfo.companyCode,
                UserName = resPaypointVianetPaymentInfo.UserName,
                ClientCode = resPaypointVianetPaymentInfo.ClientCode,
                Mode = "SIMTV" //Response SIMTV payment from Checkpayment

            };
            return objresPaypointSIMTVPaymentModel.ResponsePaypointVianetPaymentInfo(objresPaypointSIMTVPaymentInfo);
        }
        #endregion

        #region"Response CP MeroTV payments"
        public static int PaypointMeroTVPaymentInfo(PaypointModel resPaypointMeroTVPaymentInfo)
        {
            var objresPaypointMeroTVPaymentModel = new PaypointUserModel();
            var objresPaypointMeroTVPaymentInfo = new PaypointModel
            {
                description = resPaypointMeroTVPaymentInfo.description,
                amountP = resPaypointMeroTVPaymentInfo.amountP,
                PackageId = resPaypointMeroTVPaymentInfo.PackageId,
                smartCards = resPaypointMeroTVPaymentInfo.smartCards,
                ftthUser = "",
                reserveInfo = resPaypointMeroTVPaymentInfo.reserveInfo,
                billNumber = resPaypointMeroTVPaymentInfo.billNumber,
                refStan = resPaypointMeroTVPaymentInfo.refStan,
                amount = resPaypointMeroTVPaymentInfo.amount,
                transactionDate = resPaypointMeroTVPaymentInfo.transactionDate,
                customerName = resPaypointMeroTVPaymentInfo.customerName,
                companyCode = resPaypointMeroTVPaymentInfo.companyCode,
                UserName = resPaypointMeroTVPaymentInfo.UserName,
                ClientCode = resPaypointMeroTVPaymentInfo.ClientCode,
                Mode = "MeroTV" //Response vianet payment from Checkpayment

            };
            return objresPaypointMeroTVPaymentModel.ResponsePaypointVianetPaymentInfo(objresPaypointMeroTVPaymentInfo);
        }
        #endregion

        #region"Response CP SkyTV payments"
        public static int PaypointSkyTVPaymentInfo(PaypointModel resPaypointSkyTVPaymentInfo)
        {
            var objresPaypointMeroTVPaymentModel = new PaypointUserModel();
            var objresPaypointMeroTVPaymentInfo = new PaypointModel
            {
                description = resPaypointSkyTVPaymentInfo.description,
                amountP = resPaypointSkyTVPaymentInfo.amountP,
                PackageId = resPaypointSkyTVPaymentInfo.PackageId,
                smartCards = resPaypointSkyTVPaymentInfo.smartCards,
                ftthUser = resPaypointSkyTVPaymentInfo.ftthUser,
                reserveInfo = resPaypointSkyTVPaymentInfo.reserveInfo,
                billNumber = resPaypointSkyTVPaymentInfo.billNumber,
                refStan = resPaypointSkyTVPaymentInfo.refStan,
                amount = resPaypointSkyTVPaymentInfo.amount,
                transactionDate = resPaypointSkyTVPaymentInfo.transactionDate,
                customerName = resPaypointSkyTVPaymentInfo.customerName,
                companyCode = resPaypointSkyTVPaymentInfo.companyCode,
                UserName = resPaypointSkyTVPaymentInfo.UserName,
                ClientCode = resPaypointSkyTVPaymentInfo.ClientCode,
                Mode = "SkyTV" //Response vianet payment from Checkpayment

            };
            return objresPaypointMeroTVPaymentModel.ResponsePaypointVianetPaymentInfo(objresPaypointMeroTVPaymentInfo);
        }
        #endregion

        #region"Response CP WebSurfer payments"
        public static int PaypointWebSurferPaymentInfo(PaypointModel resPaypointWebSurferPaymentInfo)
        {
            var objresPaypointWebSurferPaymentModel = new PaypointUserModel();
            var objresPaypointWebSurferPaymentInfo = new PaypointModel
            {
                description = resPaypointWebSurferPaymentInfo.description,
                amountP = resPaypointWebSurferPaymentInfo.amountP,
                PackageId = resPaypointWebSurferPaymentInfo.PackageId,
                smartCards = "",
                ftthUser = resPaypointWebSurferPaymentInfo.ftthUser,
                reserveInfo = resPaypointWebSurferPaymentInfo.reserveInfo,
                billNumber = resPaypointWebSurferPaymentInfo.billNumber,
                refStan = resPaypointWebSurferPaymentInfo.refStan,
                amount = resPaypointWebSurferPaymentInfo.amount,
                transactionDate = resPaypointWebSurferPaymentInfo.transactionDate,
                customerName = resPaypointWebSurferPaymentInfo.customerName,
                companyCode = resPaypointWebSurferPaymentInfo.companyCode,
                UserName = resPaypointWebSurferPaymentInfo.UserName,
                ClientCode = resPaypointWebSurferPaymentInfo.ClientCode,
                Mode = "WebSurfer" //Response websurfer payment from Checkpayment

            };
            return objresPaypointWebSurferPaymentModel.ResponsePaypointVianetPaymentInfo(objresPaypointWebSurferPaymentInfo);
        }
        #endregion

        #region"Response CP ArrowNet payments"
        public static int PaypointArrowNetPaymentInfo(PaypointModel resPaypointArrowNetPaymentInfo)
        {
            var objresPaypointArrowNetPaymentModel = new PaypointUserModel();
            var objresPaypointArrowNetPaymentInfo = new PaypointModel
            {
                description = resPaypointArrowNetPaymentInfo.description,
                amountP = resPaypointArrowNetPaymentInfo.amountP,
                PackageId = resPaypointArrowNetPaymentInfo.PackageId,
                smartCards = "",
                ftthUser = "",
                reserveInfo = resPaypointArrowNetPaymentInfo.reserveInfo,
                billNumber = resPaypointArrowNetPaymentInfo.billNumber,
                refStan = resPaypointArrowNetPaymentInfo.refStan,
                amount = resPaypointArrowNetPaymentInfo.amount,
                transactionDate = resPaypointArrowNetPaymentInfo.transactionDate,
                customerName = resPaypointArrowNetPaymentInfo.customerName,
                companyCode = resPaypointArrowNetPaymentInfo.companyCode,
                UserName = resPaypointArrowNetPaymentInfo.UserName,
                ClientCode = resPaypointArrowNetPaymentInfo.ClientCode,
                Mode = "ArrowNet" //Response ArrowNet payment from Checkpayment

            };
            return objresPaypointArrowNetPaymentModel.ResponsePaypointVianetPaymentInfo(objresPaypointArrowNetPaymentInfo);
        }
        #endregion

        #region"Response CP DishHome Online payments"
        public static int PaypointDishHomeInfo(PaypointModel resPaypointVianetPaymentInfo)
        {
            var objresPaypointDishHomePaymentModel = new PaypointUserModel();
            var objresPaypointDishHomeOnlinePaymentInfo = new PaypointModel
            {


                description = resPaypointVianetPaymentInfo.description,
                amountP = resPaypointVianetPaymentInfo.amountP,
                Bonus = resPaypointVianetPaymentInfo.Bonus,
                PackageId = resPaypointVianetPaymentInfo.PackageId,
                billNumber = resPaypointVianetPaymentInfo.billNumber,
                refStan = resPaypointVianetPaymentInfo.refStan,
                amount = resPaypointVianetPaymentInfo.amount,
                transactionDate = resPaypointVianetPaymentInfo.transactionDate,
                customerName = resPaypointVianetPaymentInfo.customerName,
                companyCode = resPaypointVianetPaymentInfo.companyCode,
                UserName = resPaypointVianetPaymentInfo.UserName,
                ClientCode = resPaypointVianetPaymentInfo.ClientCode,
                Mode = "RSDishHomeOnline" //Response vianet payment from Checkpayment

            };
            return objresPaypointDishHomePaymentModel.ResponsePaypointDishHomeOnlinePaymentInfo(objresPaypointDishHomeOnlinePaymentInfo);
        }
        #endregion

        #region"Response CP Utility Online payments"
        public static int PaypointUtilityNCellInfo(PaypointModel resPaypointTopUpInfo)
        {
            var objresPaypointNcellPaymentModel = new PaypointUserModel();
            var objresPaypointNcellPaymentInfo = new PaypointModel
            {

                description = "",
                amountP = "",
                billNumber = resPaypointTopUpInfo.billNumber,
                refStan = resPaypointTopUpInfo.refStan,
                amount = resPaypointTopUpInfo.amount,
                transactionDate = resPaypointTopUpInfo.transactionDate,
                customerName = resPaypointTopUpInfo.customerName,
                companyCode = resPaypointTopUpInfo.companyCode,
                UserName = resPaypointTopUpInfo.UserName,
                ClientCode = resPaypointTopUpInfo.ClientCode,
                serviceCode = resPaypointTopUpInfo.serviceCode,
                paypointType = resPaypointTopUpInfo.paypointType,
                Mode = "RSNcell" //Response Ncell payment from Checkpayment

            };
            return objresPaypointNcellPaymentModel.ResponsePaypointUtilityInfo(objresPaypointNcellPaymentInfo);
        }
        #endregion

        #region"Response CP Utility Online payments"
        public static int PaypointUtilityNTCInfo(PaypointModel resPaypointTopUpInfo)
        {
            var objresPaypointNTCPaymentModel = new PaypointUserModel();
            var objresPaypointNTCPaymentInfo = new PaypointModel
            {

                description = resPaypointTopUpInfo.description,
                amountP = resPaypointTopUpInfo.amountP,
                billNumber = resPaypointTopUpInfo.billNumber,
                refStan = resPaypointTopUpInfo.refStan,
                amount = resPaypointTopUpInfo.amount,
                transactionDate = resPaypointTopUpInfo.transactionDate,
                customerName = resPaypointTopUpInfo.customerName,
                companyCode = resPaypointTopUpInfo.companyCode,
                UserName = resPaypointTopUpInfo.UserName,
                ClientCode = resPaypointTopUpInfo.ClientCode,
                serviceCode = resPaypointTopUpInfo.serviceCode,
                paypointType = "",
                Mode = "RSNTC" //Response NTC payment from Checkpayment

            };
            return objresPaypointNTCPaymentModel.ResponsePaypointUtilityInfo(objresPaypointNTCPaymentInfo);
        }
        #endregion

        #region"Response CP Utility Online payments"
        public static int PaypointUtilityNTCCDMAInfo(PaypointModel resPaypointTopUpInfo)
        {
            var objresPaypointNTCPaymentModel = new PaypointUserModel();
            var objresPaypointNTCPaymentInfo = new PaypointModel
            {

                description = "",
                amountP = "",
                billNumber = resPaypointTopUpInfo.billNumber,
                refStan = resPaypointTopUpInfo.refStan,
                amount = resPaypointTopUpInfo.amount,
                transactionDate = resPaypointTopUpInfo.transactionDate,
                customerName = resPaypointTopUpInfo.customerName,
                companyCode = resPaypointTopUpInfo.companyCode,
                UserName = resPaypointTopUpInfo.UserName,
                ClientCode = resPaypointTopUpInfo.ClientCode,
                serviceCode = resPaypointTopUpInfo.serviceCode,
                paypointType = "",
                Mode = "RSNTCCDMA" //Response NTC CDMA payment from Checkpayment

            };
            return objresPaypointNTCPaymentModel.ResponsePaypointUtilityInfo(objresPaypointNTCPaymentInfo);
        }
        #endregion

        #region"Response CP Utility Online payments"
        public static int PaypointUtilitySmartCellTopUpInfo(PaypointModel resPaypointTopUpInfo)
        {
            var objresPaypointNTCPaymentModel = new PaypointUserModel();
            var objresPaypointNTCPaymentInfo = new PaypointModel
            {

                description = "",
                amountP = "",
                billNumber = resPaypointTopUpInfo.billNumber,
                refStan = resPaypointTopUpInfo.refStan,
                amount = resPaypointTopUpInfo.amount,
                transactionDate = resPaypointTopUpInfo.transactionDate,
                customerName = resPaypointTopUpInfo.customerName,
                companyCode = resPaypointTopUpInfo.companyCode,
                UserName = resPaypointTopUpInfo.UserName,
                ClientCode = resPaypointTopUpInfo.ClientCode,
                serviceCode = resPaypointTopUpInfo.serviceCode,
                paypointType = "",
                Mode = "RSSCTopUp" //Response SmartCell TopUp payment from Checkpayment

            };
            return objresPaypointNTCPaymentModel.ResponsePaypointUtilityInfo(objresPaypointNTCPaymentInfo);
        }
        #endregion

        #region"Response CP Utility Online payments"
        public static int PaypointUtilitySmartCellEPINInfo(PaypointModel resPaypointTopUpInfo)
        {
            var objresPaypointNTCPaymentModel = new PaypointUserModel();
            var objresPaypointNTCPaymentInfo = new PaypointModel
            {

                description = "",
                amountP = "",
                billNumber = resPaypointTopUpInfo.billNumber,
                refStan = resPaypointTopUpInfo.refStan,
                amount = resPaypointTopUpInfo.amount,
                transactionDate = resPaypointTopUpInfo.transactionDate,
                customerName = resPaypointTopUpInfo.customerName,
                companyCode = resPaypointTopUpInfo.companyCode,
                UserName = resPaypointTopUpInfo.UserName,
                ClientCode = resPaypointTopUpInfo.ClientCode,
                serviceCode = resPaypointTopUpInfo.serviceCode,
                paypointType = "",
                Mode = "RSSCEPIN" //Response SmartCell TopUp payment from Checkpayment

            };
            return objresPaypointNTCPaymentModel.ResponsePaypointUtilityInfo(objresPaypointNTCPaymentInfo);
        }
        #endregion

        #region"Response CP DishHome Online payments"
        public static int PaypointDishHomePinInfo(PaypointModel resPaypointVianetPaymentInfo)
        {
            var objresPaypointDishHomePaymentModel = new PaypointUserModel();
            var objresPaypointDishHomeOnlinePaymentInfo = new PaypointModel
            {


                description = "",
                amountP = "",
                Bonus = "",
                PackageId = "",
                billNumber = resPaypointVianetPaymentInfo.billNumber,
                refStan = resPaypointVianetPaymentInfo.refStan,
                amount = resPaypointVianetPaymentInfo.amount,
                transactionDate = resPaypointVianetPaymentInfo.transactionDate,
                customerName = resPaypointVianetPaymentInfo.customerName,
                companyCode = resPaypointVianetPaymentInfo.companyCode,
                UserName = resPaypointVianetPaymentInfo.UserName,
                ClientCode = resPaypointVianetPaymentInfo.ClientCode,
                Mode = "RSDishHomePin" //Response vianet payment from Checkpayment

            };
            return objresPaypointDishHomePaymentModel.ResponsePaypointDishHomeOnlinePaymentInfo(objresPaypointDishHomeOnlinePaymentInfo);
        }
        #endregion


        #region Response excute payment

        public static int ResponseEPPaypointDHPinInfo(PaypointModel resEPPaypointInfo)
        {
            var objresEPaypointModel = new PaypointUserModel();
            var objresEPaypointInfo = new PaypointModel
            {
                companyCode = resEPPaypointInfo.companyCodeResEP,
                serviceCode = resEPPaypointInfo.serviceCodeResEP,
                account = resEPPaypointInfo.accountResEP,
                special1 = resEPPaypointInfo.special1ResEP,
                special2 = resEPPaypointInfo.special2ResEP,

                transactionDate = resEPPaypointInfo.transactionDateResEP,
                transactionId = resEPPaypointInfo.transactionIdResEP,
                refStan = resEPPaypointInfo.refStanResEP,
                amount = resEPPaypointInfo.amountResEP,
                billNumber = resEPPaypointInfo.billNumberResEP,

                userId = resEPPaypointInfo.userIdResEP,
                userPassword = resEPPaypointInfo.userPasswordResEP,
                salePointType = resEPPaypointInfo.salePointTypeResEP,
                retrievalReference = resEPPaypointInfo.retrievalReferenceResEP,
                responseCode = resEPPaypointInfo.responseCodeResEP,
                description = resEPPaypointInfo.descriptionResEP,
                customerName = resEPPaypointInfo.customerNameResEP,
                UserName = resEPPaypointInfo.UserName,
                ClientCode = resEPPaypointInfo.ClientCode,
                paypointType = resEPPaypointInfo.paypointType,
                resultMessage = resEPPaypointInfo.resultMessageResEP,
                voucherCode= resEPPaypointInfo.voucherCode,
                id = resEPPaypointInfo.id,
                Mode = "RSDHPnInf"




            };
            return objresEPaypointModel.ResponsePaypointDHPinInfo(objresEPaypointInfo);
        }
        #endregion

        #region"Response CP BroadLink Online payments"
        public static int PaypointBroadLinkInfo(PaypointModel resPaypointVianetPaymentInfo)
        {
            var objresPaypointBroadLinkPaymentModel = new PaypointUserModel();
            var objresPaypointBroadLinkOnlinePaymentInfo = new PaypointModel
            {
                description = "",
                amountP = "",
                Bonus = "",
                PackageId = "",
                billNumber = resPaypointVianetPaymentInfo.billNumber,
                refStan = resPaypointVianetPaymentInfo.refStan,
                amount = resPaypointVianetPaymentInfo.amount,
                transactionDate = resPaypointVianetPaymentInfo.transactionDate,
                customerName = resPaypointVianetPaymentInfo.customerName,
                companyCode = resPaypointVianetPaymentInfo.companyCode,
                UserName = resPaypointVianetPaymentInfo.UserName,
                ClientCode = resPaypointVianetPaymentInfo.ClientCode,
                serviceCode = resPaypointVianetPaymentInfo.serviceCode,
                paypointType = resPaypointVianetPaymentInfo.paypointType,              
                Mode = "BroadLink" //Response broadlink payment from Checkpayment

            };
            return objresPaypointBroadLinkPaymentModel.ResponsePaypointUtilityInfo(objresPaypointBroadLinkOnlinePaymentInfo);
        }
        #endregion


    }
}