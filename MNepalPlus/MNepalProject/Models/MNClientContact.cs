using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MNepalProject.Models
{
    public class MNClientContact
    {
        [Key]
        public int ID { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(8)]
        public string ClientCode { get; set; }


        [Column(TypeName = "VARCHAR")]
        [StringLength(10)]
        public string ContactNumber1 { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(10)]
        public string ContactNumber2 { get; set; }

        public MNClientContact() { }
        public MNClientContact(string ClientCode, string ContactNumber1, string ContactNumber2)
        {
            this.ClientCode = ClientCode;
            this.ContactNumber1 = ContactNumber1;
            this.ContactNumber2 = ContactNumber2;
        }
    }
}