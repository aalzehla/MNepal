using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustApp.Models
{
    public class MNAgentStatement
    {
        //start milayako 01
        public string ContactNumber1 { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        //public string TranId { get; set; }

        public string TranDate { get; set; }
        public string TranDesc { get; set; }

        public string TranID { get; set; }

        public string Date { get; set; }
       
        public string Debit { get; set; }
        public string Credit { get; set; }
        //
        public string Balance { get; set; }
        
        //end milayako 01
        public int SNo { get; set; }

        public string UserName { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public string BranchCode { get; set; }

        public string Description { get; set; }

        public string Code { get; set; }

        public string Remarks { get; set; }

        public string AccType { get; set; }

        public DateTime? TimeStamp { get; set; }



    }
}