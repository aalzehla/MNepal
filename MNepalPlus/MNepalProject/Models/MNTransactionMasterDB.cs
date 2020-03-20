using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalProject.Models;
using System.Net;

namespace MNepalProject.Models
{
    public class MNTransactionMasterDB
    {
        public MNTransactionMasterDB(MNTransactionMaster transactionm)
        {
            this.TraceId = transactionm.TraceId;
            this.FeatureCode = transactionm.FeatureCode;

            this.SourceMobile = transactionm.SourceMobile;
            this.SourceAccount = transactionm.SourceAccount;
            this.SourceBIN = transactionm.SourceBIN;
            this.SourceBranchCode = transactionm.SourceBranchCode;

            this.DestinationMobile = transactionm.DestinationMobile;
            this.DestinationAccount = transactionm.DestinationAccount;
            this.DestinationBIN = transactionm.DestinationBIN;
            this.DestinationBranchCode = transactionm.DestinationBranchCode;

            this.RequestChannel = transactionm.RequestChannel;
            this.ResponseChannel = transactionm.ResponseChannel;

            this.CreatedDate = DateTime.Now;
            this.Amount = transactionm.Amount;
            this.Description = transactionm.Description;
            this.StatusId = transactionm.StatusId;
            this.PumoriId = 1;
            this.FeeId = transactionm.FeeId;
        }

        [Key]
        public int ID { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(12)]
        public string TraceId { get; set; }

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

        [Column(TypeName = "VARCHAR")]
        [StringLength(5)]
        public string SourceBranchCode { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(10)]
        public string DestinationMobile { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(20)]
        public string DestinationAccount { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string DestinationBIN { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(5)]
        public string DestinationBranchCode { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }

        //[Column(TypeName = "VARCHAR")]
        // [StringLength(4)]
        // public string PIN { get; set; }

        public float Amount { get; set; }

        [Column(TypeName = "VARCHAR(MAX)")]
        public string Description { get; set; }

        public int StatusId { get; set; }
        public int PumoriId { get; set; }

        [Column(TypeName = "VARCHAR(4)")]
        public string FeeId { get; set; }
        public double FeeAmount { get; set; }

        //to be added into db
        public string RequestChannel { get; set; }
        public string ResponseChannel { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }

        //to be added into db
        public int ProductId { get; set; }
    }



}