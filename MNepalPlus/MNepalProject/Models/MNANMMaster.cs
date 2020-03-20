using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using MNepalProject.Models;
using System.ComponentModel.DataAnnotations;

namespace MNepalProject.Models
{
    public class MNANMMaster
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

    }
}