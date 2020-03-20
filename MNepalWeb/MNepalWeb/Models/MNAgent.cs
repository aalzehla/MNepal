using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MNepalWeb.Models
{
    public class MNAgent
    {
        [Key]
        public int ID { get; set; }

        public int ANMId { get; set; }
        [ForeignKey("ANMId")]
        public MNANMMaster AgentMaster { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(8)]
        public string ClientCode { get; set; }
    }
}