using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MNepalProject.Models
{
    public class MNStatus
    {
        [Key]
        public int ID { get; set; }

        public string StatusName { get; set; }

        public string Description { get; set; }
    }
}