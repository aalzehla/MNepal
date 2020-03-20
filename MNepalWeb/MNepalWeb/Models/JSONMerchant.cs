using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace MNepalWeb.Models
{
    public class JSONMerchant
    {
        public int mid { get; set; }
        public string mname { get; set; }
    }
}