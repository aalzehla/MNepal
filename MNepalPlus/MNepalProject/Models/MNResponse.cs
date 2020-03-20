using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MNepalProject.Models
{
    public class MNResponse
    {
        [Key]
        public int ID { get; set; }

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
        [StringLength(11)]
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


        public float FeeAmount { get; set; }

        public string message { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(6)]
        public string TraceId { get; set; }

        public DateTime TranDate { get; set; }

       
        public TimeSpan TranTime { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(12)]
        public string RetrievalRef { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(6)]
        public string ResponseCode { get; set; }

        [Column(TypeName = "VARCHAR(MAX)")]
        public string ResponseDescription { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(120)]
        public string Balance { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string AcHolderName { get; set; }

        [Column(TypeName = "VARCHAR(MAX)")]
        public string MiniStmntRec { get; set; }

        [Column(TypeName = "char")]
        [StringLength(1)]
        public char ReversalStatus { get; set; }



        public MNResponse(string OriginID,string OriginType, string ServiceCode,string DestBankCode, string DestBranchCode, string DestAccountNo, float Amount, string AcHolderName, string ResponseDescription)
        {
            this.OriginID = OriginID;
            this.OriginType = OriginType;
            this.ServiceCode = ServiceCode;
            this.DestBankCode = DestBankCode;
            this.DestBranchCode = DestBranchCode;
            this.DestAccountNo = DestAccountNo;
            this.Amount=Amount;
            this.AcHolderName=AcHolderName;
            this.ResponseDescription = ResponseDescription;
                     
        }

        public MNResponse()
        { }
    }

}