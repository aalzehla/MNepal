using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WCF.MNepal.Models
{
    public class SyncDetail
    {
        [Key]
        public int UserID { get; set; }

        [StringLength(50)]
        public string Mobile { get; set; }

        [StringLength(50)]
        public string Tid { get; set; }

        public bool? IsActiveDevice { get; set; }

        public DateTime? LoginDate { get; set; }

        public DateTime? SyncDateTime { get; set; }

        public string Mode { get; set; }
    }
}