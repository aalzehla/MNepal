﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class PaypointModel
    {
        #region Request Check Payment 
        public string companyCodeReqCP { get; set; }
        public string serviceCodeReqCP { get; set; }
        public string accountReqCP { get; set; }
        public string special1ReqCP { get; set; }
        public string special2ReqCP { get; set; }


        public string transactionDateReqCP { get; set; }
        public string transactionIdReqCP { get; set; }
        public string refStanReqCP { get; set; }
        public string amountReqCP { get; set; }
        public string billNumberReqCP { get; set; }


        public string userIdReqCP { get; set; }
        public string userPasswordReqCP { get; set; }
        public string salePointTypeReqCP { get; set; }
        public string retrievalReferenceReqCP { get; set; }
        public string remarkReqCP { get; set; }
        public string customerNameCP { get; set; }

        #endregion
        #region check payment response
        public string companyCodeResCP { get; set; }
        public string serviceCodeResCP { get; set; }
        public string accountResCP { get; set; }
        public string special1ResCP { get; set; }
        public string special2ResCP { get; set; }

        public string transactionDateResCP { get; set; }
        public string transactionIdResCP { get; set; }
        public string refStanResCP { get; set; }
        public string amountResCP { get; set; }
        public string billNumberResCP { get; set; }


        public string userIdResCP { get; set; }
        public string userPasswordResCP { get; set; }
        public string salePointTypeResCP { get; set; }
        public string retrievalReferenceResCP { get; set; }
        public string responseCodeResCP { get; set; }
        public string descriptionResCP { get; set; }
        public string resultMessageResCP { get; set; }



        #endregion




        #region for excute payment request
        public string companyCodeReqEP { get; set; }
        public string serviceCodeReqEP { get; set; }
        public string accountReqEP { get; set; }
        public string special1ReqEP { get; set; }
        public string special2ReqEP { get; set; }


        public string transactionDateReqEP { get; set; }
        public string transactionIdReqEP { get; set; }
        public string refStanReqEP { get; set; }
        public string amountReqEP { get; set; }
        public string billNumberReqEP { get; set; }


        public string userIdReqEP { get; set; }
        public string userPasswordReqEP { get; set; }
        public string salePointTypeReqEP { get; set; }
        public string retrievalReferenceReqEP { get; set; }
        public string remarkReqEP { get; set; }
        #endregion

        #region for excute payment response
        public string companyCodeResEP { get; set; }
        public string serviceCodeResEP { get; set; }
        public string accountResEP { get; set; }
        public string special1ResEP { get; set; }
        public string special2ResEP { get; set; }

        public string transactionDateResEP { get; set; }
        public string transactionIdResEP { get; set; }
        public string refStanResEP { get; set; }
        public string amountResEP { get; set; }
        public string billNumberResEP { get; set; }


        public string userIdResEP { get; set; }
        public string userPasswordResEP { get; set; }
        public string salePointTypeResEP { get; set; }
        public string retrievalReferenceResEP { get; set; }
        public string responseCodeResEP { get; set; }
        public string descriptionResEP { get; set; }
        public string resultMessageResEP { get; set; }




        public string customerNameResEP { get; set; }






        #endregion




        #region for get transaction  payment request
        public string companyCodeReqGTP { get; set; }
        public string serviceCodeReqGTP { get; set; }
        public string accountReqGTP { get; set; }
        public string special1ReqGTP { get; set; }
        public string special2ReqGTP { get; set; }


        public string transactionDateReqGTP { get; set; }
        public string transactionIdReqGTP { get; set; }
        public string refStanReqGTP { get; set; }
        public string amountReqGTP { get; set; }
        public string billNumberReqGTP { get; set; }


        public string userIdReqGTP { get; set; }
        public string userPasswordReqGTP { get; set; }
        public string salePointTypeReqGTP { get; set; }
        public string retrievalReferenceReqGTP { get; set; }
        public string remarkReqGTP { get; set; }
        #endregion

        #region for Get Transaction payment response
        public string companyCodeResGTP { get; set; }
        public string serviceCodeResGTP { get; set; }
        public string accountResGTP { get; set; }
        public string special1ResGTP { get; set; }
        public string special2ResGTP { get; set; }

        public string transactionDateResGTP { get; set; }
        public string transactionIdResGTP { get; set; }
        public string refStanResGTP { get; set; }
        public string amountResGTP { get; set; }
        public string billNumberResGTP { get; set; }


        public string userIdResGTP { get; set; }
        public string userPasswordResGTP { get; set; }
        public string salePointTypeResGTP { get; set; }
        public string retrievalReferenceResGTP { get; set; }
        public string responseCodeResGTP { get; set; }
        public string descriptionResGTP { get; set; }
        public string resultMessageResGTP { get; set; }





        public string statusResGTP { get; set; }
        public string customerNameResGTP { get; set; }




        #endregion



        //for excute payment

        public string companyCode { get; set; }
        public string serviceCode { get; set; }
        public string account { get; set; }
        public string special1 { get; set; }
        public string special2 { get; set; }


 		public string PackageId { get; set; }
        public string Bonus { get; set; }


        public string transactionDate { get; set; }
        public string transactionId { get; set; }
        public string refStan { get; set; }
        public string amount { get; set; }
        public string billNumber { get; set; }


        public string userId { get; set; }
        public string userPassword { get; set; }
        public string salePointType { get; set; }

        public string retrievalReference { get; set; }
        public string responseCode { get; set; }
        public string description { get; set; }
        public string customerName { get; set; }

        public string remarks { get; set; }
        public string Mode { get; set; }
        public string UserName { get; set; }
        public string ClientCode { get; set; }
        public string paypointType { get; set; }
        public string resultMessage { get; set; }
        public string RemainingDays { get; set; }
        public string voucherCode { get; set; }
        public string id { get; set; }
        public string smartCards { get; set; }
        public string ftthUser { get; set; }
        public string reserveInfo { get; set; }
        public string commissionAmount { get; set; }



























        //for check payment
        public string companyCodeCP { get; set; }
        public string serviceCodeCP { get; set; }
        public string accountCP { get; set; }
        public string special1CP { get; set; }
        public string special2CP { get; set; }





        public string transactionDateCP { get; set; }
        public string transactionIdCP { get; set; }
        public string refStanCP { get; set; }
        public string amountCP { get; set; }
        public string billNumberCP { get; set; }


        public string userIdCP { get; set; }
        public string userPasswordCP { get; set; }
        public string salePointTypeCP { get; set; }

        public string retrievalReferenceCP { get; set; }




        #region "payments"
        public string descriptionP { get; set; }
        public string billDateP { get; set; }
        public string billAmountP { get; set; }
        public string amountP { get; set; }
        public string totalAmountP { get; set; }





        public string statusP { get; set; }
        public string amountfactP { get; set; }
        public string amountmaskP { get; set; }
        public string amountmaxP { get; set; }
        public string amountminP { get; set; }


        public string amountstepP { get; set; }
        public string codservP { get; set; }
        public string commissionP { get; set; }

        public string commisvalueP { get; set; }

        public string destinationP { get; set; }

        public string iP { get; set; }
        public string idP { get; set; }

        public string jP { get; set; }

        public string requestIdP { get; set; }

        public string show_counterP { get; set; }
        public string i_countP { get; set; }
        #endregion



        #region"Response Checkpayment for khanepani invoice"
        public string statusKI { get; set; }
        public string total_advance_amountKI { get; set; }
        public string customer_codeKI { get; set; }
        public string addressKI { get; set; }
        public string total_credit_sales_amountKI { get; set; }





        public string customer_nameKI { get; set; }
        public string current_month_duesKI { get; set; }
        public string mobile_numberKI { get; set; }
        public string total_duesKI { get; set; }
        public string previous_duesKI { get; set; }


        public string current_month_discountKI { get; set; }
        public string current_month_fineKI { get; set; }

        #endregion


        #region"Response Checkpayment nepal waterpayment


        public string legatNumberP { get; set; }
        public string discountAmountP { get; set; }
        public string counterRentP { get; set; }
        public string fineAmountP { get; set; }
        public string billDateFromP { get; set; }

        public string billDateToP { get; set; }
        public string fioP { get; set; }


        #endregion




//START EGTP
        #region"nepal water get transaction response all


        public string ResultResGTPAll { get; set; }
        public string ResponseKeyResGTPAll { get; set; }
        public string RequestKeyResGTPAll { get; set; } 
        public string ResultKeyResGTPAll { get; set; }// not imp
        public string StanResGTPAll { get; set; }
        public string RefStanResGTPAll { get; set; }



        public string ExternalStanResGTPAll { get; set; }
        public string CompanyIDResGTPAll { get; set; }
        public string CompanyNameResGTPAll { get; set; }
        public string ServiceCodeResGTPAll { get; set; }
        public string ServiceNameResGTPAll { get; set; }




        public string AccountResGTPAll { get; set; }
        public string CurrencyResGTPAll { get; set; }
        public string CurrencyCodeResGTPAll { get; set; }
        public string AmountResGTPAll { get; set; }
        public string CommissionAmountResGTPAll { get; set; }



        public string BillNumberResGTPAll { get; set; }
        public string UserLoginResGTPAll { get; set; }
        public string SalesPointTypeResGTPAll { get; set; }
        public string StatusResGTPAll { get; set; }
        public string RegDateResGTPAll { get; set; }





        public string PaymentIdResGTPAll { get; set; }
        public string DealerIdResGTPAll { get; set; }
        public string DealerNameResGTPAll { get; set; }
        public string ResponseCodeResGTPAll { get; set; }
        public string PaySourceTypeResGTPAll { get; set; }




        public string CityResGTPAll { get; set; }
        public string AddressResGTPAll { get; set; }
        public string CloseDateResGTPAll { get; set; }
        public string ProblemResGTPAll { get; set; }















        public string Result { get; set; }
        public string ResponseKey { get; set; }
        public string RequestKey { get; set; }
        public string ResultKey { get; set; }//not imp
        public string Stan { get; set; }
        public string RefStan { get; set; }



        public string ExternalStan { get; set; }
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }




        public string Account { get; set; }
        public string Currency { get; set; }
        public string CurrencyCode { get; set; }
        public string Amount { get; set; }
        public string CommissionAmount { get; set; }


        
        public string BillNumber { get; set; }
        public string UserLogin { get; set; }
        public string SalesPointType { get; set; }
        public string Status { get; set; }
        public string RegDate { get; set; }


        


        public string PaymentId { get; set; }
        public string DealerId { get; set; }
        public string DealerName { get; set; }
        public string ResponseCode { get; set; }
        public string PaySourceType { get; set; }


         

        public string City { get; set; }
        public string Address { get; set; }
        public string CloseDate { get; set; }
        public string Problem { get; set; }
         
         


        #endregion

 
 #region"Nea Execute payement details (for eg. 013.01.002)

        public string ResultResEPAll { get; set; }
        public string ResponseKeyResEPAll { get; set; }
        public string ResultMessageResEPAll { get; set; }
        public string AccountResEPAll { get; set; }
        public string CompanyResEPAll { get; set; }




        public string SumAmountResEPAll { get; set; }
        public string CommissionResEPAll { get; set; }
        public string OfficeNameResEPAll { get; set; }
        public string CustsomerIDResEPAll { get; set; }
        public string CustomerNameResEPAll { get; set; }






        public string DueBillOfResEPAll { get; set; }
        public string BillDateResEPAll { get; set; }
        public string InvoiceAmountResEPAll { get; set; }
        public string PenaltyStatusResEPAll { get; set; }
        public string PenaltyAmountResEPAll { get; set; }

        #endregion

         //END EGTP


    }
}