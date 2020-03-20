using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalProject.Models;
using System.ComponentModel.DataAnnotations;
using PetaPoco;

namespace MNepalProject.Models
{
    public class JSONMerchant
    {
        public int mid { get; set; }
        public string mname { get; set; }
    }
}