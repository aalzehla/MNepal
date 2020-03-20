using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MNepalProject.Models;
namespace MNepalProject.Models
{
    public class MNLimitMaster
    {
        [Key]
        public int ID { get; set; }
        public int Span { get; set; }
        public DateTime LimitStart { get; set; }
        public double Amount { get; set; }
        public int NoOfTransaction { get; set; }
    }
}