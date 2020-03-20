using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace MNepalWeb.Models
{
    public class MNANMMaster
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

    }
}