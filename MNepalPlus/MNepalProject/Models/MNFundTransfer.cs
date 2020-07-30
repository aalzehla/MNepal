using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNepalProject.Models
{
    public class MNFundTransfer : ReplyMessage
    {
        public MNFundTransfer(string tid, string sc, string mobile, string sa, string amount, string da, string note, string pin, string sourcechannel, string billNo, string studName, string merchantType)
        {
            this.tid = tid;
            this.sc = sc;
            this.mobile = mobile;
            this.sa = sa;
            this.amount = amount;
            this.da = da;
            this.note = note;
            this.pin = pin;
            this.sourcechannel = sourcechannel;
            //add for merchant
            this.billNo = billNo;
            this.studName = studName;
            this.merchantType = merchantType;
        }

        public MNFundTransfer(string tid, string sc, string mobile, string sa, string amount, string da, string note, string pin, string sourcechannel, string reverseStatus, string merchantType)
        {
            this.tid = tid;
            this.sc = sc;
            this.mobile = mobile;
            this.sa = sa;
            this.amount = amount;
            this.da = da;
            this.note = note;
            this.pin = pin;
            this.sourcechannel = sourcechannel;
            this.reverseStatus = reverseStatus;
            this.merchantType = merchantType;
        }

        public MNFundTransfer(string tid, string sc, string mobile, string sa, string amount, string da, string note, string pin, string sourcechannel, string merchantType)
        {
            this.tid = tid;
            this.sc = sc;
            this.mobile = mobile;
            this.sa = sa;
            this.amount = amount;
            this.da = da;
            this.note = note;
            this.pin = pin;
            this.sourcechannel = sourcechannel;
            this.merchantType = merchantType;
        }
        public MNFundTransfer(string tid, string sc, string mobile, string sa, string amount, string da, string note, string pin, string sourcechannel)
        {
            this.tid = tid;
            this.sc = sc;
            this.mobile = mobile;
            this.sa = sa;
            this.amount = amount;
            this.da = da;
            this.note = note;
            this.pin = pin;
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
        public string vid { get; set; }
        public string description1 { get; set; }

        public string amount { get; set; }

        public string pin { get; set; }
        public string note { get; set; }
        public string sourcechannel { get; set; }
        public string prod { get; set; }

        //add for merchant
        public string studName { get; set; }
        public string billNo { get; set; }
        public string merchantType { get; set; }

        public string reverseStatus { get; set; }
        public bool valid()
        {
            if (this.tid != "" && this.sc != "" && this.mobile != "" && this.amount != "")
                return true;
            else
                return false;
        }

    }
}


