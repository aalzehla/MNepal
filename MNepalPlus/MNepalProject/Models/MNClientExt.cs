using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MNepalProject.Models;

namespace MNepalProject.Models
{
    [PetaPoco.TableName("MNClientExt")]
    [PetaPoco.PrimaryKey("ID")]
    public class MNClientExt
    {
        [Key]
        public int ID { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(8)]
        public string ClientCode { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string userType { get; set; }

        public string Message { get; set; }
        public string PIN { get; set; }

        public string WalletProfileCode { get; set; }

        public MNClientExt() { }
        public MNClientExt(string ClientCode, string UserName, string Password, string userType)
        {
            this.ClientCode = ClientCode;
            this.UserName = UserName;
            this.Password = Password;
            this.userType = userType;
            
        }

        public MNClientExt(string ClientCode, string UserName, string Password)
        {
            this.ClientCode = ClientCode;
            this.UserName = UserName;
            this.Password = Password;
        }

    }
}