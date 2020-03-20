using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalWeb;
using MNepalWeb.Models;
using System.ComponentModel.DataAnnotations;


namespace MNepalWeb.Models
{
    public class MNComAndFocusOneLog
    {

        [Key]
        public int ID { get; set; }
        [StringLength(12)]
        public string TraceId { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public int InOutFlag { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }
        
        

        public MNComAndFocusOneLog(string tid,int statusCode, string message, string ioflag)
        {
            TraceId = tid;
            StatusCode = statusCode;
            this.Message = message;
            InOutFlag = int.Parse(ioflag);
        }

        public MNComAndFocusOneLog(int statusCode, string message, string ioflag,DateTime createdDate)
        { 
            StatusCode = statusCode;
            this.Message = message;
            InOutFlag = int.Parse(ioflag);
            CreatedDate = createdDate;
        }

        public MNComAndFocusOneLog(string tid, int statusCode, string message, string ioflag, DateTime createdDate)
        {
            TraceId = tid;
            StatusCode = statusCode;
            this.Message = message;
            InOutFlag = int.Parse(ioflag);
            CreatedDate = createdDate;
        }

        public MNComAndFocusOneLog(MNFundTransfer fundtransfer,DateTime createdDate)
        {
            this.TraceId = fundtransfer.tid;
            this.StatusCode = 0;
            this.InOutFlag = 1;
            var msg = new Message();
            msg.createMessage(fundtransfer);
            this.Message = msg.jsonMessage;
            CreatedDate = createdDate;
        }

        public MNComAndFocusOneLog(BalanceQuery balancequery, DateTime createdDate)
        {
            this.TraceId = balancequery.tid;
            this.StatusCode = 0;
            this.InOutFlag = 1;
            var msg = new Message();
            msg.createMessage(balancequery);
            this.Message = msg.jsonMessage;
            CreatedDate = createdDate;
        }

     
    }
}