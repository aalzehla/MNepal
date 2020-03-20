using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{

    //Common Response Header
    public class ResponseHeader
    {
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public object finishDate { get; set; }
    }

    /*Begin eTopUp*/
    #region eTopUp
    public class EtopupRecord
    {

        public string amount { get; set; }
        public object channel { get; set; }
        public string dealerName { get; set; }
        public object doneCode { get; set; }
        public object endDate { get; set; }
        public string extTransId { get; set; }
        public string hold1 { get; set; }
        public object hold2 { get; set; }
        public string operationTime { get; set; }
        public object operationType { get; set; }
        public object @operator { get; set; }
        public string optType { get; set; }
        public object parentId { get; set; }
        public object phoneNumber { get; set; }
        public object phoneType { get; set; }
        public string recordId { get; set; }
        public object sourceDealer { get; set; }
        public string sourceNumber { get; set; }
        public object startDate { get; set; }
        public string targetDealer { get; set; }
        public string targetNumber { get; set; }
        public object transferType { get; set; }

    }
    public class eTopUpTransferResponse
    {
        public ETopupTransferRsp ETopupTransferRsp { get; set; }
        public ResponseHeader responseHeader { get; set; }
    }
    public class ETopupTransferRsp
    {
        public EtopupRecord etopupRecord { get; set; }
        public string respCode { get; set; }
        public string respDesc { get; set; }
    }
    #endregion eTopUp
    /*End eTopUp*/

    /*begin Balance Query*/
    #region BalanceQuery
    public class BalanceInfoRsp
    {
        public string balanceAmount { get; set; }
        public string balanceShow { get; set; }
        public string measureId { get; set; }
        public string respCode { get; set; }
        public string respDesc { get; set; }
    }
    public class BalanceQueryResponse
    {
        public BalanceInfoRsp balanceInfoRsp { get; set; }
        public ResponseHeader responseHeader { get; set; }
    }

    #endregion BalanceQuery
    /*end Balance Query*/

    /*Begin CheckDealer*/
    #region CheckDealer
    public class DealerLogin
    {
        public string accountNo { get; set; }
        public object address1 { get; set; }
        public object address2 { get; set; }
        public string amount { get; set; }
        public string balanceMaximum { get; set; }
        public string balanceMinimum { get; set; }
        public string businessType { get; set; }
        public string contactPerson { get; set; }
        public string contactPhone { get; set; }
        public string createDate { get; set; }
        public string dataSource { get; set; }
        public string dealerId { get; set; }
        public string dealerLevel { get; set; }
        public string dealerName { get; set; }
        public string dealerType { get; set; }
        public string district { get; set; }
        public string doneDate { get; set; }
        public string mposHp { get; set; }
        public string notes { get; set; }
        public string opId { get; set; }
        public string orgId { get; set; }
        public string owner { get; set; }
        public string panNo { get; set; }
        public string parentId { get; set; }
        public string reNumber { get; set; }
        public string recarSingleMax { get; set; }
        public string recarSingleMin { get; set; }
        public string region { get; set; }
        public string regionLevel { get; set; }
        public string registrationNo { get; set; }
        public string sreNumber { get; set; }
        public string status { get; set; }
        public string sysOpId { get; set; }
        public string tradePassword { get; set; }
        public string tranSingleMax { get; set; }
        public string tranSingleMin { get; set; }
        public string zone { get; set; }
    }

    public class CheckDealerRsp
    {
        public DealerLogin dealerLogin { get; set; }
        public string respCode { get; set; }
        public string respDesc { get; set; }
    }

    public class CheckDealerResponse
    {
        public CheckDealerRsp checkDealerRsp { get; set; }
        public ResponseHeader responseHeader { get; set; }
    }
    #endregion CheckDealer
    /*End CheckDealer*/

}