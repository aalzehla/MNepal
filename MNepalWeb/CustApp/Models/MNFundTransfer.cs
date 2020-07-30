﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustApp.Models
{
    public class MNFundTransfer//:ReplyMessage
    {
        public MNFundTransfer(string tid, string sc, string mobile, string sa, string amount, string da, string note, string pin, string sourcechannel)
        {
            this.tid = tid;
            this.sc = sc;
            this.mobile = mobile;
            this.sa = sa;
            this.amount = amount;
            this.da = da;
            this.pin = pin;
            this.note = note;
            this.sourcechannel = sourcechannel;
        } 
        
      

        public MNFundTransfer()
        {

        }

        public string tid { get; set; }
        public string mobile { get; set; }
        public string sc { get; set; }

        public string sa { get; set; }

        public string da { get; set; }

        public string amount { get; set; }

        public string pin { get; set; }
        public string note { get; set; }
        public string sourcechannel { get; set; }
        public string prod { get; set; }
       
        public bool valid()
        {
            if (this.tid != "" && this.sc != "" && this.mobile != "" && this.amount != "")
                return true;
            else
                return false;
        }

        /*For REMIT*/
        public string BeneficialName { get; set; }
        public string RequestTokenCode { get; set; }
        public string ClientCode { get; set; }

        /*For Token Redeem*/
        public string RemitCusSender { get; set; }

        public string RemitReceiver { get; set; }

        public string agentUserName { get; set; }

        public string transactionCode { get; set; }

        public string bankName { get; set; }

        public string TokenUnique { get; set; }
    }
}


