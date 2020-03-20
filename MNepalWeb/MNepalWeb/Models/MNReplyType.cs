using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalWeb;
using MNepalWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MNepalWeb.Models
{
    public class MNReplyType
    {
        public MNReplyType(string tid,string replyType)
        {
            this.ReplyType = replyType;
            this.TraceId = tid;
        }

        public int ID { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string ReplyType { get; set; }

        public string TraceId { get; set; }
    }
} 