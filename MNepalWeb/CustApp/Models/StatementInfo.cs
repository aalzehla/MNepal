using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustApp.Models
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
        //start milayako 01
        public StatementInfo( string mobileno,  string StartDate, string EndDate)
        { 
            this.mobileno = mobileno;
            this.StartDate = StartDate;
            this.EndDate = EndDate; 
        } 

        public StatementInfo()
        {

        } 

        public string mobileno { get; set; }
       
        //end milayako 01
    }
}