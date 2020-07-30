using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class AgentTable
    {
        public MNClient MNClient { get; set; }

        public MNClientContact MNClientContact { get; set; }

        public MNClientKYC MNClientKYC { get; set; }

        public MNClientKYCDoc MNClientKYCDoc { get; set; }

        public MNBankAccountMap MNBankAccountMap { get; set; }

    }

    public class MNClientKYCDoc {

        public string DocType { get; set; }
        public string PassportImage { get; set; }
        public string BackImage { get; set; }
        public string FrontImage { get; set; }
        public HttpPostedFileBase Front { get; set; }
        public HttpPostedFileBase Back { get; set; }
        public HttpPostedFileBase PassportPhoto { get; set; }
    }
}