using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MNepalWeb.Models
{
    public class MNClientContact
    {
        public int ID { get; set; }

        public string ClientCode { get; set; }

        public string ContactNumber1 { get; set; }

        public string ContactNumber2 { get; set; }

        public string EmailAddress { get; set; }

        public Guid CContactGuid { get; set; }

    }
}