using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustApp.ViewModel
{
    public class MiniStatementVM
    {
        private string _Balance, _Credit, _Debit;
        public string Sno { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Amount { get; set; }
        public string TranId { get; set; }
        //public string ReferenceNo { get; set; }
        //public string TraceNo { get; set; }
        //public string TrasStatus { get; set; }
        //public string RetrievalRef { get; set; }
        
        public string Balance {
          get {
                return _Balance;
            }
            set
            {
                _Balance = string.Format("{0:n}", Decimal.Parse(value == "" ? "0" : value));
            }
       }
      
        public string Status { get; set; }

       
        public string Debit
        {
            get
            {
                return _Debit;
            }
            set
            {
                //if (value.ToCharArray().First()=='-')
                //{
                //    value=value.Substring(1);
                //}
              _Debit = string.Format("{0:n}", Decimal.Parse(value == "" ? "0" : value));
            }
        }

        
        public string Credit {
            get
            {
                return _Credit;
            }
            set
            {
                _Credit = string.Format("{0:n}", Decimal.Parse(value == "" ? "0" : value));
            }
        }

        public string Desc2 { get; set; }

        public string  TimeStamp{ get; set; }




    }
}