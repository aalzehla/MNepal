using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalProject.Models
{
    public class MiniStatement
    {
        public string date { get; set; }
        public string amount { get; set; }
        public string transactionType { get; set; }

        public MiniStatement(string d, string amt, string trantype)
        {
            date = d;
            amount = amt;
            transactionType = trantype;
        }
    }
}