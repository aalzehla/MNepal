using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalWeb.Models
{
    public class StatementInfo
    {
        public string MainCode
        {
            get;
            set;
        }
        public string StartDate
        {
            get;
            set;
        }
        public string EndDate
        {
            get;
            set;
        }
        public string BranchCode
        {
            get;
            set;
        }

        public string TranDate
        {
            get;
            set;
        }
        
        public string Debit
        {
            get;
            set;
        }
        public string Credit
        {
            get;
            set;
        }
        public string Desc1
        {
            get;
            set;
        }
        public string Balance
        {
            get;
            set;
        }

    }
}