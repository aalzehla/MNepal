using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalWeb.Models;
using System.Net;

namespace MNepalWeb.Models
{
    public class MNTransactionLog
    {

        public MNTransactionLog(MNTransactionLog mntransactionlog)
        {
            this.TransactionId = mntransactionlog.TransactionId;
            this.UpdatedDate = mntransactionlog.UpdatedDate;
            this.StatusId = mntransactionlog.StatusId;
            this.Description = mntransactionlog.Description;

        }

        public MNTransactionLog(int TransactionId,DateTime UpdatedDate,int StatusId,string Description)
        {
            this.TransactionId = TransactionId;
            this.UpdatedDate = UpdatedDate;
            this.StatusId = StatusId;
            this.Description = Description;

        }


        public MNTransactionLog()
        {

        }
          
        [Key]
        public int ID { get; set;}

        public int TransactionId {get;set;}

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedDate { get; set; }

        public int StatusId{get;set;}

        public string Description{get;set;}
    }




}