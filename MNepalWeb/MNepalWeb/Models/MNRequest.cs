using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace MNepalWeb.Models
{
    public class MNRequest
    {
        public MNRequest() { }
        //public MNRequest(string OriginID, string OriginType, string ServiceCode, string SourceBankCode, string SourceBranchCode, string SourceAccountNo, string DestBankCode, string DestBranchCode, string DestAccountNo, float Amount, string FeeId, string TraceNo, DateTime TranDate, string RetrievalRef, string Desc1, string Desc2,/* string Desc3, string OTraceNo, string OTranDateTime,*/ string IsProcessed)
        //{
        //    this.OriginID = OriginID;
        //    this.OriginType = OriginType;
        //    this.ServiceCode = ServiceCode;
        //    this.SourceBankCode = SourceBankCode;
        //    this.SourceBranchCode = SourceBranchCode;
        //    this.SourceAccountNo = SourceAccountNo;
        //    this.DestBankCode = DestBankCode;
        //    this.DestBranchCode = DestBranchCode;
        //    this.DestAccountNo = DestAccountNo;
        //    this.Amount = Amount;
        //    this.FeeId = FeeId;
        //    this.TraceNo = TraceNo.GetLast(6); 
        //    this.TranDate = TranDate;
        //    this.RetrievalRef = RetrievalRef.PadLeft(12, '0');
        //    this.Desc1 = Desc1;
        //    this.Desc2 = Desc2;
        //    /*this.Desc3 = Desc3;
        //    this.OTraceNo = OTraceNo;
        //    this.OTranDateTime = OTranDateTime;
        //    */
        //    this.IsProcessed = IsProcessed;
        //}

        //// For Coupon Payment
        //public MNRequest(string OriginID, string OriginType, string ServiceCode, string SourceBankCode, string SourceBranchCode, string SourceAccountNo, string DestBankCode, string DestBranchCode, string DestAccountNo, float Amount, string FeeId, string TraceNo, DateTime TranDate, string RetrievalRef, string Desc1, string Desc2,string Desc3, /*string OTraceNo, string OTranDateTime,*/ string IsProcessed)
        //{
        //    this.OriginID = OriginID;
        //    this.OriginType = OriginType;
        //    this.ServiceCode = ServiceCode;
        //    this.SourceBankCode = SourceBankCode;
        //    this.SourceBranchCode = SourceBranchCode;
        //    this.SourceAccountNo = SourceAccountNo;
        //    this.DestBankCode = DestBankCode;
        //    this.DestBranchCode = DestBranchCode;
        //    this.DestAccountNo = DestAccountNo;
        //    this.Amount = Amount;
        //    this.FeeId = FeeId;
        //    this.TraceNo = TraceNo.GetLast(6);
        //    this.TranDate = TranDate;
        //    this.RetrievalRef = RetrievalRef.PadLeft(12, '0');
        //    this.Desc1 = Desc1;
        //    this.Desc2 = Desc2;
        //    this.Desc3 = Desc3;
        //    /*this.OTraceNo = OTraceNo;
        //    this.OTranDateTime = OTranDateTime;
        //    */
        //    this.IsProcessed = IsProcessed;
        //}

        ////For New User Create
        //public MNRequest(string OriginID,string OriginType, string ServiceCode, string SourceBankCode, string SourceBranchCode, string SourceAccountNo, float Amount, string FeeId, string TraceNo, DateTime TranDate, string RetrievalRef, string Desc1, string IsProcessed)
        //{
        //    this.OriginID = OriginID;
        //    this.OriginType = OriginType;
        //    this.ServiceCode = ServiceCode;
        //    this.SourceBankCode = SourceBankCode;
        //    this.SourceBranchCode = SourceBranchCode;
        //    this.SourceAccountNo = SourceAccountNo;
        //    this.Amount = Amount;
        //    this.FeeId = FeeId;
        //    this.TraceNo = TraceNo.GetLast(6);
        //    this.TranDate = TranDate;
        //    this.RetrievalRef = RetrievalRef.PadLeft(12,'0');
        //    this.Desc1 = Desc1;
        //    this.IsProcessed = IsProcessed;
        //}

        [Column(TypeName = "VARCHAR")]
        [StringLength(16)]
        public string OriginID { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string OriginType { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(2)]
        public string ServiceCode { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string SourceBankCode { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(5)]
        public string SourceBranchCode { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(20)]
        public string SourceAccountNo { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string DestBankCode { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(5)]
        public string DestBranchCode { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(20)]
        public string DestAccountNo { get; set; }


        public float Amount { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string FeeId { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(6)]
        public string TraceNo { get; set; }


        public DateTime TranDate { get; set; }


        [Column(TypeName = "VARCHAR")]
        [StringLength(12)]
        public string RetrievalRef { get; set; }


        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string Desc1 { get; set; }


        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string Desc2 { get; set; }

       
        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string Desc3 { get; set; }

        /*

       [Column(TypeName = "VARCHAR")]
       [StringLength(6)]
       public string OTraceNo { get; set; }

       [Column(TypeName = "VARCHAR")]
       [StringLength(10)]
       public string OTranDateTime { get; set; }
       */
        public string IsProcessed { get; set; }

    }
}